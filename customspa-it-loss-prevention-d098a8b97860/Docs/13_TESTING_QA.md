# Loss Prevention Tool - Testing & QA Documentation

## Table of Contents
1. [Testing Strategy](#testing-strategy)
2. [Test Environment Setup](#test-environment-setup)
3. [Unit Testing](#unit-testing)
4. [Integration Testing](#integration-testing)
5. [End-to-End Testing](#end-to-end-testing)
6. [Performance Testing](#performance-testing)
7. [Security Testing](#security-testing)
8. [Test Data Management](#test-data-management)
9. [Test Automation](#test-automation)
10. [Quality Assurance Checklist](#quality-assurance-checklist)

## Testing Strategy

### Testing Pyramid

```
                    ▲
                   / \
                  /   \
                 /  E2E \          (10% - UI, Full System)
                /_________\
               /           \
              / Integration  \     (30% - API, Database)
             /_________________\
            /                   \
           /     Unit Tests       \  (60% - Business Logic)
          /_________________________\
```

### Coverage Targets

| Test Type | Target Coverage | Current Coverage |
|-----------|----------------|------------------|
| Unit Tests | 80% | TBD |
| Integration Tests | 70% | TBD |
| E2E Tests | Critical paths | TBD |
| API Tests | 100% endpoints | TBD |

### Test Environments

| Environment | Purpose | Data | Refresh Schedule |
|-------------|---------|------|------------------|
| Local Dev | Developer testing | Synthetic | As needed |
| CI/CD | Automated testing | Synthetic | Per commit |
| QA/Test | Manual testing | Sanitized production copy | Weekly |
| Staging | Pre-production | Production copy | Daily |
| Production | Live system | Live data | N/A |

## Test Environment Setup

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- MongoDB test instance
- Docker (optional, for containerized tests)

### Local Test Environment

**1. Install Test Dependencies**:

```bash
# Backend test packages
cd LossPrevention.API
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing

# Frontend test packages
cd LossPrevention.UI
npm install --save-dev @vue/test-utils vitest jsdom @vitest/ui
```

**2. Configure Test Database**:

```json
// appsettings.Test.json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "LossPrevention_Test"
  },
  "JwtSettings": {
    "SecretKey": "TEST-SECRET-KEY-FOR-UNIT-TESTS-ONLY",
    "ExpiryHours": 1
  }
}
```

**3. Test Data Seeding**:

```bash
# Seed test database
mongosh LossPrevention_Test < tests/seed-data.js
```

## Unit Testing

### Backend Unit Tests (.NET)

**Project Structure**:
```
LossPrevention.Tests/
├── UnitTests/
│   ├── Services/
│   │   ├── UserServiceTests.cs
│   │   ├── WorkspaceServiceTests.cs
│   │   └── RuleServiceTests.cs
│   ├── Helpers/
│   │   ├── DistanceHelperTests.cs
│   │   └── MappingHelperTests.cs
│   └── Validators/
│       └── UserValidatorTests.cs
└── TestData/
    └── TestDataFactory.cs
```

**Example: UserService Tests**:

```csharp
// LossPrevention.Tests/UnitTests/Services/UserServiceTests.cs
using Xunit;
using Moq;
using FluentAssertions;
using LossPrevention.Application.Services.Users;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Driver;

namespace LossPrevention.Tests.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IMongoRepository<User>> _mockUserRepo;
        private readonly Mock<IMongoRepository<Role>> _mockRoleRepo;
        private readonly Mock<IMongoRepository<Permission>> _mockPermRepo;
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IMongoRepository<User>>();
            _mockRoleRepo = new Mock<IMongoRepository<Role>>();
            _mockPermRepo = new Mock<IMongoRepository<Permission>>();
            _sut = new UserService(_mockUserRepo.Object, _mockRoleRepo.Object, _mockPermRepo.Object);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var expectedUser = new User
            {
                Username = "testuser",
                PasswordHash = HashPassword("password123"),
                Roles = new List<ObjectId>()
            };

            _mockUserRepo
                .Setup(x => x.FindOneAsync(It.IsAny<FilterDefinition<User>>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _sut.LoginAsync("testuser", "password123");

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                PasswordHash = HashPassword("correctpassword"),
                Roles = new List<ObjectId>()
            };

            _mockUserRepo
                .Setup(x => x.FindOneAsync(It.IsAny<FilterDefinition<User>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.LoginAsync("testuser", "wrongpassword");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateUsername_ThrowsException()
        {
            // Arrange
            _mockUserRepo
                .Setup(x => x.FindOneAsync(It.IsAny<FilterDefinition<User>>()))
                .ReturnsAsync(new User { Username = "existing" });

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _sut.CreateUserAsync(new CreateUserRequest { Username = "existing" })
            );
        }

        private string HashPassword(string password)
        {
            // Simplified for testing
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }
    }
}
```

**Example: Distance Helper Tests**:

```csharp
// LossPrevention.Tests/UnitTests/Helpers/DistanceHelperTests.cs
public class DistanceHelperTests
{
    [Theory]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(1, 1, 1, 1, 0)]
    [InlineData(0, 0, 3, 4, 5)]
    [InlineData(1, 2, 4, 6, 4.47)]
    public void CalculateEuclideanDistance_ReturnsCorrectDistance(
        double x1, double y1, double x2, double y2, double expected)
    {
        // Act
        var result = DistanceHelper.CalculateDistance(x1, y1, x2, y2);

        // Assert
        result.Should().BeApproximately(expected, 0.01);
    }

    [Fact]
    public void NormalizeValue_NumericField_ReturnsNormalizedValue()
    {
        // Arrange
        var values = new List<double> { 10, 20, 30, 40, 50 };
        var target = 30.0;

        // Act
        var result = DistanceHelper.Normalize(target, values);

        // Assert
        result.Should().BeApproximately(0.5, 0.01); // Midpoint
    }
}
```

**Run Unit Tests**:
```bash
dotnet test --filter Category=Unit
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Unit Tests (Vue.js + Vitest)

**vitest.config.ts**:
```typescript
import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  test: {
    globals: true,
    environment: 'jsdom',
    coverage: {
      provider: 'istanbul',
      reporter: ['text', 'html', 'lcov']
    }
  }
})
```

**Example: Store Tests**:

```typescript
// tests/stores/loginStore.spec.ts
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useLoginStore } from '@/stores/loginStore'
import api from '@/api/api'

vi.mock('@/api/api')

describe('LoginStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
  })

  it('should login successfully with valid credentials', async () => {
    const store = useLoginStore()
    
    vi.mocked(api.post).mockResolvedValue({
      data: { token: 'fake-jwt-token' }
    })

    await store.login('testuser', 'password123')

    expect(store.isLoggedIn).toBe(true)
    expect(localStorage.getItem('token')).toBe('fake-jwt-token')
  })

  it('should logout and clear token', () => {
    const store = useLoginStore()
    localStorage.setItem('token', 'fake-token')
    store.isLoggedIn = true

    store.logout()

    expect(store.isLoggedIn).toBe(false)
    expect(localStorage.getItem('token')).toBeNull()
  })

  it('should validate expired token', () => {
    const store = useLoginStore()
    const expiredToken = createExpiredToken()
    localStorage.setItem('token', expiredToken)

    store.validateToken()

    expect(store.isLoggedIn).toBe(false)
  })
})
```

**Example: Component Tests**:

```typescript
// tests/components/Login.spec.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import LoginForm from '@/components/Login.vue'

describe('LoginForm', () => {
  it('should render login form', () => {
    const wrapper = mount(LoginForm)
    
    expect(wrapper.find('input[type="text"]').exists()).toBe(true)
    expect(wrapper.find('input[type="password"]').exists()).toBe(true)
    expect(wrapper.find('button[type="submit"]').exists()).toBe(true)
  })

  it('should validate required fields', async () => {
    const wrapper = mount(LoginForm)
    const form = wrapper.find('form')
    
    await form.trigger('submit')
    
    expect(wrapper.text()).toContain('Username is required')
    expect(wrapper.text()).toContain('Password is required')
  })

  it('should call login on form submit', async () => {
    const wrapper = mount(LoginForm)
    
    await wrapper.find('input[type="text"]').setValue('testuser')
    await wrapper.find('input[type="password"]').setValue('password')
    await wrapper.find('form').trigger('submit')
    
    // Assert login method was called
    expect(wrapper.emitted('login')).toBeTruthy()
  })
})
```

**Run Frontend Tests**:
```bash
npm run test
npm run test:coverage
npm run test:ui  # Open Vitest UI
```

## Integration Testing

### API Integration Tests

**Test API with Real Database**:

```csharp
// LossPrevention.Tests/IntegrationTests/ApiTests/UserEndpointTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class UserEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public UserEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new
        {
            username = "admin",
            password = "Admin@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/login", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUsers_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        var newUser = new
        {
            username = "newuser",
            password = "P@ssw0rd",
            email = "newuser@test.com",
            firstName = "New",
            lastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", newUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Database Integration Tests

```csharp
// Test with real MongoDB (TestContainers)
public class DatabaseIntegrationTests : IAsyncLifetime
{
    private MongoDbContainer _container;
    private IMongoClient _client;

    public async Task InitializeAsync()
    {
        _container = new MongoDbBuilder()
            .WithImage("mongo:7")
            .Build();

        await _container.StartAsync();
        _client = new MongoClient(_container.GetConnectionString());
    }

    [Fact]
    public async Task InsertUser_ShouldPersistToDatabase()
    {
        // Arrange
        var db = _client.GetDatabase("test");
        var collection = db.GetCollection<User>("Users");
        var user = new User { Username = "test", Email = "test@test.com" };

        // Act
        await collection.InsertOneAsync(user);
        var result = await collection.Find(x => x.Username == "test").FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@test.com");
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

## End-to-End Testing

### Playwright E2E Tests

**Install Playwright**:
```bash
cd LossPrevention.UI
npm install -D @playwright/test
npx playwright install
```

**playwright.config.ts**:
```typescript
import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  use: {
    baseURL: 'http://localhost:5173',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure'
  },
  projects: [
    { name: 'chromium', use: { browserName: 'chromium' } },
    { name: 'firefox', use: { browserName: 'firefox' } },
    { name: 'webkit', use: { browserName: 'webkit' } }
  ]
})
```

**Example E2E Tests**:

```typescript
// e2e/login.spec.ts
import { test, expect } from '@playwright/test'

test.describe('Login Flow', () => {
  test('should login with valid credentials', async ({ page }) => {
    await page.goto('/')
    
    await page.fill('input[type="text"]', 'admin')
    await page.fill('input[type="password"]', 'Admin@123')
    await page.click('button[type="submit"]')
    
    await expect(page).toHaveURL('/home')
    await expect(page.locator('text=Welcome admin')).toBeVisible()
  })

  test('should show error with invalid credentials', async ({ page }) => {
    await page.goto('/')
    
    await page.fill('input[type="text"]', 'admin')
    await page.fill('input[type="password"]', 'wrongpassword')
    await page.click('button[type="submit"]')
    
    await expect(page.locator('text=Invalid credentials')).toBeVisible()
  })
})

// e2e/dashboard.spec.ts
test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('/')
    await page.fill('input[type="text"]', 'admin')
    await page.fill('input[type="password"]', 'Admin@123')
    await page.click('button[type="submit"]')
    await page.waitForURL('/home')
  })

  test('should create new dashboard', async ({ page }) => {
    await page.click('text=Dashboards')
    await page.click('text=Create Dashboard')
    
    await page.fill('input[name="name"]', 'Test Dashboard')
    await page.fill('textarea[name="description"]', 'Test Description')
    await page.click('button:has-text("Save")')
    
    await expect(page.locator('text=Test Dashboard')).toBeVisible()
  })

  test('should add chart to dashboard', async ({ page }) => {
    await page.goto('/dashboard/new')
    
    // Open chart dialog
    await page.click('button:has-text("Add Chart")')
    
    // Configure chart
    await page.selectOption('select[name="chartType"]', 'bar')
    await page.fill('input[name="title"]', 'Sales by Store')
    await page.click('button:has-text("Add")')
    
    await expect(page.locator('text=Sales by Store')).toBeVisible()
  })
})
```

**Run E2E Tests**:
```bash
npx playwright test
npx playwright test --headed  # With browser
npx playwright show-report
```

## Performance Testing

### Load Testing with k6

**Install k6**:
```bash
# Windows
choco install k6

