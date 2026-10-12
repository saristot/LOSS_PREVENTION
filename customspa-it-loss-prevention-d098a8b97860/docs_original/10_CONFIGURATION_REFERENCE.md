# Configuration Reference

## Table of Contents

1. [Overview](#overview)
2. [API Configuration (appsettings.json)](#api-configuration-appsettingsjson)
3. [Data Ingestion Service Configuration](#data-ingestion-service-configuration)
4. [Environment-Specific Configuration](#environment-specific-configuration)
5. [MongoDB Configuration](#mongodb-configuration)
6. [JWT Authentication Settings](#jwt-authentication-settings)
7. [Email/SMTP Configuration](#emailsmtp-configuration)
8. [Data Retention Configuration](#data-retention-configuration)
9. [Frontend Configuration](#frontend-configuration)
10. [Logging Configuration](#logging-configuration)
11. [Launch Settings](#launch-settings)
12. [Performance Tuning](#performance-tuning)
13. [Configuration Examples](#configuration-examples)
14. [Configuration Validation](#configuration-validation)
15. [Environment Variables](#environment-variables)

---

## Overview

The Loss Prevention Tool uses a layered configuration approach with JSON-based configuration files for both backend services (API and Data Ingestion Service) and frontend build configurations. This document provides a comprehensive reference for all configuration options available in the system.

### Configuration File Locations

- **API Configuration**: `LossPrevention.API/appsettings.json`
- **Data Ingestion Service**: `LossPrevention.DataIngestionService/appsettings.json`
- **Frontend Build Config**: `LossPrevention.UI/vite.config.js`
- **Frontend Framework Config**: `LossPrevention.UI/nuxt.config.ts`
- **Launch Settings**: `LossPrevention.API/Properties/launchSettings.json`

---

## API Configuration (appsettings.json)

### Complete Configuration Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DataRetention": {
    "TransactionRetentionDays": 180
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "LossPrevention",
    "CollectionName_MappingConfiguration": "Mappings",
    "CollectionName_RulesConfiguration": "Rules",
    "CollectionName_Users": "Users",
    "CollectionName_Roles": "Roles",
    "CollectionName_Permissions": "Permissions",
    "CollectionName_ReportData": "ReportData",
    "CollectionName_Workspaces": "Workspaces",
    "CollectionName_Dashboards": "Dashboards",
    "CollectionName_DataIngestionConfigurations": "DataIngestionConfigurations",
    "CollectionName_DataIngestionSchedules": "DataIngestionSchedules",
    "CollectionName_Notifications": "Notifications",
    "CollectionName_Groups": "Groups",
    "CollectionName_FraudDetectionSettings": "FraudDetectionSettings"
  },
  "JwtSettings": {
    "SecretKey": "e96eae77-3a2c-4e96-a660-fbfbb8edcd62",
    "Issuer": "LossPrevention",
    "Audience": "User",
    "ExpiryHours": 1
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "25",
    "FromEmail": "noreply@lossprevention.local",
    "FromName": "Loss Prevention System",
    "FrontendUrl": "http://localhost:5173"
  }
}
```

### Configuration Sections Overview

| Section | Purpose | Required |
|---------|---------|----------|
| `Logging` | Controls application logging levels and outputs | Yes |
| `DataRetention` | Defines data retention policies | Yes |
| `AllowedHosts` | Specifies allowed host headers | Yes |
| `MongoDbSettings` | MongoDB connection and collection settings | Yes |
| `JwtSettings` | JWT authentication configuration | Yes |
| `Email` | SMTP email service configuration | Yes |

---

## Data Ingestion Service Configuration

### Complete Configuration Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "LossPrevention",
    "CollectionName_MappingConfiguration": "Mappings",
    "CollectionName_RulesConfiguration": "Rules",
    "CollectionName_Users": "Users",
    "CollectionName_Roles": "Roles",
    "CollectionName_Permissions": "Permissions",
    "CollectionName_ReportData": "ReportData"
  }
}
```

### Key Differences from API Configuration

The Data Ingestion Service uses a simplified configuration focusing on:
- Basic logging
- MongoDB connectivity
- Core collections needed for data processing

Collections specific to user interface features (Workspaces, Dashboards, Notifications) are not required in this service.

---

## Environment-Specific Configuration

### Development Environment

**Configuration File**: `appsettings.Development.json` (create if needed)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "MongoDB.Driver": "Debug"
    }
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "LossPrevention_Dev"
  },
  "JwtSettings": {
    "SecretKey": "dev-secret-key-change-in-production",
    "ExpiryHours": 8
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "25",
    "FrontendUrl": "http://localhost:5173"
  }
}
```

**Key Development Settings**:
- More verbose logging (Debug level)
- Longer JWT expiry for development convenience
- Local SMTP server or mail trap
- Separate development database

### Production Environment

**Configuration File**: `appsettings.Production.json` (create if needed)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://prod-server:27017/?authSource=admin",
    "DatabaseName": "LossPrevention_Prod"
  },
  "JwtSettings": {
    "SecretKey": "USE-STRONG-SECRET-FROM-ENVIRONMENT-VARIABLE",
    "ExpiryHours": 1
  },
  "Email": {
    "SmtpHost": "smtp.company.com",
    "SmtpPort": "587",
    "FromEmail": "noreply@company.com",
    "FromName": "Loss Prevention System",
    "FrontendUrl": "https://lossprevention.company.com"
  },
  "DataRetention": {
    "TransactionRetentionDays": 365
  }
}
```

**Key Production Settings**:
- Minimal logging (Warning/Error levels only)
- Strong authentication credentials
- Short JWT expiry for security
- Production SMTP server with TLS
- Longer data retention periods
- Production URLs

### Staging Environment

**Configuration File**: `appsettings.Staging.json` (create if needed)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://staging-server:27017",
    "DatabaseName": "LossPrevention_Staging"
  },
  "JwtSettings": {
    "ExpiryHours": 2
  },
  "Email": {
    "SmtpHost": "smtp-staging.company.com",
    "SmtpPort": "587",
    "FrontendUrl": "https://lossprevention-staging.company.com"
  }
}
```

---

## MongoDB Configuration

### MongoDbSettings Section

All MongoDB configuration is contained within the `MongoDbSettings` section.

#### ConnectionString

**Type**: `string`  
**Required**: Yes  
**Default**: `mongodb://localhost:27017`

Defines the MongoDB connection string.

**Format Options**:

```
# Local development (no authentication)
mongodb://localhost:27017

# Production with authentication
mongodb://username:password@server:27017/?authSource=admin

# Replica set
mongodb://server1:27017,server2:27017,server3:27017/?replicaSet=rs0

# Atlas cloud
mongodb+srv://username:password@cluster.mongodb.net/?retryWrites=true&w=majority

# With additional options
mongodb://server:27017/?maxPoolSize=50&connectTimeoutMS=10000
```

**Common Connection String Options**:

| Option | Description | Example |
|--------|-------------|---------|
| `authSource` | Authentication database | `?authSource=admin` |
| `replicaSet` | Replica set name | `?replicaSet=rs0` |
| `ssl` | Use SSL/TLS | `?ssl=true` |
| `maxPoolSize` | Connection pool size | `?maxPoolSize=100` |
| `connectTimeoutMS` | Connection timeout | `?connectTimeoutMS=10000` |
| `retryWrites` | Retry write operations | `?retryWrites=true` |
| `w` | Write concern | `?w=majority` |

#### DatabaseName

**Type**: `string`  
**Required**: Yes  
**Default**: `LossPrevention`

The name of the MongoDB database to use.

**Recommendations**:
- Development: `LossPrevention_Dev`
- Staging: `LossPrevention_Staging`
- Production: `LossPrevention_Prod`

#### Collection Names

All collection names are configurable to support different naming conventions or multi-tenant scenarios.

| Property | Default Value | Purpose |
|----------|---------------|---------|
| `CollectionName_MappingConfiguration` | `Mappings` | Data field mapping configurations |
| `CollectionName_RulesConfiguration` | `Rules` | Business rule definitions |
| `CollectionName_Users` | `Users` | User accounts and authentication |
| `CollectionName_Roles` | `Roles` | User roles and permissions |
| `CollectionName_Permissions` | `Permissions` | Granular permission definitions |
| `CollectionName_ReportData` | `ReportData` | Processed transaction and report data |
| `CollectionName_Workspaces` | `Workspaces` | User workspace configurations |
| `CollectionName_Dashboards` | `Dashboards` | Dashboard layouts and widgets |
| `CollectionName_DataIngestionConfigurations` | `DataIngestionConfigurations` | Data source configurations |
| `CollectionName_DataIngestionSchedules` | `DataIngestionSchedules` | Scheduled data import jobs |
| `CollectionName_Notifications` | `Notifications` | System notifications |
| `CollectionName_Groups` | `Groups` | User groups and organization units |
| `CollectionName_FraudDetectionSettings` | `FraudDetectionSettings` | Fraud detection parameters |

### MongoDB Best Practices

#### Indexes

Ensure appropriate indexes are created for optimal performance:

```javascript
// Users collection
db.Users.createIndex({ "Email": 1 }, { unique: true });
db.Users.createIndex({ "Username": 1 }, { unique: true });

// ReportData collection
db.ReportData.createIndex({ "WorkspaceId": 1, "TransactionDate": -1 });
db.ReportData.createIndex({ "TransactionDate": -1 });

// Rules collection
db.Rules.createIndex({ "IsActive": 1, "Priority": 1 });

// Workspaces collection
db.Workspaces.createIndex({ "UserId": 1 });
```

#### Connection Pooling

For production environments, configure connection pooling:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://server:27017/?maxPoolSize=100&minPoolSize=10&maxIdleTimeMS=300000"
  }
}
```

**Recommended Pool Sizes**:
- Small applications: `maxPoolSize=50`
- Medium applications: `maxPoolSize=100`
- Large applications: `maxPoolSize=200`

---

## JWT Authentication Settings

### JwtSettings Section

Configures JSON Web Token authentication used for API security.

#### SecretKey

**Type**: `string`  
**Required**: Yes  
**Default**: None (must be set)  
**Security**: HIGH - Store in environment variables in production

The secret key used to sign and validate JWT tokens.

**Requirements**:
- Minimum length: 32 characters
- Use cryptographically secure random string
- Never commit to source control
- Rotate periodically in production

**Generating Secure Keys**:

```powershell
# PowerShell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Minimum 0 -Maximum 255 }))

# Or use a GUID (less secure but acceptable for development)
[Guid]::NewGuid().ToString()
```

```bash
# Linux/Mac
openssl rand -base64 64
```

#### Issuer

**Type**: `string`  
**Required**: Yes  
**Default**: `LossPrevention`

The issuer claim identifies the principal that issued the JWT.

**Example Values**:
- Development: `LossPrevention-Dev`
- Production: `LossPrevention-Prod`
- Custom: `YourCompany.LossPrevention`

#### Audience

**Type**: `string`  
**Required**: Yes  
**Default**: `User`

The audience claim identifies the recipients that the JWT is intended for.

**Common Values**:
- `User` - End users
- `Admin` - Administrative users
- `API` - API consumers
- `Mobile` - Mobile applications

#### ExpiryHours

**Type**: `integer`  
**Required**: Yes  
**Default**: `1`  
**Range**: 1-168 (1 hour to 7 days)

Defines how long a JWT token remains valid after issuance.

**Recommendations**:
- Production: `1` hour (high security)
- Development: `8` hours (convenience)
- Mobile apps: `24` hours (user experience)
- Remember me: `168` hours (7 days)

**Security Considerations**:
- Shorter expiry = higher security, more re-authentication
- Longer expiry = better user experience, higher risk if compromised
- Consider implementing refresh tokens for long-lived sessions

### JWT Configuration Example

```json
{
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "LossPrevention-${ENVIRONMENT}",
    "Audience": "User",
    "ExpiryHours": 1
  }
}
```

---

## Email/SMTP Configuration

### Email Section

Configures the email service used for notifications, password resets, and system alerts.

#### SmtpHost

**Type**: `string`  
**Required**: Yes  
**Default**: `localhost`

The SMTP server hostname or IP address.

**Common SMTP Servers**:
- Gmail: `smtp.gmail.com`
- Office 365: `smtp.office365.com`
- SendGrid: `smtp.sendgrid.net`
- AWS SES: `email-smtp.us-east-1.amazonaws.com`
- Local development: `localhost` or `127.0.0.1`

#### SmtpPort

**Type**: `string`  
**Required**: Yes  
**Default**: `25`

The SMTP server port number.

**Common Ports**:
- `25` - Standard SMTP (unencrypted)
- `587` - SMTP with STARTTLS (recommended)
- `465` - SMTP with SSL/TLS (legacy)
- `2525` - Alternative SMTP port (some providers)

**Recommendations**:
- Production: Use port `587` with STARTTLS
- Development: Use port `25` or local mail server

#### FromEmail

**Type**: `string`  
**Required**: Yes  
**Default**: `noreply@lossprevention.local`

The email address used as the sender for all system emails.

**Format**: Valid email address (RFC 5322)

**Best Practices**:
- Use a no-reply address for automated emails
- Ensure the domain is configured with SPF/DKIM/DMARC
- Use a domain you control
- Consider separate addresses for different purposes:
  - `noreply@` - Automated notifications
  - `alerts@` - System alerts
  - `support@` - Support-related emails

#### FromName

**Type**: `string`  
**Required**: Yes  
**Default**: `Loss Prevention System`

The display name shown to email recipients.

**Examples**:
- `Loss Prevention System`
- `Company Name - Loss Prevention`
- `LP Alert System`

#### FrontendUrl

**Type**: `string`  
**Required**: Yes  
**Default**: `http://localhost:5173`

The base URL of the frontend application, used for generating links in emails (password reset, notifications, etc.).

**Environment-Specific Values**:
- Development: `http://localhost:5173`
- Staging: `https://lossprevention-staging.company.com`
- Production: `https://lossprevention.company.com`

**Important**: Must include the protocol (`http://` or `https://`) and should NOT have a trailing slash.

### Email Configuration Examples

#### Development (Local Mail Server)

```json
{
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "25",
    "FromEmail": "dev@localhost",
    "FromName": "LP Dev System",
    "FrontendUrl": "http://localhost:5173"
  }
}
```

#### Production (Office 365)

```json
{
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": "587",
    "FromEmail": "noreply@company.com",
    "FromName": "Loss Prevention System",
    "FrontendUrl": "https://lossprevention.company.com"
  }
}
```

#### Production (SendGrid)

```json
{
  "Email": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": "587",
    "FromEmail": "noreply@company.com",
    "FromName": "Loss Prevention System",
    "FrontendUrl": "https://lossprevention.company.com"
  }
}
```

### Email Service Notes

The current implementation uses SMTP without authentication. For production use with authenticated SMTP servers, you may need to extend the `EmailService` class to support:
- Username/password authentication
- API key authentication
- SSL/TLS encryption
- Custom headers

---

## Data Retention Configuration

### DataRetention Section

Controls how long transaction and reporting data is retained in the system.

#### TransactionRetentionDays

**Type**: `integer`  
**Required**: Yes  
**Default**: `180`  
**Range**: 1-3650 (1 day to 10 years)

Number of days to retain transaction data before automatic deletion or archival.

**Recommendations by Industry**:
- Retail: 180-365 days
- Financial Services: 2555 days (7 years)
- Healthcare: 2555 days (7 years)
- General Business: 365 days

**Legal Considerations**:
- Check industry-specific retention requirements
- Consider GDPR/privacy regulations
- Document your retention policy
- Implement secure archival for long-term storage

### Data Retention Best Practices

#### Implementing Automated Cleanup

Create a scheduled task or background service to clean up old data:

```csharp
// Pseudo-code for cleanup service
var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
var oldRecords = await _repository.FindAsync(x => x.CreatedDate < cutoffDate);
await _repository.DeleteManyAsync(oldRecords);
```

#### Archival Strategy

Instead of deletion, consider archiving:

```json
{
  "DataRetention": {
    "TransactionRetentionDays": 180,
    "ArchiveRetentionDays": 2555,
    "ArchiveConnectionString": "mongodb://archive-server:27017"
  }
}
```

---

## Frontend Configuration

### Vite Configuration (vite.config.js)

```javascript
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import path from "path";

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 5173,
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5264',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, '')
      }
    }
  },
  build: {
    outDir: 'dist',
    sourcemap: false,
    minify: 'terser',
    chunkSizeWarningLimit: 1000
  }
});
```

#### Configuration Options

| Option | Description | Default | Recommended |
|--------|-------------|---------|-------------|
| `server.port` | Development server port | `5173` | `5173` |
| `server.host` | Enable network access | `false` | `true` |
| `build.outDir` | Build output directory | `dist` | `dist` |
| `build.sourcemap` | Generate source maps | `false` | `false` (prod), `true` (dev) |
| `build.minify` | Minification method | `esbuild` | `terser` (prod) |
| `build.chunkSizeWarningLimit` | Chunk size warning (KB) | `500` | `1000` |

### Nuxt Configuration (nuxt.config.ts)

```typescript
import { defineNuxtConfig } from "nuxt";

export default defineNuxtConfig({
  ssr: true,
  buildModules: ["@pinia/nuxt", "@nuxtjs/vuetify"],
  css: ["vuetify/styles"],
  vuetify: {
    theme: {
      themes: {
        light: {
          primary: "#1976D2",
          secondary: "#424242",
          accent: "#82B1FF",
          error: "#FF5252",
          info: "#2196F3",
          success: "#4CAF50",
          warning: "#FB8C00",
        },
      },
    },
  },
  runtimeConfig: {
    public: {
      apiBase: process.env.API_BASE_URL || 'http://localhost:5264'
    }
  }
});
```

#### Configuration Options

| Option | Description | Recommended Value |
|--------|-------------|-------------------|
| `ssr` | Enable server-side rendering | `true` |
| `buildModules` | Build-time modules | `["@pinia/nuxt", "@nuxtjs/vuetify"]` |
| `runtimeConfig.public.apiBase` | API base URL | Environment-specific |

### Frontend Environment Variables

Create a `.env` file in the `LossPrevention.UI` directory:

#### Development (.env.development)

```env
# API Configuration
VITE_API_BASE_URL=http://localhost:5264
VITE_API_TIMEOUT=30000

# Feature Flags
VITE_ENABLE_FRAUD_DETECTION=true
VITE_ENABLE_DISTANCE_ANALYSIS=true
VITE_ENABLE_DEBUG_MODE=true

# Application Settings
VITE_APP_NAME=Loss Prevention Tool
VITE_APP_VERSION=1.0.0
```

#### Production (.env.production)

```env
# API Configuration
VITE_API_BASE_URL=https://api.lossprevention.company.com
VITE_API_TIMEOUT=30000

# Feature Flags
VITE_ENABLE_FRAUD_DETECTION=true
VITE_ENABLE_DISTANCE_ANALYSIS=true
VITE_ENABLE_DEBUG_MODE=false

# Application Settings
VITE_APP_NAME=Loss Prevention Tool
VITE_APP_VERSION=1.0.0
```

#### Accessing Environment Variables

In Vue components:

```javascript
// Access environment variables
const apiUrl = import.meta.env.VITE_API_BASE_URL;
const debugMode = import.meta.env.VITE_ENABLE_DEBUG_MODE === 'true';
```

---

## Logging Configuration

### Logging Section

Controls application logging behavior and verbosity.

#### LogLevel Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "MongoDB.Driver": "Warning",
      "System": "Warning"
    }
  }
}
```

#### Log Levels

| Level | Description | When to Use |
|-------|-------------|-------------|
| `Trace` | Very detailed logs | Deep debugging only |
| `Debug` | Detailed logs | Development/troubleshooting |
| `Information` | General information | Normal operations |
| `Warning` | Warning messages | Potential issues |
| `Error` | Error messages | Errors and exceptions |
| `Critical` | Critical failures | System failures |
| `None` | No logging | Disable logging |

#### Category-Specific Logging

Configure different log levels for different parts of the application:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.HttpLogging": "Information",
      "MongoDB.Driver": "Warning",
      "MongoDB.Driver.Core.Clusters": "Information",
      "LossPrevention.Application": "Debug",
      "LossPrevention.Infrastructure": "Information"
    }
  }
}
```

### Advanced Logging Configuration

#### File Logging

To add file logging, configure Serilog or NLog. Example with Serilog:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/lossprevention-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Console"
      }
    ]
  }
}
```

#### Structured Logging

Enable structured logging for better log analysis:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/lossprevention-.json",
          "formatter": "Serilog.Formatting.Json.JsonFormatter",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

---

## Launch Settings

### launchSettings.json

Located at `LossPrevention.API/Properties/launchSettings.json`, this file configures how the application launches in different environments.

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:55993",
      "sslPort": 44348
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5264",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7110;http://localhost:5264",
      "launchUrl": "/swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Launch Profile Options

| Profile | Use Case | URL |
|---------|----------|-----|
| `http` | HTTP-only development | http://localhost:5264 |
| `https` | HTTPS development with Swagger | https://localhost:7110 |
| `IIS Express` | IIS development | Configured in iisSettings |

### Custom Launch Profiles

Add custom profiles for specific scenarios:

```json
{
  "profiles": {
    "Production-Like": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5264",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    },
    "Docker": {
      "commandName": "Docker",
      "launchBrowser": true,
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}",
      "environmentVariables": {
        "ASPNETCORE_URLS": "http://+:80"
      }
    }
  }
}
```

---

## Performance Tuning

### MongoDB Performance Settings

#### Connection String Optimization

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://server:27017/?maxPoolSize=200&minPoolSize=50&maxIdleTimeMS=300000&serverSelectionTimeoutMS=5000&connectTimeoutMS=10000"
  }
}
```

