# Loss Prevention Tool - System Architecture

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [System Components](#system-components)
3. [Technology Stack](#technology-stack)
4. [Data Flow](#data-flow)
5. [Security Architecture](#security-architecture)
6. [Scalability & Performance](#scalability--performance)
7. [Integration Points](#integration-points)

## Architecture Overview

The Loss Prevention Tool follows a modern **microservices-inspired architecture** with clear separation between presentation, application, domain, and infrastructure layers. The system is designed for:

- **Scalability**: Horizontal scaling of API and UI components
- **Maintainability**: Clear separation of concerns
- **Extensibility**: Plugin-based data processing
- **Performance**: Asynchronous processing and caching

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      Client Layer                           │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Vue.js 3 SPA + Vuetify UI                    │  │
│  │  (Desktop, Tablet, Mobile Responsive)                │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTPS/REST API
                         │ JWT Authentication
┌────────────────────────▼────────────────────────────────────┐
│                      API Layer                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      LossPrevention.API (FastEndpoints)              │  │
│  │  - Authentication & Authorization                    │  │
│  │  - Request Validation                                │  │
│  │  - Swagger Documentation                             │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                  Application Layer                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │        LossPrevention.Application                    │  │
│  │  - Business Logic Services                           │  │
│  │  - DTOs & Mappings                                   │  │
│  │  - Validation & Handlers                             │  │
│  │  - Fraud Detection Engine                            │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                    Domain Layer                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         LossPrevention.Domain                        │  │
│  │  - Domain Entities                                   │  │
│  │  - Domain Exceptions                                 │  │
│  │  - Business Rules                                    │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                Infrastructure Layer                         │
│  ┌──────────────────────────────────────────────────────┐  │
│  │      LossPrevention.Infrastructure                   │  │
│  │  - MongoDB Repository Pattern                        │  │
│  │  - Email Services                                    │  │
│  │  - File Processing                                   │  │
│  │  - External Service Integration                      │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                    Data Layer                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              MongoDB Database                        │  │
│  │  Collections:                                        │  │
│  │  - Users, Roles, Permissions                         │  │
│  │  - ReportData (Main transaction data)                │  │
│  │  - Workspaces, Dashboards                            │  │
│  │  - Rules, Mappings                                   │  │
│  │  - Notifications, Groups                             │  │
│  │  - DataIngestionConfigurations                       │  │
│  │  - FraudDetectionSettings                            │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘

                         ┌─────────────────────┐
                         │ Background Services │
                         │                     │
                         │ Data Ingestion      │
                         │ Service             │
                         │ (Scheduled/Manual)  │
                         └─────────────────────┘
```

## System Components

### 1. Frontend (LossPrevention.UI)

**Technology**: Vue.js 3 + Nuxt 3 + Vuetify 3

**Structure**:
```
src/
├── components/          # Reusable Vue components
│   ├── ai/             # Natural language query
│   ├── dashboard/      # Dashboard components
│   ├── manage/         # Admin management components
│   ├── reports/        # Report builder components
│   └── workspace/      # Workspace management
├── stores/             # Pinia state management
├── router/             # Vue Router configuration
├── api/                # API client services
├── interfaces/         # TypeScript interfaces
└── helpers/            # Utility functions
```

**Key Responsibilities**:
- User interface rendering
- State management (Pinia)
- Client-side validation
- API communication
- Local caching
- Responsive design

### 2. API Layer (LossPrevention.API)

**Technology**: .NET 8 + FastEndpoints

**Structure**:
```
Endpoints/
├── User/               # Authentication & user management
│   ├── Users/
│   ├── Roles/
│   └── Permissions/
├── Dashboard/          # Dashboard CRUD operations
├── Data/               # Data query & analysis
├── DataIngestion/      # Data import configuration
├── FraudDetection/     # Fraud settings & detection
├── Groups/             # User group management
├── Mappings/           # Field mapping configuration
├── Notifications/      # Notification system
├── Rules/              # Business rule engine
└── Workspaces/         # Workspace management
```

**Key Responsibilities**:
- Request routing
- Authentication (JWT)
- Authorization (permission-based)
- Input validation
- API documentation (Swagger)
- CORS configuration
- Error handling

**Configuration**:
- `appsettings.json` - Application settings
- `Program.cs` - Service registration & middleware

### 3. Application Layer (LossPrevention.Application)

**Technology**: .NET 8 Class Library

**Structure**:
```
├── Services/           # Business logic services
│   ├── Data/          # Data processing
│   ├── DataIngestion/ # Data import
│   ├── Users/         # User management
│   └── Workspaces/    # Workspace services
├── DTO/               # Data Transfer Objects
├── Handlers/          # Request/Response handlers
├── Helpers/           # Utility classes
│   ├── BsonHelper
│   ├── DistanceHelper
│   ├── JsonHelper
│   ├── MappingHelper
│   └── RuleHelper
├── Interfaces/        # Service interfaces
├── Mappings/          # Object mapping
└── Validators/        # Business rule validation
```

**Key Services**:
- `UserService` - User authentication & management
- `WorkspaceService` - Workspace operations
- `ReportDataService` - Data querying & analysis
- `DistanceDataService` - Euclidean distance analysis
- `RuleConfigurationService` - Rule engine
- `MappingService` - Field mapping
- `DataIngestionService` - Data import
- `XmlProcessingService` - XML data processing
- `EmailService` - Email notifications
- `PasswordResetService` - Password management

### 4. Domain Layer (LossPrevention.Domain)

**Technology**: .NET 8 Class Library

**Structure**:
```
Entities/
├── Users/
│   ├── User
│   ├── Role
│   ├── Permission
│   └── UserWithRolesAndPermissions
├── Workspaces/
│   ├── Workspace
│   ├── Tab
│   ├── Field
│   └── Query
├── Dashboards/
│   ├── DashboardDocument
│   └── DashboardBlock
├── Data/
│   └── MappingItem
├── Rules/
│   └── RuleConfiguration
├── Notifications/
│   └── NotificationDocument
├── Groups/
│   └── GroupDocument
├── DataIngestion/
│   ├── DataIngestionConfiguration
│   └── DataIngestionSchedule
└── FraudDetection/
    └── FraudDetectionSettings
```

**Key Entities**:
- **User**: Authentication and authorization
- **Workspace**: Report definitions with tabs
- **Dashboard**: Visual analytics layouts
- **MappingItem**: Field definitions and transformations
- **RuleConfiguration**: Business rule definitions
- **NotificationDocument**: Alert and message data

### 5. Infrastructure Layer (LossPrevention.Infrastructure)

**Technology**: .NET 8 Class Library

**Structure**:
```
├── Repositories/
│   ├── MongoRepository<T>     # Generic repository
│   ├── IMongoRepository<T>    # Repository interface
│   └── DapperRepository       # SQL fallback (unused)
├── Configuration/
│   ├── MongoDbSettings
│   └── JwtSettings
├── Services/
│   ├── EmailService
│   ├── SftpFileProcessingService
│   ├── FileProcessingCoordinator
│   ├── XmlProcessingService
│   ├── CsvProcessingService
│   └── JsonProcessingService
└── InfrastructureServiceExtensions.cs
```

**Key Features**:
- **MongoRepository**: Generic repository pattern for all entities
- **File Processing**: Multi-format data ingestion (XML, CSV, JSON)
- **SFTP Support**: Automated file retrieval
- **Email Service**: SMTP integration for notifications

### 6. Data Ingestion Service (LossPrevention.DataIngestionService)

**Technology**: .NET 8 Console Application / Background Service

**Purpose**: Automated data processing from external sources

**Features**:
- Parallel file processing
- Configurable batch sizes
- Backpressure handling (channels)
- Source file tracking
- Automatic rule application
- TTL-based data retention

**Processing Flow**:
```
1. Monitor SFTP/File System
2. Download/Read files (XML, CSV, JSON)
3. Parse and validate data
4. Apply field mappings
5. Execute business rules
6. Enrich with metadata (_sourceFile, _processedAt)
7. Insert to MongoDB (batched)
8. Move processed files
9. Log processing results
```

### 7. Database (MongoDB)

**Collections**:

| Collection | Purpose | Key Fields |
|------------|---------|------------|
| Users | User accounts | Username, PasswordHash, Email, Roles[] |
| Roles | Role definitions | RoleName, Permissions[] |
| Permissions | Permission catalog | PermissionName, Description |
| ReportData | Transaction data | Dynamic (based on mappings) |
| Workspaces | Report definitions | Name, Tabs[], Queries |
| Dashboards | Dashboard layouts | WorkspaceId, Blocks[] |
| Mappings | Field definitions | Name, Alias, DataType |
| Rules | Business rules | Name, Conditions, Actions |
| Notifications | Alerts & messages | Type, From, To[], Message |
| Groups | User groups | Name, Members[] |
| DataIngestionConfigurations | Import settings | Sources, Schedules |
| FraudDetectionSettings | Fraud thresholds | Thresholds{} |

**Indexes**:
- User: Username (unique), Email (unique)
- ReportData: _processedAt (TTL index), dynamic fields
- Dashboards: WorkspaceId
- Notifications: To[], IsRead

## Technology Stack

### Backend Stack
- **.NET 8**: Latest LTS version
- **C# 12**: Modern language features
- **FastEndpoints**: Minimalist API framework (alternative to MVC)
- **MongoDB Driver**: Official MongoDB C# driver
- **JWT Bearer**: Authentication middleware
- **Newtonsoft.Json**: JSON serialization

### Frontend Stack
- **Vue.js 3.5**: Composition API
- **Nuxt 3**: SSR framework
- **Vuetify 3.7**: Material Design components
- **Pinia 2.2**: State management
- **Vue Router 4.6**: Navigation
- **AG Grid Community 35**: Data grid
- **Chart.js 4.5**: Charting
- **Axios 1.9**: HTTP client
- **TypeScript 5**: Type safety

### Database
- **MongoDB 7.x**: NoSQL database
- **BSON**: Binary JSON format
- **Aggregation Pipeline**: Complex queries

### DevOps & Tools
- **Vite 6.3**: Frontend build tool
- **Visual Studio 2022**: IDE
- **VS Code**: Frontend development
- **Git**: Version control
- **Swagger/OpenAPI**: API documentation

## Data Flow

### 1. User Authentication Flow
```
┌──────┐      ┌─────┐      ┌──────────┐      ┌──────────┐
│ User │─────▶│ UI  │─────▶│   API    │─────▶│   DB     │
└──────┘      └─────┘      └──────────┘      └──────────┘
              Credentials    Validate          Check User
                             Password          Get Roles
                                              Get Permissions
              ◀────────────────────────────────
              JWT Token (includes permissions)
```

### 2. Query Execution Flow
```
1. User builds query in UI (Workspace component)
2. UI sends MongoDB aggregation pipeline to API
3. API validates permissions
4. ReportDataService executes query against ReportData collection
5. Results returned with pagination
6. UI renders in AG Grid
7. Optional: Apply fraud detection analysis
```

### 3. Data Ingestion Flow
```
1. Background service monitors SFTP/file system
2. Download files (XML, CSV, JSON)
3. Parse files using appropriate processor
4. Apply field mappings (MappingService)
5. Execute business rules (RuleConfigurationService)
6. Add metadata (_sourceFile, _processedAt)
7. Batch insert to MongoDB (1000 records/batch)
8. Move processed files to archive
9. Log success/errors
```

### 4. Dashboard Rendering Flow
```
1. Load dashboard definition (blocks, queries)
2. For each block:
   a. If chart: Execute query, transform data, render Chart.js
   b. If table: Execute query, paginate, render AG Grid
   c. If text: Render markdown/HTML
3. Save layout changes to MongoDB
4. Real-time updates via polling/websockets (future)
```

## Security Architecture

### Authentication
- **JWT Tokens**: Signed with HMAC-SHA256
- **Token Expiry**: Configurable (default: 1 hour)
- **Refresh**: Re-login required after expiry
- **Password**: BCrypt hashing with salt

### Authorization
- **Permission-Based**: 41 fine-grained permissions
- **Role-Based**: Users assigned to roles
- **Endpoint Protection**: FastEndpoints `Permissions()` attribute
- **Field-Level**: Lock users to specific data (LockField/LockValue)

### Data Security
- **Encryption in Transit**: HTTPS/TLS
- **Encryption at Rest**: MongoDB encryption (optional)
- **SQL Injection**: N/A (MongoDB BSON)
- **XSS Protection**: Vue.js auto-escaping
- **CSRF Protection**: JWT stateless tokens

### Security Headers (Production)
- CORS: Restricted origins
- HSTS: Force HTTPS
- Content-Security-Policy: XSS prevention
- X-Frame-Options: Clickjacking prevention

## Scalability & Performance

### Horizontal Scaling
- **API**: Multiple instances behind load balancer
- **UI**: Served via CDN
- **Database**: MongoDB replica sets or sharding

### Caching Strategy
- **Frontend**: Pinia stores, localStorage
- **Backend**: .NET MemoryCache for mappings/rules
- **Database**: MongoDB query cache

### Performance Optimizations
- **Pagination**: All queries paginated (default: 50 records)
- **Lazy Loading**: Components loaded on-demand
- **Aggregation Pipeline**: Efficient MongoDB queries
- **Indexes**: Strategic indexing on query fields
- **TTL Indexes**: Automatic old data cleanup
- **Batch Processing**: Data ingestion in batches (1000)
- **Parallel Processing**: Multi-threaded file processing

### Monitoring
- **.NET Logging**: ILogger interface
- **MongoDB Profiling**: Slow query detection
- **Frontend**: Browser console errors
- **Health Checks**: API /health endpoint (future)

## Integration Points

### Inbound Integrations
- **SFTP**: Scheduled file retrieval
- **File System**: Monitor folders for new files
- **REST API**: External systems can POST data

### Outbound Integrations
- **SMTP**: Email notifications
- **Export**: CSV, Excel, PDF generation
- **Webhooks**: (Planned) Event notifications

### Third-Party Services
- **AI/LLM**: Natural language query processing (OpenAI-compatible)
- **Email Service**: SMTP-compatible providers
- **Cloud Storage**: (Planned) AWS S3, Azure Blob

## Deployment Architecture

### Development
```
Local Machine
├── IIS Express / Kestrel (API)
├── Vite Dev Server (UI)
└── MongoDB (Docker or local)
```

### Production (Azure)
```
Azure Resources
├── Azure Functions (API)
├── Azure Static Web Apps (UI)
├── Azure Cosmos DB (MongoDB API)
├── Azure Load Balancer
├── Azure Application Insights (Monitoring)
└── Azure Key Vault (Secrets)
```

### Production (On-Premise)
```
Infrastructure
├── IIS (API + UI)
├── MongoDB Cluster (3 nodes)
├── Windows/Linux Servers
└── Nginx Load Balancer
```

## Disaster Recovery

### Backup Strategy
- **Database**: Daily automated backups
- **Configuration**: Source control (Git)
- **User Data**: Included in DB backups

### Recovery Time Objective (RTO)
- **Database Restore**: < 1 hour
- **Application Redeploy**: < 30 minutes

### Recovery Point Objective (RPO)
- **Data Loss**: < 24 hours (daily backups)
- **Hot Standby**: < 1 minute (if replica sets used)

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Maintained By**: Development Team