# Linux
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

**Load Test Script**:

```javascript
// tests/performance/load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '1m', target: 10 },   // Ramp up to 10 users
    { duration: '3m', target: 10 },   // Stay at 10 users
    { duration: '1m', target: 50 },   // Ramp up to 50 users
    { duration: '3m', target: 50 },   // Stay at 50 users
    { duration: '1m', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% of requests < 500ms
    http_req_failed: ['rate<0.01'],    // < 1% errors
  },
};

const BASE_URL = 'https://localhost:7110/api';
let token = '';

export function setup() {
  // Login once
  const loginRes = http.post(`${BASE_URL}/users/login`, JSON.stringify({
    username: 'admin',
    password: 'Admin@123'
  }), {
    headers: { 'Content-Type': 'application/json' }
  });
  
  return { token: JSON.parse(loginRes.body).token };
}

export default function(data) {
  const params = {
    headers: {
      'Authorization': `Bearer ${data.token}`,
      'Content-Type': 'application/json'
    }
  };

  // Test workspaces endpoint
  let res = http.get(`${BASE_URL}/workspaces`, params);
  check(res, {
    'workspace status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);

  // Test data query
  res = http.post(`${BASE_URL}/data/query`, JSON.stringify({
    pipeline: [{ $limit: 50 }],
    skip: 0,
    take: 50
  }), params);
  
  check(res, {
    'query status is 200': (r) => r.status === 200,
    'query time < 1000ms': (r) => r.timings.duration < 1000,
  });

  sleep(2);
}
```