**Key Parameters**:
- `maxPoolSize`: Maximum connections (default: 100)
- `minPoolSize`: Minimum connections (default: 0)
- `maxIdleTimeMS`: Max idle time (default: 0 - never close)
- `serverSelectionTimeoutMS`: Server selection timeout (default: 30000)
- `connectTimeoutMS`: Connection timeout (default: 10000)
- `socketTimeoutMS`: Socket timeout (default: 0 - no timeout)

#### Write Concern

For high-throughput scenarios, adjust write concern:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://server:27017/?w=1&journal=false"
  }
}
```

**Write Concern Options**:
- `w=0`: No acknowledgment (fastest, least safe)
- `w=1`: Acknowledge write (default, balanced)
- `w=majority`: Majority acknowledgment (safest, slowest)
- `journal=true`: Wait for journal sync (safer)

### API Performance Settings

#### Kestrel Server Configuration

Add to `appsettings.json`:

```json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100,
      "MaxRequestBodySize": 52428800,
      "KeepAliveTimeout": "00:02:00",
      "RequestHeadersTimeout": "00:00:30"
    }
  }
}
```

#### Memory Cache Configuration

```json
{
  "MemoryCache": {
    "SizeLimit": 1024,
    "CompactionPercentage": 0.25,
    "ExpirationScanFrequency": "00:05:00"
  }
}
```

### Frontend Performance Settings

#### Vite Build Optimization

```javascript
export default defineConfig({
  build: {
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true
      }
    },
    rollupOptions: {
      output: {
        manualChunks: {
          'vendor': ['vue', 'vue-router', 'pinia'],
          'ui': ['vuetify'],
          'charts': ['chart.js'],
          'grid': ['ag-grid-community', 'ag-grid-vue3']
        }
      }
    },
    chunkSizeWarningLimit: 1000
  }
});
```

---

## Configuration Examples

### Scenario 1: Development Environment Setup

**API appsettings.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "DataRetention": {
    "TransactionRetentionDays": 30
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "LossPrevention_Dev"
  },
  "JwtSettings": {
    "SecretKey": "dev-secret-key-do-not-use-in-production-12345",
    "Issuer": "LossPrevention-Dev",
    "Audience": "User",
    "ExpiryHours": 8
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "25",
    "FromEmail": "dev@localhost",
    "FromName": "LP Dev",
    "FrontendUrl": "http://localhost:5173"
  }
}
```

