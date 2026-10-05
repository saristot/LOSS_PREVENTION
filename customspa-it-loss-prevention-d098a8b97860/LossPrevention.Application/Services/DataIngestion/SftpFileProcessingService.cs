using LossPrevention.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Renci.SshNet;
using System.Text;

namespace LossPrevention.Application.Services.DataIngestion
{
    public interface ISftpFileProcessingService
    {
        Task<SftpProcessingResult> ProcessSftpFilesAsync(
            string host,
            int port,
            string username,
            string password,
            string remoteDirectory,
            IFileProcessingService fileProcessor,
            CancellationToken cancellationToken = default);
    }

    public class SftpProcessingResult
    {
        public int FilesProcessed { get; set; }
        public int FilesSkipped { get; set; }
        public int FilesFailed { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool Success { get; set; }
    }

    public sealed class SftpFileProcessingService : ISftpFileProcessingService
    {
        private readonly ILogger<SftpFileProcessingService> _logger;
        private const string ProcessedFolder = "processed";
        private const string FailedFolder = "failed";

        public SftpFileProcessingService(ILogger<SftpFileProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task<SftpProcessingResult> ProcessSftpFilesAsync(
            string host,
            int port,
            string username,
            string password,
            string remoteDirectory,
            IFileProcessingService fileProcessor,
            CancellationToken cancellationToken = default)
        {
            var result = new SftpProcessingResult { Success = true };

            try
            {
                _logger.LogInformation("Connecting to SFTP server {Host}:{Port}", host, port);

                using var client = new SftpClient(host, port, username, password);

                // Set timeout
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(30);

                await Task.Run(() => client.Connect(), cancellationToken);

                if (!client.IsConnected)
                {
                    throw new InvalidOperationException("Failed to connect to SFTP server");
                }

                _logger.LogInformation("Connected to SFTP server successfully");

                // Ensure remote directory exists
                if (!client.Exists(remoteDirectory))
                {
                    throw new DirectoryNotFoundException($"Remote directory not found: {remoteDirectory}");
                }

                // Ensure processed and failed folders exist
                await EnsureFoldersExistAsync(client, remoteDirectory);

                // Get file extension based on processor
                var extension = GetFileExtension(fileProcessor.SupportedFileType);

                // List files in the remote directory
                var files = client.ListDirectory(remoteDirectory)
                    .Where(f => !f.IsDirectory &&
                                f.Name != "." &&
                                f.Name != ".." &&
                                f.Name.EndsWith($".{extension}", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _logger.LogInformation("Found {Count} {FileType} files in {Directory}",
                    files.Count, fileProcessor.SupportedFileType, remoteDirectory);

                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await ProcessSingleSftpFileAsync(
                            client,
                            file,
                            remoteDirectory,
                            fileProcessor,
                            result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing SFTP file: {FileName}", file.Name);
                        result.Errors.Add($"{file.Name}: {ex.Message}");
                        result.FilesFailed++;

                        // Try to move to failed folder
                        try
                        {
                            await MoveFileAsync(client, file.FullName,
                                Path.Combine(remoteDirectory, FailedFolder, file.Name).Replace("\\", "/"));
                        }
                        catch (Exception moveEx)
                        {
                            _logger.LogError(moveEx, "Failed to move file to failed folder: {FileName}", file.Name);
                        }
                    }
                }

                client.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SFTP processing failed");
                result.Success = false;
                result.Errors.Add($"SFTP Error: {ex.Message}");
            }

            return result;
        }

        private async Task ProcessSingleSftpFileAsync(
            SftpClient client,
            Renci.SshNet.Sftp.ISftpFile file,
            string remoteDirectory,
            IFileProcessingService fileProcessor,
            SftpProcessingResult result)
        {
            _logger.LogInformation("Processing SFTP file: {FileName}", file.Name);

            // Stream the file content from SFTP
            using var memoryStream = new MemoryStream();
            await Task.Run(() => client.DownloadFile(file.FullName, memoryStream));
            memoryStream.Position = 0;

            // Save to temp file for processing
            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(file.Name));

            try
            {
                // Write stream to temp file
                await using (var fileStream = File.Create(tempFilePath))
                {
                    await memoryStream.CopyToAsync(fileStream);
                }

                // Process the file using the existing processor
                var documents = await fileProcessor.ProcessFileAsync(tempFilePath, file.Name);

                if (documents == null || !documents.Any())
                {
                    _logger.LogWarning("No documents extracted from SFTP file: {FileName}", file.Name);
                    result.FilesSkipped++;
                    return;
                }

                _logger.LogInformation("Successfully processed {FileName}: {RecordCount} records",
                    file.Name, documents.Count);

                result.FilesProcessed++;

                // Move file to processed folder
                var processedPath = Path.Combine(remoteDirectory, ProcessedFolder, file.Name).Replace("\\", "/");
                await MoveFileAsync(client, file.FullName, processedPath);

                _logger.LogInformation("Moved {FileName} to processed folder", file.Name);
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temp file: {TempFile}", tempFilePath);
                    }
                }
            }
        }

        private async Task EnsureFoldersExistAsync(SftpClient client, string remoteDirectory)
        {
            var processedPath = Path.Combine(remoteDirectory, ProcessedFolder).Replace("\\", "/");
            var failedPath = Path.Combine(remoteDirectory, FailedFolder).Replace("\\", "/");

            await Task.Run(() =>
            {
                if (!client.Exists(processedPath))
                {
                    _logger.LogInformation("Creating processed folder: {Path}", processedPath);
                    client.CreateDirectory(processedPath);
                }

                if (!client.Exists(failedPath))
                {
                    _logger.LogInformation("Creating failed folder: {Path}", failedPath);
                    client.CreateDirectory(failedPath);
                }
            });
        }

        private async Task MoveFileAsync(SftpClient client, string sourcePath, string destinationPath)
        {
            await Task.Run(() =>
            {
                // If destination file exists, delete it first (or append timestamp)
                if (client.Exists(destinationPath))
                {
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = Path.GetFileNameWithoutExtension(destinationPath);
                    var extension = Path.GetExtension(destinationPath);
                    var directory = Path.GetDirectoryName(destinationPath)?.Replace("\\", "/");
                    destinationPath = Path.Combine(directory ?? "", $"{fileName}_{timestamp}{extension}").Replace("\\", "/");
                }

                client.RenameFile(sourcePath, destinationPath);
            });
        }

        private static string GetFileExtension(string fileType)
        {
            return fileType.ToUpperInvariant() switch
            {
                "XML" => "xml",
                "CSV" => "csv",
                "JSON" => "json",
                _ => throw new InvalidOperationException($"Unsupported file type: {fileType}")
            };
        }
    }
}