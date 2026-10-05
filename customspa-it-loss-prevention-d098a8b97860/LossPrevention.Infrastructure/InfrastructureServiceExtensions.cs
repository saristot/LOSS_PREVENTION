using LossPrevention.Application.Entities.Workspaces;
using LossPrevention.Domain.Dashboards;
using LossPrevention.Domain.Entities.Data;
using LossPrevention.Domain.Entities.DataIngestion;
using LossPrevention.Domain.Entities.FraudDetection;
using LossPrevention.Domain.Entities.Rules;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Domain.Groups;
using LossPrevention.Domain.Notifications;
using LossPrevention.Infrastructure.Configuration;
using LossPrevention.Infrastructure.Models;
using LossPrevention.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoDbSettings = new MongoDbSettings();
            configuration.GetSection("MongoDbSettings").Bind(mongoDbSettings);
            services.AddSingleton(Options.Create(mongoDbSettings));

            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.AddSingleton(Options.Create(jwtSettings));

            services.AddScoped<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            return services;
        }

        public static void AddRepositoryServiceCollection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMongoRepository<RuleConfiguration>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<RuleConfiguration>(client, settings.DatabaseName, settings.CollectionName_RulesConfiguration);
            });

            services.AddScoped<IMongoRepository<MappingItem>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<MappingItem>(client, settings.DatabaseName, settings.CollectionName_MappingConfiguration);
            });

            services.AddScoped<IMongoRepository<BsonDocument>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<BsonDocument>(client, settings.DatabaseName, settings.CollectionName_ReportData);
            });

            services.AddScoped<IMongoRepository<User>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<User>(client, settings.DatabaseName, settings.CollectionName_Users);
            });

            services.AddScoped<IMongoRepository<User>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<User>(client, settings.DatabaseName, settings.CollectionName_Users);
            });

            services.AddScoped<IMongoRepository<PasswordResetToken>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<PasswordResetToken>(client, settings.DatabaseName, "PasswordResetTokens");
            });

            services.AddScoped<IMongoRepository<Role>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<Role>(client, settings.DatabaseName, settings.CollectionName_Roles);
            });

            services.AddScoped<IMongoRepository<Permission>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<Permission>(client, settings.DatabaseName, settings.CollectionName_Permissions);
            });

            services.AddScoped<IMongoRepository<Workspace>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<Workspace>(client, settings.DatabaseName, settings.CollectionName_Workspaces);
            });

            services.AddScoped<IMongoRepository<DashboardDocument>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<DashboardDocument>(client, settings.DatabaseName, settings.CollectionName_Dashboards);
            });

            // Data Ingestion repositories
            services.AddScoped<IMongoRepository<DataIngestionConfiguration>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<DataIngestionConfiguration>(client, settings.DatabaseName, settings.CollectionName_DataIngestionConfigurations);
            });

            services.AddScoped<IMongoRepository<DataIngestionSchedule>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<DataIngestionSchedule>(client, settings.DatabaseName, settings.CollectionName_DataIngestionSchedules);
            });

            services.AddScoped<IMongoRepository<NotificationDocument>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<NotificationDocument>(client, settings.DatabaseName, settings.CollectionName_Notifications);
            });

            services.AddScoped<IMongoRepository<GroupDocument>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<GroupDocument>(client, settings.DatabaseName, settings.CollectionName_Groups);
            });

            services.AddScoped<IMongoRepository<FraudDetectionSettings>>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoRepository<FraudDetectionSettings>(client, settings.DatabaseName, settings.CollectionName_FraudDetectionSettings);
            });
        }
    }
}