**Frontend .env.development**:
```env
VITE_API_BASE_URL=http://localhost:5264
VITE_ENABLE_DEBUG_MODE=true
```

### Scenario 2: Production Deployment

**API appsettings.Production.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "DataRetention": {
    "TransactionRetentionDays": 365
  },
  "AllowedHosts": "lossprevention.company.com",
  "MongoDbSettings": {
    "ConnectionString": "mongodb://admin:${MONGO_PASSWORD}@prod-mongo-1:27017,prod-mongo-2:27017,prod-mongo-3:27017/?replicaSet=rs0&authSource=admin&ssl=true&maxPoolSize=200",
    "DatabaseName": "LossPrevention_Prod"
  },
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "LossPrevention-Prod",
    "Audience": "User",
    "ExpiryHours": 1
  },
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": "587",
    "FromEmail": "noreply@company.com",
    "FromName": "Loss Prevention System",
    "FrontendUrl": "https://lossprevention.company.com"
  },
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 200,
      "MaxRequestBodySize": 52428800
    }
  }
}
```

**Frontend .env.production**:
```env
VITE_API_BASE_URL=https://api.lossprevention.company.com
VITE_ENABLE_DEBUG_MODE=false
```

### Scenario 3: Docker Deployment

**docker-compose.yml**:
```yaml
version: '3.8'
services:
  api:
    image: lossprevention-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MongoDbSettings__ConnectionString=mongodb://mongo:27017
      - MongoDbSettings__DatabaseName=LossPrevention
      - JwtSettings__SecretKey=${JWT_SECRET}
      - Email__SmtpHost=smtp.company.com
      - Email__FrontendUrl=https://lossprevention.company.com
    ports:
      - "5264:80"
    depends_on:
      - mongo
  
  mongo:
    image: mongo:7.0
    volumes:
      - mongo-data:/data/db
    ports:
      - "27017:27017"
  
  frontend:
    image: lossprevention-ui:latest
    environment:
      - VITE_API_BASE_URL=http://api:80
    ports:
      - "5173:80"

