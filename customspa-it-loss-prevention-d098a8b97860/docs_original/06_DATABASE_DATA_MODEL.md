# Loss Prevention Tool - Database & Data Model Documentation

## Table of Contents
1. [Database Overview](#database-overview)
2. [Connection Configuration](#connection-configuration)
3. [Collections Overview](#collections-overview)
4. [Collection Details](#collection-details)
5. [Data Retention & TTL Policies](#data-retention--ttl-policies)
6. [Indexes & Performance](#indexes--performance)
7. [Backup & Restore Procedures](#backup--restore-procedures)
8. [Performance Optimization](#performance-optimization)
9. [Data Migration](#data-migration)
10. [Aggregation Pipeline Examples](#aggregation-pipeline-examples)

---

## Database Overview

The Loss Prevention Tool uses **MongoDB** as its primary database system. MongoDB is a NoSQL document database that provides high performance, high availability, and automatic scaling.

### Why MongoDB?

- **Flexible Schema**: Perfect for dynamic transactional data with varying structures
- **Horizontal Scalability**: Easy to scale out with replica sets and sharding
- **Rich Query Language**: Supports complex aggregations and analytics
- **JSON-like Documents**: Natural fit for modern API development
- **High Performance**: Optimized for read-heavy workloads typical in analytics

### Database Name
- **Production**: `LossPrevention`
- **Development**: `LossPrevention` (local instance)

### MongoDB Version
- **Recommended**: MongoDB 7.0+
- **Minimum**: MongoDB 5.0+

---

## Connection Configuration

### appsettings.json Configuration

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "LossPrevention",
    "CollectionName_Users": "Users",
    "CollectionName_Roles": "Roles",
    "CollectionName_Permissions": "Permissions",
    "CollectionName_ReportData": "ReportData",
    "CollectionName_RulesConfiguration": "Rules",
    "CollectionName_MappingConfiguration": "Mappings",
    "CollectionName_Workspaces": "Workspaces",
    "CollectionName_Dashboards": "Dashboards",
    "CollectionName_DataIngestionConfigurations": "DataIngestionConfigurations",
    "CollectionName_DataIngestionSchedules": "DataIngestionSchedules",
    "CollectionName_Notifications": "Notifications",
    "CollectionName_Groups": "Groups",
    "CollectionName_FraudDetectionSettings": "FraudDetectionSettings"
  }
}
```

### Connection String Formats

#### Local Development
```
mongodb://localhost:27017
```

#### Authenticated Connection
```
mongodb://username:password@host:27017/LossPrevention?authSource=admin
```

#### Replica Set
```
mongodb://user:password@host1:27017,host2:27017,host3:27017/LossPrevention?replicaSet=rs0&authSource=LossPrevention
```

#### Azure Cosmos DB (MongoDB API)
```
mongodb://accountname:key@accountname.mongo.cosmos.azure.com:10255/LossPrevention?ssl=true&replicaSet=globaldb&retrywrites=false
```

### Environment Variables

```bash
# Windows PowerShell
$env:MongoDbSettings__ConnectionString = "mongodb://..."
$env:MongoDbSettings__DatabaseName = "LossPrevention"

# Linux/Mac
export MongoDbSettings__ConnectionString="mongodb://..."
export MongoDbSettings__DatabaseName="LossPrevention"
```

### C# Configuration Class

```csharp
namespace LossPrevention.Infrastructure.Models
{
    public sealed class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string CollectionName_Users { get; set; } = string.Empty;
        public string CollectionName_Roles { get; set; } = string.Empty;
        public string CollectionName_Permissions { get; set; } = string.Empty;
        public string CollectionName_ReportData { get; set; } = string.Empty;
        public string CollectionName_RulesConfiguration { get; set; } = string.Empty;
        public string CollectionName_MappingConfiguration { get; set; } = string.Empty;
        public string CollectionName_Workspaces { get; set; } = string.Empty;
        public string CollectionName_Dashboards { get; set; } = string.Empty;
        public string CollectionName_DataIngestionConfigurations { get; set; } = string.Empty;
        public string CollectionName_DataIngestionSchedules { get; set; } = string.Empty;
        public string CollectionName_Notifications { get; set; } = string.Empty;
        public string CollectionName_Groups { get; set; } = string.Empty;
        public string CollectionName_FraudDetectionSettings { get; set; } = string.Empty;
    }
}
```

---

## Collections Overview

| Collection Name | Purpose | Estimated Size | Update Frequency |
|----------------|---------|----------------|------------------|
| **Users** | User accounts and authentication | Small (< 1K docs) | Low |
| **Roles** | Role definitions | Small (< 100 docs) | Very Low |
| **Permissions** | Permission definitions | Small (< 100 docs) | Very Low |
| **ReportData** | Transaction/business data | Large (millions) | High (batch loads) |
| **Rules** | Validation/fraud detection rules | Small (< 1K docs) | Low |
| **Mappings** | Field mappings and metadata | Medium (< 10K docs) | Low |
| **Workspaces** | User workspace configurations | Medium (< 10K docs) | Medium |
| **Dashboards** | Dashboard layouts and widgets | Medium (< 10K docs) | Medium |
| **DataIngestionConfigurations** | Data import configurations | Small (< 1K docs) | Low |
| **DataIngestionSchedules** | Data import schedules | Small (< 1K docs) | Medium |
| **Notifications** | In-app notifications | Medium (grows over time) | High |
| **Groups** | User groups | Small (< 1K docs) | Low |
| **FraudDetectionSettings** | Fraud detection thresholds | Very Small (1-10 docs) | Very Low |

---

## Collection Details

### 1. Users Collection

#### Purpose
Stores user account information, credentials, and authentication data.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "Username": "john.doe",
  "PasswordHash": "$2a$11$...",
  "PasswordSalt": "random_salt",
  "Email": "john.doe@company.com",
  "FirstName": "John",
  "LastName": "Doe",
  "RegistrationDate": ISODate("2026-01-15T10:30:00Z"),
  "IsActive": true,
  "Roles": [
    ObjectId("..."),  // References Roles collection
    ObjectId("...")
  ],
  "LockField": "StoreID",      // Optional: Field-level security
  "LockValue": "STORE001"      // Optional: Restricts data access
}
```

#### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `_id` | ObjectId | Yes | Unique user identifier |
| `Username` | String | Yes | Unique username for login |
| `PasswordHash` | String | Yes | Bcrypt hashed password |
| `PasswordSalt` | String | Yes | Salt used for password hashing |
| `Email` | String | Yes | User email address |
| `FirstName` | String | Yes | User's first name |
| `LastName` | String | Yes | User's last name |
| `RegistrationDate` | DateTime | Yes | Account creation timestamp |
| `IsActive` | Boolean | Yes | Account active status |
| `Roles` | Array[ObjectId] | Yes | References to Role documents |
| `LockField` | String | No | Field name for row-level security |
| `LockValue` | String | No | Value to filter data by |

#### Indexes

```javascript
db.Users.createIndex({ "Username": 1 }, { unique: true })
db.Users.createIndex({ "Email": 1 }, { unique: true })
db.Users.createIndex({ "IsActive": 1 })
db.Users.createIndex({ "Roles": 1 })
```

#### Sample Document

```json
{
  "_id": ObjectId("65a1b2c3d4e5f6789a0b1c2d"),
  "Username": "admin",
  "PasswordHash": "$2a$11$abcdefghijklmnopqrstuvwxyz123456789",
  "PasswordSalt": "randomsalt123",
  "Email": "admin@lossprevention.com",
  "FirstName": "System",
  "LastName": "Administrator",
  "RegistrationDate": ISODate("2026-01-01T00:00:00Z"),
  "IsActive": true,
  "Roles": [
    ObjectId("65a1b2c3d4e5f6789a0b1c2e")
  ],
  "LockField": null,
  "LockValue": null
}
```

#### Relationships
- **One-to-Many** with **Roles**: A user can have multiple roles
- **One-to-Many** with **Notifications**: A user can receive many notifications

---

### 2. Roles Collection

#### Purpose
Defines user roles and their associated permissions.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "RoleName": "Administrator",
  "Description": "Full system access",
  "Permissions": [
    ObjectId("..."),  // References Permissions collection
    ObjectId("...")
  ]
}
```

#### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `_id` | ObjectId | Yes | Unique role identifier |
| `RoleName` | String | Yes | Role name (e.g., "Admin", "Viewer") |
| `Description` | String | Yes | Role description |
| `Permissions` | Array[ObjectId] | Yes | References to Permission documents |

#### Indexes

```javascript
db.Roles.createIndex({ "RoleName": 1 }, { unique: true })
```

#### Sample Document

```json
{
  "_id": ObjectId("65a1b2c3d4e5f6789a0b1c2e"),
  "RoleName": "Administrator",
  "Description": "Full system access with all permissions",
  "Permissions": [
    ObjectId("65a1b2c3d4e5f6789a0b1c2f"),
    ObjectId("65a1b2c3d4e5f6789a0b1c30"),
    ObjectId("65a1b2c3d4e5f6789a0b1c31")
  ]
}
```

#### Relationships
- **Many-to-Many** with **Users**: Users can have multiple roles
- **Many-to-Many** with **Permissions**: Roles can have multiple permissions

---

### 3. Permissions Collection

#### Purpose
Defines granular permissions for access control.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "PermissionName": "users.view",
  "PermissionText": "View Users",
  "Description": "Allows viewing user list and details"
}
```

#### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `_id` | ObjectId | Yes | Unique permission identifier |
| `PermissionName` | String | Yes | Permission key (e.g., "users.view") |
| `PermissionText` | String | Yes | Human-readable permission name |
| `Description` | String | Yes | Permission description |

#### Indexes

```javascript
db.Permissions.createIndex({ "PermissionName": 1 }, { unique: true })
```

#### Sample Document

```json
{
  "_id": ObjectId("65a1b2c3d4e5f6789a0b1c2f"),
  "PermissionName": "users.view",
  "PermissionText": "View Users",
  "Description": "Allows viewing the user list and user details"
}
```

#### Common Permissions

- `users.view`, `users.create`, `users.edit`, `users.delete`
- `workspaces.view`, `workspaces.create`, `workspaces.edit`, `workspaces.delete`
- `data.view`, `data.import`, `data.export`
- `rules.view`, `rules.manage`
- `dashboards.view`, `dashboards.create`, `dashboards.edit`, `dashboards.delete`

---

### 4. ReportData Collection

#### Purpose
Stores the actual transactional/business data imported into the system. This is the primary data collection used for analysis and reporting.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "TransactionID": "TXN-20260212-001",
  "StoreID": "STORE001",
  "RegisterID": "REG05",
  "CashierID": "CASH123",
  "BeginDateTime": ISODate("2026-02-12T10:30:00Z"),
  "EndDateTime": ISODate("2026-02-12T10:35:00Z"),
  "Total": 125.50,
  "Tax": 10.04,
  "Subtotal": 115.46,
  "Items": [
    {
      "SKU": "ITEM001",
      "Description": "Product Name",
      "Quantity": 2,
      "UnitPrice": 25.00,
      "Total": 50.00,
      "Discount": 0.00
    }
  ],
  "Tender": [
    {
      "Type": "Credit Card",
      "Amount": 125.50
    }
  ],
  "Voids": [],
  "Refunds": [],
  // Dynamic fields based on imported data
  "CustomField1": "value",
  "CustomField2": 100
}
```

#### Key Characteristics

- **Dynamic Schema**: Fields vary based on imported data format
- **Large Volume**: Can contain millions of documents
- **TTL Index**: Automatically deleted after retention period (default: 180 days)

#### Indexes

```javascript
// TTL Index for automatic data expiration
db.ReportData.createIndex(
  { "BeginDateTime": 1 }, 
  { 
    expireAfterSeconds: 15552000,  // 180 days
    name: "ttl_BeginDateTime" 
  }
)

// Dynamic indexes created based on field usage
db.ReportData.createIndex({ "StoreID": 1 })
db.ReportData.createIndex({ "RegisterID": 1 })
db.ReportData.createIndex({ "CashierID": 1 })
db.ReportData.createIndex({ "TransactionID": 1 })
db.ReportData.createIndex({ "Total": 1 })
db.ReportData.createIndex({ "BeginDateTime": -1 })
```

#### Sample Document

```json
{
  "_id": ObjectId("65a1b2c3d4e5f6789a0b1c40"),
  "TransactionID": "TXN-20260212-001",
  "StoreID": "STORE001",
  "RegisterID": "REG05",
  "CashierID": "CASH123",
  "BeginDateTime": ISODate("2026-02-12T10:30:00Z"),
  "EndDateTime": ISODate("2026-02-12T10:35:00Z"),
  "Total": 125.50,
  "Tax": 10.04,
  "Subtotal": 115.46,
  "Items": [
    {
      "SKU": "ITEM001",
      "Description": "Wireless Mouse",
      "Quantity": 2,
      "UnitPrice": 25.00,
      "Total": 50.00,
      "Discount": 0.00
    },
    {
      "SKU": "ITEM002",
      "Description": "USB Cable",
      "Quantity": 3,
      "UnitPrice": 21.82,
      "Total": 65.46,
      "Discount": 0.00
    }
  ],
  "Tender": [
    {
      "Type": "Credit Card",
      "Amount": 125.50
    }
  ]
}
```

#### Relationships
- Referenced by **Workspaces** (via queries)
- Used by **Rules** for validation
- Used by **Dashboards** for visualization

---

### 5. Rules (RulesConfiguration) Collection

#### Purpose
Stores validation and fraud detection rules applied to imported data.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "RuleName": "High Value Transaction",
  "RuleDescription": "Flags transactions over $500",
  "FieldPath": "Total",
  "ValueToCheck": "500",
  "AllowRangeCheck": true,
  "MinValue": 500.00,
  "MaxValue": null,
  "Enabled": true,
  "SumValues": false
}
```

#### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `_id` | ObjectId | Yes | Unique rule identifier |
| `RuleName` | String | Yes | Rule name |
| `RuleDescription` | String | Yes | Rule description |
| `FieldPath` | String | Yes | Path to field in data (e.g., "Tender.Amount") |
| `ValueToCheck` | String | Yes | Value or threshold to check |
| `AllowRangeCheck` | Boolean | Yes | Whether to use min/max range |
| `MinValue` | Decimal | No | Minimum value for range check |
| `MaxValue` | Decimal | No | Maximum value for range check |
| `Enabled` | Boolean | Yes | Whether rule is active |
| `SumValues` | Boolean | Yes | Whether to sum array field values |

#### Indexes

```javascript
db.Rules.createIndex({ "RuleName": 1 })
db.Rules.createIndex({ "Enabled": 1 })
```

#### Sample Document

```json
{
  "_id": ObjectId("65a1b2c3d4e5f6789a0b1c50"),
  "RuleName": "Excessive Refund Amount",
  "RuleDescription": "Flags transactions with refund amounts over $200",
  "FieldPath": "Refunds.Amount",
  "ValueToCheck": "200",
  "AllowRangeCheck": true,
  "MinValue": 200.00,
  "MaxValue": null,
  "Enabled": true,
  "SumValues": true
}
```

---

### 6. Mappings (MappingConfiguration) Collection

#### Purpose
Stores field mapping configurations that define how imported data fields are interpreted and displayed.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "Name": "BeginDateTime",
  "Alias": "Transaction Start Time",
  "DataType": "DateTime",
  "Format": "yyyy-MM-dd HH:mm:ss",
  "IsVisible": true,
  "IsCalculated": false,
  "IsLookup": false,
  "IsArray": false,
  "LongestLength": 0,
  "SetDefaultValue": false,
  "DefaultValue": "",
  "CollectionName": ""
}
```

#### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `_id` | ObjectId | Yes | Unique mapping identifier |
| `Name` | String | Yes | Original field name in source data |
| `Alias` | String | Yes | Display name in UI |
| `DataType` | String | Yes | Data type (String, Number, DateTime, Boolean) |
| `Format` | String | No | Format string for dates/numbers |
| `IsVisible` | Boolean | Yes | Whether field is visible in UI |
| `IsCalculated` | Boolean | Yes | Whether field is calculated |
| `IsLookup` | Boolean | Yes | Whether field is a lookup/reference |
| `IsArray` | Boolean | Yes | Whether field contains array data |
| `LongestLength` | Int32 | No | Maximum field length observed |
| `SetDefaultValue` | Boolean | Yes | Whether to use default value |
| `DefaultValue` | String | No | Default value if missing |
| `CollectionName` | String | No | Collection name for lookups |

#### Indexes

```javascript
db.Mappings.createIndex({ "Name": 1 })
db.Mappings.createIndex({ "Alias": 1 })
db.Mappings.createIndex({ "IsVisible": 1 })
```

#### Sample Document

```json
{
  "_id": ObjectId("65a1b2c3d4e5f6789a0b1c60"),
  "Name": "BeginDateTime",
  "Alias": "Transaction Start Time",
  "DataType": "DateTime",
  "Format": "yyyy-MM-dd HH:mm:ss",
  "IsVisible": true,
  "IsCalculated": false,
  "IsLookup": false,
  "IsArray": false,
  "LongestLength": 19,
  "SetDefaultValue": false,
  "DefaultValue": "",
  "CollectionName": ""
}
```

---

### 7. Workspaces Collection

#### Purpose
Stores user-created workspace configurations including tabs, queries, and field selections.

#### Schema

```javascript
{
  "_id": "workspace-001",  // String ID
  "Name": "Sales Analysis",
  "Description": "Daily sales analysis workspace",
  "Tabs": [
    {
      "Id": "tab-001",
      "Title": "High Value Transactions",
      "Description": "Transactions over $500",
      "SelectedFields": [
        {
          "Name": "TransactionID",
          "Alias": "Transaction ID",
          "DataType": "String",
          "GroupBy": false,
          "Aggregation": "",
          "IsCalculated": false,
          "Expression": "",
          "Prefix": "",
          "Suffix": "",
          "Visible": true
        }
      ],
      "GroupByField": "StoreID",
      "Query": {
        "Type": "AND",
        "Conditions": [
          {
            "Field": "Total",
            "Operator": "gte",
            "Value": 500
          }
        ],
        "Id": "query-001"
      },
      "DesignMode": false
    }
  ]
}
```

#### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `_id` | String | Yes | Unique workspace identifier |
| `Name` | String | Yes | Workspace name |
| `Description` | String | Yes | Workspace description |
| `Tabs` | Array | Yes | Array of tab configurations |

#### Indexes

```javascript
db.Workspaces.createIndex({ "Name": 1 })
```

#### Sample Document

```json
{
  "_id": "workspace-sales-001",
  "Name": "Daily Sales Dashboard",
  "Description": "Overview of daily sales metrics",
  "Tabs": [
    {
      "Id": "tab-high-value",
      "Title": "High Value Sales",
      "Description": "Sales transactions over $500",
      "SelectedFields": [
        {
          "Name": "TransactionID",
          "Alias": "Transaction ID",
          "DataType": "String",
          "GroupBy": false,
          "Aggregation": "",
          "IsCalculated": false,
          "Expression": "",
          "Prefix": "",
          "Suffix": "",
          "Visible": true
        },
        {
          "Name": "Total",
          "Alias": "Amount",
          "DataType": "Number",
          "GroupBy": false,
          "Aggregation": "sum",
          "IsCalculated": false,
          "Expression": "",
          "Prefix": "$",
          "Suffix": "",
          "Visible": true
        }
      ],
      "GroupByField": "",
      "Query": {
        "Type": "AND",
        "Conditions": [
          {
            "Field": "Total",
            "Operator": "gte",
            "Value": 500
          }
        ],
        "Id": "query-001"
      },
      "DesignMode": false
    }
  ]
}
```

#### Relationships
- **One-to-Many** with **Dashboards**: A workspace can have multiple dashboards

---

### 8. Dashboards Collection

#### Purpose
Stores dashboard layouts and widget configurations.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "Name": "Sales Overview",
  "WorkspaceId": ObjectId("..."),  // References Workspaces
  "TabId": "tab-001",
  "Blocks": [
    {
      "I": "block-001",
      "X": 0,
      "Y": 0,
      "W": 6,
      "H": 4,
      "Type": "chart",
      "Data": {
        "chartType": "bar",
        "title": "Sales by Store",
        "query": { /* ... */ }
      }
    }
  ],
  "CreatedAtUtc": ISODate("2026-02-01T00:00:00Z"),
  "UpdatedAtUtc": ISODate("2026-02-10T15:30:00Z")
}
```

#### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `_id` | ObjectId | Yes | Unique dashboard identifier |
| `Name` | String | Yes | Dashboard name |
| `WorkspaceId` | ObjectId | Yes | Reference to workspace |
| `TabId` | String | Yes | Tab ID within workspace |
| `Blocks` | Array | Yes | Dashboard widget blocks |
| `CreatedAtUtc` | DateTime | Yes | Creation timestamp |
| `UpdatedAtUtc` | DateTime | Yes | Last update timestamp |

#### Indexes

```javascript
db.Dashboards.createIndex({ "WorkspaceId": 1 })
db.Dashboards.createIndex({ "Name": 1 })
db.Dashboards.createIndex({ "CreatedAtUtc": -1 })
```

#### Sample Document

```json
{
  "_id": ObjectId("65a1b2c3d4e5f6789a0b1c70"),
  "Name": "Store Performance Dashboard",
  "WorkspaceId": ObjectId("65a1b2c3d4e5f6789a0b1c65"),
  "TabId": "tab-store-overview",
  "Blocks": [
    {
      "I": "widget-001",
      "X": 0,
      "Y": 0,
      "W": 6,
      "H": 4,
      "Type": "chart",
      "Data": {
        "chartType": "bar",
        "title": "Total Sales by Store",
        "xField": "StoreID",
        "yField": "Total",
        "aggregation": "sum"
      }
    },
    {
      "I": "widget-002",
      "X": 6,
      "Y": 0,
      "W": 6,
      "H": 4,
      "Type": "metric",
      "Data": {
        "title": "Total Revenue",
        "value": "$125,430.50",
        "trend": "+12.5%"
      }
    }
  ],
  "CreatedAtUtc": ISODate("2026-02-01T10:00:00Z"),
  "UpdatedAtUtc": ISODate("2026-02-12T14:20:00Z")
}
```

#### Relationships
- **Many-to-One** with **Workspaces**: Multiple dashboards per workspace

---

### 9. DataIngestionConfigurations Collection

#### Purpose
Stores configurations for data import sources (SFTP, file system, etc.).

#### Schema

```javascript
{
  "_id": "65a1b2c3d4e5f6789a0b1c80",
  "SelectedSources": ["sftp", "filesystem"],
  "SelectedFileType": "xml",
  "SftpHost": "sftp.example.com",
  "SftpPort": 22,
  "SftpUsername": "user",
  "SftpPassword": "encrypted_password",
  "SftpRemoteDirectory": "/data/exports",
  "FileSystemPath": "C:\\Data\\Imports",
  "LastRunAt": ISODate("2026-02-12T08:00:00Z"),
  "ManualLoad": false,
  "ScheduleType": "recurring",
  "ScheduleDate": null,
  "ScheduleTime": "08:00",
  "Recurrence": "daily",
  "SelectedDaysOfWeek": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
  "CreatedAt": ISODate("2026-01-01T00:00:00Z"),
  "UpdatedAt": ISODate("2026-02-10T00:00:00Z"),
  "UseMappings": true
}
```

#### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `_id` | String | Yes | Unique configuration identifier |
| `SelectedSources` | Array[String] | Yes | Data sources (sftp, filesystem) |
| `SelectedFileType` | String | Yes | File type (xml, csv, json) |
| `SftpHost` | String | No | SFTP server hostname |
| `SftpPort` | Int32 | No | SFTP server port (default: 22) |
| `SftpUsername` | String | No | SFTP username |
| `SftpPassword` | String | No | SFTP password (encrypted) |
| `SftpRemoteDirectory` | String | No | Remote directory path |
| `FileSystemPath` | String | No | Local file system path |
| `LastRunAt` | DateTime | No | Last execution timestamp |
| `ManualLoad` | Boolean | Yes | Whether manual loading is enabled |
| `ScheduleType` | String | Yes | "one-time" or "recurring" |
| `ScheduleDate` | DateTime | No | Scheduled date for one-time loads |
| `ScheduleTime` | String | No | Scheduled time (HH:mm format) |
| `Recurrence` | String | Yes | "daily" or "weekly" |
| `SelectedDaysOfWeek` | Array[String] | No | Days for weekly recurrence |
| `CreatedAt` | DateTime | Yes | Creation timestamp |
| `UpdatedAt` | DateTime | Yes | Last update timestamp |
| `UseMappings` | Boolean | Yes | Whether to apply field mappings |

#### Indexes

```javascript
db.DataIngestionConfigurations.createIndex({ "CreatedAt": -1 })
db.DataIngestionConfigurations.createIndex({ "LastRunAt": -1 })
```

---

### 10. DataIngestionSchedules Collection

#### Purpose
Stores execution schedules for data ingestion jobs.

#### Schema

```javascript
{
  "_id": "65a1b2c3d4e5f6789a0b1c90",
  "ConfigurationId": "65a1b2c3d4e5f6789a0b1c80",
  "ScheduleType": "recurring",
  "ScheduleDate": null,
  "ScheduleTime": "08:00",
  "Recurrence": "daily",
  "SelectedDaysOfWeek": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
  "IsActive": true,
  "LastExecuted": ISODate("2026-02-12T08:00:00Z"),
  "NextExecution": ISODate("2026-02-13T08:00:00Z"),
  "CreatedAt": ISODate("2026-01-01T00:00:00Z"),
  "UpdatedAt": ISODate("2026-02-12T08:05:00Z")
}
```

#### Indexes

```javascript
db.DataIngestionSchedules.createIndex({ "ConfigurationId": 1 })
db.DataIngestionSchedules.createIndex({ "IsActive": 1 })
db.DataIngestionSchedules.createIndex({ "NextExecution": 1 })
```

---

### 11. Notifications Collection

#### Purpose
Stores in-app notifications for users, roles, and groups.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "Type": "message",  // message, alert, reply
  "Title": "New Data Import Completed",
  "Message": "1,234 records imported successfully",
  "From": "system",
  "FromName": "System",
  "To": ["user_id_1", "user_id_2"],
  "ToType": "user",  // user, role, group
  "ReplyTo": "",
  "IsRead": false,
  "CreatedAtUtc": ISODate("2026-02-12T10:30:00Z")
}
```

#### Indexes

```javascript
db.Notifications.createIndex({ "To": 1 })
db.Notifications.createIndex({ "IsRead": 1 })
db.Notifications.createIndex({ "CreatedAtUtc": -1 })
db.Notifications.createIndex({ "From": 1 })
```

---

### 12. Groups Collection

#### Purpose
Stores user groups for organizing users and managing notifications.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "Name": "Store Managers",
  "Description": "All store manager users",
  "Members": [
    ObjectId("..."),  // User IDs
    ObjectId("...")
  ],
  "CreatedAtUtc": ISODate("2026-01-01T00:00:00Z"),
  "UpdatedAtUtc": ISODate("2026-02-01T00:00:00Z")
}
```

#### Indexes

```javascript
db.Groups.createIndex({ "Name": 1 })
db.Groups.createIndex({ "Members": 1 })
```

---

### 13. FraudDetectionSettings Collection

#### Purpose
Stores fraud detection threshold configurations and parameters.

#### Schema

```javascript
{
  "_id": ObjectId("..."),
  "thresholds": {
    "HighValue": {
      "Percentile": 99.0,
      "StdDevMultiplier": 3.0,
      "MinimumValue": 500.0,
      "Description": "High value transaction threshold",
      "Detects": "Unusually large transaction amounts"
    },
    "ExtremeHighValue": {
      "Percentile": 99.9,
      "Multiplier": 5.0,
      "MinimumValue": 1000.0,
      "Description": "Extreme high value threshold",
      "Detects": "Exceptionally large transactions"
    },
    "RefundAmount": {
      "Percentile": 95.0,
      "Description": "Excessive refund amount",
      "Detects": "Suspicious refund patterns"
    },
    "VoidCount": {
      "Percentile": 90.0,
      "Description": "Excessive void transactions",
      "Detects": "Potential void abuse"
    }
    // ... additional thresholds
  },
  "createdAt": ISODate("2026-01-01T00:00:00Z"),
  "updatedAt": ISODate("2026-02-01T00:00:00Z"),
  "createdBy": "admin",
  "updatedBy": "admin"
}
```

#### Indexes

```javascript
db.FraudDetectionSettings.createIndex({ "updatedAt": -1 })
```

---

## Data Retention & TTL Policies

### Overview

MongoDB's Time-To-Live (TTL) feature automatically deletes documents after a specified time period. This is crucial for managing data growth in the ReportData collection.

### TTL Configuration

#### appsettings.json
```json
{
  "DataRetention": {
    "TransactionRetentionDays": 180
  }
}
```

#### Creating TTL Index

```javascript
// TTL index on BeginDateTime field
db.ReportData.createIndex(
  { "BeginDateTime": 1 }, 
  { 
    expireAfterSeconds: 15552000,  // 180 days = 180 * 24 * 60 * 60
    name: "ttl_BeginDateTime" 
  }
)
```

#### C# Implementation

```csharp
// DatabaseInitializationService.cs
private async Task EnsureTtlIndexAsync()
{
    var collection = _reportDataRepository.Collection;
    var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending("BeginDateTime");
    var indexOptions = new CreateIndexOptions
    {
        ExpireAfter = TimeSpan.FromDays(_retentionDays),
        Name = "ttl_BeginDateTime"
    };
    
    var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, indexOptions);
    await collection.Indexes.CreateOneAsync(indexModel);
}
```

### Retention Policies by Collection

| Collection | Retention Policy | TTL Enabled |
|-----------|------------------|-------------|
| **ReportData** | 180 days (configurable) | Yes |
| **Notifications** | 90 days (recommended) | Optional |
| **Users** | Indefinite | No |
| **Workspaces** | Indefinite | No |
| **Dashboards** | Indefinite | No |
| **All Others** | Indefinite | No |

### Monitoring TTL

```javascript
// Check TTL index status
db.ReportData.getIndexes().forEach(function(index) {
  if (index.expireAfterSeconds) {
    print("TTL Index: " + index.name);
    print("Expires after: " + index.expireAfterSeconds + " seconds");
    print("Expires after: " + (index.expireAfterSeconds / 86400) + " days");
  }
})

// Check documents to be deleted
db.ReportData.find({
  "BeginDateTime": {
    $lt: new Date(Date.now() - (180 * 24 * 60 * 60 * 1000))
  }
}).count()
```

### Modifying TTL

```javascript
// Drop existing TTL index
db.ReportData.dropIndex("ttl_BeginDateTime")

// Create new TTL index with different retention (365 days)
db.ReportData.createIndex(
  { "BeginDateTime": 1 }, 
  { 
    expireAfterSeconds: 31536000,  // 365 days
    name: "ttl_BeginDateTime" 
  }
)
```

---

## Indexes & Performance

### Index Strategy

The Loss Prevention Tool uses a dynamic indexing strategy:

1. **Default Indexes**: Created on collection initialization
2. **Dynamic Indexes**: Created based on field usage patterns
3. **Compound Indexes**: Created for common query patterns

### Creating Indexes

#### Using MongoDB Shell

```javascript
// Single field index
db.ReportData.createIndex({ "StoreID": 1 })

// Compound index
db.ReportData.createIndex({ "StoreID": 1, "BeginDateTime": -1 })

// Text index for search
db.ReportData.createIndex({ "Description": "text" })

// Sparse index (only indexes documents with the field)
db.ReportData.createIndex({ "LockField": 1 }, { sparse: true })

// Unique index
db.Users.createIndex({ "Username": 1 }, { unique: true })
```

#### Programmatic Index Creation

```csharp
// C# - Dynamic index creation
public async Task CreateIndexesAsync(IEnumerable<string> fieldNames)
{
    var indexModels = fieldNames.Select(field =>
        new CreateIndexModel<TDocument>(
            Builders<TDocument>.IndexKeys.Ascending(field)
        )).ToList();

    if (indexModels.Any())
    {
        await _collection.Indexes.CreateManyAsync(indexModels);
    }
}
```

### Index Suggestions

The system includes an **IndexService** that analyzes query patterns and suggests indexes:

```csharp
// Analyzes documents and suggests indexes based on field types
private HashSet<IndexSuggestion> SuggestIndexes(BsonDocument document)
{
    var suggestions = new HashSet<IndexSuggestion>();

    foreach (var element in document.Elements)
    {
        var name = element.Name;
        var value = element.Value;

        if (name == "_id") continue;

        if (value.IsBoolean)
        {
            suggestions.Add(new IndexSuggestion
            {
                FieldName = name,
                Reason = "Boolean field - consider indexing if used for filtering"
            });
        }
        else if (value.IsBsonDateTime)
        {
            suggestions.Add(new IndexSuggestion
            {
                FieldName = name,
                Reason = "DateTime field - consider indexing for range queries"
            });
        }
        // ... additional logic
    }

    return suggestions;
}
```

### Monitoring Index Usage

```javascript
// Get index statistics
db.ReportData.aggregate([{ $indexStats: {} }])

// Identify unused indexes
db.ReportData.aggregate([
  { $indexStats: {} },
  { $match: { "accesses.ops": { $lt: 100 } } }
])

// Check index size
db.ReportData.stats().indexSizes
```

### Index Best Practices

1. **Index Frequently Queried Fields**: StoreID, RegisterID, CashierID, BeginDateTime
2. **Use Compound Indexes**: For queries with multiple filter criteria
3. **Limit Index Count**: Too many indexes slow down writes
4. **Monitor Index Usage**: Drop unused indexes
5. **Use Covered Queries**: Include all queried fields in index
6. **Consider Index Size**: Indexes consume memory

### Recommended Indexes

```javascript
// Users collection
db.Users.createIndex({ "Username": 1 }, { unique: true })
db.Users.createIndex({ "Email": 1 }, { unique: true })
db.Users.createIndex({ "IsActive": 1 })

// ReportData collection
db.ReportData.createIndex({ "BeginDateTime": 1 }, { expireAfterSeconds: 15552000 })
db.ReportData.createIndex({ "StoreID": 1 })
db.ReportData.createIndex({ "RegisterID": 1 })
db.ReportData.createIndex({ "CashierID": 1 })
db.ReportData.createIndex({ "TransactionID": 1 })
db.ReportData.createIndex({ "StoreID": 1, "BeginDateTime": -1 })

// Workspaces collection
db.Workspaces.createIndex({ "Name": 1 })

// Dashboards collection
db.Dashboards.createIndex({ "WorkspaceId": 1 })
db.Dashboards.createIndex({ "CreatedAtUtc": -1 })

// Notifications collection
db.Notifications.createIndex({ "To": 1 })
db.Notifications.createIndex({ "IsRead": 1 })
db.Notifications.createIndex({ "CreatedAtUtc": -1 })

// DataIngestionSchedules collection
db.DataIngestionSchedules.createIndex({ "NextExecution": 1 })
db.DataIngestionSchedules.createIndex({ "IsActive": 1 })
```

---

## Backup & Restore Procedures

### Using mongodump & mongorestore

#### Backup Entire Database

```bash
# Basic backup
mongodump --uri="mongodb://localhost:27017" --db=LossPrevention --out=/backups/

# With authentication
mongodump \
  --uri="mongodb://username:password@localhost:27017/LossPrevention?authSource=admin" \
  --out=/backups/$(date +%Y%m%d)

# Compress backup
mongodump \
  --uri="mongodb://localhost:27017" \
  --db=LossPrevention \
  --archive=/backups/LossPrevention_$(date +%Y%m%d).archive \
  --gzip
```

#### Backup Specific Collections

```bash
# Backup only Users and Roles
mongodump \
  --uri="mongodb://localhost:27017" \
  --db=LossPrevention \
  --collection=Users \
  --out=/backups/

mongodump \
  --uri="mongodb://localhost:27017" \
  --db=LossPrevention \
  --collection=Roles \
  --out=/backups/
```

#### Restore Database

```bash
# Basic restore
mongorestore --uri="mongodb://localhost:27017" /backups/20260212/

# Restore compressed archive
mongorestore \
  --uri="mongodb://localhost:27017" \
  --archive=/backups/LossPrevention_20260212.archive \
  --gzip

# Restore to different database
mongorestore \
  --uri="mongodb://localhost:27017" \
  --db=LossPrevention_Test \
  /backups/20260212/LossPrevention/
```

### Automated Backup Script (Linux)

```bash
#!/bin/bash
# /usr/local/bin/mongodb-backup.sh

BACKUP_DIR="/backups/mongodb"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="LossPrevention"
RETENTION_DAYS=30

# Create backup
mongodump \
  --uri="mongodb://backup_user:password@localhost:27017/${DB_NAME}?authSource=admin" \
  --archive="${BACKUP_DIR}/${DB_NAME}_${DATE}.archive" \
  --gzip

# Check if backup was successful
if [ $? -eq 0 ]; then
  echo "Backup completed successfully: ${DB_NAME}_${DATE}.archive"
else
  echo "Backup failed!"
  exit 1
fi

# Delete backups older than retention period
find ${BACKUP_DIR} -name "${DB_NAME}_*.archive" -mtime +${RETENTION_DAYS} -delete

echo "Old backups cleaned up (older than ${RETENTION_DAYS} days)"
```

#### Schedule with Cron

```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * /usr/local/bin/mongodb-backup.sh >> /var/log/mongodb-backup.log 2>&1
```

### Automated Backup Script (Windows PowerShell)

```powershell
# mongodb-backup.ps1
$BackupDir = "C:\Backups\MongoDB"
$Date = Get-Date -Format "yyyyMMdd_HHmmss"
$DbName = "LossPrevention"
$RetentionDays = 30

# Create backup directory if it doesn't exist
if (!(Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir
}

# Create backup
$BackupFile = "$BackupDir\${DbName}_${Date}.archive"
& mongodump --uri="mongodb://localhost:27017/$DbName" --archive=$BackupFile --gzip

if ($LASTEXITCODE -eq 0) {
    Write-Host "Backup completed successfully: $BackupFile"
} else {
    Write-Host "Backup failed!"
    exit 1
}

# Delete old backups
Get-ChildItem -Path $BackupDir -Filter "${DbName}_*.archive" | 
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-$RetentionDays) } | 
    Remove-Item -Force

Write-Host "Old backups cleaned up (older than $RetentionDays days)"
```

#### Schedule with Task Scheduler

```powershell
# Create scheduled task (run as Administrator)
$Action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
  -Argument "-File C:\Scripts\mongodb-backup.ps1"

$Trigger = New-ScheduledTaskTrigger -Daily -At 2am

Register-ScheduledTask -TaskName "MongoDB Backup" `
  -Action $Action `
  -Trigger $Trigger `
  -Description "Daily MongoDB backup at 2 AM"
```

### Cloud Backup (Azure Blob Storage)

```bash
#!/bin/bash
# Backup to Azure Blob Storage

BACKUP_FILE="/tmp/LossPrevention_$(date +%Y%m%d).archive"

# Create backup
mongodump \
  --uri="mongodb://localhost:27017" \
  --db=LossPrevention \
  --archive=$BACKUP_FILE \
  --gzip

# Upload to Azure
az storage blob upload \
  --account-name mystorageaccount \
  --container-name mongodb-backups \
  --name $(basename $BACKUP_FILE) \
  --file $BACKUP_FILE

# Clean up local file
rm $BACKUP_FILE
```

### Replica Set Backup

For production environments with replica sets, always backup from a secondary node to avoid impacting primary performance:

```bash
# Connect to secondary node
mongodump \
  --host=secondary-node.example.com:27017 \
  --db=LossPrevention \
  --archive=/backups/LossPrevention_$(date +%Y%m%d).archive \
  --gzip
```

### Point-in-Time Recovery

For point-in-time recovery, enable oplog:

```bash
# Backup with oplog
mongodump \
  --uri="mongodb://localhost:27017" \
  --db=LossPrevention \
  --oplog \
  --archive=/backups/LossPrevention_oplog_$(date +%Y%m%d).archive \
  --gzip

# Restore to specific point in time
mongorestore \
  --uri="mongodb://localhost:27017" \
  --oplogReplay \
  --oplogLimit="2026-02-12T10:30:00" \
  --archive=/backups/LossPrevention_oplog_20260212.archive \
  --gzip
```

---

## Performance Optimization

### Query Optimization

#### Use Projection to Limit Fields

```javascript
// Bad: Retrieves all fields
db.ReportData.find({ "StoreID": "STORE001" })

// Good: Retrieves only needed fields
db.ReportData.find(
  { "StoreID": "STORE001" },
  { "TransactionID": 1, "Total": 1, "BeginDateTime": 1 }
)
```

#### Use Covered Queries

```javascript
// Create compound index
db.ReportData.createIndex({ "StoreID": 1, "Total": 1, "BeginDateTime": 1 })

// Query can be satisfied entirely from the index
db.ReportData.find(
  { "StoreID": "STORE001" },
  { "StoreID": 1, "Total": 1, "BeginDateTime": 1, "_id": 0 }
)
```

#### Limit Result Sets

```javascript
// Always use limit for large result sets
db.ReportData.find({ "StoreID": "STORE001" }).limit(100)

// Use skip and limit for pagination
db.ReportData.find({ "StoreID": "STORE001" })
  .skip(100)
  .limit(100)
```

### Aggregation Optimization

#### Early Filtering with $match

```javascript
// Good: Filter early in pipeline
db.ReportData.aggregate([
  { $match: { "StoreID": "STORE001" } },  // Early filtering
  { $group: { "_id": "$CashierID", "total": { $sum: "$Total" } } },
  { $sort: { "total": -1 } },
  { $limit: 10 }
])
```

#### Use $project to Reduce Document Size

```javascript
db.ReportData.aggregate([
  { $match: { "BeginDateTime": { $gte: ISODate("2026-02-01") } } },
  { $project: { "StoreID": 1, "Total": 1, "CashierID": 1 } },  // Reduce size
  { $group: { "_id": "$CashierID", "total": { $sum: "$Total" } } }
])
```

#### Leverage Indexes in Aggregations

```javascript
// Create index to support aggregation
db.ReportData.createIndex({ "StoreID": 1, "BeginDateTime": -1 })

// Aggregation uses index
db.ReportData.aggregate([
  { $match: { "StoreID": "STORE001", "BeginDateTime": { $gte: ISODate("2026-02-01") } } },
  { $sort: { "BeginDateTime": -1 } },  // Uses index for sorting
  { $limit: 100 }
])
```

### Connection Pooling

Ensure proper connection pooling in appsettings:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017/?minPoolSize=10&maxPoolSize=100&maxIdleTimeMS=30000"
  }
}
```

### Monitoring Performance

#### Explain Query Plans

```javascript
// Analyze query execution
db.ReportData.find({ "StoreID": "STORE001" }).explain("executionStats")

// Check if index is used
db.ReportData.find({ "StoreID": "STORE001" }).explain().queryPlanner.winningPlan

// Analyze aggregation pipeline
db.ReportData.aggregate([
  { $match: { "StoreID": "STORE001" } },
  { $group: { "_id": "$CashierID", "total": { $sum: "$Total" } } }
], { explain: true })
```

#### Profile Slow Queries

```javascript
// Enable profiling for slow queries (> 100ms)
db.setProfilingLevel(1, { slowms: 100 })

// View slow queries
db.system.profile.find().sort({ ts: -1 }).limit(10)

// Disable profiling
db.setProfilingLevel(0)
```

### Hardware Recommendations

#### Development
- **CPU**: 4 cores
- **RAM**: 16 GB
- **Storage**: 100 GB SSD
- **MongoDB RAM**: At least 4-8 GB for working set

#### Production
- **CPU**: 8+ cores
- **RAM**: 32+ GB (MongoDB uses available RAM for caching)
- **Storage**: 500 GB+ NVMe SSD
- **MongoDB RAM**: Ideally, working set should fit in RAM

---

## Data Migration

### Migrating from Old System

#### Step 1: Export Data from Source System

```bash
# Export to CSV
mysql -u user -p database -e "SELECT * FROM transactions" > transactions.csv

# Export to JSON
mongoexport --db=OldDB --collection=Transactions --out=transactions.json
```

#### Step 2: Transform Data

```python
# transform_data.py
import json
import csv
from datetime import datetime

def transform_csv_to_mongo(csv_file, json_file):
    with open(csv_file, 'r') as f_in, open(json_file, 'w') as f_out:
        reader = csv.DictReader(f_in)
        for row in reader:
            doc = {
                "TransactionID": row["txn_id"],
                "StoreID": row["store"],
                "Total": float(row["amount"]),
                "BeginDateTime": datetime.fromisoformat(row["date"]),
                # ... map other fields
            }
            f_out.write(json.dumps(doc) + '\n')
```

#### Step 3: Import into MongoDB

```bash
# Import JSON documents
mongoimport \
  --uri="mongodb://localhost:27017" \
  --db=LossPrevention \
  --collection=ReportData \
  --file=transactions.json

# Import with upsert (prevents duplicates)
mongoimport \
  --uri="mongodb://localhost:27017" \
  --db=LossPrevention \
  --collection=ReportData \
  --file=transactions.json \
  --mode=upsert \
  --upsertFields=TransactionID
```

### Schema Migration

#### Adding New Fields

```javascript
// Add new field to all documents
db.ReportData.updateMany(
  { "NewField": { $exists: false } },
  { $set: { "NewField": "default_value" } }
)
```

#### Renaming Fields

```javascript
// Rename field
db.ReportData.updateMany(
  {},
  { $rename: { "OldFieldName": "NewFieldName" } }
)
```

#### Changing Field Types

```javascript
// Convert string to number
db.ReportData.find({ "Total": { $type: "string" } }).forEach(function(doc) {
  db.ReportData.updateOne(
    { "_id": doc._id },
    { $set: { "Total": parseFloat(doc.Total) } }
  )
})
```

### Version Migration Script

```javascript
// migration_v1_to_v2.js
print("Starting migration from v1 to v2...");

// Step 1: Add new collection
db.createCollection("FraudDetectionSettings");

// Step 2: Migrate existing rules to new format
db.Rules.find().forEach(function(rule) {
  db.Rules.updateOne(
    { "_id": rule._id },
    { $set: { "Version": 2, "MigratedAt": new Date() } }
  );
});

// Step 3: Add indexes
db.FraudDetectionSettings.createIndex({ "updatedAt": -1 });

print("Migration completed successfully!");
```

### Run Migration

```bash
# Execute migration script
mongosh mongodb://localhost:27017/LossPrevention migration_v1_to_v2.js
```

---

## Aggregation Pipeline Examples

### Example 1: Sales by Store

```javascript
db.ReportData.aggregate([
  // Filter by date range
  {
    $match: {
      "BeginDateTime": {
        $gte: ISODate("2026-02-01T00:00:00Z"),
        $lt: ISODate("2026-03-01T00:00:00Z")
      }
    }
  },
  // Group by store and sum totals
  {
    $group: {
      "_id": "$StoreID",
      "TotalSales": { $sum: "$Total" },
      "TransactionCount": { $sum: 1 },
      "AverageTransaction": { $avg: "$Total" }
    }
  },
  // Sort by total sales descending
  {
    $sort: { "TotalSales": -1 }
  },
  // Limit to top 10 stores
  {
    $limit: 10
  },
  // Format output
  {
    $project: {
      "_id": 0,
      "StoreID": "$_id",
      "TotalSales": { $round: ["$TotalSales", 2] },
      "TransactionCount": 1,
      "AverageTransaction": { $round: ["$AverageTransaction", 2] }
    }
  }
])
```

### Example 2: Hourly Sales Trend

```javascript
db.ReportData.aggregate([
  {
    $match: {
      "BeginDateTime": {
        $gte: ISODate("2026-02-12T00:00:00Z"),
        $lt: ISODate("2026-02-13T00:00:00Z")
      }
    }
  },
  {
    $group: {
      "_id": { $hour: "$BeginDateTime" },
      "TotalSales": { $sum: "$Total" },
      "TransactionCount": { $sum: 1 }
    }
  },
  {
    $sort: { "_id": 1 }
  },
  {
    $project: {
      "_id": 0,
      "Hour": "$_id",
      "TotalSales": { $round: ["$TotalSales", 2] },
      "TransactionCount": 1
    }
  }
])
```

### Example 3: Top Performing Cashiers

```javascript
db.ReportData.aggregate([
  {
    $match: {
      "BeginDateTime": { $gte: ISODate("2026-02-01T00:00:00Z") }
    }
  },
  {
    $group: {
      "_id": {
        "CashierID": "$CashierID",
        "StoreID": "$StoreID"
      },
      "TotalSales": { $sum: "$Total" },
      "TransactionCount": { $sum: 1 },
      "AverageTransaction": { $avg: "$Total" }
    }
  },
  {
    $sort: { "TotalSales": -1 }
  },
  {
    $limit: 20
  },
  {
    $project: {
      "_id": 0,
      "CashierID": "$_id.CashierID",
      "StoreID": "$_id.StoreID",
      "TotalSales": { $round: ["$TotalSales", 2] },
      "TransactionCount": 1,
      "AverageTransaction": { $round: ["$AverageTransaction", 2] }
    }
  }
])
```

### Example 4: Refund Analysis

```javascript
db.ReportData.aggregate([
  // Match transactions with refunds
  {
    $match: {
      "Refunds": { $exists: true, $ne: [] },
      "BeginDateTime": { $gte: ISODate("2026-02-01T00:00:00Z") }
    }
  },
  // Unwind refunds array
  {
    $unwind: "$Refunds"
  },
  // Group by store
  {
    $group: {
      "_id": "$StoreID",
      "TotalRefundAmount": { $sum: "$Refunds.Amount" },
      "RefundCount": { $sum: 1 }
    }
  },
  // Sort by refund amount
  {
    $sort: { "TotalRefundAmount": -1 }
  },
  {
    $project: {
      "_id": 0,
      "StoreID": "$_id",
      "TotalRefundAmount": { $round: ["$TotalRefundAmount", 2] },
      "RefundCount": 1
    }
  }
])
```

### Example 5: High-Value Transactions (Fraud Detection)

```javascript
db.ReportData.aggregate([
  {
    $match: {
      "Total": { $gte: 500 },
      "BeginDateTime": {
        $gte: ISODate("2026-02-01T00:00:00Z"),
        $lt: ISODate("2026-03-01T00:00:00Z")
      }
    }
  },
  {
    $lookup: {
      from: "Users",
      localField: "CashierID",
      foreignField: "Username",
      as: "CashierInfo"
    }
  },
  {
    $project: {
      "TransactionID": 1,
      "StoreID": 1,
      "RegisterID": 1,
      "CashierID": 1,
      "Total": 1,
      "BeginDateTime": 1,
      "CashierName": {
        $concat: [
          { $arrayElemAt: ["$CashierInfo.FirstName", 0] },
          " ",
          { $arrayElemAt: ["$CashierInfo.LastName", 0] }
        ]
      }
    }
  },
  {
    $sort: { "Total": -1 }
  },
  {
    $limit: 50
  }
])
```

### Example 6: Daily Sales Comparison

```javascript
db.ReportData.aggregate([
  {
    $match: {
      "BeginDateTime": {
        $gte: ISODate("2026-02-01T00:00:00Z"),
        $lt: ISODate("2026-03-01T00:00:00Z")
      }
    }
  },
  {
    $group: {
      "_id": {
        $dateToString: { format: "%Y-%m-%d", date: "$BeginDateTime" }
      },
      "TotalSales": { $sum: "$Total" },
      "TransactionCount": { $sum: 1 }
    }
  },
  {
    $sort: { "_id": 1 }
  },
  {
    $project: {
      "_id": 0,
      "Date": "$_id",
      "TotalSales": { $round: ["$TotalSales", 2] },
      "TransactionCount": 1
    }
  }
])
```

### Example 7: Statistical Analysis with $facet

```javascript
db.ReportData.aggregate([
  {
    $match: {
      "BeginDateTime": { $gte: ISODate("2026-02-01T00:00:00Z") }
    }
  },
  {
    $facet: {
      "statistics": [
        {
          $group: {
            "_id": null,
            "TotalRevenue": { $sum: "$Total" },
            "AverageTransaction": { $avg: "$Total" },
            "MaxTransaction": { $max: "$Total" },
            "MinTransaction": { $min: "$Total" },
            "TransactionCount": { $sum: 1 }
          }
        }
      ],
      "topStores": [
        {
          $group: {
            "_id": "$StoreID",
            "TotalSales": { $sum: "$Total" }
          }
        },
        { $sort: { "TotalSales": -1 } },
        { $limit: 5 }
      ],
      "topCashiers": [
        {
          $group: {
            "_id": "$CashierID",
            "TotalSales": { $sum: "$Total" }
          }
        },
        { $sort: { "TotalSales": -1 } },
        { $limit: 5 }
      ]
    }
  }
])
```

---

## Additional Resources

### MongoDB Documentation
- [MongoDB Manual](https://docs.mongodb.com/manual/)
- [Aggregation Pipeline](https://docs.mongodb.com/manual/core/aggregation-pipeline/)
- [Indexing Strategies](https://docs.mongodb.com/manual/applications/indexes/)
- [TTL Indexes](https://docs.mongodb.com/manual/core/index-ttl/)

### C# MongoDB Driver
- [MongoDB.Driver Documentation](https://mongodb.github.io/mongo-csharp-driver/)
- [BSON Serialization](https://mongodb.github.io/mongo-csharp-driver/2.14/reference/bson/)

### Performance Tools
- [MongoDB Compass](https://www.mongodb.com/products/compass) - GUI for MongoDB
- [Studio 3T](https://studio3t.com/) - Advanced MongoDB IDE
- [MongoShell](https://www.mongodb.com/docs/mongodb-shell/) - MongoDB shell

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-12 | System | Initial documentation |

---

**End of Document**
