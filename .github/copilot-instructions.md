# AI Agent Instructions for BlogApp

This guide helps AI coding agents understand the key aspects of the BlogApp project structure and conventions.

## 🏗️ Architecture Overview

- **Clean Architecture** implementation with layers:
  - `BlogApp.API` - Web API and configuration
  - `BlogApp.Application` - Business logic and CQRS
  - `BlogApp.Domain` - Core entities and interfaces
  - `BlogApp.Infrastructure` - Data access and external services
  - `BlogApp.Worker` - Background jobs and message consumers

## 📋 Key Patterns & Conventions

### Data Access
- Generic Repository pattern with `IGenericRepository<T>` base interface
- Unit of Work pattern via `IUnitOfWork` for transaction management
- Entity Framework Core with PostgreSQL
- Redis caching with `ICacheService` abstraction
- MinIO S3 for file storage via `IFileService`

### Authentication & Security
- JWT authentication with refresh tokens
- Role-based authorization
- Password history validation
- Rate limiting on API endpoints
- Input validation middleware
- Anti-forgery protection
- Request correlation tracking

### Service Registration
- Register dependencies in appropriate `Program.cs` Configure* methods
- Use scoped lifetime for repositories and services
- Use singleton for long-lived services like RabbitMQ connections

### Configuration
- Uses HashiCorp Vault for secret management
- Environment-specific settings in VAULT.json
- Required environment variables:
  - `VAULT_ADDR` - Vault server address
  - `VAULT_TOKEN` - Authentication token
  - `BLOG_APP` - Secret path

## 🔄 Development Workflow

### Build & Run
```bash
# Build solution
make build

# Run API project
make run

# Run tests with coverage
make test-coverage

# Generate coverage report
make coverage-report
```

### Docker Environment
```bash
# Start all services
docker-compose up -d

# Stop services
docker-compose down
```

### Testing
- Unit tests inherit from `BaseTestClass` variants:
  - `BaseControllerTest` - For API controllers
  - `BaseApplicationTest` - For CQRS handlers
  - `BaseServiceTest` - For service implementations
- Use `TestHelper` for common test utilities
- Mock external dependencies and configuration
- Tests use in-memory database by default

## 🔌 Integration Points

- **RabbitMQ** - Message queue for async processing
- **Redis** - Caching layer
- **MinIO S3** - File storage
- **Firebase** - Push notifications
- **Elasticsearch** - Search and logging
- **HashiCorp Vault** - Secret management

## 🚨 Common Gotchas

1. Always use `IUnitOfWork` for data access to ensure proper transaction handling
2. Cache invalidation needed after entity updates via `ICacheInvalidationService`
3. File uploads require validation via `IFileValidationService`
4. Background jobs should be registered in the worker project
5. Security headers and middleware order in API pipeline is critical