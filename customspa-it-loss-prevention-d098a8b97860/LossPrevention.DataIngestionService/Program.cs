using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Application.Interfaces.Data.Rules;
using LossPrevention.Application.Interfaces.Indexes;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Application.Services.Data;
using LossPrevention.Application.Services.DataIngestion;
using LossPrevention.Application.Services.Rules;
using LossPrevention.Application.Services.Users;
using LossPrevention.Domain.Entities.Rules;
using LossPrevention.Infrastructure.DependencyInjection;
using LossPrevention.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Channels;

internal class Program
{
    static readonly int MaxDegree = Environment.ProcessorCount;   // file/enrichment parallelism
    const int ChannelCapacity = 10_000;                           // backpressure
    const int BatchSize = 1_000;                                  // insert batch
    static readonly TimeSpan MaxBatchDelay = TimeSpan.FromMilliseconds(100);

    private static async Task Main(string[] args)
    {
        // STEP 1: Load configuration and create temp provider ONLY for rule loading
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var configuration = configBuilder.Build();

        // Load rules (but don't process files here)
        var tempServices = new ServiceCollection();
        tempServices.AddInfrastructureServices(configuration);
        tempServices.AddRepositoryServiceCollection(configuration);

        using (var tempProvider = tempServices.BuildServiceProvider())
        {
            var ruleRepo = tempProvider.GetRequiredService<IMongoRepository<RuleConfiguration>>();
            var rules = await ruleRepo.GetAllAsync();
            // Just load rules, don't process files
        }

        // STEP 2: Build final host
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructureServices(configuration);
                services.AddRepositoryServiceCollection(configuration);

                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IUserRoleService, UserRoleService>();
                services.AddScoped<IUserPermissionService, UserPermissionService>();
                services.AddScoped<IXmlEnrichmentService, XmlEnrichmentService>();
                services.AddScoped<IXmlProcessingService, XmlProcessingService>();
                services.AddScoped<IRuleConfigurationService, RuleConfigurationService>();
                services.AddScoped<IMappingService, MappingService>();
                services.AddScoped<IIndexService, IndexService>();
            })
            .Build();

        // STEP 3: Process XML files ONLY ONCE using the final host
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var filenames = Directory.GetFiles(@"C:\xmlstore5\xml");
        var processor = services.GetRequiredService<IXmlProcessingService>();

        var sw = Stopwatch.StartNew();
        Console.WriteLine("Starting Processing");

        var cts = new CancellationTokenSource();
        var token = cts.Token;

        await Parallel.ForEachAsync(filenames, new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxDegree,
            CancellationToken = token
        }, async (file, ct) =>
        {
            try
            {
                var xml = await File.ReadAllTextAsync(file, ct);
                var bson = await processor.ProcessAsync(xml);

                // Add source file tracking BEFORE insert
                bson["_sourceFile"] = Path.GetFileName(file);
                bson["_processedAt"] = DateTime.UtcNow;

                // Insert the document (processor does not insert)
                var repo = services.GetRequiredService<IMongoRepository<BsonDocument>>();
                await repo.InsertOneAsync(bson);

                // bson should now contain an _id (driver will generate one if missing)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
            }
        });

        // Map the processed data
        var mappingService = services.GetRequiredService<IMappingService>();
        await mappingService.ProcessMappings("ReportData", 1000);

        var indexService = services.GetRequiredService<IIndexService>();
        await indexService.ProcessIndexesAsync();

        await mappingService.FinalizeTypesAsync();

        sw.Stop();
        Console.WriteLine($"Completed in {sw.Elapsed.TotalSeconds} Seconds");
    }
}
