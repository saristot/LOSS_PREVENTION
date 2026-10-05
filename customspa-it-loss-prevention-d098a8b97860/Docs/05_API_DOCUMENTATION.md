# API Documentation

**Loss Prevention Tool - API Reference v1.0**

## Table of Contents

1. [Overview](#overview)
2. [Base URL](#base-url)
3. [Authentication & Authorization](#authentication--authorization)
4. [Common Response Codes](#common-response-codes)
5. [Error Handling](#error-handling)
6. [Pagination](#pagination)
7. [Rate Limiting](#rate-limiting)
8. [API Endpoints](#api-endpoints)
   - [Authentication](#authentication)
   - [User Management](#user-management)
   - [Role Management](#role-management)
   - [Permission Management](#permission-management)
   - [Workspaces](#workspaces)
   - [Dashboards](#dashboards)
   - [Data Management](#data-management)
   - [Rules](#rules)
   - [Mappings](#mappings)
   - [Notifications](#notifications)
   - [Groups](#groups)
   - [Data Ingestion](#data-ingestion)
   - [Fraud Detection](#fraud-detection)
9. [Swagger/OpenAPI](#swaggeropenapi)

---

## Overview

The Loss Prevention Tool API is a RESTful API built using FastEndpoints and ASP.NET Core. It provides comprehensive endpoints for managing transactions, fraud detection, user management, and data analysis for loss prevention operations.

**Technology Stack:**
- ASP.NET Core
- FastEndpoints
- MongoDB
- JWT Authentication
- Swagger/OpenAPI

---

## Base URL

```
Development: http://localhost:<port>
Production: https://your-domain.com
```

---

## Authentication & Authorization

### JWT Token Authentication

The API uses JSON Web Tokens (JWT) for authentication. Tokens must be included in the Authorization header for protected endpoints.

**JWT Configuration:**
- **Issuer:** LossPrevention
- **Audience:** User
- **Secret Key:** Configured in appsettings.json (e96eae77-3a2c-4e96-a660-fbfbb8edcd62 in development)
- **Expiry:** 1 hour
- **Algorithm:** HMAC-SHA256

**Token Claims:**
- `ClaimTypes.Name`: Username
- `Permissions`: User permissions (multiple claims)
- `LockField`: Optional field-level security lock
- `LockValue`: Optional field-level security value

### Request Headers

```http
Authorization: Bearer <your_jwt_token>
Content-Type: application/json
```

### Permission-Based Authorization

The API uses a granular permission system with 41 different permissions. Each protected endpoint requires specific permissions.

**Permission Categories:**
- User Management (4 permissions)
- Role Management (5 permissions)
- Permission Management (5 permissions)
- Workspace Management (4 permissions)
- Dashboard Management (5 permissions)
- Data & Report Management (3 permissions)
- Mapping Management (4 permissions)
- Rule Management (5 permissions)
- Data Ingestion Management (2 permissions)
- Notification Management (1 permission)
- Group Management (1 permission)
- Fraud Detection Settings (2 permissions)

See [Permissions Reference](#permissions-reference) for a complete list.

---

## Common Response Codes

| Status Code | Description |
|-------------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 400 | Bad Request - Invalid input or validation error |
| 401 | Unauthorized - Missing or invalid authentication token |
| 403 | Forbidden - User lacks required permissions |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error - Server error occurred |

---

## Error Handling

Errors are returned in a standardized format:

```json
{
  "errors": {
    "field_name": ["Error message 1", "Error message 2"]
  },
  "statusCode": 400
}
```

**Example Error Response:**

```json
{
  "errors": {
    "validation": ["At least one source must be selected"],
    "query": ["Missing or invalid 'Take' or 'QueryPipeline'."]
  },
  "statusCode": 400
}
```

---

## Pagination

Endpoints that return lists support pagination using the following parameters:

**Query Parameters:**
- `Skip`: Number of records to skip (default: 0)
- `Take`: Number of records to return (required for some endpoints)

**Response Format:**

```json
{
  "data": [...],
  "total": 1500
}
```

---

## Rate Limiting

Currently, no rate limiting is configured. This may be implemented in future versions.

---

## API Endpoints

### Authentication

#### Login

Authenticates a user and returns a JWT token.

**Endpoint:** `POST /users/login`

**Authentication Required:** No (AllowAnonymous)

**Request Body:**

```json
{
  "username": "string",
  "password": "string"
}
```

**Response (200 OK):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (401 Unauthorized):**

Authentication failed - invalid credentials.

**Example:**

```bash
curl -X POST http://localhost:5000/users/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password123"}'
```

---

### User Management

#### Create User

Creates a new user account.

**Endpoint:** `POST /users/create`

**Authentication Required:** Yes

**Permission Required:** `CAN_CREATE_USER`

**Request Body:**

```json
{
  "username": "string",
  "password": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "isActive": true,
  "roles": ["roleId1", "roleId2"]
}
```

**Response (201 Created):**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "registrationDate": "2026-02-12T10:30:00Z",
  "isActive": true,
  "roles": ["roleId1", "roleId2"]
}
```

**Response Codes:**
- 201: User created successfully
- 400: Invalid input
- 403: Insufficient permissions
- 500: Server error

---

#### Get All Users

Retrieves a list of all users.

**Endpoint:** `GET /users`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_USER`

**Response (200 OK):**

```json
[
  {
    "_id": "507f1f77bcf86cd799439011",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "registrationDate": "2026-02-12T10:30:00Z",
    "isActive": true,
    "roles": ["roleId1"]
  }
]
```

---

#### Get User by ID

Retrieves a specific user by ID.

**Endpoint:** `GET /users/id/{Id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_USER`

**Path Parameters:**
- `Id` (string): User ID

**Response (200 OK):**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "registrationDate": "2026-02-12T10:30:00Z",
  "isActive": true,
  "roles": ["roleId1"]
}
```

---

#### Get User by Username

Retrieves a specific user by username.

**Endpoint:** `GET /users/username/{Username}`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_USER`

**Path Parameters:**
- `Username` (string): Username

---

#### Get User Roles and Permissions

Retrieves roles and permissions for a specific user.

**Endpoint:** `GET /users/rolesandpermissions/{Id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_USER`

**Path Parameters:**
- `Id` (string): User ID

**Response (200 OK):**

```json
{
  "userId": "507f1f77bcf86cd799439011",
  "username": "string",
  "roles": [
    {
      "roleId": "string",
      "roleName": "string"
    }
  ],
  "permissions": ["CAN_CREATE_USER", "CAN_VIEW_USER"]
}
```

---

#### Update User

Updates an existing user.

**Endpoint:** `POST /users/update`

**Authentication Required:** Yes

**Permission Required:** `CAN_UPDATE_USER`

**Request Body:**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "isActive": true,
  "roles": ["roleId1"]
}
```

**Response (200 OK):** Updated user object

---

#### Delete User

Deletes a user by ID.

**Endpoint:** `DELETE /users/id/{UserId}`

**Authentication Required:** Yes

**Permission Required:** `CAN_DELETE_USER`

**Path Parameters:**
- `UserId` (string): User ID to delete

**Response (200 OK):**

```json
{
  "success": true,
  "message": "User deleted successfully"
}
```

---

#### Forgot Password

Initiates password reset process.

**Endpoint:** `POST /users/forgot-password`

**Authentication Required:** No

**Request Body:**

```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**

```json
{
  "message": "Password reset email sent"
}
```

---

#### Validate Reset Token

Validates a password reset token.

**Endpoint:** `POST /users/validate-reset-token`

**Authentication Required:** No

**Request Body:**

```json
{
  "token": "string"
}
```

**Response (200 OK):**

```json
{
  "valid": true
}
```

---

#### Reset Password

Resets user password using a reset token.

**Endpoint:** `POST /users/reset-password`

**Authentication Required:** No

**Request Body:**

```json
{
  "token": "string",
  "newPassword": "string"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Password reset successful"
}
```

---

#### Validate Password

Validates password strength.

**Endpoint:** `POST /users/validatepassword`

**Authentication Required:** Yes

**Request Body:**

```json
{
  "password": "string"
}
```

**Response (200 OK):**

```json
{
  "valid": true,
  "message": "Password meets requirements"
}
```

---

### Role Management

#### Create Role

Creates a new role.

**Endpoint:** `POST /roles`

**Authentication Required:** Yes

**Permission Required:** `CAN_CREATE_ROLE`

**Request Body:**

```json
{
  "roleName": "string",
  "description": "string",
  "permissions": ["permissionId1", "permissionId2"]
}
```

**Response (200 OK):**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "roleName": "string",
  "description": "string",
  "permissions": ["permissionId1", "permissionId2"]
}
```

---

#### Get All Roles

Retrieves all roles.

**Endpoint:** `GET /roles/all`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_ROLE`

**Response (200 OK):**

```json
[
  {
    "_id": "507f1f77bcf86cd799439011",
    "roleName": "Admin",
    "description": "Administrator role",
    "permissions": ["permissionId1"]
  }
]
```

---

#### Get Role by ID

Retrieves a specific role by ID.

**Endpoint:** `GET /roles/by-id`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_ROLE`

**Query Parameters:**
- `id` (string): Role ID

---

#### Get Role by Name

Retrieves a specific role by name.

**Endpoint:** `GET /roles/by-name`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_ROLE`

**Query Parameters:**
- `name` (string): Role name

---

#### Update Role

Updates an existing role.

**Endpoint:** `PUT /roles/update`

**Authentication Required:** Yes

**Permission Required:** `CAN_UPDATE_ROLE`

**Request Body:**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "roleName": "string",
  "description": "string",
  "permissions": ["permissionId1"]
}
```

---

#### Delete Role

Deletes a role.

**Endpoint:** `DELETE /roles/delete`

**Authentication Required:** Yes

**Permission Required:** `CAN_DELETE_ROLE`

**Query Parameters:**
- `id` (string): Role ID to delete

---

#### Add User to Role

Assigns a role to a user.

**Endpoint:** `POST /roles/user`

**Authentication Required:** Yes

**Permission Required:** `CAN_ASSIGN_ROLE`

**Request Body:**

```json
{
  "userId": "string",
  "roleId": "string"
}
```

---

#### Remove Role from User

Removes a role from a user.

**Endpoint:** `DELETE /roles/remove-role`

**Authentication Required:** Yes

**Permission Required:** `CAN_ASSIGN_ROLE`

**Query Parameters:**
- `userId` (string)
- `roleId` (string)

---

#### Get User Roles

Gets all roles for a specific user.

**Endpoint:** `GET /users/roles`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_ROLE`

**Query Parameters:**
- `userId` (string)

---

#### Check User in Role

Checks if a user has a specific role.

**Endpoint:** `GET /roles/is-in-role`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_ROLE`

**Query Parameters:**
- `userId` (string)
- `roleId` (string)

**Response (200 OK):**

```json
{
  "isInRole": true
}
```

---

### Permission Management

#### Create Permission

Creates a new permission.

**Endpoint:** `POST /permissions/create`

**Authentication Required:** Yes

**Permission Required:** `CAN_CREATE_PERMISSION`

**Request Body:**

```json
{
  "permissionName": "CAN_DO_SOMETHING",
  "permissionText": "Can Do Something",
  "description": "Allows user to do something"
}
```

**Response (200 OK):**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "permissionName": "CAN_DO_SOMETHING",
  "permissionText": "Can Do Something",
  "description": "Allows user to do something"
}
```

---

#### Get All Permissions

Retrieves all permissions.

**Endpoint:** `GET /permissions/all`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_PERMISSION`

**Response (200 OK):**

```json
[
  {
    "_id": "507f1f77bcf86cd799439011",
    "permissionName": "CAN_CREATE_USER",
    "permissionText": "Create User",
    "description": "Allows the user to create new users"
  }
]
```

---

#### Get Permission by ID

Retrieves a specific permission by ID.

**Endpoint:** `GET /permissions/by-id`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_PERMISSION`

**Query Parameters:**
- `id` (string): Permission ID

---

#### Get Permission by Name

Retrieves a specific permission by name.

**Endpoint:** `GET /permissions/by-name`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_PERMISSION`

**Query Parameters:**
- `name` (string): Permission name

---

#### Get Role Permissions

Gets all permissions for a specific role.

**Endpoint:** `GET /permissions`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_PERMISSION`

**Query Parameters:**
- `roleId` (string)

---

#### Update Permission

Updates an existing permission.

**Endpoint:** `PUT /permissions/update`

**Authentication Required:** Yes

**Permission Required:** `CAN_UPDATE_PERMISSION`

**Request Body:**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "permissionName": "CAN_DO_SOMETHING",
  "permissionText": "Can Do Something",
  "description": "Updated description"
}
```

---

#### Delete Permission

Deletes a permission.

**Endpoint:** `DELETE /permissions/delete`

**Authentication Required:** Yes

**Permission Required:** `CAN_DELETE_PERMISSION`

**Query Parameters:**
- `id` (string): Permission ID

---

#### Add Permission to Role

Adds a permission to a role.

**Endpoint:** `POST /permissions/add-permission`

**Authentication Required:** Yes

**Permission Required:** `CAN_ASSIGN_PERMISSION`

**Request Body:**

```json
{
  "roleId": "string",
  "permissionId": "string"
}
```

---

#### Remove Permission from Role

Removes a permission from a role.

**Endpoint:** `DELETE /permissions`

**Authentication Required:** Yes

**Permission Required:** `CAN_ASSIGN_PERMISSION`

**Query Parameters:**
- `roleId` (string)
- `permissionId` (string)

---

#### Check User Permission

Checks if a user has a specific permission.

**Endpoint:** `GET /permissions/user`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_PERMISSION`

**Query Parameters:**
- `userId` (string)
- `permissionName` (string)

**Response (200 OK):**

```json
{
  "hasPermission": true
}
```

---

### Workspaces

#### Create Workspace

Creates a new workspace with tabs and queries.

**Endpoint:** `POST /workspaces`

**Authentication Required:** No (AllowAnonymous)

**Request Body:**

```json
{
  "name": "string",
  "description": "string",
  "tabs": [
    {
      "id": "string",
      "title": "string",
      "description": "string",
      "selectedFields": [
        {
          "name": "string",
          "alias": "string",
          "dataType": "string",
          "groupBy": true,
          "aggregation": "sum",
          "isCalculated": false,
          "expression": "string",
          "prefix": "$",
          "suffix": "",
          "visible": true
        }
      ],
      "groupByField": "string",
      "query": {
        "type": "AND",
        "conditions": [
          {
            "field": "string",
            "operator": "equals",
            "value": "string"
          }
        ],
        "id": "string"
      },
      "designMode": false
    }
  ]
}
```

**Response (200 OK):**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "name": "string",
  "description": "string",
  "tabs": [...]
}
```

**Response Codes:**
- 200: Workspace created successfully
- 400: Invalid input
- 500: Server error

---

#### Get All Workspaces

Retrieves all workspaces.

**Endpoint:** `GET /workspaces`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_WORKSPACE`

**Response (200 OK):**

```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "name": "string",
    "description": "string",
    "tabs": [...]
  }
]
```

---

#### Get Workspace by ID

Retrieves a specific workspace by ID.

**Endpoint:** `GET /workspaces/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_WORKSPACE`

**Path Parameters:**
- `id` (string): Workspace ID

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "name": "string",
  "description": "string",
  "tabs": [
    {
      "id": "string",
      "title": "string",
      "description": "string",
      "selectedFields": [...],
      "groupByField": "string",
      "query": {...},
      "designMode": false
    }
  ]
}
```

---

#### Update Workspace

Updates an existing workspace.

**Endpoint:** `PUT /workspaces/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_UPDATE_WORKSPACE`

**Path Parameters:**
- `id` (string): Workspace ID

**Request Body:** Same structure as Create Workspace

**Response (200 OK):** Updated workspace object

---

#### Delete Workspace

Deletes a workspace.

**Endpoint:** `DELETE /workspaces/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_DELETE_WORKSPACE`

**Path Parameters:**
- `id` (string): Workspace ID

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Workspace deleted successfully"
}
```

---

### Dashboards

#### Create Dashboard

Creates a new dashboard with blocks.

**Endpoint:** `POST /dashboards`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_DASHBOARDS`

**Summary:** Create a dashboard

**Description:** Creates a new dashboard document with blocks.

**Request Body:**

```json
{
  "name": "string",
  "description": "string",
  "blocks": [
    {
      "id": "string",
      "type": "chart",
      "title": "string",
      "config": {
        "chartType": "bar",
        "dataSource": "string"
      },
      "position": {
        "x": 0,
        "y": 0,
        "width": 4,
        "height": 3
      }
    }
  ]
}
```

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "name": "string",
  "description": "string",
  "blocks": [...],
  "createdAt": "2026-02-12T10:30:00Z",
  "updatedAt": "2026-02-12T10:30:00Z"
}
```

---

#### Get All Dashboards

Retrieves all dashboards.

**Endpoint:** `GET /dashboards`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_DASHBOARD` or `CAN_MANAGE_DASHBOARDS`

**Response (200 OK):**

```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "name": "string",
    "description": "string",
    "blocks": [...],
    "createdAt": "2026-02-12T10:30:00Z"
  }
]
```

---

#### Get Dashboard by ID

Retrieves a specific dashboard.

**Endpoint:** `GET /dashboards/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_DASHBOARD` or `CAN_MANAGE_DASHBOARDS`

**Path Parameters:**
- `id` (string): Dashboard ID

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "name": "string",
  "description": "string",
  "blocks": [...],
  "createdAt": "2026-02-12T10:30:00Z",
  "updatedAt": "2026-02-12T10:30:00Z"
}
```

---

#### Update Dashboard

Updates an existing dashboard.

**Endpoint:** `PUT /dashboards/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_UPDATE_DASHBOARD` or `CAN_MANAGE_DASHBOARDS`

**Path Parameters:**
- `id` (string): Dashboard ID

**Request Body:** Same structure as Create Dashboard

**Response (200 OK):** Updated dashboard object

---

#### Delete Dashboard

Deletes a dashboard.

**Endpoint:** `DELETE /dashboards/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_DELETE_DASHBOARD` or `CAN_MANAGE_DASHBOARDS`

**Path Parameters:**
- `id` (string): Dashboard ID

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Dashboard deleted successfully"
}
```

---

### Data Management

#### Create Transaction

Uploads and processes one or more XML transaction files.

**Endpoint:** `POST /data/create-transactions`

**Authentication Required:** Yes

**Permission Required:** `CAN_CREATE_TRANSACTION`

**Content-Type:** `multipart/form-data`

**Request Body:**
- `files`: One or more XML files

**Response (200 OK):**

```json
[
  "Inserted: transaction1.xml id: 507f1f77bcf86cd799439011",
  "Inserted: transaction2.xml id: 507f1f77bcf86cd799439012"
]
```

**Response Codes:**
- 200: Files processed successfully
- 400: No files provided or invalid format
- 403: Insufficient permissions
- 500: Processing error

**Example:**

```bash
curl -X POST http://localhost:5000/data/create-transactions \
  -H "Authorization: Bearer <token>" \
  -F "files=@transaction1.xml" \
  -F "files=@transaction2.xml"
```

---

#### Query Report Data

Returns a flattened list of report data using an aggregation pipeline.

**Endpoint:** `POST /data/report/query`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_REPORT`

**Description:** Returns a flattened list of report data from the report collection using an aggregation pipeline.

**Request Body:**

```json
{
  "queryPipeline": [
    { "$match": { "Status": "Completed" } },
    { "$group": { "_id": "$Category", "total": { "$sum": "$Amount" } } }
  ],
  "skip": 0,
  "take": 100
}
```

**Response (200 OK):**

```json
{
  "data": [
    {
      "_id": "Category1",
      "total": 5000.00
    }
  ],
  "total": 150
}
```

**Response Codes:**
- 200: Query executed successfully
- 400: Missing or invalid query pipeline
- 403: Insufficient permissions
- 500: Query execution error

**Notes:**
- Results are cached for improved performance
- The `take` parameter is required and must be greater than 0
- Pipeline stages must be valid MongoDB aggregation operators

---

#### Get Distance Analysis

Computes Euclidean Distance for provided fields and date range.

**Endpoint:** `POST /distance`

**Authentication Required:** No (AllowAnonymous)

**Summary:** Computes Euclidean Distance for provided fields and date range.

**Request Body:**

```json
{
  "id": "6831e59ccc5492cce99141aa",
  "startDateField": "TransactionDate",
  "endDateField": "TransactionDate",
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-01-31T23:59:59Z",
  "keyField": "CustomerId",
  "fields": [
    {
      "name": "Amount",
      "weight": 1.0
    },
    {
      "name": "Quantity",
      "weight": 0.5
    }
  ]
}
```

**Response (200 OK):**

```json
{
  "sourceDocumentId": "6831e59ccc5492cce99141aa",
  "comparisons": [
    {
      "comparedDocumentId": "string",
      "distance": 0.85,
      "fieldDistances": {
        "Amount": 0.5,
        "Quantity": 0.35
      }
    }
  ]
}
```

**Response Codes:**
- 200: Distance calculation successful
- 400: Invalid input parameters
- 500: Calculation error

---

### Rules

#### Create Rule

Creates a new rule configuration.

**Endpoint:** `POST /rules`

**Authentication Required:** Yes

**Permission Required:** `CAN_CREATE_RULE`

**Request Body:**

```json
{
  "ruleName": "string",
  "ruleDescription": "string",
  "fieldPath": "Tender.Amount",
  "valueToCheck": "1000",
  "allowRangeCheck": true,
  "minValue": "100",
  "maxValue": "5000",
  "enabled": true,
  "sumValues": false
}
```

**Response (200 OK):**

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "ruleName": "string",
  "ruleDescription": "string",
  "fieldPath": "Tender.Amount",
  "valueToCheck": "1000",
  "allowRangeCheck": true,
  "minValue": "100",
  "maxValue": "5000",
  "enabled": true,
  "sumValues": false
}
```

---

#### Get All Rules

Retrieves all rule configurations.

**Endpoint:** `GET /rules`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_RULE`

**Response (200 OK):**

```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "ruleName": "High Value Transaction",
    "ruleDescription": "Flags transactions over $5000",
    "fieldPath": "Total",
    "allowRangeCheck": true,
    "minValue": "5000",
    "enabled": true
  }
]
```

---

#### Get Rule by ID

Retrieves a specific rule by ID.

**Endpoint:** `GET /rules/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_RULE`

**Path Parameters:**
- `id` (string): Rule ID

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "ruleName": "string",
  "ruleDescription": "string",
  "fieldPath": "string",
  "enabled": true
}
```

---

#### Update Rule

Updates an existing rule.

**Endpoint:** `PUT /rules`

**Authentication Required:** Yes

**Permission Required:** `CAN_UPDATE_RULE`

**Request Body:** Same structure as Create Rule (must include `id` field)

**Response (200 OK):** Updated rule object

---

#### Delete Rule

Deletes a rule.

**Endpoint:** `DELETE /rules/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_DELETE_RULE`

**Path Parameters:**
- `id` (string): Rule ID

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Rule deleted successfully"
}
```

---

#### Apply Rules

Applies rule configurations to data.

**Endpoint:** `GET /rules/apply`

**Authentication Required:** Yes

**Permission Required:** `CAN_APPLY_RULE`

**Description:** Executes all enabled rules against the report data.

**Response (200 OK):**

```json
{
  "rulesApplied": 5,
  "documentsAffected": 125,
  "violations": [
    {
      "documentId": "string",
      "ruleName": "string",
      "violation": "string"
    }
  ]
}
```

---

### Mappings

#### Create Mapping

Creates a new field mapping.

**Endpoint:** `POST /data/mappings`

**Authentication Required:** Yes

**Permission Required:** `CAN_CREATE_MAPPINGS`

**Summary:** Create a new field mapping.

**Request Body:**

```json
{
  "name": "CustomerName",
  "alias": "Customer",
  "dataType": "string",
  "isVisible": true,
  "isArray": false,
  "isCalculated": false,
  "isLookup": false,
  "collectionName": "Customers",
  "longestLength": 100
}
```

**Response (201 Created):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "name": "CustomerName",
  "alias": "Customer",
  "dataType": "string",
  "isVisible": true,
  "isArray": false,
  "isCalculated": false,
  "isLookup": false,
  "collectionName": "Customers",
  "longestLength": 100
}
```

**Response Codes:**
- 201: Mapping created successfully
- 400: Invalid input or mapping already exists
- 403: Insufficient permissions

---

#### Get All Mappings

Retrieves all field mappings.

**Endpoint:** `GET /data/mappings`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_MAPPINGS`

**Description:** Returns a list of all field mappings.

**Response (200 OK):**

```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "name": "CustomerName",
    "alias": "Customer",
    "dataType": "string",
    "isVisible": true
  }
]
```

---

#### Update Mapping

Updates an existing field mapping.

**Endpoint:** `PUT /data/mappings/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_UPDATE_MAPPINGS`

**Path Parameters:**
- `id` (string): Mapping ID

**Request Body:** Same structure as Create Mapping

**Response (200 OK):** Updated mapping object

---

#### Delete Mapping

Deletes a field mapping.

**Endpoint:** `DELETE /data/mappings/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_DELETE_MAPPINGS`

**Path Parameters:**
- `id` (string): Mapping ID

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Mapping deleted successfully"
}
```

---

### Notifications

#### Create Notification

Creates a new notification and sends it to specified recipients.

**Endpoint:** `POST /notifications`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_NOTIFICATIONS`

**Summary:** Create a notification

**Description:** Creates a new notification and sends it to specified recipients.

**Request Body:**

```json
{
  "title": "string",
  "message": "string",
  "type": "info",
  "priority": "normal",
  "recipients": ["userId1", "userId2"],
  "metadata": {
    "category": "alert",
    "relatedId": "string"
  }
}
```

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "title": "string",
  "message": "string",
  "type": "info",
  "priority": "normal",
  "recipients": ["userId1", "userId2"],
  "createdAt": "2026-02-12T10:30:00Z",
  "isRead": false
}
```

---

#### Get All Notifications

Retrieves all notifications.

**Endpoint:** `GET /notifications`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_NOTIFICATIONS`

**Query Parameters:**
- `userId` (optional): Filter by user
- `isRead` (optional): Filter by read status

**Response (200 OK):**

```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "title": "string",
    "message": "string",
    "type": "info",
    "isRead": false,
    "createdAt": "2026-02-12T10:30:00Z"
  }
]
```

---

#### Mark Notification as Read

Marks a notification as read.

**Endpoint:** `PUT /notifications/{id}/read`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_NOTIFICATIONS`

**Path Parameters:**
- `id` (string): Notification ID

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Notification marked as read"
}
```

---

#### Reply to Notification

Adds a reply to a notification.

**Endpoint:** `POST /notifications/{id}/reply`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_NOTIFICATIONS`

**Path Parameters:**
- `id` (string): Notification ID

**Request Body:**

```json
{
  "message": "string",
  "userId": "string"
}
```

**Response (200 OK):**

```json
{
  "replyId": "string",
  "message": "string",
  "userId": "string",
  "createdAt": "2026-02-12T10:30:00Z"
}
```

---

#### Delete Notification

Deletes a notification.

**Endpoint:** `DELETE /notifications/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_NOTIFICATIONS`

**Path Parameters:**
- `id` (string): Notification ID

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Notification deleted successfully"
}
```

---

### Groups

#### Create Group

Creates a new group with members.

**Endpoint:** `POST /groups`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_GROUPS`

**Summary:** Create a group

**Description:** Creates a new group with members.

**Request Body:**

```json
{
  "name": "string",
  "description": "string",
  "members": ["userId1", "userId2"],
  "permissions": ["permissionId1"],
  "metadata": {
    "department": "string",
    "location": "string"
  }
}
```

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "name": "string",
  "description": "string",
  "members": ["userId1", "userId2"],
  "permissions": ["permissionId1"],
  "createdAt": "2026-02-12T10:30:00Z"
}
```

---

#### Get All Groups

Retrieves all groups.

**Endpoint:** `GET /groups`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_GROUPS`

**Response (200 OK):**

```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "name": "string",
    "description": "string",
    "memberCount": 5,
    "createdAt": "2026-02-12T10:30:00Z"
  }
]
```

---

#### Get Group by ID

Retrieves a specific group.

**Endpoint:** `GET /groups/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_GROUPS`

**Path Parameters:**
- `id` (string): Group ID

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "name": "string",
  "description": "string",
  "members": ["userId1", "userId2"],
  "permissions": ["permissionId1"],
  "createdAt": "2026-02-12T10:30:00Z",
  "updatedAt": "2026-02-12T10:30:00Z"
}
```

---

#### Update Group

Updates an existing group.

**Endpoint:** `PUT /groups/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_GROUPS`

**Path Parameters:**
- `id` (string): Group ID

**Request Body:** Same structure as Create Group

**Response (200 OK):** Updated group object

---

#### Delete Group

Deletes a group.

**Endpoint:** `DELETE /groups/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_GROUPS`

**Path Parameters:**
- `id` (string): Group ID

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Group deleted successfully"
}
```

---

#### Add Member to Group

Adds a member to a group.

**Endpoint:** `POST /groups/{groupId}/members/{userId}`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_GROUPS`

**Path Parameters:**
- `groupId` (string): Group ID
- `userId` (string): User ID to add

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Member added to group"
}
```

---

#### Remove Member from Group

Removes a member from a group.

**Endpoint:** `DELETE /groups/{groupId}/members/{userId}`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_GROUPS`

**Path Parameters:**
- `groupId` (string): Group ID
- `userId` (string): User ID to remove

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Member removed from group"
}
```

---

### Data Ingestion

#### Get Data Ingestion Configuration

Retrieves the current data ingestion configuration.

**Endpoint:** `GET /api/data-ingestion`

**Authentication Required:** No (AllowAnonymous)

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "selectedSources": ["sftp", "filesystem"],
  "selectedFileType": "xml",
  "sftpHost": "sftp.example.com",
  "sftpPort": 22,
  "sftpUsername": "user",
  "sftpRemoteDirectory": "/data",
  "fileSystemPath": "C:\\data\\import",
  "scheduleType": "recurring",
  "scheduleTime": "02:00:00",
  "scheduleDate": null,
  "recurrence": "daily"
}
```

---

#### Update Data Ingestion Configuration

Saves complete data ingestion configuration.

**Endpoint:** `PUT /api/data-ingestion`

**Authentication Required:** No (AllowAnonymous)

**Summary:** Save complete data ingestion configuration.

**Request Body:**

```json
{
  "selectedSources": ["sftp"],
  "selectedFileType": "xml",
  "sftpHost": "sftp.example.com",
  "sftpPort": 22,
  "sftpUsername": "user",
  "sftpPassword": "password",
  "sftpRemoteDirectory": "/data",
  "fileSystemPath": null,
  "scheduleType": "recurring",
  "scheduleTime": "02:00:00",
  "scheduleDate": null,
  "recurrence": "daily",
  "selectedDaysOfWeek": [1, 2, 3, 4, 5]
}
```

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "selectedSources": ["sftp"],
  "selectedFileType": "xml",
  ...
}
```

**Validation Rules:**
- At least one source must be selected
- File type is required
- SFTP configuration required if SFTP source selected:
  - Host is required
  - Port must be between 1 and 65535
  - Username is required
  - Password is required
  - Remote directory is required
- File system path required if filesystem source selected
- Schedule time is required
- Schedule date required for one-time schedules
- At least one day required for weekly recurrence

**Response Codes:**
- 200: Configuration saved successfully
- 400: Validation errors (see error response for details)

---

#### Update Data Ingestion Sources

Updates selected data sources.

**Endpoint:** `PATCH /api/data-ingestion/sources`

**Authentication Required:** No (AllowAnonymous)

**Request Body:**

```json
{
  "selectedSources": ["sftp", "filesystem"]
}
```

---

#### Update Data Ingestion Schedule

Updates schedule configuration.

**Endpoint:** `PATCH /api/data-ingestion/schedule`

**Authentication Required:** No (AllowAnonymous)

**Request Body:**

```json
{
  "scheduleType": "recurring",
  "scheduleTime": "02:00:00",
  "scheduleDate": null,
  "recurrence": "daily"
}
```

---

#### Update Recurrence Options

Updates recurrence options for recurring schedules.

**Endpoint:** `PUT /api/data-ingestion/recurrence-options`

**Authentication Required:** No (AllowAnonymous)

**Request Body:**

```json
{
  "recurrence": "weekly",
  "selectedDaysOfWeek": [1, 2, 3, 4, 5]
}
```

---

#### Run Data Ingestion

Manually triggers data ingestion process.

**Endpoint:** `POST /api/data-ingestion/run`

**Authentication Required:** No (AllowAnonymous)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Data ingestion started",
  "jobId": "string",
  "filesProcessed": 0,
  "startTime": "2026-02-12T10:30:00Z"
}
```

---

#### Clear Data Ingestion Configuration

Deletes the data ingestion configuration.

**Endpoint:** `DELETE /api/data-ingestion`

**Authentication Required:** No (AllowAnonymous)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Configuration cleared"
}
```

---

#### Clear Data Ingestion Schedule

Clears the schedule configuration.

**Endpoint:** `DELETE /api/data-ingestion/schedule`

**Authentication Required:** No (AllowAnonymous)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Schedule cleared"
}
```

