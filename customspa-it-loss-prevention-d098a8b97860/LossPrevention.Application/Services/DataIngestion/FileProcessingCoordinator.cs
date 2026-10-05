using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.DataIngestion
{
    public interface IFileProcessingCoordinator
    {
        Task<FileProcessingResult> RunIngestionAsync(CancellationToken cancellationToken = default);
    }

    public class FileProcessingResult
    {
        public int FilesProcessed { get; set; }
        public int RecordsInserted { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public sealed class FileProcessingCoordinator : IFileProcessingCoordinator
    {
        private readonly IDataIngestionService _configService;
        private readonly IMongoRepository<BsonDocument> _reportDataRepository;
        private readonly IMappingService _mappingService;
        private readonly IEnumerable<IFileProcessingService> _fileProcessors;
        private readonly ISftpFileProcessingService _sftpService;
        private readonly ILogger<FileProcessingCoordinator> _logger;

        public FileProcessingCoordinator(
            IDataIngestionService configService,
            IMongoRepository<BsonDocument> reportDataRepository,
            IMappingService mappingService,
            IEnumerable<IFileProcessingService> fileProcessors,
            ISftpFileProcessingService sftpService,
            ILogger<FileProcessingCoordinator> logger)
        {
            _configService = configService;
            _reportDataRepository = reportDataRepository;
            _mappingService = mappingService;
            _fileProcessors = fileProcessors;
            _sftpService = sftpService;
            _logger = logger;
        }

        public async Task<FileProcessingResult> RunIngestionAsync(CancellationToken cancellationToken = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = new FileProcessingResult { Success = true };

            try
            {
                _logger.LogInformation("Starting data ingestion...");

                // Get configuration
                var config = await _configService.GetConfigurationAsync();
                if (config == null)
                {
                    throw new InvalidOperationException("No data ingestion configuration found");
                }

                // Validate configuration
                if (config.SelectedSources == null || !config.SelectedSources.Any())
                {
                    throw new InvalidOperationException("No data source selected");
                }

                if (string.IsNullOrWhiteSpace(config.SelectedFileType))
                {
                    throw new InvalidOperationException("No file type selected");
                }

                // Get the appropriate file processor
                var processor = _fileProcessors.FirstOrDefault(p =>
                    p.SupportedFileType.Equals(config.SelectedFileType, StringComparison.OrdinalIgnoreCase));

                if (processor == null)
                {
                    throw new InvalidOperationException($"No processor found for file type: {config.SelectedFileType}");
                }

                // Process based on source type
                foreach (var source in config.SelectedSources)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Normalize the source value (trim and lowercase)
                    var normalizedSource = source.Trim().ToLowerInvariant();

                    if (normalizedSource == "filesystem" || normalizedSource == "file system")
                    {
                        await ProcessFileSystemFilesAsync(config, processor, result, cancellationToken);
                    }
                    else if (normalizedSource == "sftp")
                    {
                        await ProcessSftpFilesAsync(config, processor, result, cancellationToken);
                    }
                }

                // Apply mappings if configured
                if (config.UseMappings && result.RecordsInserted > 0)
                {
                    _logger.LogInformation("Applying field mappings...");
                    await _mappingService.ProcessMappings("ReportData", int.MaxValue);
                    await _mappingService.FinalizeTypesAsync();
                }

                sw.Stop();
                result.Duration = sw.Elapsed;

                _logger.LogInformation(
                    "Data ingestion completed. Files: {FilesProcessed}, Records: {RecordsInserted}, Duration: {Duration}s",
                    result.FilesProcessed, result.RecordsInserted, result.Duration.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data ingestion failed");
                result.Success = false;
                result.Errors.Add(ex.Message);
                sw.Stop();
                result.Duration = sw.Elapsed;
            }

            return result;
        }

        private async Task ProcessFileSystemFilesAsync(
            Domain.Entities.DataIngestion.DataIngestionConfiguration config,
            IFileProcessingService processor,
            FileProcessingResult result,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(config.FileSystemPath))
            {
                throw new InvalidOperationException("File system path not configured");
            }

            if (!Directory.Exists(config.FileSystemPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {config.FileSystemPath}");
            }

            _logger.LogInformation("Processing files from: {Path}", config.FileSystemPath);

            // Get file extension based on file type
            var extension = GetFileExtension(config.SelectedFileType);
            var searchPattern = $"*.{extension}";

            var files = Directory.GetFiles(config.FileSystemPath, searchPattern);
            _logger.LogInformation("Found {Count} files to process", files.Length);

            foreach (var filePath in files)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await ProcessSingleFileAsync(filePath, Path.GetFileName(filePath), processor, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file: {File}", filePath);
                    result.Errors.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                }
            }
        }

        private async Task ProcessSftpFilesAsync(
            Domain.Entities.DataIngestion.DataIngestionConfiguration config,
            IFileProcessingService processor,
            FileProcessingResult result,
            CancellationToken cancellationToken)
        {
            // Validate SFTP configuration
            if (string.IsNullOrWhiteSpace(config.SftpHost))
            {
                throw new InvalidOperationException("SFTP host not configured");
            }

            if (string.IsNullOrWhiteSpace(config.SftpUsername))
            {
                throw new InvalidOperationException("SFTP username not configured");
            }

            if (string.IsNullOrWhiteSpace(config.SftpPassword))
            {
                throw new InvalidOperationException("SFTP password not configured");
            }

            if (string.IsNullOrWhiteSpace(config.SftpRemoteDirectory))
            {
                throw new InvalidOperationException("SFTP remote directory not configured");
            }

            _logger.LogInformation("Processing files from SFTP: {Host}:{Port}{Directory}",
                config.SftpHost, config.SftpPort, config.SftpRemoteDirectory);

            // Use the SFTP service to process files
            var sftpResult = await _sftpService.ProcessSftpFilesAsync(
                config.SftpHost,
                config.SftpPort,
                config.SftpUsername,
                config.SftpPassword,
                config.SftpRemoteDirectory,
                processor,
                cancellationToken);

            // For SFTP, we need to track the files and get the actual documents
            // Let's process them through our tracking system
            await ProcessSftpResultsAsync(sftpResult, processor, config, result, cancellationToken);

            // Merge SFTP errors into main result
            result.Errors.AddRange(sftpResult.Errors);

            if (!sftpResult.Success)
            {
                result.Success = false;
            }
        }

        private async Task ProcessSftpResultsAsync(
            SftpProcessingResult sftpResult,
            IFileProcessingService processor,
            Domain.Entities.DataIngestion.DataIngestionConfiguration config,
            FileProcessingResult result,
            CancellationToken cancellationToken)
        {
            // Re-download and process files from SFTP processed folder to insert into DB
            // This is a simplified approach - in production you might want to optimize this

            using var client = new Renci.SshNet.SftpClient(
                config.SftpHost,
                config.SftpPort,
                config.SftpUsername,
                config.SftpPassword);

            try
            {
                await Task.Run(() => client.Connect(), cancellationToken);

                var processedPath = Path.Combine(config.SftpRemoteDirectory, "processed").Replace("\\", "/");

                if (!client.Exists(processedPath))
                {
                    _logger.LogWarning("Processed folder does not exist: {Path}", processedPath);
                    return;
                }

                var extension = GetFileExtension(config.SelectedFileType);
                var processedFiles = client.ListDirectory(processedPath)
                    .Where(f => !f.IsDirectory &&
                                f.Name != "." &&
                                f.Name != ".." &&
                                f.Name.EndsWith($".{extension}", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var file in processedFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        // Check if we've already inserted this file into the database
                        if (await IsFileAlreadyProcessedAsync(file.Name))
                        {
                            _logger.LogInformation("File already in database, skipping: {FileName}", file.Name);
                            continue;
                        }

                        // Download and process
                        using var memoryStream = new MemoryStream();
                        await Task.Run(() => client.DownloadFile(file.FullName, memoryStream), cancellationToken);
                        memoryStream.Position = 0;

                        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(file.Name));

                        try
                        {
                            await using (var fileStream = File.Create(tempFilePath))
                            {
                                await memoryStream.CopyToAsync(fileStream, cancellationToken);
                            }

                            var documents = await processor.ProcessFileAsync(tempFilePath, file.Name);

                            if (documents != null && documents.Any())
                            {
                                // Add tracking metadata
                                var processedAt = DateTime.UtcNow;
                                foreach (var doc in documents)
                                {
                                    doc["_sourceFile"] = file.Name;
                                    doc["_processedAt"] = processedAt;
                                    doc["_sourceType"] = "sftp";
                                }

                                // Insert into database
                                await _reportDataRepository.InsertManyAsync(documents);

                                // Mark as processed
                                await MarkFileAsProcessedAsync(file.Name, documents.Count, processedAt);

                                result.FilesProcessed++;
                                result.RecordsInserted += documents.Count;

                                _logger.LogInformation("Inserted {RecordCount} records from SFTP file: {FileName}",
                                    documents.Count, file.Name);
                            }
                        }
                        finally
                        {
                            if (File.Exists(tempFilePath))
                            {
                                File.Delete(tempFilePath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing SFTP file for database insertion: {FileName}", file.Name);
                        result.Errors.Add($"DB Insertion - {file.Name}: {ex.Message}");
                    }
                }

                client.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SFTP results");
                result.Errors.Add($"SFTP Results Processing: {ex.Message}");
            }
        }

        private async Task ProcessSingleFileAsync(
            string filePath,
            string fileName,
            IFileProcessingService processor,
            FileProcessingResult result)
        {
            _logger.LogInformation("Processing file: {FileName}", fileName);

            // Check if already processed
            if (await IsFileAlreadyProcessedAsync(fileName))
            {
                _logger.LogInformation("File already processed, skipping: {FileName}", fileName);
                return;
            }

            // Process the file
            var documents = await processor.ProcessFileAsync(filePath, fileName);

            if (documents == null || !documents.Any())
            {
                _logger.LogWarning("No documents extracted from file: {FileName}", fileName);
                return;
            }

            // Add tracking metadata to all documents
            var processedAt = DateTime.UtcNow;
            foreach (var doc in documents)
            {
                doc["_sourceFile"] = fileName;
                doc["_processedAt"] = processedAt;
                doc["_sourceType"] = "filesystem";
            }

            // Insert documents
            await _reportDataRepository.InsertManyAsync(documents);

            // Mark file as processed
            await MarkFileAsProcessedAsync(fileName, documents.Count, processedAt);

            result.FilesProcessed++;
            result.RecordsInserted += documents.Count;

            _logger.LogInformation(
                "Successfully processed {FileName}: {RecordCount} records",
                fileName, documents.Count);
        }

        private async Task<bool> IsFileAlreadyProcessedAsync(string fileName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("fileName", fileName);
            var processedFiles = await _reportDataRepository.Collection.Database
                .GetCollection<BsonDocument>("ProcessedFiles")
                .Find(filter)
                .Limit(1)
                .ToListAsync();

            return processedFiles.Any();
        }

        private async Task MarkFileAsProcessedAsync(string fileName, int recordCount, DateTime processedAt)
        {
            var processedFileDoc = new BsonDocument
            {
                { "fileName", fileName },
                { "recordCount", recordCount },
                { "processedAt", processedAt }
            };

            await _reportDataRepository.Collection.Database
                .GetCollection<BsonDocument>("ProcessedFiles")
                .InsertOneAsync(processedFileDoc);
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