volumes:
  mongo-data:
```

### Scenario 4: Multi-Tenant Configuration

**appsettings.json** (with tenant-specific databases):
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://server:27017",
    "DatabaseName": "LossPrevention_{TenantId}"
  }
}
```

Implement tenant resolution in middleware:

```csharp
// Example: Resolve database name based on subdomain
var tenant = context.Request.Host.Host.Split('.')[0];
var dbName = $"LossPrevention_{tenant}";
```

---

## Configuration Validation

### Automatic Validation on Startup

Implement configuration validation in `Program.cs`:

```csharp
// Add after builder.Services configuration
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Validate MongoDB settings
if (string.IsNullOrEmpty(mongoSettings?.ConnectionString))
    throw new InvalidOperationException("MongoDbSettings:ConnectionString is required");
if (string.IsNullOrEmpty(mongoSettings?.DatabaseName))
    throw new InvalidOperationException("MongoDbSettings:DatabaseName is required");

// Validate JWT settings
if (string.IsNullOrEmpty(jwtSettings?.SecretKey))
    throw new InvalidOperationException("JwtSettings:SecretKey is required");
if (jwtSettings.SecretKey.Length < 32)
    throw new InvalidOperationException("JwtSettings:SecretKey must be at least 32 characters");
if (jwtSettings.ExpiryHours < 1 || jwtSettings.ExpiryHours > 168)
    throw new InvalidOperationException("JwtSettings:ExpiryHours must be between 1 and 168");
```