---

### Fraud Detection

#### Get Fraud Detection Settings

Retrieves fraud detection threshold configuration.

**Endpoint:** `GET /api/fraud-detection/settings`

**Authentication Required:** Yes

**Permission Required:** `CAN_VIEW_FRAUD_SETTINGS`

**Summary:** Get fraud detection threshold configuration

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "thresholds": {
    "highValue": {
      "percentile": 95,
      "stdDevMultiplier": 2.0,
      "minimumValue": 1000,
      "description": "High value transactions",
      "detects": "Transactions in the top 5%"
    },
    "extremeHighValue": {
      "percentile": 99,
      "multiplier": 3.0,
      "minimumValue": 5000,
      "description": "Extreme high value transactions",
      "detects": "Transactions in the top 1%"
    },
    "lowValue": {
      "percentile": 5,
      "stdDevMultiplier": 2.0,
      "maximumValue": 10,
      "description": "Unusually low value transactions",
      "detects": "Transactions in the bottom 5%"
    },
    "frequencyAnomaly": {
      "stdDevMultiplier": 3.0,
      "minimumCount": 10,
      "description": "Unusual transaction frequency",
      "detects": "Abnormal frequency patterns"
    }
  },
  "createdAt": "2026-02-12T10:30:00Z",
  "updatedAt": "2026-02-12T10:30:00Z",
  "createdBy": "userId",
  "updatedBy": "userId"
}
```

**Response Codes:**
- 200: Settings retrieved (may be null if not configured)
- 403: Insufficient permissions

---

#### Create Fraud Detection Settings

Creates initial fraud detection threshold configuration.

**Endpoint:** `POST /api/fraud-detection/settings`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_FRAUD_SETTINGS`