**Run Load Test**:
```bash
k6 run tests/performance/load-test.js
```

### Database Performance Tests

```javascript
// MongoDB performance test
use LossPrevention_Test

// Insert 100,000 test records
for (let i = 0; i < 100000; i++) {
  db.ReportData.insertOne({
    TransactionID: `TXN${i}`,
    Amount: Math.random() * 1000,
    StoreID: `STORE${i % 100}`,
    TransactionDate: new Date(),
    _processedAt: new Date()
  })
}

// Test query performance
db.ReportData.explain("executionStats").aggregate([
  { $match: { StoreID: "STORE001" } },
  { $group: { _id: null, total: { $sum: "$Amount" } } }
])

// Should use index and complete in < 100ms
```

## Security Testing

### OWASP ZAP Scan

```bash
# Pull ZAP Docker image
docker pull owasp/zap2docker-stable

# Run baseline scan
docker run -t owasp/zap2docker-stable zap-baseline.py \
  -t https://localhost:7110 \
  -r zap-report.html

# Run full scan
docker run -t owasp/zap2docker-stable zap-full-scan.py \
  -t https://localhost:7110 \
  -r zap-full-report.html
```

### SQL Injection Tests (N/A - MongoDB)

### XSS Tests

```typescript
// Test XSS protection
test('should sanitize user input', async () => {
  const maliciousInput = '<script>alert("XSS")</script>'
  
  const response = await api.post('/users', {
    username: maliciousInput,
    ...otherFields
  })
  
  // Should either reject or sanitize
  expect(response.data.username).not.toContain('<script>')
})
```