### Configuration Validation Checklist

Before deploying, verify:

#### MongoDB Configuration
- [ ] Connection string is valid and accessible
- [ ] Database name follows naming conventions
- [ ] All required collection names are specified
- [ ] Connection pooling is configured for production
- [ ] Authentication credentials are secure (if applicable)

#### JWT Configuration
- [ ] Secret key is at least 32 characters
- [ ] Secret key is stored in environment variables (production)
- [ ] Issuer and Audience are environment-specific
- [ ] ExpiryHours is appropriate for security requirements

#### Email Configuration
- [ ] SMTP host is accessible
- [ ] SMTP port is correct for encryption method
- [ ] FromEmail domain is configured (SPF/DKIM)
- [ ] FrontendUrl matches the actual frontend URL
- [ ] Email templates render correctly

#### Data Retention
- [ ] Retention period meets legal requirements
- [ ] Cleanup job is scheduled and tested
- [ ] Archive strategy is in place (if needed)

#### Logging
- [ ] Log levels are appropriate for environment
- [ ] Sensitive data is not logged
- [ ] Log rotation is configured
- [ ] Log aggregation is set up (production)

#### Performance
- [ ] Connection pool sizes are tuned
- [ ] Memory limits are set
- [ ] Timeout values are reasonable
- [ ] Caching is configured

