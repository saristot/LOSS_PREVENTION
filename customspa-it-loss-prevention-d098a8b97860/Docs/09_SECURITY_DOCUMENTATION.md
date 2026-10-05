# Loss Prevention Tool - Security Documentation

## Table of Contents
1. [Security Overview](#security-overview)
2. [Authentication](#authentication)
3. [Authorization](#authorization)
4. [Data Security](#data-security)
5. [Network Security](#network-security)
6. [Application Security](#application-security)
7. [Security Best Practices](#security-best-practices)
8. [Compliance & Auditing](#compliance--auditing)
9. [Incident Response](#incident-response)
10. [Security Checklist](#security-checklist)

## Security Overview

The Loss Prevention Tool implements a **defense-in-depth** security strategy with multiple layers of protection:

```
┌─────────────────────────────────────────────────┐
│         Network Layer (HTTPS/TLS)               │
├─────────────────────────────────────────────────┤
│         Application Layer (JWT + RBAC)          │
├─────────────────────────────────────────────────┤
│         Business Logic Layer (Validation)        │
├─────────────────────────────────────────────────┤
│         Data Layer (Encryption + Access Control) │
└─────────────────────────────────────────────────┘
```

### Security Principles

1. **Least Privilege**: Users granted minimum necessary permissions
2. **Defense in Depth**: Multiple security layers
3. **Fail Secure**: Systems fail to secure state
4. **Security by Design**: Security considered from architecture phase
5. **Separation of Duties**: No single user has excessive control

## Authentication

### JWT (JSON Web Token) Authentication

#### Token Structure
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "Name": "admin",
    "Permissions": ["CAN_VIEW_DATA", "CAN_CREATE_USER", ...],
    "LockField": "StoreID",
    "LockValue": "STORE001",
    "iss": "LossPrevention",
    "aud": "User",
    "exp": 1709395200
  },
  "signature": "HMACSHA256(...)"
}
```

#### Token Lifecycle

```
1. User Login
   ├─ POST /api/users/login
   ├─ Validate credentials (username + password)
   ├─ Verify user is active
   ├─ Retrieve roles and permissions
   └─ Generate JWT token (default: 1 hour expiry)

2. Token Usage
   ├─ Client stores token (localStorage)
   ├─ Include in Authorization header: Bearer {token}
   ├─ API validates signature and expiry
   └─ Extract claims for authorization

3. Token Expiry
   ├─ Automatic expiry after configured hours
   ├─ Client detects 401 Unauthorized
   └─ Redirect to login page
```

#### Configuration

**Production Settings** (appsettings.json):
```json
{
  "JwtSettings": {
    "SecretKey": "USE-STRONG-SECRET-KEY-MINIMUM-32-CHARACTERS-CHANGE-THIS",
    "Issuer": "LossPrevention",
    "Audience": "User",
    "ExpiryHours": 1
  }
}
```

**Important**: 
- Secret key MUST be at least 32 characters
- Use a cryptographically secure random generator
- Store in Azure Key Vault or environment variables (never in code)
- Rotate keys periodically (every 90 days recommended)

#### Password Security

**Password Requirements**:
- Minimum length: 8 characters
- Must contain: uppercase, lowercase, number, special character
- Password history: Previous 5 passwords rejected
- Account lockout: 5 failed attempts = 15-minute lockout

**Password Hashing**:
```csharp
// Implementation uses PBKDF2 with salt
public string HashPassword(string password)
{
    using var rng = new RNGCryptoServiceProvider();
    byte[] salt = new byte[128 / 8];
    rng.GetBytes(salt);
    
    string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: password,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));
    
    return $"{Convert.ToBase64String(salt)}:{hash}";
}
```

**Password Reset Flow**:
```
1. User requests reset → POST /api/users/forgot-password
2. System generates secure token (valid 1 hour)
3. Email sent with reset link
4. User clicks link → GET /reset-password?token={token}
5. User enters new password → POST /api/users/reset-password
6. Token validated and invalidated
7. Password updated with new hash
```

**Security Features**:
- Reset tokens expire after 1 hour
- Tokens are single-use only
- Old password cannot be reused
- Account lockout on too many reset attempts

## Authorization

### Role-Based Access Control (RBAC)

#### Permission System

**41 Granular Permissions** organized by category:

| Category | Permissions | Description |
|----------|-------------|-------------|
| User Management | CAN_CREATE_USER, CAN_UPDATE_USER, CAN_DELETE_USER, CAN_VIEW_USER | User CRUD operations |
| Role Management | CAN_CREATE_ROLE, CAN_UPDATE_ROLE, CAN_DELETE_ROLE, CAN_VIEW_ROLE, CAN_ASSIGN_ROLE | Role management |
| Permission Management | CAN_CREATE_PERMISSION, CAN_UPDATE_PERMISSION, CAN_DELETE_PERMISSION, CAN_VIEW_PERMISSION, CAN_ASSIGN_PERMISSION | Permission admin |
| Workspace | CAN_CREATE_WORKSPACE, CAN_UPDATE_WORKSPACE, CAN_DELETE_WORKSPACE, CAN_VIEW_WORKSPACE | Workspace operations |
| Dashboard | CAN_CREATE_DASHBOARD, CAN_UPDATE_DASHBOARD, CAN_DELETE_DASHBOARD, CAN_VIEW_DASHBOARD, CAN_MANAGE_DASHBOARDS | Dashboard operations |
| Data | CAN_CREATE_TRANSACTION, CAN_ANALYZE_DATA, CAN_VIEW_REPORT | Data operations |
| Mapping | CAN_CREATE_MAPPINGS, CAN_UPDATE_MAPPINGS, CAN_DELETE_MAPPINGS, CAN_VIEW_MAPPINGS | Field mappings |
| Rules | CAN_CREATE_RULE, CAN_UPDATE_RULE, CAN_DELETE_RULE, CAN_APPLY_RULE, CAN_VIEW_RULE | Business rules |
| Data Ingestion | CAN_MANAGE_DATA_INGESTION, CAN_VIEW_DATA_INGESTION | Import management |
| Notifications | CAN_MANAGE_NOTIFICATIONS | Notification admin |
| Groups | CAN_MANAGE_GROUPS | Group management |
| Fraud Detection | CAN_MANAGE_FRAUD_SETTINGS, CAN_VIEW_FRAUD_SETTINGS | Fraud configuration |

#### Permission Enforcement

**API Level** (FastEndpoints):
```csharp
public override void Configure()
{
    Get("/users/{Id}");
    Permissions("CAN_VIEW_USER");  // Only users with this permission can access
    Description(b => b.WithName("GetUser").Produces<UserDTO>(200));
}
```

**UI Level** (Vue.js):
```typescript
// Check permission before showing UI element
const canManageUsers = computed(() => 
  hasPermission('CAN_CREATE_USER') || 
  hasPermission('CAN_UPDATE_USER')
);
```

#### Field-Level Security

**Data Locking**: Restrict users to specific data subsets

```json
// User locked to specific store
{
  "Username": "store_manager_001",
  "LockField": "StoreID",
  "LockValue": "STORE001"
}
```

**Query Filtering**:
```csharp
// Automatically injected into all queries
if (!string.IsNullOrEmpty(user.LockField) && !string.IsNullOrEmpty(user.LockValue))
{
    var lockFilter = new BsonDocument(user.LockField, user.LockValue);
    pipeline.Insert(0, new BsonDocument("$match", lockFilter));
}
```

**Example**: Store manager can only view transactions from their store.

### Role Hierarchy

```
Administrator
├── Full system access
└── All 41 permissions

Manager
├── View all data
├── Create reports and dashboards
├── Manage their team's users
└── Cannot modify system settings

Analyst
├── View data
├── Create personal workspaces
├── Export reports
└── No admin access

Store User
├── View data locked to their store
├── Basic reporting
└── No admin access
```

## Data Security

### Encryption

#### Data in Transit
- **TLS 1.2+** for all API communications
- **HTTPS** enforced via HSTS headers
- **Certificate Management**: 
  - Let's Encrypt for cloud deployments
  - Enterprise CA certificates for on-premise

#### Data at Rest
- **MongoDB Encryption**: Enable MongoDB encryption at rest
  ```yaml
  security:
    enableEncryption: true
    encryptionKeyFile: /path/to/keyfile
  ```
- **Azure Cosmos DB**: Encryption enabled by default
- **Backups**: Encrypted backup storage

### Sensitive Data Handling

**Password Storage**:
- Never stored in plain text
- PBKDF2 with 10,000 iterations
- Unique salt per password

**JWT Secrets**:
- Stored in Key Vault (Azure) or environment variables
- Never committed to source control
- Rotated every 90 days

**Email Addresses**:
- Used for authentication and password reset only
- Not shared with third parties
- Validated before storage

### Data Retention

**Configurable Retention** (appsettings.json):
```json
{
  "DataRetention": {
    "TransactionRetentionDays": 180
  }
}
```

**TTL Index** on ReportData collection:
```javascript
db.ReportData.createIndex(
  { "_processedAt": 1 },
  { expireAfterSeconds: 15552000 }  // 180 days
)
```

**Compliance**:
- GDPR: Right to erasure implemented via deletion endpoints
- Data minimization: Only necessary fields collected
- Audit logs: 1-year retention

## Network Security

### CORS Configuration

**API** (Program.cs):
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy
            .WithOrigins("https://lossprevention.yourdomain.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

**Important**:
- Never use `AllowAnyOrigin()` in production
- Specify exact domain(s)
- Use `AllowCredentials()` with JWT tokens

### Firewall Rules

**MongoDB**:
```bash
# Allow only application servers
sudo ufw allow from 10.0.1.0/24 to any port 27017
sudo ufw deny 27017
```

**API Server**:
```bash
# Allow HTTPS from anywhere
sudo ufw allow 443/tcp

# Allow SSH from specific IPs only
sudo ufw allow from 203.0.113.0/24 to any port 22
```

**Azure Network Security Groups**:
```bash
az network nsg rule create \
  --resource-group rg-lossprevention-prod \
  --nsg-name nsg-lossprevention \
  --name AllowHTTPS \
  --priority 100 \
  --source-address-prefixes Internet \
  --destination-port-ranges 443 \
  --protocol Tcp \
  --access Allow
```

### DDoS Protection

**Azure**:
- Enable Azure DDoS Protection Standard
- Configure rate limiting in Azure Front Door

**On-Premise**:
- Use Cloudflare or similar CDN
- Configure rate limiting in Nginx:
  ```nginx
  limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
  
  location /api/ {
      limit_req zone=api burst=20 nodelay;
      proxy_pass http://backend;
  }
  ```

## Application Security

### Input Validation

**FastEndpoints Validators**:
```csharp
public class CreateUserValidator : Validator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_]+$");
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]");
    }
}
```

**MongoDB Injection Prevention**:
- Use BSON documents (not string concatenation)
- Validate all user input
- Use parameterized queries

### XSS Protection

**Vue.js**:
- Automatic escaping of dynamic content
- Use `v-html` only for trusted content
- Content Security Policy headers

**API Response Headers**:
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
    await next();
});
```

### CSRF Protection

**JWT Tokens** (stateless):
- No CSRF protection needed for API-only backends
- Tokens in Authorization header (not cookies)

**If using cookies**:
```csharp
services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});
```

### SQL Injection

**N/A**: System uses MongoDB (BSON documents), not SQL

### File Upload Security

**XML/CSV/JSON Processing**:
```csharp
// Validate file size
if (file.Length > 10_000_000) // 10 MB limit
    throw new ValidationException("File too large");

// Validate file type
var allowedExtensions = new[] { ".xml", ".csv", ".json" };
var extension = Path.GetExtension(file.FileName).ToLower();
if (!allowedExtensions.Contains(extension))
    throw new ValidationException("Invalid file type");

// Scan for malicious content (anti-virus integration)
await antiVirusService.ScanAsync(file);
```

## Security Best Practices

### Development

1. **Never commit secrets**:
   - Use `.gitignore` for appsettings.json
   - Use user secrets for development: `dotnet user-secrets set "JwtSettings:SecretKey" "dev-secret"`
   - Use environment variables or Key Vault for production

2. **Code reviews**:
   - All changes require peer review
   - Security-focused review checklist
   - Static analysis tools (SonarQube, Snyk)

3. **Dependency management**:
   - Regularly update NuGet packages
   - Monitor for known vulnerabilities
   - Use `dotnet list package --vulnerable`

### Deployment

1. **Secure configuration**:
   ```bash
   # Use Azure Key Vault
   az keyvault secret set --vault-name kv-prod --name JwtSecretKey --value "..."
   
   # Reference in App Service
   az webapp config appsettings set \
     --name app-lossprevention-api \
     --settings JwtSettings__SecretKey="@Microsoft.KeyVault(...)"
   ```

2. **HTTPS only**:
   - Redirect HTTP to HTTPS
   - Enable HSTS:
     ```csharp
     app.UseHsts();
     app.UseHttpsRedirection();
     ```

3. **Minimal attack surface**:
   - Disable unused features
   - Remove development endpoints in production
   - Use minimal Docker base images

### Operations

1. **Regular updates**:
   - Apply security patches within 7 days
   - Update OS and framework regularly
   - Monitor security advisories

2. **Access control**:
   - Use SSH keys (not passwords)
   - Implement MFA for admin access
   - Regular access reviews

3. **Monitoring**:
   - Log all authentication attempts
   - Alert on suspicious activity
   - Regular security audits

## Compliance & Auditing

### Audit Logging

**Events Logged**:
- User login/logout
- Failed authentication attempts
- Permission changes
- Data access (optional, performance impact)
- Configuration changes

**Log Format** (structured JSON):
```json
{
  "timestamp": "2026-02-12T10:30:00Z",
  "level": "INFO",
  "event": "USER_LOGIN",
  "userId": "507f1f77bcf86cd799439011",
  "username": "john.doe",
  "ipAddress": "203.0.113.42",
  "userAgent": "Mozilla/5.0...",
  "success": true
}
```

**Log Retention**: 1 year minimum

**Log Storage**:
- Azure: Application Insights
- On-Premise: ELK Stack or Splunk

### GDPR Compliance

**Right to Access**: 
```
GET /api/users/{id} - Returns all user data
```

**Right to Erasure**:
```
DELETE /api/users/{id} - Deletes user and anonymizes data
```

**Data Portability**:
```
GET /api/users/{id}/export - Exports user data in JSON format
```

**Privacy by Design**:
- Collect only necessary data
- Pseudonymization where possible
- Encrypted storage and transmission

### SOC 2 / ISO 27001

**Access Controls**:
- ✅ Role-based access control
- ✅ Principle of least privilege
- ✅ Regular access reviews

**Data Protection**:
- ✅ Encryption in transit (TLS)
- ✅ Encryption at rest (optional)
- ✅ Secure key management

**Monitoring**:
- ✅ Audit logging
- ✅ Intrusion detection
- ✅ Incident response plan

## Incident Response

### Security Incident Types

1. **Unauthorized Access**
2. **Data Breach**
3. **DDoS Attack**
4. **Malware/Ransomware**
5. **Insider Threat**

### Incident Response Plan

#### Phase 1: Detection & Analysis
```
1. Identify the incident
2. Determine scope and severity
3. Activate incident response team
4. Preserve evidence
```

#### Phase 2: Containment
```
1. Isolate affected systems
2. Block attacker access
3. Prevent data exfiltration
4. Document all actions
```

#### Phase 3: Eradication
```
1. Remove malware/backdoors
2. Patch vulnerabilities
3. Reset compromised credentials
4. Verify system integrity
```

#### Phase 4: Recovery
```
1. Restore from clean backups
2. Gradually restore services
3. Monitor for reinfection
4. Verify business operations
```

#### Phase 5: Post-Incident
```
1. Document lessons learned
2. Update security controls
3. Notify stakeholders (if required)
4. Implement preventive measures
```

### Contact Information

**Security Team**: security@yourdomain.com  
**Emergency Hotline**: +1-XXX-XXX-XXXX  
**Legal**: legal@yourdomain.com

## Security Checklist

### Pre-Production

- [ ] Change all default passwords
- [ ] Generate strong JWT secret (32+ characters)
- [ ] Configure HTTPS with valid certificate
- [ ] Enable MongoDB authentication
- [ ] Restrict CORS to specific domain
- [ ] Configure firewall rules
- [ ] Enable audit logging
- [ ] Set up monitoring and alerting
- [ ] Perform security scan (OWASP ZAP)
- [ ] Conduct penetration test
- [ ] Review and update backup procedures
- [ ] Document incident response plan
- [ ] Train team on security procedures

### Post-Production

- [ ] Monitor authentication failures daily
- [ ] Review audit logs weekly
- [ ] Update dependencies monthly
- [ ] Rotate JWT secret every 90 days
- [ ] Conduct security audit annually
- [ ] Perform disaster recovery drill quarterly
- [ ] Review user access permissions monthly
- [ ] Test backups monthly

### Ongoing

- [ ] Monitor security advisories
- [ ] Apply critical patches within 7 days
- [ ] Review new user accounts within 24 hours
- [ ] Investigate suspicious activity immediately
- [ ] Update documentation continuously

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Next Review**: May 2026  
**Owner**: Security Team
