# Loss Prevention Tool

A comprehensive loss prevention and fraud detection system built with .NET 8 API backend and Vue.js 3 frontend.

## Overview

The Loss Prevention Tool is an enterprise-grade application designed to help organizations detect, analyze, and prevent fraud and losses across their operations. It combines powerful data ingestion capabilities, flexible reporting, AI-powered analytics, and sophisticated fraud detection mechanisms.

### Key Features

- **Dynamic Report Builder**: Create custom reports with drag-and-drop interface
- **Interactive Dashboards**: Build visual dashboards with charts, tables, and widgets
- **AI-Powered Queries**: Natural language processing for data queries
- **Fraud Detection**: Configurable rule-based fraud detection engine with 15+ fraud types
- **Euclidean Distance Analysis**: Pattern matching and behavioral profiling
- **Data Ingestion**: Multi-format support (XML, CSV, JSON) with SFTP integration
- **Role-Based Access Control**: 41 granular permissions for fine-grained security
- **Field-Level Security**: Lock specific data fields for sensitive information
- **Notifications & Collaboration**: In-app notifications and group collaboration

## Technology Stack

### Backend
- .NET 8 Web API
- FastEndpoints for minimal APIs
- MongoDB for data storage
- JWT Authentication

### Frontend
- Vue.js 3 (Composition API)
- Nuxt 3 for SSR
- Vuetify 3 for UI components
- Pinia for state management
- AG Grid for data tables
- Chart.js for visualizations

## Quick Start

See [docs/03_SETUP_INSTALLATION.md](docs/03_SETUP_INSTALLATION.md) for detailed setup instructions.

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- MongoDB 7.0+

### Basic Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd LostPreventionTool
```

2. **Setup MongoDB**
```bash
# Start MongoDB
mongod --dbpath /path/to/data

# Initialize database (run scripts in MongoDBScripts folder)
```

3. **Run the API**
```bash
cd LossPrevention.API
dotnet restore
dotnet run
```

4. **Run the Frontend**
```bash
cd LossPrevention.UI
npm install
npm run dev
```

## Project Structure

```
LossPrevention.sln
├── LossPrevention.API/              # Web API project
│   ├── Endpoints/                   # API endpoints
│   ├── MongoDBScripts/              # Database initialization scripts
│   └── Properties/                  # Launch settings
├── LossPrevention.Application/      # Application layer
│   ├── DTO/                         # Data transfer objects
│   ├── Handlers/                    # Request/response handlers
│   ├── Services/                    # Business logic services
│   └── Validators/                  # Input validation
├── LossPrevention.Domain/           # Domain layer
│   ├── Entities/                    # Domain entities
│   └── Exceptions/                  # Domain exceptions
├── LossPrevention.Infrastructure/   # Infrastructure layer
│   ├── Repositories/                # Data access
│   └── Services/                    # External services
├── LossPrevention.DataIngestionService/ # Data ingestion service
└── LossPrevention.UI/               # Vue.js frontend
    └── src/                         # Source files
```

## 📚 Complete Documentation

### Getting Started
- **[Executive Overview](docs/01_EXECUTIVE_OVERVIEW.md)** - Business value, key features, and stakeholder information
- **[System Architecture](docs/02_SYSTEM_ARCHITECTURE.md)** - Technical architecture, components, and design patterns
- **[Setup & Installation Guide](docs/03_SETUP_INSTALLATION.md)** - Step-by-step installation for development
- **[Deployment Guide](docs/04_DEPLOYMENT_GUIDE.md)** - Production deployment procedures for Azure, on-premise, and Docker

### Technical Reference
- **[API Documentation](docs/05_API_DOCUMENTATION.md)** - Complete REST API reference with all 78 endpoints
- **[Database & Data Model](docs/06_DATABASE_DATA_MODEL.md)** - MongoDB schema, collections, and indexes
- **[Frontend Architecture](docs/07_FRONTEND_ARCHITECTURE.md)** - Vue.js components, stores, and routing (41 components)
- **[Backend Architecture](docs/08_BACKEND_ARCHITECTURE.md)** - .NET layers, services, and repositories (70+ components)
- **[Security Documentation](docs/09_SECURITY_DOCUMENTATION.md)** - Authentication, authorization, and security best practices
- **[Configuration Reference](docs/10_CONFIGURATION_REFERENCE.md)** - All configuration options and environment variables

### User Guides
- **[User Manual & Admin Guide](docs/11_USER_MANUAL.md)** - Complete end-user and administrator guide (130+ pages)
- **[Distance Analysis Quick Start](docs/DISTANCE_ANALYSIS_QUICK_START.md)** - Quick guide for Euclidean distance analysis
- **[Fraud Detection API](docs/FRAUD_DETECTION_API.md)** - Fraud detection features and usage

### Operations & Maintenance
- **[Maintenance & Operations Guide](docs/12_MAINTENANCE_OPERATIONS.md)** - System maintenance, monitoring, and troubleshooting
- **[Testing & QA Documentation](docs/13_TESTING_QA.md)** - Testing strategies, automation, and quality assurance
- **[Change Log & Roadmap](docs/14_CHANGELOG.md)** - Version history, planned features, and roadmap
- **[Roadmap Details](docs/roadmap.txt)** - Detailed development roadmap and feature status

## Quick Links

| Category | Documents |
|----------|-----------|
| **Business** | [Executive Overview](docs/01_EXECUTIVE_OVERVIEW.md) |
| **Architecture** | [System Architecture](docs/02_SYSTEM_ARCHITECTURE.md), [Frontend](docs/07_FRONTEND_ARCHITECTURE.md), [Backend](docs/08_BACKEND_ARCHITECTURE.md) |
| **Development** | [Setup Guide](docs/03_SETUP_INSTALLATION.md), [API Docs](docs/05_API_DOCUMENTATION.md), [Database](docs/06_DATABASE_DATA_MODEL.md), [Configuration](docs/10_CONFIGURATION_REFERENCE.md) |
| **Operations** | [Deployment](docs/04_DEPLOYMENT_GUIDE.md), [Maintenance](docs/12_MAINTENANCE_OPERATIONS.md), [Testing](docs/13_TESTING_QA.md) |
| **Security** | [Security Guide](docs/09_SECURITY_DOCUMENTATION.md) |
| **User Help** | [User Manual](docs/11_USER_MANUAL.md), [Distance Analysis](docs/DISTANCE_ANALYSIS_QUICK_START.md), [Fraud Detection](docs/FRAUD_DETECTION_API.md) |
| **Planning** | [Changelog](docs/14_CHANGELOG.md), [Roadmap](docs/roadmap.txt) |

## Key Statistics

- **Backend**: 78 API endpoints, 15+ services, 18+ domain entities
- **Frontend**: 41 Vue components, 15 Pinia stores
- **Database**: 13 MongoDB collections with optimized indexes
- **Security**: 41 granular permissions, JWT authentication, RBAC
- **Fraud Detection**: 15 configurable fraud detection types
- **Data Formats**: XML, CSV, JSON ingestion support

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

Proprietary - All rights reserved

## Support

For issues and questions, please contact the development team.

---

**Version**: 1.0.0  
**Last Updated**: February 2026  
**Documentation Status**: Complete