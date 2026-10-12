# Loss Prevention Tool - Change Log

## Version History

### Version 1.0.0 - February 12, 2026 (Current)

**Status**: Production Ready

#### ✅ Features Completed

**User Management**
- User authentication with JWT tokens
- Password reset functionality with email notifications
- Role-based access control (RBAC) with 41 permissions
- Field-level data locking (LockField/LockValue)
- User groups for collaboration
- Session management with configurable expiry

**Workspaces & Reports**
- Dynamic workspace creation and management
- Multi-tab report designer
- Query builder with conditions and aggregations
- Expression editor for calculated fields
- Conditional formatting and highlighting
- Prefix/suffix formatting
- Heatmap visualization
- Heatmap drilldown capability
- Export to CSV, Excel, PDF

**Dashboards**
- Drag-and-drop dashboard designer
- Multiple block types:
  - Charts (line, bar, pie, doughnut)
  - Tabular reports with pagination
  - Text blocks (markdown support)
  - Image blocks
- Dashboard sharing and permissions
- Real-time data updates

**Data Management**
- Field mapping configuration
- Multiple data type support (String, Number, Decimal, Date, Boolean, Array)
- Data validation and transformation
- Dynamic schema support

**Data Ingestion**
- Multi-format support (XML, CSV, JSON)
- SFTP file processing
- File system monitoring
- Scheduled data imports
- Automatic file archiving
- Source file tracking
- Configurable data retention (TTL-based, default 180 days)

**Fraud Detection**
- Generic rule-based fraud detection engine
- 15 configurable fraud detection types:
  - High value transactions
  - Extreme high values
  - Low value padding
  - Refund amount fraud
  - Refund count fraud
  - Void amount fraud
  - Void count fraud
  - Discount amount abuse
  - Discount count abuse
  - Cash shortage detection
  - Transaction count anomalies
  - Item count anomalies
  - After-hours transactions
  - Weekend activity patterns
  - Failed transaction patterns
- Statistical threshold configuration (percentiles, standard deviations)
- Real-time fraud analysis
- Fraud alert export capabilities

**AI & Analytics**
- Natural Language Query (NQL) - Query data using plain English
- AI-powered report generation
- Euclidean distance analysis for pattern matching
- K-Nearest Neighbors support
- Behavioral profiling capabilities

**Notifications & Collaboration**
- In-app notification system
- User, role, and group-based notifications
- Reply functionality for conversations
- Read/unread tracking
- Notification filtering and search

**Security**
- JWT authentication with HMAC-SHA256
- PBKDF2 password hashing with salt
- Permission-based authorization
- CORS configuration
- Audit logging
- Password complexity requirements
- Account lockout protection

#### 🐛 Bug Fixes

- Fixed Roles API permissions
- Fixed Mappings API permissions
- Fixed Data Ingestion API permissions
- Fixed Permissions API permissions
- Fixed Workspaces API permissions
- Fixed Notifications read endpoint (415 Unsupported Media Type)
- Fixed conditional formatting issues in report designer
- Fixed locked-down user access issues
- Fixed fraud detection endpoint (404 Not Found)

#### 🔧 Technical Improvements

- Optimized MongoDB aggregation pipelines
- Improved AI query batching for better results
- Enhanced data distance UI and functionality
- Added ability to add conditions to tabular reports
- Implemented generic fraud detection solution
- Improved query performance for large datasets
- Added TTL index for automatic data cleanup

#### 📝 Documentation

- Complete API documentation (41 endpoints)
- User manual with step-by-step guides
- Administrator guide
- Security documentation
- Deployment guides (Azure, On-Premise, Docker)
- Database documentation
- Architecture documentation

---

### Version 0.9.0 - January 2026 (Beta)

#### Features Added
- User login and authentication
- Password reset capability
- Workspace management
- Report designer with expression editor
- Dashboard basics
- Data mapping
- Initial fraud detection

#### Known Issues
- Fraud detection API endpoint missing
- Performance issues with 1000+ rows
- Conditional formatting bugs

---

## Roadmap (See [roadmap.txt](roadmap.txt) for full details)

### Next Release (v1.1.0) - Target: Q2 2026

**In Development** 🚧
- Lookup tables for data enrichment
- Performance optimization for large datasets (1000+ rows)
- Real-world data testing and validation
- Rule engine auto-apply on ingestion (testing phase)
- User field-level locks (testing phase)
- SFTP configuration testing