**Request Body:**

```json
{
  "thresholds": {
    "highValue": {
      "percentile": 95,
      "stdDevMultiplier": 2.0,
      "minimumValue": 1000,
      "description": "High value transactions",
      "detects": "Transactions in the top 5%"
    },
    "extremeHighValue": {
      "percentile": 99,
      "multiplier": 3.0,
      "minimumValue": 5000,
      "description": "Extreme high value transactions",
      "detects": "Transactions in the top 1%"
    },
    "lowValue": {
      "percentile": 5,
      "stdDevMultiplier": 2.0,
      "maximumValue": 10,
      "description": "Unusually low value transactions",
      "detects": "Transactions in the bottom 5%"
    },
    "frequencyAnomaly": {
      "stdDevMultiplier": 3.0,
      "minimumCount": 10,
      "description": "Unusual transaction frequency",
      "detects": "Abnormal frequency patterns"
    }
  },
  "createdBy": "userId"
}
```

**Response (200 OK):**

```json
{
  "id": "507f1f77bcf86cd799439011",
  "thresholds": {...},
  "createdAt": "2026-02-12T10:30:00Z",
  "createdBy": "userId"
}
```

---

#### Update Fraud Detection Settings

Updates fraud detection threshold configuration.

**Endpoint:** `PUT /api/fraud-detection/settings/{id}`