### Configuration Testing Script

Create a PowerShell script to validate configuration:

```powershell
# test-config.ps1
$apiConfig = Get-Content "LossPrevention.API/appsettings.json" | ConvertFrom-Json

Write-Host "Testing Configuration..."

# Test MongoDB connection
$mongoConn = $apiConfig.MongoDbSettings.ConnectionString
Write-Host "MongoDB Connection: $mongoConn"

# Test JWT secret length
$jwtSecret = $apiConfig.JwtSettings.SecretKey
if ($jwtSecret.Length -lt 32) {
    Write-Warning "JWT Secret Key is too short (${$jwtSecret.Length} chars, need 32+)"
} else {
    Write-Host "JWT Secret Key length: OK" -ForegroundColor Green
}

# Test SMTP connectivity
$smtpHost = $apiConfig.Email.SmtpHost
$smtpPort = $apiConfig.Email.SmtpPort
Write-Host "Testing SMTP: ${smtpHost}:${smtpPort}"

Write-Host "Configuration test complete."
```

---

## Environment Variables

### Using Environment Variables in Configuration

ASP.NET Core automatically reads environment variables and can override `appsettings.json` values.

#### Syntax

Environment variables use double underscores (`__`) to represent nested configuration sections:

```bash
# Override MongoDB connection string
MongoDbSettings__ConnectionString=mongodb://server:27017

# Override JWT secret
JwtSettings__SecretKey=my-super-secret-key

# Override email host
Email__SmtpHost=smtp.company.com
```

