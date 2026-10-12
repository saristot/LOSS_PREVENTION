# Loss Prevention Tool - Deployment Guide

## Table of Contents
1. [Deployment Options](#deployment-options)
2. [Azure Cloud Deployment](#azure-cloud-deployment)
3. [On-Premise Deployment](#on-premise-deployment)
4. [Docker Deployment](#docker-deployment)
5. [Environment Configuration](#environment-configuration)
6. [CI/CD Pipeline](#cicd-pipeline)
7. [Post-Deployment](#post-deployment)
8. [Monitoring & Maintenance](#monitoring--maintenance)

## Deployment Options

### Comparison Matrix

| Criteria | Azure Cloud | On-Premise | Docker/Kubernetes |
|----------|-------------|------------|-------------------|
| **Scalability** | Excellent (auto-scale) | Manual scaling | Good (orchestration) |
| **Cost** | Pay-as-you-go | High upfront | Medium |
| **Maintenance** | Low (managed) | High | Medium |
| **Control** | Medium | Full control | High |
| **Setup Time** | Fast (hours) | Slow (days/weeks) | Fast (hours) |
| **Security** | Shared responsibility | Full responsibility | Full responsibility |
| **Recommended For** | SaaS, Multi-tenant | Enterprise, Sensitive data | Hybrid, Multi-cloud |

## Azure Cloud Deployment

### Architecture

```
Internet
    │
    ├─── Azure Front Door (CDN + WAF)
    │
    ├─── Azure Static Web Apps (Vue.js UI)
    │         │
    │         └─── Vite Build (Static files)
    │
    └─── Azure Functions / App Service (API)
              │
              ├─── Application Insights (Monitoring)
              │
              └─── Azure Cosmos DB (MongoDB API)
                   OR
              └─── MongoDB Atlas
```

### Prerequisites

- Azure subscription
- Azure CLI installed
- .NET 8 SDK
- Node.js and npm

### Step 1: Create Azure Resources

#### Create Resource Group
```bash
az login

az group create \
  --name rg-lossprevention-prod \
  --location eastus
```

#### Create Azure Cosmos DB (MongoDB API)
```bash
az cosmosdb create \
  --name cosmos-lossprevention-prod \
  --resource-group rg-lossprevention-prod \
  --kind MongoDB \
  --server-version 7.0 \
  --default-consistency-level Session \
  --locations regionName=eastus failoverPriority=0 isZoneRedundant=False
```

**Alternative: Use MongoDB Atlas**
- Sign up at mongodb.com/cloud/atlas
- Create M10 or higher cluster for production
- Enable VNet peering with Azure

#### Create Key Vault (for secrets)
```bash
az keyvault create \
  --name kv-lossprevention-prod \
  --resource-group rg-lossprevention-prod \
  --location eastus
```

#### Store Secrets
```bash
# Get Cosmos DB connection string
COSMOS_CONN=$(az cosmosdb keys list \
  --name cosmos-lossprevention-prod \
  --resource-group rg-lossprevention-prod \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" -o tsv)

# Store in Key Vault
az keyvault secret set \
  --vault-name kv-lossprevention-prod \
  --name MongoDbConnectionString \
  --value "$COSMOS_CONN"

# Store JWT Secret
az keyvault secret set \
  --vault-name kv-lossprevention-prod \
  --name JwtSecretKey \
  --value "$(openssl rand -base64 32)"
```

### Step 2: Deploy Backend (Azure Functions)

#### Install Azure Functions Core Tools
```bash
npm install -g azure-functions-core-tools@4
```

#### Create Function App
```bash
az functionapp create \
  --name func-lossprevention-api-prod \
  --resource-group rg-lossprevention-prod \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4 \
  --storage-account stlossprevprod
```

#### Configure App Settings
```bash
az functionapp config appsettings set \
  --name func-lossprevention-api-prod \
  --resource-group rg-lossprevention-prod \
  --settings \
    "MongoDbSettings__ConnectionString=@Microsoft.KeyVault(SecretUri=https://kv-lossprevention-prod.vault.azure.net/secrets/MongoDbConnectionString/)" \
    "MongoDbSettings__DatabaseName=LossPrevention" \
    "JwtSettings__SecretKey=@Microsoft.KeyVault(SecretUri=https://kv-lossprevention-prod.vault.azure.net/secrets/JwtSecretKey/)" \
    "JwtSettings__Issuer=LossPrevention" \
    "JwtSettings__Audience=User" \
    "JwtSettings__ExpiryHours=1"
```

#### Deploy API
```bash
cd LossPrevention.API

# Publish
dotnet publish -c Release

# Deploy
func azure functionapp publish func-lossprevention-api-prod
```

**Alternative: Use Azure App Service**
```bash
az webapp create \
  --name app-lossprevention-api-prod \
  --resource-group rg-lossprevention-prod \
  --plan asp-lossprevention-prod \
  --runtime "DOTNET|8.0"

# Deploy
az webapp deployment source config-zip \
  --name app-lossprevention-api-prod \
  --resource-group rg-lossprevention-prod \
  --src publish.zip
```

### Step 3: Deploy Frontend (Azure Static Web Apps)

#### Create Static Web App
```bash
az staticwebapp create \
  --name swa-lossprevention-ui-prod \
  --resource-group rg-lossprevention-prod \
  --location eastus \
  --sku Standard
```

#### Build Frontend
```bash
cd LossPrevention.UI

# Update API endpoint in .env.production
echo "VITE_API_URL=https://func-lossprevention-api-prod.azurewebsites.net/api" > .env.production

# Build
npm run build
```

#### Deploy
```bash
# Get deployment token
DEPLOY_TOKEN=$(az staticwebapp secrets list \
  --name swa-lossprevention-ui-prod \
  --resource-group rg-lossprevention-prod \
  --query "properties.apiKey" -o tsv)

# Deploy using SWA CLI
npm install -g @azure/static-web-apps-cli
swa deploy ./dist \
  --deployment-token $DEPLOY_TOKEN \
  --app-name swa-lossprevention-ui-prod
```

### Step 4: Configure Custom Domain & SSL

```bash
# Add custom domain
az staticwebapp hostname set \
  --name swa-lossprevention-ui-prod \
  --resource-group rg-lossprevention-prod \
  --hostname lossprevention.yourdomain.com

# SSL certificate is auto-provisioned by Azure
```

### Step 5: Enable Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app ai-lossprevention-prod \
  --resource-group rg-lossprevention-prod \
  --location eastus

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app ai-lossprevention-prod \
  --resource-group rg-lossprevention-prod \
  --query "instrumentationKey" -o tsv)

# Add to Function App
az functionapp config appsettings set \
  --name func-lossprevention-api-prod \
  --resource-group rg-lossprevention-prod \
  --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY"
```

## On-Premise Deployment

### Server Requirements

#### Minimum Specs
- **OS**: Windows Server 2019+ or Linux (Ubuntu 20.04+)
- **CPU**: 4 cores
- **RAM**: 16 GB
- **Storage**: 100 GB SSD
- **Network**: 100 Mbps

#### Recommended Specs (Production)
- **OS**: Windows Server 2022 or Ubuntu 22.04 LTS
- **CPU**: 8+ cores
- **RAM**: 32 GB+
- **Storage**: 500 GB SSD (NVMe preferred)
- **Network**: 1 Gbps

### Step 1: Install Prerequisites

#### Windows Server

```powershell
# Install .NET 8 Runtime
winget install Microsoft.DotNet.Runtime.8

# Install IIS
Install-WindowsFeature -name Web-Server -IncludeManagementTools

# Install URL Rewrite Module
choco install urlrewrite

# Install Application Request Routing (for reverse proxy)
choco install iis-arr
```

#### Linux (Ubuntu)

```bash
# Install .NET 8 Runtime
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-8.0

# Install Nginx
sudo apt-get install -y nginx

# Install MongoDB
wget -qO - https://www.mongodb.org/static/pgp/server-7.0.asc | sudo apt-key add -
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu jammy/mongodb-org/7.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-7.0.list
sudo apt-get update
sudo apt-get install -y mongodb-org
sudo systemctl start mongod
sudo systemctl enable mongod
```

### Step 2: Deploy Backend

#### Windows (IIS)

1. **Publish API**
```powershell
cd LossPrevention.API
dotnet publish -c Release -o C:\inetpub\wwwroot\LossPreventionAPI
```

2. **Create IIS Site**
```powershell
# Import IIS module
Import-Module WebAdministration

# Create App Pool
New-WebAppPool -Name "LossPreventionAPI" 
Set-ItemProperty IIS:\AppPools\LossPreventionAPI -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\LossPreventionAPI -Name startMode -Value AlwaysRunning

# Create Website
New-Website -Name "LossPreventionAPI" `
  -Port 5000 `
  -PhysicalPath "C:\inetpub\wwwroot\LossPreventionAPI" `
  -ApplicationPool "LossPreventionAPI"

# Configure web.config
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\LossPrevention.API.dll" 
                stdoutLogEnabled="true" 
                stdoutLogFile=".\logs\stdout"
                hostingModel="InProcess" />
  </system.webServer>
</configuration>
"@ | Out-File C:\inetpub\wwwroot\LossPreventionAPI\web.config
```

3. **Configure HTTPS**
```powershell
# Import certificate
$cert = Import-PfxCertificate -FilePath "C:\certs\lossprevention.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password (ConvertTo-SecureString -String "password" -AsPlainText -Force)

# Bind to site
New-WebBinding -Name "LossPreventionAPI" -Protocol https -Port 443
$binding = Get-WebBinding -Name "LossPreventionAPI" -Protocol https
$binding.AddSslCertificate($cert.Thumbprint, "my")
```

#### Linux (systemd + Nginx)

1. **Deploy API**
```bash
# Publish
cd LossPrevention.API
dotnet publish -c Release -o /var/www/lossprevention-api

# Set permissions
sudo chown -R www-data:www-data /var/www/lossprevention-api
```

2. **Create systemd Service**
```bash
sudo nano /etc/systemd/system/lossprevention-api.service
```

```ini
[Unit]
Description=Loss Prevention API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/lossprevention-api
ExecStart=/usr/bin/dotnet /var/www/lossprevention-api/LossPrevention.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=lossprevention-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
# Enable and start service
sudo systemctl enable lossprevention-api
sudo systemctl start lossprevention-api
```

3. **Configure Nginx Reverse Proxy**
```bash
sudo nano /etc/nginx/sites-available/lossprevention
```

```nginx
upstream lossprevention_api {
    server localhost:5000;
}

server {
    listen 80;
    listen [::]:80;
    server_name lossprevention.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name lossprevention.yourdomain.com;

    ssl_certificate /etc/ssl/certs/lossprevention.crt;
    ssl_certificate_key /etc/ssl/private/lossprevention.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    # API
    location /api/ {
        proxy_pass http://lossprevention_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # UI
    location / {
        root /var/www/lossprevention-ui;
        try_files $uri $uri/ /index.html;
        
        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
```

```bash
# Enable site
sudo ln -s /etc/nginx/sites-available/lossprevention /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### Step 3: Deploy Frontend

#### Build Frontend
```bash
cd LossPrevention.UI

# Update API URL
echo "VITE_API_URL=https://lossprevention.yourdomain.com/api" > .env.production

# Build
npm run build
```

#### Deploy Static Files

**Windows (IIS)**
```powershell
# Copy files
Copy-Item -Path .\dist\* -Destination C:\inetpub\wwwroot\LossPreventionUI -Recurse

# Create IIS site
New-Website -Name "LossPreventionUI" `
  -Port 80 `
  -PhysicalPath "C:\inetpub\wwwroot\LossPreventionUI"
```

**Linux (Nginx)**
```bash
sudo cp -r dist/* /var/www/lossprevention-ui/
sudo chown -R www-data:www-data /var/www/lossprevention-ui
```

### Step 4: Configure MongoDB

#### Replica Set (High Availability)

```bash
# On each MongoDB node
sudo nano /etc/mongod.conf
```

```yaml
replication:
  replSetName: "rs0"

net:
  bindIp: 0.0.0.0  # Listen on all IPs (use firewall to restrict)
  port: 27017
```

```bash
# Restart MongoDB
sudo systemctl restart mongod

# On primary node, initiate replica set
mongosh
```

```javascript
rs.initiate({
  _id: "rs0",
  members: [
    { _id: 0, host: "mongodb1.yourdomain.com:27017" },
    { _id: 1, host: "mongodb2.yourdomain.com:27017" },
    { _id: 2, host: "mongodb3.yourdomain.com:27017" }
  ]
})
```

#### Enable Authentication

```javascript
use admin
db.createUser({
  user: "admin",
  pwd: "strongPassword",
  roles: [ { role: "root", db: "admin" } ]
})

db.createUser({
  user: "lossprevention_app",
  pwd: "appPassword",
  roles: [ 
    { role: "readWrite", db: "LossPrevention" },
    { role: "dbAdmin", db: "LossPrevention" }
  ]
})
```

```bash
# Enable auth in config
sudo nano /etc/mongod.conf
```

```yaml
security:
  authorization: enabled
```

```bash
sudo systemctl restart mongod
```

Update connection string in appsettings.json:
```json
"ConnectionString": "mongodb://lossprevention_app:appPassword@mongodb1.yourdomain.com:27017,mongodb2.yourdomain.com:27017,mongodb3.yourdomain.com:27017/LossPrevention?replicaSet=rs0&authSource=LossPrevention"
```

## Docker Deployment

### Create Dockerfiles

#### Backend Dockerfile
```dockerfile
# LossPrevention.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LossPrevention.API/01. LossPrevention.API.csproj", "LossPrevention.API/"]
COPY ["LossPrevention.Application/LossPrevention.Application.csproj", "LossPrevention.Application/"]
COPY ["LossPrevention.Domain/LossPrevention.Domain.csproj", "LossPrevention.Domain/"]
COPY ["LossPrevention.Infrastructure/LossPrevention.Infrastructure.csproj", "LossPrevention.Infrastructure/"]
RUN dotnet restore "LossPrevention.API/01. LossPrevention.API.csproj"
COPY . .
WORKDIR "/src/LossPrevention.API"
RUN dotnet build "01. LossPrevention.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "01. LossPrevention.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LossPrevention.API.dll"]
```

#### Frontend Dockerfile
```dockerfile
# LossPrevention.UI/Dockerfile
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  mongodb:
    image: mongo:7
    container_name: lossprevention-mongodb
    restart: always
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: ${MONGO_ROOT_PASSWORD}
      MONGO_INITDB_DATABASE: LossPrevention
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
      - ./mongo-init.js:/docker-entrypoint-initdb.d/mongo-init.js:ro
    networks:
      - lossprevention-network

  api:
    build:
      context: .
      dockerfile: LossPrevention.API/Dockerfile
    container_name: lossprevention-api
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MongoDbSettings__ConnectionString=mongodb://admin:${MONGO_ROOT_PASSWORD}@mongodb:27017/LossPrevention?authSource=admin
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
    ports:
      - "5000:80"
    depends_on:
      - mongodb
    networks:
      - lossprevention-network

  ui:
    build:
      context: LossPrevention.UI
      dockerfile: Dockerfile
    container_name: lossprevention-ui
    restart: always
    ports:
      - "80:80"
      - "443:443"
    depends_on:
      - api
    networks:
      - lossprevention-network

volumes:
  mongodb_data:
    driver: local

networks:
  lossprevention-network:
    driver: bridge
```

### Deploy with Docker Compose

```bash
# Create .env file
cat > .env << EOF
MONGO_ROOT_PASSWORD=strongMongoPassword
JWT_SECRET_KEY=$(openssl rand -base64 32)
EOF

# Build and start
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

## Environment Configuration

### appsettings.json by Environment

#### Development
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017"
  },
  "JwtSettings": {
    "ExpiryHours": 8
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "25"
  }
}
```

#### Production
```json
{
  "MongoDbSettings": {
    "ConnectionString": "@Microsoft.KeyVault(...) or env variable"
  },
  "JwtSettings": {
    "ExpiryHours": 1
  },
  "Email": {
    "SmtpHost": "smtp.production.com",
    "SmtpPort": "587",
    "UseSsl": true
  },
  "DataRetention": {
    "TransactionRetentionDays": 365
  }
}
```

### Environment Variables

```bash
# Set in Azure or Docker
export MongoDbSettings__ConnectionString="mongodb://..."
export JwtSettings__SecretKey="..."
export JwtSettings__ExpiryHours="1"
```

## CI/CD Pipeline

### Azure DevOps Pipeline

```yaml
# azure-pipelines.yml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

stages:
- stage: Build
  jobs:
  - job: BuildBackend
    steps:
    - task: UseDotNet@2
      inputs:
        version: '8.x'
    
    - script: dotnet restore
      displayName: 'Restore packages'
    
    - script: dotnet build --configuration $(buildConfiguration)
      displayName: 'Build solution'
    
    - script: dotnet publish LossPrevention.API -c Release -o $(Build.ArtifactStagingDirectory)/api
      displayName: 'Publish API'
    
    - task: PublishBuildArtifacts@1
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)/api'
        ArtifactName: 'api'

  - job: BuildFrontend
    steps:
    - task: NodeTool@0
      inputs:
        versionSpec: '18.x'
    
    - script: |
        cd LossPrevention.UI
        npm ci
        npm run build
      displayName: 'Build UI'
    
    - task: PublishBuildArtifacts@1
      inputs:
        PathtoPublish: 'LossPrevention.UI/dist'
        ArtifactName: 'ui'

- stage: Deploy
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - deployment: DeployToProduction
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureFunctionApp@1
            inputs:
              azureSubscription: 'Azure-Subscription'
              appType: 'functionApp'
              appName: 'func-lossprevention-api-prod'
              package: '$(Pipeline.Workspace)/api'
          
          - task: AzureStaticWebApp@0
            inputs:
              app_location: '$(Pipeline.Workspace)/ui'
              azure_static_web_apps_api_token: $(DEPLOY_TOKEN)
```

### GitHub Actions

```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
    
    - name: Build Backend
      run: |
        dotnet restore
        dotnet publish LossPrevention.API -c Release -o ./publish/api
    
    - name: Build Frontend
      run: |
        cd LossPrevention.UI
        npm ci
        npm run build
    
    - name: Deploy to Azure
      run: |
        # Deploy API
        az functionapp deployment source config-zip \
          --resource-group ${{ secrets.AZURE_RG }} \
          --name ${{ secrets.AZURE_FUNCTION_NAME }} \
          --src ./publish/api.zip
        
        # Deploy UI
        swa deploy ./LossPrevention.UI/dist \
          --deployment-token ${{ secrets.SWA_TOKEN }}
```

## Post-Deployment

### 1. Smoke Tests

```bash
# Test API health
curl https://lossprevention.yourdomain.com/api/swagger

# Test login
curl -X POST https://lossprevention.yourdomain.com/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'

# Test UI
curl https://lossprevention.yourdomain.com
```

### 2. Load Testing

```bash
# Using Apache Bench
ab -n 1000 -c 10 https://lossprevention.yourdomain.com/api/workspaces

# Using Artillery
npm install -g artillery
artillery quick --count 10 --num 50 https://lossprevention.yourdomain.com
```

### 3. Security Scan

```bash
# SSL/TLS check
sslscan lossprevention.yourdomain.com

# OWASP ZAP scan
docker run -t owasp/zap2docker-stable zap-baseline.py \
  -t https://lossprevention.yourdomain.com
```

## Monitoring & Maintenance

### Azure Application Insights

```csharp
// Already configured via appsettings
// View in Azure Portal > Application Insights
// - Live Metrics
// - Performance
// - Failures
// - Users
```

### Custom Logging

```bash
# View logs
# Azure Functions
az functionapp log tail --name func-lossprevention-api-prod --resource-group rg-lossprevention-prod

# On-Premise (Linux)
sudo journalctl -u lossprevention-api -f

# Docker
docker logs -f lossprevention-api
```

### Database Maintenance

```javascript
// MongoDB maintenance
use LossPrevention

// Check index usage
db.ReportData.aggregate([{$indexStats:{}}])

// Compact collections
db.runCommand({compact: 'ReportData'})

// Check replication lag
rs.printSlaveReplicationInfo()
```

### Backups

```bash
# MongoDB backup (automated)
mongodump --uri="mongodb://..." --out=/backups/$(date +%Y%m%d)

# Restore
mongorestore --uri="mongodb://..." /backups/20260212
```

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Next Review**: Monthly