**Authentication Required:** Yes

**Permission Required:** `CAN_MANAGE_FRAUD_SETTINGS`

**Path Parameters:**
- `id` (string): Settings ID

**Request Body:** Same structure as Create Fraud Detection Settings

**Response (200 OK):** Updated settings object with `updatedAt` and `updatedBy` fields

---

## Permissions Reference

Below is the complete list of all 41 permissions used throughout the API:

### User Management (4)
- `CAN_CREATE_USER` - Create new users
- `CAN_VIEW_USER` - View user details
- `CAN_UPDATE_USER` - Update user information
- `CAN_DELETE_USER` - Delete users

### Role Management (5)
- `CAN_CREATE_ROLE` - Create new roles
- `CAN_VIEW_ROLE` - View role details
- `CAN_UPDATE_ROLE` - Update roles
- `CAN_DELETE_ROLE` - Delete roles
- `CAN_ASSIGN_ROLE` - Add or remove users to/from roles

### Permission Management (5)
- `CAN_CREATE_PERMISSION` - Create new permissions
- `CAN_VIEW_PERMISSION` - View permissions
- `CAN_UPDATE_PERMISSION` - Update permissions
- `CAN_DELETE_PERMISSION` - Delete permissions
- `CAN_ASSIGN_PERMISSION` - Add or remove permissions to/from roles

