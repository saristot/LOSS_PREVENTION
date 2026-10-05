using FastEndpoints;
using FastEndpoints.Swagger;
using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.Services.DataIngestion;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Application.Interfaces.Data.Rules;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Application.Interfaces.Workspaces;
using LossPrevention.Application.Services.Data;
using LossPrevention.Application.Services.Rules;
using LossPrevention.Application.Services.Users;
using LossPrevention.Application.Services.Workspaces;
using LossPrevention.Infrastructure.Configuration;
using LossPrevention.Infrastructure.DependencyInjection;
using LossPrevention.Infrastructure.Interfaces;
using LossPrevention.Infrastructure.Models;
using LossPrevention.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174") // Vue dev server
            .AllowAnyHeader()
            .AllowAnyMethod() // ⬅️ includes OPTIONS, POST, etc.
            .AllowCredentials(); // only if you're sending cookies or auth headers
    });
});

// Load configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Load rules from MongoDB using a temporary provider
var tempServices = new ServiceCollection();
tempServices.AddInfrastructureServices(builder.Configuration);
tempServices.AddRepositoryServiceCollection(builder.Configuration);

using var tempProvider = tempServices.BuildServiceProvider();
var settings = tempProvider.GetRequiredService<IOptions<JwtSettings>>().Value;

builder.Services.AddMemoryCache();

// Register services
builder.Services.AddAuthorization();
builder.Services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Loss Prevention API";
            s.Version = "v1.0";
        };
    });

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey))
        };
    });

// Infrastructure & repository services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddRepositoryServiceCollection(builder.Configuration);

// Application services
builder.Services.AddSingleton<MongoDbSettings>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IXmlEnrichmentService, XmlEnrichmentService>();
builder.Services.AddScoped<IXmlProcessingService, XmlProcessingService>();
builder.Services.AddScoped<IRuleConfigurationService, RuleConfigurationService>();
builder.Services.AddScoped<IMappingService, MappingService>();
builder.Services.AddScoped<IReportDataservice, ReportDataservice>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IDistanceDataservice, DistanceDataService>();

// Data Ingestion services
builder.Services.AddScoped<IDataIngestionService, DataIngestionService>();
builder.Services.AddScoped<IDataIngestionScheduleService, DataIngestionScheduleService>();

// File Processing Services (XML, CSV, JSON)
builder.Services.AddScoped<IFileProcessingService, XmlProcessingService>();
builder.Services.AddScoped<IFileProcessingService, CsvProcessingService>();
builder.Services.AddScoped<IFileProcessingService, JsonProcessingService>();
builder.Services.AddScoped<IFileProcessingCoordinator, FileProcessingCoordinator>();
// SFTP File Processing Service
builder.Services.AddScoped<ISftpFileProcessingService, SftpFileProcessingService>();

// Database initialization & Background Services
builder.Services.AddScoped<IDatabaseInitializationService, DatabaseInitializationService>();
builder.Services.AddHostedService<DataIngestionBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
    await dbInit.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerGen();
    app.UseSwaggerUi();
}

app.UseCors("AllowVueDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.Run();