### Authentication Tests

```typescript
test('should reject requests without token', async () => {
  const response = await fetch('/api/users')
  expect(response.status).toBe(401)
})

test('should reject expired tokens', async () => {
  const expiredToken = generateExpiredToken()
  const response = await fetch('/api/users', {
    headers: { Authorization: `Bearer ${expiredToken}` }
  })
  expect(response.status).toBe(401)
})
```

## Test Data Management

### Test Data Factory

```csharp
// TestDataFactory.cs
public class TestDataFactory
{
    public static User CreateUser(string username = "testuser")
    {
        return new User
        {
            Id = ObjectId.GenerateNewId(),
            Username = username,
            Email = $"{username}@test.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = HashPassword("password123"),
            Roles = new List<ObjectId>(),
            RegistrationDate = DateTime.UtcNow
        };
    }

    public static Workspace CreateWorkspace(string name = "Test Workspace")
    {
        return new Workspace
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = name,
            Description = "Test Description",
            Tabs = new List<Tab>
            {
                new Tab
                {
                    Id = "tab-1",
                    Title = "Test Tab",
                    SelectedFields = new List<Field>(),
                    Query = new Query()
                }
            }
        };
    }

    public static BsonDocument CreateTransactionData()
    {
        return new BsonDocument
        {
            { "TransactionID", $"TXN{Guid.NewGuid()}" },
            { "Amount", new BsonDecimal128(99.99M) },
            { "StoreID", "STORE001" },
            { "TransactionDate", DateTime.UtcNow },
            { "_processedAt", DateTime.UtcNow }
        };
    }
}
```

