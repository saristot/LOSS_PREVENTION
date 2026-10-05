# Backend Architecture Documentation

## Table of Contents
1. [Technology Stack](#technology-stack)
2. [Solution Structure](#solution-structure)
3. [Architecture Patterns](#architecture-patterns)
4. [Layer Breakdown](#layer-breakdown)
5. [Component Inventory](#component-inventory)
6. [Key Services](#key-services)
7. [Dependency Injection](#dependency-injection)
8. [Data Processing Pipeline](#data-processing-pipeline)
9. [Background Services](#background-services)
10. [Security Implementation](#security-implementation)
11. [Error Handling](#error-handling)
12. [Logging and Monitoring](#logging-and-monitoring)
13. [Testing Approach](#testing-approach)

---

## Technology Stack

### Core Technologies
| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Application framework |
| C# | 11.0 | Programming language |
| MongoDB | 3.4.0 | Primary database |
| MongoDB Driver | 3.4.0 | Database connectivity |

### API Layer
| Package | Version | Purpose |
|---------|---------|---------|
| FastEndpoints | 6.0.0 | HTTP endpoint framework |
| FastEndpoints.Security | 6.0.0 | Authentication/authorization |
| FastEndpoints.Swagger | 6.0.0 | API documentation |
| Microsoft.IdentityModel.Tokens | Latest | JWT token validation |

### Application Layer
| Package | Version | Purpose |
|---------|---------|---------|
| FluentValidation | 11.11.0 | Input validation |
| Newtonsoft.Json | 13.0.3 | JSON processing |
| SSH.NET | 2025.1.0 | SFTP connectivity |
| MongoDB.Bson | 3.4.0 | BSON document handling |

### Infrastructure Layer
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Extensions.* | 9.0.4 | Dependency injection, configuration |
| Microsoft.AspNetCore.App | Framework | Core ASP.NET functionality |

---

## Solution Structure

### Projects Overview

```
LossPrevention.sln
├── LossPrevention.API                      # HTTP API Layer (FastEndpoints)
├── LossPrevention.Application              # Business Logic & Services
├── LossPrevention.Domain                   # Core Entities & Business Rules
├── LossPrevention.Infrastructure           # Data Access & External Services
└── LossPrevention.DataIngestionService     # Batch Processing Console App
```

### Project Dependencies

```
API → Application → Domain
      ↓            ↑
Infrastructure ────┘

DataIngestionService → Application → Domain
                       ↓            ↑
                 Infrastructure ────┘
```

---

## Architecture Patterns

### 1. **Clean Architecture**
The solution follows Clean Architecture principles with clear separation of concerns:
- **Domain Layer**: Core business entities and rules (framework-independent)
- **Application Layer**: Business logic, services, interfaces
- **Infrastructure Layer**: Data access, external services implementation
- **API Layer**: HTTP endpoints, request/response handling

### 2. **Repository Pattern**
Generic repository pattern for data access:
- `IMongoRepository<TDocument>` interface
- `MongoRepository<TDocument>` implementation
- Type-safe database operations
- Expression-based queries

### 3. **CQRS-like Structure**
Endpoints follow command/query separation:
- Read operations: `EndpointWithoutRequest<TResponse>`
- Write operations: `Endpoint<TRequest, TResponse>`

### 4. **Dependency Injection**
Constructor injection throughout:
- Service registration in `Program.cs`
- Extension methods for modular configuration
- Scoped, Singleton, and Transient lifetimes

### 5. **Strategy Pattern**
File processing uses strategy pattern:
- `IFileProcessingService` interface
- Multiple implementations: XML, CSV, JSON
- `FileProcessingCoordinator` orchestrates processing

---

## Layer Breakdown

### API Layer (LossPrevention.API)

**Responsibility**: HTTP endpoint definitions, request/response handling, authentication

**Key Components**:
- FastEndpoints-based HTTP endpoints
- JWT authentication configuration
- CORS policy setup
- Swagger/OpenAPI documentation
- Middleware pipeline configuration

**Structure**:
```
Endpoints/
├── Dashboard/          # Dashboard management
├── Data/              # Transaction data operations
├── DataIngestion/     # File ingestion triggers
├── FraudDetection/    # Fraud detection settings
├── Groups/            # Group management
├── Mappings/          # Field mapping configuration
├── Notifications/     # User notifications
├── Rules/             # Business rule management
├── User/              # User, role, permission management
└── Workspaces/        # Workspace configuration
```

### Application Layer (LossPrevention.Application)

**Responsibility**: Business logic implementation, data transformation, validation

**Key Components**:
- Service implementations
- DTOs for data transfer
- Business logic helpers
- Validation rules
- Service interfaces

**Structure**:
```
Services/
├── Data/              # Data processing services
├── DataIngestion/     # File ingestion orchestration
├── Rules/             # Rule evaluation
├── Users/             # User management
└── Workspaces/        # Workspace operations

Helpers/
├── BsonHelper.cs              # BSON manipulation
├── DistanceHelper.cs          # Distance calculations
├── JsonHelper.cs              # JSON utilities
├── MappingHelper.cs           # Field mapping logic
├── RuleHelper.cs              # Rule evaluation
└── XmlToBsonConverterHelper.cs # XML to BSON conversion

DTO/
├── Dashboard/         # Dashboard DTOs
├── Data/             # Data operation DTOs
├── DataIngestion/    # Ingestion configuration DTOs
├── FraudDetection/   # Fraud detection DTOs
├── Mappings/         # Mapping DTOs
├── Rules/            # Rule DTOs
├── Users/            # User/Role/Permission DTOs
└── Workspaces/       # Workspace DTOs

Validators/
├── CreateDashboardValidator.cs
└── UpdateDashboardValidator.cs
```

### Domain Layer (LossPrevention.Domain)

**Responsibility**: Core business entities, value objects, domain exceptions

**Key Components**:
```
Entities/
├── Dashboards/
│   └── DashboardDocument.cs
├── Data/
│   └── MappingItem.cs
├── DataIngestion/
│   ├── DataIngestionConfiguration.cs
│   └── DataIngestionSchedule.cs
├── FraudDetection/
│   └── FraudDetectionSettings.cs
├── Groups/
│   └── GroupDocument.cs
├── Indexes/
│   └── IndexSuggestion.cs
├── Notifications/
│   └── NotificationDocument.cs
├── Rules/
│   └── RuleConfiguration.cs
├── Users/
│   ├── User.cs
│   ├── Role.cs
│   ├── Permission.cs
│   ├── PasswordResetToken.cs
│   └── UserWithRolesAndPermissions.cs
└── Workspaces/
    ├── Workspace.cs
    ├── Tab.cs
    ├── Query.cs
    ├── Field.cs
    └── Condition.cs

Exceptions/
├── DeleteUserRoleException.cs
└── InvalidCustomerException.cs
```

### Infrastructure Layer (LossPrevention.Infrastructure)

**Responsibility**: Data persistence, external service integration, cross-cutting concerns

**Key Components**:
```
Repositories/
├── IMongoRepository.cs        # Generic repository interface
├── MongoRepository.cs         # MongoDB implementation
├── IDapperRepository.cs       # Dapper interface (future)
└── DapperRepository.cs        # SQL Server implementation (future)

Configuration/
├── MongoDbSettings.cs         # Database configuration
└── JwtSettings.cs             # JWT configuration

Services/
└── EmailService.cs            # SMTP email service

Helpers/
└── PasswordHasher.cs          # Password hashing/verification

DependencyInjection/
└── InfrastructureServiceExtensions.cs  # Service registration
```

---

## Component Inventory

### API Layer Components

| Component | Path | Responsibility |
|-----------|------|----------------|
| Program.cs | `/Program.cs` | Application startup, DI configuration, middleware setup |
| CreateWorkspaceEndpoint | `/Endpoints/Workspaces/` | Create new workspace |
| GetAllWorkspacesEndpoint | `/Endpoints/Workspaces/` | Retrieve all workspaces |
| UpdateWorkspaceEndpoint | `/Endpoints/Workspaces/` | Update workspace configuration |
| DeleteWorkspaceEndpoint | `/Endpoints/Workspaces/` | Delete workspace |
| GetReportDataEndpoint | `/Endpoints/Data/` | Query transaction data |
| GetDistanceEndpoint | `/Endpoints/Data/` | Calculate distance between transactions |
| CreateTransactionEndpoint | `/Endpoints/Data/` | Create new transaction |
| GetMappingsEndpoint | `/Endpoints/Mappings/` | Get field mappings |
| CreateMappingEndpoint | `/Endpoints/Mappings/` | Create field mapping |
| UpdateMappingEndpoint | `/Endpoints/Mappings/` | Update field mapping |
| DeleteMappingEndpoint | `/Endpoints/Mappings/` | Delete field mapping |
| GetAllRulesEndpoint | `/Endpoints/Rules/` | Get all business rules |
| CreateRuleEndpoint | `/Endpoints/Rules/` | Create business rule |
| UpdateRuleEndpoint | `/Endpoints/Rules/` | Update business rule |
| DeleteRuleEndpoint | `/Endpoints/Rules/` | Delete business rule |
| ApplyRulesEndpoint | `/Endpoints/Rules/` | Apply rules to data |
| CreateNotificationEndpoint | `/Endpoints/Notifications/` | Create notification |
| ListNotificationsEndpoint | `/Endpoints/Notifications/` | List user notifications |
| MarkAsReadEndpoint | `/Endpoints/Notifications/` | Mark notification as read |
| ReplyToNotificationEndpoint | `/Endpoints/Notifications/` | Reply to notification |
| DeleteNotificationEndpoint | `/Endpoints/Notifications/` | Delete notification |

### Application Layer Components

#### Services

| Service | Path | Responsibility |
|---------|------|----------------|
| UserService | `/Services/Users/` | User CRUD, authentication |
| UserRoleService | `/Services/Users/` | Role management |
| UserPermissionService | `/Services/Users/` | Permission management |
| PasswordResetService | `/Services/Users/` | Password reset workflow |
| WorkspaceService | `/Services/Workspaces/` | Workspace operations |
| XmlProcessingService | `/Services/Data/` | XML file processing |
| CsvProcessingService | `/Services/Data/` | CSV file processing |
| JsonProcessingService | `/Services/Data/` | JSON file processing |
| XmlEnrichmentService | `/Services/Data/` | XML data enrichment |
| MappingService | `/Services/Data/` | Field mapping operations |
| ReportDataservice | `/Services/Data/` | Transaction data queries |
| DistanceDataService | `/Services/Data/` | Distance calculations |
| IndexSuggestionHelper | `/Services/Data/` | Index recommendations |
| DataIngestionService | `/Services/DataIngestion/` | Ingestion configuration |
| DataIngestionScheduleService | `/Services/DataIngestion/` | Schedule management |
| FileProcessingCoordinator | `/Services/DataIngestion/` | File processing orchestration |
| SftpFileProcessingService | `/Services/DataIngestion/` | SFTP file download/processing |
| DataIngestionBackgroundService | `/Services/DataIngestion/` | Background scheduled ingestion |
| DatabaseInitializationService | `/Services/Data/` | Database initialization |

#### Helpers

| Helper | Path | Responsibility |
|--------|------|----------------|
| BsonHelper | `/Helpers/` | BSON document manipulation utilities |
| DistanceHelper | `/Helpers/` | Geographic distance calculations |
| JsonHelper | `/Helpers/` | JSON parsing and conversion |
| MappingHelper | `/Helpers/` | Field mapping transformations |
| RuleHelper | `/Helpers/` | Business rule evaluation logic |
| XmlToBsonConverterHelper | `/Helpers/` | XML to BSON conversion |

#### Interfaces

| Interface | Path | Responsibility |
|-----------|------|----------------|
| IUserService | `/Interfaces/User/` | User management contract |
| IUserRoleService | `/Interfaces/User/` | Role management contract |
| IUserPermissionService | `/Interfaces/User/` | Permission management contract |
| IPasswordResetService | `/Interfaces/User/` | Password reset contract |
| IWorkspaceService | `/Interfaces/Workspaces/` | Workspace operations contract |
| IXmlProcessingService | `/Interfaces/Data/` | XML processing contract |
| IXmlEnrichmentService | `/Interfaces/Data/` | XML enrichment contract |
| IXmlEnrichmentRule | `/Interfaces/Data/` | Enrichment rule contract |
| IMappingService | `/Interfaces/Data/` | Mapping operations contract |
| IReportDataservice | `/Interfaces/Data/` | Data query contract |
| IFileProcessingService | `/Interfaces/Data/` | File processing contract |
| IFileProcessingCoordinator | `/Interfaces/DataIngestion/` | Processing orchestration contract |
| IDataIngestionService | `/Interfaces/DataIngestion/` | Ingestion configuration contract |
| IDatabaseInitializationService | `/Interfaces/Data/` | Database initialization contract |
| IIndexService | `/Interfaces/Indexes/` | Index management contract |

### Infrastructure Layer Components

| Component | Path | Responsibility |
|-----------|------|----------------|
| MongoRepository<T> | `/Repositories/` | Generic MongoDB CRUD operations |
| IMongoRepository<T> | `/Repositories/` | Repository interface |
| DapperRepository | `/Repositories/` | SQL Server data access (future) |
| EmailService | `/Services/` | SMTP email sending |
| PasswordHasher | `/Helpers/` | Secure password hashing (PBKDF2) |
| MongoDbSettings | `/Configuration/` | MongoDB connection settings |
| JwtSettings | `/Configuration/` | JWT authentication settings |
| InfrastructureServiceExtensions | `/` | Service registration extensions |

### Domain Layer Components

| Component | Path | Responsibility |
|-----------|------|----------------|
| User | `/Entities/Users/` | User entity |
| Role | `/Entities/Users/` | Role entity |
| Permission | `/Entities/Users/` | Permission entity |
| PasswordResetToken | `/Entities/Users/` | Password reset token |
| RuleConfiguration | `/Entities/Rules/` | Business rule definition |
| MappingItem | `/Entities/Data/` | Field mapping definition |
| Workspace | `/Entities/Workspaces/` | Workspace configuration |
| DashboardDocument | `/Entities/Dashboards/` | Dashboard definition |
| NotificationDocument | `/Entities/Notifications/` | User notification |
| GroupDocument | `/Entities/Groups/` | User group |
| DataIngestionConfiguration | `/Entities/DataIngestion/` | Ingestion settings |
| DataIngestionSchedule | `/Entities/DataIngestion/` | Ingestion schedule |
| FraudDetectionSettings | `/Entities/FraudDetection/` | Fraud detection configuration |
| IndexSuggestion | `/Entities/Indexes/` | Index recommendation |

---

## Key Services

### 1. User Management Services

#### UserService
**Purpose**: Core user management operations

**Key Responsibilities**:
- User registration with password hashing (PBKDF2)
- User authentication and login
- User CRUD operations
- Password verification
- User activation/deactivation
- Field-level data locking per user

**Key Methods**:
```csharp
Task<User> CreateUserAsync(User user, string password)
Task<bool> UpdateUserAsync(User user)
Task<bool> DeleteUserAsync(ObjectId userId)
Task<User> AuthenticateAsync(string username, string password)
Task<User> GetUserByIdAsync(ObjectId userId)
Task<List<User>> GetAllUsersAsync()
```

#### UserRoleService
**Purpose**: Role assignment and management

**Key Responsibilities**:
- Assign roles to users
- Remove roles from users
- Get user roles
- Role-based authorization

#### UserPermissionService
**Purpose**: Permission management

**Key Responsibilities**:
- Assign permissions to users/roles
- Check user permissions
- Permission-based authorization

#### PasswordResetService
**Purpose**: Password reset workflow

**Key Responsibilities**:
- Generate password reset tokens
- Validate reset tokens
- Reset user passwords
- Token expiration management

### 2. Data Processing Services

#### XmlProcessingService
**Purpose**: Process XML files into BSON documents

**Key Responsibilities**:
- Parse XML files
- Enrich XML with business rules
- Convert XML to flattened BSON
- Return structured documents for insertion

**Processing Flow**:
```
XML File → Parse → Enrich → Convert to BSON → Return Document
```

#### CsvProcessingService
**Purpose**: Process CSV files into BSON documents

**Key Responsibilities**:
- Parse CSV files
- Map columns to fields
- Convert rows to BSON documents
- Handle type conversions

#### JsonProcessingService
**Purpose**: Process JSON files into BSON documents

**Key Responsibilities**:
- Parse JSON files
- Convert JSON to BSON
- Handle nested structures
- Validate JSON structure

#### FileProcessingCoordinator
**Purpose**: Orchestrate file processing across multiple sources

**Key Responsibilities**:
- Coordinate file processing from multiple sources (filesystem, SFTP)
- Select appropriate processor based on file type
- Batch insert documents into MongoDB
- Track processing metrics (files, records, errors)
- Handle cancellation and error recovery
- Trigger post-processing (mapping, indexing)

**Processing Pipeline**:
```
Configuration → Select Source → Download Files → Process Files → 
Batch Insert → Apply Mappings → Create Indexes → Return Results
```

#### XmlEnrichmentService
**Purpose**: Apply business rules to enrich XML data

**Key Responsibilities**:
- Apply enrichment rules to XML
- Add calculated fields
- Transform data based on rules
- Extensible rule system

### 3. Data Ingestion Services

#### DataIngestionService
**Purpose**: Manage data ingestion configuration

**Key Responsibilities**:
- Store ingestion configuration
- Manage file sources (filesystem, SFTP)
- Configure file type selection
- Set processing options

#### DataIngestionScheduleService
**Purpose**: Manage ingestion schedules

**Key Responsibilities**:
- Create recurring schedules
- One-time ingestion setup
- Schedule validation
- Schedule activation/deactivation

#### SftpFileProcessingService
**Purpose**: Download and process files from SFTP servers

**Key Responsibilities**:
- Connect to SFTP servers
- List remote files
- Download files matching patterns
- Handle SSH authentication
- Clean up downloaded files

### 4. Mapping & Rule Services

#### MappingService
**Purpose**: Manage field mappings and transformations

**Key Responsibilities**:
- Define field mappings (name → alias)
- Transform data based on mappings
- Detect data types
- Calculate field lengths
- Apply mappings to documents
- Generate type definitions

**Processing Flow**:
```
Scan Documents → Detect Fields → Create Mappings → 
Apply Transformations → Generate Types
```

#### RuleConfigurationService
**Purpose**: Manage business rules (not found in search, likely exists)

**Key Responsibilities**:
- Create/update/delete rules
- Evaluate rules against data
- Rule validation
- Rule execution

**RuleHelper Capabilities**:
- Evaluate rules on BSON documents
- Support field path navigation
- Range checking (min/max)
- Value comparison
- Sum aggregation across arrays
- Nested document traversal

### 5. Query & Analysis Services

#### ReportDataservice
**Purpose**: Query and retrieve transaction data

**Key Responsibilities**:
- Complex MongoDB aggregations
- Dynamic query building
- Pagination
- Filtering and sorting
- Field projection

#### DistanceDataService
**Purpose**: Calculate distances between transactions

**Key Responsibilities**:
- Geographic distance calculations
- Haversine formula implementation
- Transaction proximity analysis
- Fraud detection support

#### WorkspaceService
**Purpose**: Manage user workspace configurations

**Key Responsibilities**:
- Create/update/delete workspaces
- Manage workspace tabs
- Configure workspace queries
- Field visibility configuration

### 6. Infrastructure Services

#### EmailService
**Purpose**: Send email notifications

**Key Responsibilities**:
- SMTP email delivery
- Password reset emails
- HTML email templates
- Email configuration management

**Configuration**:
- SMTP host/port
- From address/name
- Frontend URL for links
- Development-friendly (smtp4dev)

---

## Dependency Injection

### Service Registration Architecture

#### Program.cs (API)
```csharp
// Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddRepositoryServiceCollection(builder.Configuration);

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IXmlProcessingService, XmlProcessingService>();
builder.Services.AddScoped<IDataIngestionService, DataIngestionService>();
// ... more services

// File processing services (strategy pattern)
builder.Services.AddScoped<IFileProcessingService, XmlProcessingService>();
builder.Services.AddScoped<IFileProcessingService, CsvProcessingService>();
builder.Services.AddScoped<IFileProcessingService, JsonProcessingService>();
builder.Services.AddScoped<IFileProcessingCoordinator, FileProcessingCoordinator>();

// Background services
builder.Services.AddHostedService<DataIngestionBackgroundService>();
```

#### InfrastructureServiceExtensions.cs

**MongoDB Configuration**:
```csharp
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    // Bind settings from appsettings.json
    var mongoDbSettings = new MongoDbSettings();
    configuration.GetSection("MongoDbSettings").Bind(mongoDbSettings);
    services.AddSingleton(Options.Create(mongoDbSettings));

    var jwtSettings = new JwtSettings();
    configuration.GetSection("JwtSettings").Bind(jwtSettings);
    services.AddSingleton(Options.Create(jwtSettings));

    // MongoDB client
    services.AddScoped<IMongoClient>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
        return new MongoClient(settings.ConnectionString);
    });

    return services;
}
```

**Repository Registration**:
```csharp
public static void AddRepositoryServiceCollection(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    // Register repositories for each entity type
    services.AddScoped<IMongoRepository<RuleConfiguration>>(sp => {
        var client = sp.GetRequiredService<IMongoClient>();
        var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
        return new MongoRepository<RuleConfiguration>(
            client, 
            settings.DatabaseName, 
            settings.CollectionName_RulesConfiguration);
    });

    services.AddScoped<IMongoRepository<User>>(sp => { ... });
    services.AddScoped<IMongoRepository<Role>>(sp => { ... });
    services.AddScoped<IMongoRepository<Permission>>(sp => { ... });
    services.AddScoped<IMongoRepository<BsonDocument>>(sp => { ... });
    services.AddScoped<IMongoRepository<MappingItem>>(sp => { ... });
    services.AddScoped<IMongoRepository<Workspace>>(sp => { ... });
    // ... more repositories
}
```

### Service Lifetimes

| Lifetime | Usage | Examples |
|----------|-------|----------|
| **Singleton** | Shared across application lifetime | MongoDbSettings, JwtSettings, MemoryCache |
| **Scoped** | One instance per HTTP request | All repositories, most services |
| **Transient** | New instance every time | Not heavily used in this architecture |
| **Hosted** | Background services | DataIngestionBackgroundService |

### Authentication Configuration

```csharp
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(settings.SecretKey))
        };
    });
```

---

## Data Processing Pipeline

### XML Processing Pipeline

```
┌─────────────────┐
│  XML File       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  File Read      │ (File.ReadAllTextAsync)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  XML Parse      │ (XElement.Parse)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Enrichment     │ (XmlEnrichmentService)
│  - Apply Rules  │
│  - Add Fields   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Flatten        │ (XmlToBsonConverterHelper)
│  - XML → BSON   │
│  - Nested paths │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Add Metadata   │
│  - _sourceFile  │
│  - _processedAt │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Batch Insert   │ (MongoRepository)
└─────────────────┘
```

### CSV Processing Pipeline

```
┌─────────────────┐
│  CSV File       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Read All Lines │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Parse Headers  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Parse Rows     │
│  - Type detect  │
│  - Map to BSON  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Batch Insert   │
└─────────────────┘
```

### JSON Processing Pipeline

```
┌─────────────────┐
│  JSON File      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  JSON Parse     │ (JObject.Parse)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Convert BSON   │ (BsonDocument.Parse)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Batch Insert   │
└─────────────────┘
```

### File Processing Coordination

```
┌───────────────────────────────────┐
│  FileProcessingCoordinator        │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│  Get Configuration                │
│  - Selected sources               │
│  - File type                      │
│  - Paths/credentials              │
└───────────┬───────────────────────┘
            │
            ▼
      ┌─────┴─────┐
      │           │
      ▼           ▼
┌─────────┐  ┌─────────┐
│Filesystem│  │  SFTP   │
└────┬────┘  └────┬────┘
     │            │
     │            ▼
     │       ┌─────────────────┐
     │       │Download Files   │
     │       │(SftpService)    │
     │       └────┬────────────┘
     │            │
     └────┬───────┘
          │
          ▼
┌─────────────────────────────────┐
│  Select Processor               │
│  - XML → XmlProcessingService   │
│  - CSV → CsvProcessingService   │
│  - JSON → JsonProcessingService │
└───────────┬─────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│  Process Files (Parallel)         │
│  - MaxDegreeOfParallelism         │
│  - Batch processing               │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│  Batch Insert to MongoDB          │
│  - Unordered bulk insert          │
│  - Bypass validation              │
└───────────┬───────────────────────┘
            │
            ▼
┌───────────────────────────────────┐
│  Post-Processing                  │
│  - Apply mappings                 │
│  - Create indexes                 │
│  - Finalize types                 │
└───────────────────────────────────┘
```

### Mapping Processing Pipeline

```
┌─────────────────────────────┐
│  Scan ReportData Collection │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Detect All Fields          │
│  - Traverse documents       │
│  - Build field list         │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Create/Update Mappings     │
│  - Set data types           │
│  - Calculate lengths        │
│  - Set visibility           │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Apply Mappings to Data     │
│  - Add alias fields         │
│  - Transform values         │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Generate Type Definitions  │
│  - TypeScript interfaces    │
│  - Documentation            │
└─────────────────────────────┘
```

---

## Background Services

### DataIngestionBackgroundService

**Type**: IHostedService  
**Lifetime**: Hosted (runs continuously)

**Purpose**: Periodically check and execute scheduled data ingestion tasks

**Configuration**:
- Check interval: 1 minute
- Initial delay: 10 seconds (allows app initialization)

**Processing Logic**:
```csharp
1. Wake up every 1 minute
2. Check if scheduled ingestion should run now
3. If yes:
   a. Create scoped service provider
   b. Get configuration from DataIngestionService
   c. Call FileProcessingCoordinator.RunIngestionAsync()
   d. Log results (files processed, records inserted)
   e. Update last run timestamp
4. If no:
   - Continue waiting
5. Repeat until application shutdown
```

**Schedule Types Supported**:
- **One-time**: Run once, then disable
- **Daily**: Run at specific time each day
- **Weekly**: Run on specific days of week
- **Hourly**: Run every N hours

**Error Handling**:
- Catches all exceptions
- Logs errors
- Continues running (resilient)
- Does not crash application

**Run Prevention**:
- Checks last run timestamp
- Prevents double-runs within 50 seconds
- Timezone-aware scheduling

**Example Configuration**:
```json
{
  "scheduleType": "daily",
  "dailyTime": "02:00",
  "enabled": true,
  "selectedSources": ["sftp"],
  "selectedFileType": "XML"
}
```

---

## Security Implementation

### Authentication

#### JWT (JSON Web Tokens)
**Configuration**:
```csharp
ValidateIssuer = true          // Verify token issuer
ValidateAudience = true        // Verify token audience
ValidateLifetime = true        // Check expiration
ValidateIssuerSigningKey = true // Verify signature
IssuerSigningKey = SymmetricSecurityKey(SecretKey)
```

**Token Structure**:
```json
{
  "sub": "userId",
  "username": "john.doe",
  "roles": ["Admin", "User"],
  "exp": 1234567890,
  "iss": "LossPreventionAPI",
  "aud": "LossPreventionClients"
}
```

**Token Generation**: Handled by FastEndpoints.Security

**Token Expiry**: Configurable (default 1 hour)

### Password Security

#### PBKDF2 Hashing
**Implementation**: PasswordHasher class

**Algorithm**: PBKDF2 (Password-Based Key Derivation Function 2)

**Configuration**:
- Iterations: 10,000+
- Key length: 32 bytes
- Salt: Random 16 bytes per user
- Hash function: SHA-256

**Storage**:
```csharp
User {
  PasswordHash: Base64(PBKDF2(password, salt))
  PasswordSalt: Base64(salt)
}
```

**Verification**:
```csharp
bool VerifyPassword(string password, string hash, string salt)
{
    var computedHash = PBKDF2(password, Base64Decode(salt));
    return computedHash == Base64Decode(hash);
}
```

### Authorization

#### Role-Based Access Control (RBAC)

**Entities**:
```
User → [Roles] → [Permissions]
```

**User Entity**:
```csharp
public class User {
    public ObjectId _id { get; set; }
    public string Username { get; set; }
    public List<ObjectId> Roles { get; set; }
    public string LockField { get; set; }  // Field-level security
    public string LockValue { get; set; }  // User can only see specific values
}
```

**Role Entity**:
```csharp
public class Role {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public List<ObjectId> Permissions { get; set; }
}
```

**Permission Entity**:
```csharp
public class Permission {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string Resource { get; set; }
    public string Action { get; set; }
}
```

#### Field-Level Security
**User-specific data filtering**:
```csharp
User {
  LockField: "StoreNumber"
  LockValue: "12345"
}
// User can only see transactions where StoreNumber = "12345"
```

### CORS Configuration

```csharp
options.AddPolicy("AllowVueDev", policy =>
{
    policy
        .WithOrigins("http://localhost:5173", "http://localhost:5174")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});
```

### Password Reset Security

**Token Generation**:
- Cryptographically random token
- Stored with expiration (1 hour)
- One-time use
- Invalidated after successful reset

**Reset Flow**:
```
1. User requests reset
2. System generates secure token
3. Token stored in PasswordResetTokens collection
4. Email sent with link containing token
5. User clicks link, validates token
6. If valid and not expired, allow password change
7. Token deleted after use
```

---

## Error Handling

### Exception Strategy

#### Custom Exceptions

| Exception | Purpose | Layer |
|-----------|---------|-------|
| InvalidCustomerException | Invalid customer data | Domain |
| DeleteUserRoleException | Cannot delete user role | Domain |
| InvalidOperationException | Business rule violation | Application |
| KeyNotFoundException | Entity not found | Application |
| DirectoryNotFoundException | File path not found | Application |

#### Exception Handling Pattern

**Service Layer**:
```csharp
public async Task<User> CreateUserAsync(User user, string password)
{
    // Validate
    var existingUser = await _userRepository.FindOneAsync(u => u.Username == user.Username);
    if (existingUser != null)
    {
        throw new InvalidOperationException($"Username '{user.Username}' is already taken.");
    }

    // Process
    // ...
}
```

**Endpoint Layer** (FastEndpoints):
```csharp
public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
{
    try
    {
        var user = await _userService.CreateUserAsync(req.User, req.Password);
        await SendOkAsync(user, ct);
    }
    catch (InvalidOperationException ex)
    {
        await SendAsync(new ErrorResponse { Message = ex.Message }, 400, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating user");
        await SendAsync(new ErrorResponse { Message = "An error occurred" }, 500, ct);
    }
}
```

### Validation Strategy

#### FluentValidation

**Example Validator**:
```csharp
public class CreateDashboardValidator : AbstractValidator<CreateDashboardRequest>
{
    public CreateDashboardValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Dashboard name is required")
            .MaximumLength(100).WithMessage("Name must be 100 characters or less");

        RuleFor(x => x.Layout)
            .NotEmpty().WithMessage("Layout is required");
    }
}
```

**Automatic Validation**: FastEndpoints automatically validates requests

### Error Response Format

```csharp
public class ErrorResponse
{
    public string Message { get; set; }
    public List<string> Errors { get; set; }
    public string TraceId { get; set; }
}
```

### Resilience Patterns

#### Repository Level
- Bulk insert with `IsOrdered = false` (continues on error)
- Bypass document validation for performance
- Transaction support (MongoDB 4.0+)

#### Background Service Level
- Try-catch around scheduled tasks
- Log errors but continue running
- Graceful degradation

#### File Processing Level
- Parallel processing with cancellation support
- Error collection per file
- Partial success handling

---

## Logging and Monitoring

### Logging Infrastructure

**Provider**: Microsoft.Extensions.Logging (built-in)

**Sinks**: Configured in appsettings.json

**Log Levels**:
```
Trace    → Detailed diagnostic information
Debug    → Debugging information
Information → General informational messages
Warning  → Warning messages
Error    → Error messages
Critical → Critical failures
```

### Logging Locations

#### API Layer (Program.cs)
```csharp
_logger.LogInformation("Starting Loss Prevention API");
_logger.LogError(ex, "Application startup failed");
```

#### Background Services
```csharp
_logger.LogInformation("Data Ingestion Background Service is starting");
_logger.LogInformation("Scheduled ingestion triggered");
_logger.LogInformation(
    "Scheduled ingestion completed: {Files} files, {Records} records",
    result.FilesProcessed, result.RecordsInserted);
_logger.LogWarning(
    "Scheduled ingestion completed with errors: {Errors}",
    string.Join(", ", result.Errors));
_logger.LogError(ex, "Scheduled ingestion failed");
```

#### File Processing
```csharp
_logger.LogInformation("Starting data ingestion...");
_logger.LogInformation("Processing {Count} files from filesystem", files.Length);
_logger.LogError(ex, "Error processing file {File}", filename);
```

### Performance Monitoring

#### Timing Metrics
```csharp
var sw = Stopwatch.StartNew();
// ... processing
sw.Stop();
Console.WriteLine($"Completed in {sw.Elapsed.TotalSeconds} Seconds");
```

#### Processing Metrics
```csharp
public class FileProcessingResult
{
    public int FilesProcessed { get; set; }
    public int RecordsInserted { get; set; }
    public List<string> Errors { get; set; }
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
}
```

### Database Initialization Logging

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
    await dbInit.InitializeAsync();  // Logs initialization steps
}
```

### Monitoring Recommendations

1. **Application Insights**: Add for production monitoring
2. **Serilog**: Consider for structured logging
3. **Health Checks**: Add endpoint for service health
4. **Metrics**: Track API response times, error rates
5. **Alerts**: Configure for critical errors

---

## Testing Approach

### Current State
**Note**: No test projects currently exist in the solution

### Recommended Testing Strategy

#### Unit Testing

**Framework**: xUnit or NUnit  
**Mocking**: Moq or NSubstitute

**Test Projects**:
```
LossPrevention.Domain.Tests
LossPrevention.Application.Tests
LossPrevention.Infrastructure.Tests
LossPrevention.API.Tests
```

**Example Test Structure**:
```csharp
public class UserServiceTests
{
    private readonly Mock<IMongoRepository<User>> _userRepositoryMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IMongoRepository<User>>();
        _sut = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new User { Username = "john.doe" };
        _userRepositoryMock
            .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateUserAsync(new User { Username = "john.doe" }, "password"));
    }
}
```

#### Integration Testing

**Framework**: xUnit with WebApplicationFactory  
**Database**: MongoDB in Docker for test isolation

**Example**:
```csharp
public class UserEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_ReturnsCreated()
    {
        // Arrange
        var request = new CreateUserRequest { ... };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

#### Repository Testing

**Approach**: Test against real MongoDB instance

```csharp
public class MongoRepositoryTests : IDisposable
{
    private readonly IMongoClient _client;
    private readonly IMongoRepository<User> _repository;

    public MongoRepositoryTests()
    {
        _client = new MongoClient("mongodb://localhost:27017");
        _repository = new MongoRepository<User>(_client, "TestDb", "Users");
    }

    [Fact]
    public async Task InsertOneAsync_SavesDocument()
    {
        // Arrange
        var user = new User { Username = "test" };

        // Act
        await _repository.InsertOneAsync(user);

        // Assert
        var saved = await _repository.GetByIdAsync(user._id);
        Assert.NotNull(saved);
        Assert.Equal("test", saved.Username);
    }

    public void Dispose()
    {
        _client.DropDatabase("TestDb");
    }
}
```

#### End-to-End Testing

**Framework**: Playwright or Selenium  
**Scope**: Full workflow testing including frontend

**Example Scenarios**:
- User registration → login → create workspace
- File ingestion → mapping → query data
- Rule configuration → apply rules → verify results

#### Performance Testing

**Tool**: JMeter or k6

**Test Scenarios**:
- File processing throughput
- Concurrent API requests
- Database query performance
- Background service reliability

### Testing Best Practices

1. **Arrange-Act-Assert** pattern
2. **Test isolation** (no shared state)
3. **Test data builders** for complex objects
4. **Integration tests** for repositories
5. **Unit tests** for business logic
6. **Mock external dependencies** (SMTP, SFTP)
7. **Test edge cases** and error conditions
8. **Performance benchmarks** for critical paths

---

## Additional Architectural Considerations

### Scalability

**Current Architecture**:
- Stateless API design
- MongoDB horizontal scaling support
- Parallel file processing
- Background service resilience

**Future Enhancements**:
- Redis caching for frequently accessed data
- Message queue (RabbitMQ/Azure Service Bus) for background jobs
- API rate limiting
- Load balancing support

### Performance Optimizations

**Implemented**:
- Bulk insert operations (unordered, bypass validation)
- Parallel file processing (`Parallel.ForEachAsync`)
- BSON document batching
- Index creation for common queries

**Recommended**:
- Response caching
- Connection pooling monitoring
- Query result pagination
- Lazy loading for large datasets

### Deployment Considerations

**Docker Support**:
- Create Dockerfile for API
- Docker Compose for full stack (API + MongoDB + smtp4dev)
- Environment-based configuration

**CI/CD**:
- Build pipeline (restore, build, test)
- Docker image creation
- Automated deployment
- Database migration scripts

### Monitoring and Observability

**Recommendations**:
- Application Performance Monitoring (APM)
- Centralized logging (ELK stack or Azure Application Insights)
- Health check endpoints
- Metrics dashboard (Grafana)
- Distributed tracing (OpenTelemetry)

---

## Conclusion

The Loss Prevention Tool backend is built on a solid foundation using Clean Architecture principles, modern .NET 8 features, and industry-standard patterns. The architecture provides:

✅ **Separation of Concerns**: Clear layer boundaries  
✅ **Testability**: Dependency injection throughout  
✅ **Maintainability**: Organized structure and naming  
✅ **Scalability**: Parallel processing and bulk operations  
✅ **Security**: JWT authentication, RBAC, password hashing  
✅ **Flexibility**: Strategy pattern for file processing  
✅ **Resilience**: Error handling and background service recovery  

The system is well-positioned for future enhancements and can be extended with additional features while maintaining architectural integrity.

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Maintainer**: Development Team