#### Setting Environment Variables

**Windows (PowerShell)**:
```powershell
$env:MongoDbSettings__ConnectionString = "mongodb://server:27017"
$env:JwtSettings__SecretKey = "my-secret-key"
```

**Windows (Persistent)**:
```powershell
[System.Environment]::SetEnvironmentVariable("MongoDbSettings__ConnectionString", "mongodb://server:27017", "Machine")
```

**Linux/Mac**:
```bash
export MongoDbSettings__ConnectionString="mongodb://server:27017"
export JwtSettings__SecretKey="my-secret-key"
```

**Docker**:
```dockerfile
ENV MongoDbSettings__ConnectionString="mongodb://mongo:27017"
ENV JwtSettings__SecretKey="my-secret-key"
```

### Recommended Environment Variables for Production

Create a `.env` file for production (never commit to source control):

```env
# .env.production (API)
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80

# MongoDB
MongoDbSettings__ConnectionString=mongodb://username:password@server:27017/?authSource=admin
MongoDbSettings__DatabaseName=LossPrevention_Prod

# JWT
JwtSettings__SecretKey=REPLACE_WITH_SECURE_RANDOM_STRING_AT_LEAST_32_CHARS
JwtSettings__Issuer=LossPrevention-Prod
JwtSettings__ExpiryHours=1

# Email
Email__SmtpHost=smtp.office365.com
Email__SmtpPort=587
Email__FromEmail=noreply@company.com
Email__FromName=Loss Prevention System
Email__FrontendUrl=https://lossprevention.company.com

# Data Retention
DataRetention__TransactionRetentionDays=365

# Logging
Logging__LogLevel__Default=Warning
Logging__LogLevel__Microsoft.AspNetCore=Error
```

### Azure App Service Configuration

In Azure Portal, configure application settings:

| Name | Value | Slot Setting |
|------|-------|--------------|
| `MongoDbSettings__ConnectionString` | `mongodb://...` | ✓ |
| `JwtSettings__SecretKey` | `***` | ✓ |
| `Email__SmtpHost` | `smtp.office365.com` | ✓ |
| `Email__FrontendUrl` | `https://app.company.com` | ✓ |

### AWS Elastic Beanstalk Configuration

Create `.ebextensions/environment.config`:

```yaml
option_settings:
  aws:elasticbeanstalk:application:environment:
    ASPNETCORE_ENVIRONMENT: Production
    MongoDbSettings__ConnectionString: mongodb://server:27017
    JwtSettings__SecretKey: your-secret-key
    Email__SmtpHost: smtp.company.com
    Email__FrontendUrl: https://app.company.com
```

### Kubernetes Secrets

Create Kubernetes secrets:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: lossprevention-secrets
type: Opaque
stringData:
  mongo-connection: mongodb://username:password@mongo:27017
  jwt-secret: your-secure-jwt-secret-key-here