### Anonymizing Production Data

```javascript
// anonymize-data.js
// Copy production data and anonymize for testing

db.Users.find().forEach(function(doc) {
  db.Users_Test.insertOne({
    ...doc,
    _id: ObjectId(),
    Email: `user${doc._id}@test.com`,
    FirstName: "Test",
    LastName: "User",
    PasswordHash: "REDACTED"
  })
})
```

## Test Automation

### CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/test.yml
name: Test Suite

on: [push, pull_request]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    services:
      mongodb:
        image: mongo:7
        ports:
          - 27017:27017
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run unit tests
        run: dotnet test --no-build --filter Category=Unit
      
      - name: Run integration tests
        run: dotnet test --no-build --filter Category=Integration
        env:
          MONGODB_URI: mongodb://localhost:27017
      
      - name: Generate coverage report
        run: dotnet test --collect:"XPlat Code Coverage"
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: npm ci
        working-directory: ./LossPrevention.UI
      
      - name: Run tests
        run: npm test
        working-directory: ./LossPrevention.UI
      
      - name: Run E2E tests
        run: npx playwright test
        working-directory: ./LossPrevention.UI

  security-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Run Snyk security scan
        run: npx snyk test --all-projects
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
```

## Quality Assurance Checklist

### Pre-Release QA

**Functionality**:
- [ ] All core features working
- [ ] CRUD operations for all entities
- [ ] Authentication and authorization
- [ ] Data import/export
- [ ] Fraud detection accuracy
- [ ] Dashboard rendering
- [ ] Report generation

**Performance**:
- [ ] Page load < 2 seconds
- [ ] API response < 500ms
- [ ] Query response < 1 second
- [ ] Large export (10k records) < 30 seconds
- [ ] No memory leaks

**Security**:
- [ ] No critical vulnerabilities
- [ ] SQL injection (N/A - MongoDB)
- [ ] XSS protection
- [ ] CSRF protection
- [ ] Secure password storage
- [ ] JWT token validation
- [ ] HTTPS enforced

**Compatibility**:
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Edge (latest)
- [ ] Safari (latest)
- [ ] Mobile responsive

**Accessibility**:
- [ ] Keyboard navigation
- [ ] Screen reader compatible
- [ ] Color contrast (WCAG AA)
- [ ] Focus indicators

### Test Reports

**Generate Test Report**:
```bash
# .NET test report
dotnet test --logger "html;logfilename=test-results.html"

# Vitest coverage report
npm run test:coverage
# Open: coverage/index.html

# Playwright report
npx playwright show-report
```

**Coverage Badge**:
```markdown
![Coverage](https://img.shields.io/codecov/c/github/yourorg/lossprevention)
```

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Maintained By**: QA Team
