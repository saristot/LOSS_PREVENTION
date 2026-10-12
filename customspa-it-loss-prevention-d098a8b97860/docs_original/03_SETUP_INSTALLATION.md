# Loss Prevention Tool - Setup & Installation Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Database Setup](#database-setup)
3. [Backend Setup](#backend-setup)
4. [Frontend Setup](#frontend-setup)
5. [Data Ingestion Service Setup](#data-ingestion-service-setup)
6. [Initial Configuration](#initial-configuration)
7. [Verification](#verification)
8. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

#### Backend Development
- **Visual Studio 2022** (17.8 or later) OR **Visual Studio Code**
- **.NET 8 SDK** (version 8.0.0 or later)
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verify installation: `dotnet --version`

#### Frontend Development
- **Node.js** (version 18.0 or later, LTS recommended)
  - Download: https://nodejs.org/
  - Verify installation: `node --version`
- **npm** (comes with Node.js) or **yarn**
  - Verify: `npm --version`

#### Database
- **MongoDB** (version 6.0 or later)
  - Option 1: MongoDB Community Edition (local)
    - Download: https://www.mongodb.com/try/download/community
  - Option 2: MongoDB Atlas (cloud, free tier available)
    - Sign up: https://www.mongodb.com/cloud/atlas
  - Option 3: Docker
    ```bash
    docker run -d -p 27017:27017 --name mongodb mongo:latest
    ```
  - Verify: `mongo --version` or `mongosh --version`

### Optional Tools
- **MongoDB Compass** - GUI for database management
- **Postman** - API testing
- **Git** - Version control
- **Docker Desktop** - Containerization

### System Requirements

#### Minimum
- **OS**: Windows 10/11, Linux, macOS
- **RAM**: 8 GB
- **Storage**: 10 GB free space
- **CPU**: Dual-core processor

#### Recommended
- **RAM**: 16 GB or more
- **Storage**: 50 GB SSD
- **CPU**: Quad-core processor or better

## Database Setup

### Option 1: Local MongoDB Installation

#### Windows
1. Download MongoDB Community Server
2. Run installer with default settings
3. MongoDB installs as a Windows Service
4. Verify service is running:
   ```powershell
   Get-Service mongodb
   ```

#### Linux (Ubuntu)
```bash
# Import MongoDB GPG key
wget -qO - https://www.mongodb.org/static/pgp/server-7.0.asc | sudo apt-key add -

# Add MongoDB repository
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu jammy/mongodb-org/7.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-7.0.list

# Update and install
sudo apt-get update
sudo apt-get install -y mongodb-org

# Start service
sudo systemctl start mongod
sudo systemctl enable mongod
```

#### macOS
```bash
# Using Homebrew
brew tap mongodb/brew
brew install mongodb-community@7.0
brew services start mongodb-community@7.0
```

### Option 2: MongoDB Atlas (Cloud)

1. **Create Account**
   - Visit https://www.mongodb.com/cloud/atlas
   - Sign up for free tier

2. **Create Cluster**
   - Choose free tier (M0)
   - Select region closest to your users
   - Click "Create Cluster"

3. **Configure Access**
   - Database Access: Create database user
   - Network Access: Add IP address (0.0.0.0/0 for development)

4. **Get Connection String**
   - Click "Connect" on your cluster
   - Choose "Connect your application"
   - Copy connection string (looks like `mongodb+srv://...`)

### Initialize Database

1. **Connect to MongoDB**
   ```bash
   # Local
   mongosh

   # Atlas
   mongosh "mongodb+srv://your-cluster-url"
   ```

2. **Create Database and Collections**
   ```javascript
   // Switch to database
   use LossPrevention

   // Collections are auto-created, but you can pre-create them
   db.createCollection("Users")
   db.createCollection("Roles")
   db.createCollection("Permissions")
   db.createCollection("ReportData")
   db.createCollection("Workspaces")
   db.createCollection("Dashboards")
   db.createCollection("Mappings")
   db.createCollection("Rules")
   db.createCollection("Notifications")
   db.createCollection("Groups")
   db.createCollection("DataIngestionConfigurations")
   db.createCollection("FraudDetectionSettings")
   ```

3. **Run Initialization Scripts**

   Navigate to `LossPrevention.API/MongoDBScripts/` and run scripts in order:

   ```bash
   # Clean up any existing permissions (if re-installing)
   mongosh < 00_CleanupDuplicatePermissions.js

   # Create all permissions
   mongosh < 01_CreatePermissions.js

   # Create admin role with all permissions
   mongosh < 02_AddPermissionsToAdminRole.js
   ```

4. **Create Admin User** (via MongoDB shell)
   ```javascript
   use LossPrevention

   // Find the Admin role ID
   const adminRole = db.Roles.findOne({ RoleName: "Administrator" })

   // Create admin user (password: Admin@123)
   db.Users.insertOne({
     Username: "admin",
     Email: "admin@lossprevention.local",
     FirstName: "System",
     LastName: "Administrator",
     PasswordHash: "$2a$11$xJ4v5Y8k9ZqX.wYQ7V4Xp.YQhM8fZ8vF7qHR5kL9xB3mN6pO8rT2W", // Admin@123
     PasswordSalt: "generated-salt",
     Roles: [adminRole._id],
     LockField: null,
     LockValue: null,
     RegistrationDate: new Date()
   })
   ```

   **Note**: In production, you should create a user via the API after setup, which will properly hash the password.

## Backend Setup

### 1. Clone Repository
```bash
git clone https://your-repo-url/LossPreventionTool.git
cd LossPreventionTool
```

### 2. Configure API Settings

Edit `LossPrevention.API/appsettings.json`:

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
    "ConnectionString": "mongodb://localhost:27017",  // Change if using Atlas
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
    "SecretKey": "YOUR-SECRET-KEY-CHANGE-THIS-IN-PRODUCTION-MIN-32-CHARS",
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

**Important Configuration Notes**:

- **MongoDB ConnectionString**: 
  - Local: `mongodb://localhost:27017`
  - Atlas: `mongodb+srv://username:password@cluster.mongodb.net`
  
- **JwtSettings.SecretKey**: 
  - Generate a strong secret (minimum 32 characters)
  - Keep this secret secure!
  - Change default value before production

- **Email Settings**:
  - For development, use a local SMTP server like [Papercut](https://github.com/ChangemakerStudios/Papercut-SMTP)
  - For production, use your organization's SMTP server

### 3. Restore NuGet Packages

```bash
cd LossPrevention.API
dotnet restore
```

### 4. Build the Solution

```bash
# From solution root
dotnet build LossPrevention.sln
```

### 5. Run the API

#### Option A: Visual Studio
1. Open `LossPrevention.sln`
2. Set `LossPrevention.API` as startup project
3. Press F5 or click "Start Debugging"
4. API will start on `https://localhost:7110`

#### Option B: Command Line
```bash
cd LossPrevention.API
dotnet run
```

#### Option C: Watch Mode (auto-restart on code changes)
```bash
cd LossPrevention.API
dotnet watch run
```

### 6. Verify API is Running

- Swagger UI: https://localhost:7110/swagger
- Health check: https://localhost:7110/api/health (if implemented)

## Frontend Setup

### 1. Navigate to UI Project
```bash
cd LossPrevention.UI
```

### 2. Install Dependencies
```bash
npm install
```

If you encounter errors, try:
```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install
```

### 3. Configure API Endpoint

Edit `src/api/api.ts` (if not already configured):

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7110/api',  // Adjust port if needed
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

### 4. Start Development Server

```bash
npm run dev
```

The UI will start on `http://localhost:5173` (Vite default)

### 5. Build for Production

```bash
npm run build
```

Output will be in `dist/` directory.

## Data Ingestion Service Setup

### 1. Configure Service

Edit `LossPrevention.DataIngestionService/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "LossPrevention",
    "CollectionName_ReportData": "ReportData",
    "CollectionName_RulesConfiguration": "Rules",
    "CollectionName_MappingConfiguration": "Mappings"
  }
}
```

### 2. Configure Data Source

In `Program.cs`, update the file path:

```csharp
// Line ~74
var filenames = Directory.GetFiles(@"C:\xmlstore5\xml");  // Change this path
```

Or use SFTP configuration via the UI (Manage > Data Ingestion).

### 3. Run Manually (for testing)

```bash
cd LossPrevention.DataIngestionService
dotnet run
```

### 4. Install as Windows Service (Production)

```powershell
# Publish service
dotnet publish -c Release -o C:\Services\LossPreventionIngestion

# Create Windows Service
sc create LossPreventionIngestion binPath="C:\Services\LossPreventionIngestion\LossPrevention.DataIngestionService.exe"

# Start service
sc start LossPreventionIngestion
```

### 5. Configure Scheduled Task (Alternative)

1. Open Task Scheduler
2. Create new task
3. Trigger: Daily or custom schedule
4. Action: Start program `dotnet`
5. Arguments: `run --project "path\to\LossPrevention.DataIngestionService.csproj"`

## Initial Configuration

### 1. Login to Application

1. Navigate to `http://localhost:5173`
2. Login with admin credentials:
   - Username: `admin`
   - Password: `Admin@123`

### 2. Create Mappings

1. Go to **Settings > Manage Mappings**
2. Click **Add Mapping**
3. Define your data fields:

   Example for transaction data:
   ```
   Name: TransactionID
   Alias: TxnID
   DataType: String
   
   Name: Amount
   Alias: Amt
   DataType: Decimal128
   
   Name: CustomerName
   Alias: Customer
   DataType: String
   
   Name: TransactionDate
   Alias: TxnDate
   DataType: Date
   ```

### 3. Create Workspaces

1. Go to **Workspaces**
2. Click **Create Workspace**
3. Add tabs for different report types
4. Configure queries using the query builder

### 4. Configure Fraud Detection

1. Go to **Settings > Fraud Detection**
2. Configure thresholds:
   - High Value Transactions
   - Extreme Values
   - Low Value Padding
   - Refund Fraud
   - Void Fraud

### 5. Create Users and Roles

1. Go to **Manage > Users**
2. Create additional users
3. Assign roles based on responsibilities
4. Configure field-level locks if needed

### 6. Set Up Notifications

1. Go to **Manage > Groups**
2. Create user groups (e.g., "Store Managers", "Loss Prevention Team")
3. Configure notification rules

## Verification

### Test Backend
```bash
# Check API health
curl https://localhost:7110/swagger

# Test login
curl -X POST https://localhost:7110/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

### Test Frontend
1. Open browser to `http://localhost:5173`
2. Login successfully
3. Navigate to different sections
4. Check browser console for errors (F12)

### Test Database
```javascript
// Connect to MongoDB
mongosh

use LossPrevention

// Check collections exist
show collections

// Check admin user exists
db.Users.findOne({Username: "admin"})

// Check permissions exist
db.Permissions.countDocuments()  // Should be 41+
```

### Test Data Ingestion
1. Place a test file in configured directory
2. Run data ingestion service
3. Check MongoDB ReportData collection
4. Verify records appear in UI

## Troubleshooting

### API Won't Start

**Problem**: Port already in use
```
Solution: Change port in launchSettings.json
- Navigate to: LossPrevention.API/Properties/launchSettings.json
- Change "applicationUrl" to different port
```

**Problem**: MongoDB connection fails
```
Error: MongoConnectionException
Solution: 
- Verify MongoDB is running: mongosh
- Check connection string in appsettings.json
- Check firewall rules
- For Atlas: Verify IP whitelist
```

**Problem**: Missing JWT secret
```
Error: JWT bearer authentication failed
Solution: Ensure JwtSettings.SecretKey is set in appsettings.json
```

### UI Won't Start

**Problem**: npm install fails
```bash
# Clear cache
npm cache clean --force

# Delete and reinstall
rm -rf node_modules package-lock.json
npm install
```

**Problem**: API calls fail (CORS)
```
Error: CORS policy blocked
Solution: Verify CORS settings in API Program.cs
- Ensure AllowVueDev policy includes your UI URL
- Check UI is running on allowed origin (http://localhost:5173)
```

**Problem**: White screen / blank page
```
Solution:
- Check browser console (F12) for errors
- Verify API is running and accessible
- Check network tab for failed requests
- Ensure token is not expired
```

### Database Issues

**Problem**: Collections not created
```bash
# Manually create collections
mongosh
use LossPrevention
db.createCollection("Users")
# ... create other collections
```

**Problem**: Permission denied
```
Solution:
- Check MongoDB user has read/write permissions
- For Atlas: Verify database user configuration
```

**Problem**: Data not appearing in UI
```
Solution:
- Check data exists: db.ReportData.countDocuments()
- Verify mappings are configured
- Check query syntax in workspace
- Review browser console for errors
```

### Data Ingestion Issues

**Problem**: Files not processing
```
Solution:
- Verify file path in Program.cs
- Check file permissions
- Verify file format (XML, CSV, JSON)
- Check logs for errors
```

**Problem**: Data not inserting to DB
```
Solution:
- Verify MongoDB connection
- Check mapping configuration
- Review error logs
- Test with smaller file
```

## Next Steps

After successful installation:

1. **Read the User Manual**: [11_USER_MANUAL.md](11_USER_MANUAL.md)
2. **Configure for Production**: [04_DEPLOYMENT_GUIDE.md](04_DEPLOYMENT_GUIDE.md)
3. **Set up Security**: [09_SECURITY_DOCUMENTATION.md](09_SECURITY_DOCUMENTATION.md)
4. **Import Sample Data**: Load test data to familiarize yourself
5. **Create Training Materials**: Document your organization's specific workflows

## Getting Help

- **Documentation**: Check other docs in `/docs` folder
- **Logs**: Check application logs for detailed errors
- **MongoDB Logs**: Check MongoDB logs for database issues
- **Browser Console**: F12 to view frontend errors
- **API Swagger**: Test endpoints directly via Swagger UI

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Support Contact**: [Your support email]