### Workspace Management (4)
- `CAN_CREATE_WORKSPACE` - Create workspaces
- `CAN_VIEW_WORKSPACE` - View workspaces
- `CAN_UPDATE_WORKSPACE` - Update workspaces
- `CAN_DELETE_WORKSPACE` - Delete workspaces

### Dashboard Management (5)
- `CAN_CREATE_DASHBOARD` - Create dashboards
- `CAN_VIEW_DASHBOARD` - View dashboards
- `CAN_UPDATE_DASHBOARD` - Update dashboards
- `CAN_DELETE_DASHBOARD` - Delete dashboards
- `CAN_MANAGE_DASHBOARDS` - Full dashboard management (create, view, update, delete)

### Data & Report Management (3)
- `CAN_CREATE_TRANSACTION` - Create transactions
- `CAN_VIEW_REPORT` - View reports and data
- `CAN_ANALYZE_DATA` - Run data analysis (distance, trends, etc.)

### Mapping Management (4)
- `CAN_CREATE_MAPPINGS` - Create field mappings
- `CAN_VIEW_MAPPINGS` - View field mappings
- `CAN_UPDATE_MAPPINGS` - Update field mappings
- `CAN_DELETE_MAPPINGS` - Delete field mappings

### Rule Management (5)
- `CAN_CREATE_RULE` - Create rules
- `CAN_VIEW_RULE` - View rules
- `CAN_UPDATE_RULE` - Update rules
- `CAN_DELETE_RULE` - Delete rules
- `CAN_APPLY_RULE` - Apply rules to data