**Planned Features** 📋
- Multi-tenancy support (simplified version)
- SSO integration (SAML, OAuth)
- Advanced charting capabilities
- Linked reports (report switching)
- Blueprint/workflow automation

### Future Releases (v2.0+)

**Major Features**
- Case management tool with form editor
- Receipt view and analysis
- Behavioral profiling enhancements
- AI dashboard generation
- Summarized data views
- Fluid investigation experience
- Custom branding per client (URL-dependent)

**Infrastructure**
- Database seeding/setup wizard
- DevOps automation
- Unit tests in CI/CD pipeline
- Branching strategy implementation
- License management system

### Long-Term Vision

**Advanced Analytics**
- Enhanced K-Nearest Neighbors (KNN)
- Machine learning model integration
- Predictive fraud detection
- Real-time anomaly detection
- Pattern recognition improvements

**Integration**
- Webhook support
- REST API extensions
- Third-party system connectors
- Data lake integration

**User Experience**
- Mobile app (iOS/Android)
- Offline mode
- Voice commands
- Advanced visualizations

---

## Breaking Changes

### Version 1.0.0
- JWT token expiry changed to 1 hour (was 8 hours in development)
- MongoDB connection string format updated for replica sets
- Password complexity requirements now enforced
- Field mapping schema changes (DataType field renamed)

---

## Upgrade Notes

### Upgrading from v0.9 to v1.0

**Database Changes**:
```javascript
// Run these MongoDB scripts in order
use LossPrevention
load('MongoDBScripts/00_CleanupDuplicatePermissions.js')
load('MongoDBScripts/01_CreatePermissions.js')
load('MongoDBScripts/02_AddPermissionsToAdminRole.js')

// Add TTL index for data retention
db.ReportData.createIndex(
  { "_processedAt": 1 },
  { expireAfterSeconds: 15552000 }  // 180 days
)
```

**Configuration Changes**:
```json
// Add to appsettings.json
{
  "DataRetention": {
    "TransactionRetentionDays": 180
  }
}
```

**Code Changes**:
- Update JWT secret key (minimum 32 characters)
- Update MongoDB connection string if using authentication
- Update CORS policy with production URL

---

## Known Issues

### Current (v1.0.0)

**Performance**
- Scrolling through fraud detection results can be slow with 1000+ rows
- Large exports (>10,000 records) may timeout without pagination

**Features**
- Fraud detection settings endpoint returns 404 (workaround: configure via database directly)
- Date range picker doesn't support all date formats
- Mobile UI needs responsive improvements

### Workarounds

**Fraud Detection Settings 404**:
```javascript
// Insert settings manually
db.FraudDetectionSettings.insertOne({
  _id: ObjectId(),
  thresholds: {
    highValue: { percentile: 0.95, stdDevMultiplier: 2, minimumValue: 500 }
    // ... other thresholds
  }
})
```

---

## Deprecation Notices

### Planned for v2.0
- Legacy report format (pre-v1.0) will no longer be supported
- Old authentication method (if any) will be removed
- Support for MongoDB < 6.0 will be dropped

---

## Security Advisories

### CVE-2026-XXXX (If applicable)
No known security vulnerabilities in v1.0.0

**Best Practices**:
- Always use HTTPS in production
- Rotate JWT secret key every 90 days
- Keep MongoDB authentication enabled
- Apply security patches within 7 days
- Use strong passwords (minimum 12 characters recommended)

---

## Contributors

**Development Team**:
- Backend Development: .NET Team
- Frontend Development: Vue.js Team
- Database Design: Data Architecture Team
- Security: Security Team
- Documentation: Technical Writers
- QA & Testing: QA Team

**Special Thanks**:
- Beta testers and early adopters
- Community contributors
- Open source projects we depend on

---

## Support

**Getting Help**:
- Documentation: `/docs` folder
- Issues: GitHub Issues
- Email: support@yourdomain.com
- Community Forum: forum.yourdomain.com

**Reporting Bugs**:
1. Check existing issues
2. Create detailed bug report with:
   - Version number
   - Steps to reproduce
   - Expected vs actual behavior
   - Screenshots/logs
   - Environment details

**Feature Requests**:
Submit feature requests via GitHub Issues with `enhancement` label

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Next Update**: After each release