```

Reference in deployment:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: lossprevention-api
spec:
  template:
    spec:
      containers:
      - name: api
        image: lossprevention-api:latest
        env:
        - name: MongoDbSettings__ConnectionString
          valueFrom:
            secretKeyRef:
              name: lossprevention-secrets
              key: mongo-connection
        - name: JwtSettings__SecretKey
          valueFrom:
            secretKeyRef:
              name: lossprevention-secrets
              key: jwt-secret
```

---

## Configuration Security Best Practices

### 1. Never Commit Secrets to Source Control

Use `.gitignore` to exclude sensitive files:

```gitignore
appsettings.Production.json
appsettings.Staging.json
*.secrets.json
.env
.env.local
.env.production
```

### 2. Use Secret Management Tools

- **Azure Key Vault**: For Azure deployments
- **AWS Secrets Manager**: For AWS deployments
- **HashiCorp Vault**: For on-premises or multi-cloud
- **Docker Secrets**: For Docker Swarm
- **Kubernetes Secrets**: For Kubernetes deployments

### 3. Rotate Credentials Regularly

- JWT secret keys: Every 90 days
- Database passwords: Every 90 days
- SMTP credentials: Every 90 days
- API keys: Every 90 days

### 4. Use Different Secrets Per Environment

Never reuse production secrets in development or staging.

### 5. Implement Configuration Encryption

For sensitive settings in configuration files:

```csharp
// Encrypt sensitive configuration sections
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
```

### 6. Audit Configuration Changes

- Log all configuration changes
- Require approval for production config changes
- Use version control for configuration files (excluding secrets)

---

## Troubleshooting Configuration Issues

### Common Issues

#### 1. MongoDB Connection Failures

**Symptom**: `Unable to connect to MongoDB`

**Solutions**:
- Verify connection string format
- Check network connectivity
- Verify authentication credentials
- Ensure MongoDB service is running
- Check firewall rules

**Test Connection**:
```powershell
# Test MongoDB connection
mongosh "mongodb://localhost:27017" --eval "db.version()"
```

#### 2. JWT Token Validation Errors

**Symptom**: `401 Unauthorized` or `Invalid token`

**Solutions**:
- Verify SecretKey matches between token generation and validation
- Check Issuer and Audience values
- Verify token hasn't expired
- Ensure system clocks are synchronized

#### 3. Email Send Failures

**Symptom**: Emails not being sent

**Solutions**:
- Verify SMTP host and port
- Check SMTP server authentication (if required)
- Test SMTP connectivity: `telnet smtp.company.com 587`
- Verify email address format
- Check spam/firewall rules

#### 4. Configuration Not Loading

**Symptom**: Default values being used instead of config

**Solutions**:
- Verify `appsettings.json` is copied to output directory
- Check file path in `Program.cs`
- Verify JSON syntax is valid
- Check environment variable overrides

**Validate JSON**:
```powershell
Get-Content appsettings.json | ConvertFrom-Json
```

---

## Configuration Migration Guide

### Migrating from Development to Production

1. **Create Production Configuration File**:
   - Copy `appsettings.json` to `appsettings.Production.json`
   - Update all environment-specific values

2. **Update Connection Strings**:
   - Replace localhost with production server addresses
   - Add authentication credentials
   - Configure SSL/TLS if required

3. **Secure Secrets**:
   - Move secrets to environment variables or secret management
   - Update deployment scripts to inject secrets

4. **Update URLs**:
   - Change `FrontendUrl` to production domain
   - Update CORS origins in `Program.cs`

5. **Adjust Timeouts and Limits**:
   - Increase connection pool sizes
   - Adjust JWT expiry for production security

6. **Configure Logging**:
   - Reduce log verbosity
   - Set up log aggregation
   - Configure alerts for errors

### Version Control Strategy

**Recommended approach**:
- Commit: `appsettings.json` (with development defaults)
- Commit: `appsettings.Development.json` (if needed)
- .gitignore: `appsettings.Production.json`
- .gitignore: `appsettings.Staging.json`
- Use: Environment variables for production secrets

---

## Summary

This configuration reference covers all aspects of configuring the Loss Prevention Tool:

- **Backend Configuration**: MongoDB, JWT, Email, Data Retention
- **Frontend Configuration**: Vite, Nuxt, Environment Variables
- **Environment Management**: Development, Staging, Production
- **Security**: Secret management, validation, best practices
- **Performance**: Connection pooling, caching, optimization
- **Deployment**: Docker, Kubernetes, Cloud platforms

For additional support or questions about configuration:
1. Review the relevant section in this document
2. Check the troubleshooting section
3. Consult the deployment guide (04_DEPLOYMENT_GUIDE.md)
4. Review security documentation (09_SECURITY_DOCUMENTATION.md)

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Maintained By**: Development Team