### Data Ingestion Management (2)
- `CAN_MANAGE_DATA_INGESTION` - Configure and run data ingestion processes
- `CAN_VIEW_DATA_INGESTION` - View data ingestion configuration

### Notification Management (1)
- `CAN_MANAGE_NOTIFICATIONS` - Full notification management

### Group Management (1)
- `CAN_MANAGE_GROUPS` - Full group management

### Fraud Detection Settings (2)
- `CAN_VIEW_FRAUD_SETTINGS` - View fraud detection threshold settings
- `CAN_MANAGE_FRAUD_SETTINGS` - Create and update fraud detection settings

---

## Swagger/OpenAPI

The API includes Swagger/OpenAPI documentation for interactive API exploration and testing.

**Configuration:**
- **Title:** Loss Prevention API
- **Version:** v1.0
- **Framework:** FastEndpoints with Swagger integration

**Accessing Swagger UI:**

In development mode, Swagger UI is available at:

```
http://localhost:<port>/swagger
```

**Features:**
- Interactive API documentation
- Try-it-out functionality for testing endpoints
- Request/response schema visualization
- Authentication support (Bearer token)

**Swagger Document Endpoint:**

```
GET /swagger/v1/swagger.json
```

**Configuration in Program.cs:**

```csharp
builder.Services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Loss Prevention API";
            s.Version = "v1.0";
        };
    });

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerGen();
    app.UseSwaggerUi();
}
```

---

## Additional Notes

### CORS Configuration

The API is configured to allow requests from Vue.js development servers:

```
Allowed Origins:
- http://localhost:5173
- http://localhost:5174
```

CORS Policy: `AllowVueDev`
- Allows any header
- Allows any method
- Allows credentials

### Data Retention

Transaction data is retained for 180 days by default (configurable in appsettings.json).

### Caching

The API implements in-memory caching for improved performance on frequently accessed data:
- Report query results
- Mapping configurations

Cache keys are generated using hash of the request parameters.

### Background Services

The following background services run continuously:
- **DataIngestionBackgroundService**: Processes scheduled data ingestion tasks

### Database

**MongoDB Collections:**
- Users
- Roles
- Permissions
- Workspaces
- Dashboards
- ReportData
- Rules
- Mappings
- DataIngestionConfigurations
- DataIngestionSchedules
- Notifications
- Groups
- FraudDetectionSettings

### File Processing

The API supports multiple file formats for data ingestion:
- XML (XmlProcessingService)
- CSV (CsvProcessingService)
- JSON (JsonProcessingService)

File processing is coordinated through `IFileProcessingCoordinator`.

### SFTP Support

The API includes SFTP file processing capabilities (`SftpFileProcessingService`) for remote file ingestion.

---

## Support

For additional information, please refer to:
- [Executive Overview](01_EXECUTIVE_OVERVIEW.md)
- [System Architecture](02_SYSTEM_ARCHITECTURE.md)
- [Setup & Installation](03_SETUP_INSTALLATION.md)
- [Deployment Guide](04_DEPLOYMENT_GUIDE.md)
- [Fraud Detection API](FRAUD_DETECTION_API.md)
- [Distance Analysis Quick Start](DISTANCE_ANALYSIS_QUICK_START.md)

---

**Document Version:** 1.0  
**Last Updated:** February 12, 2026  
**API Version:** v1.0
