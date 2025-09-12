# BlogApp Makefile
# This file provides common commands for development and deployment

.PHONY: help build test clean run docker-build docker-run docker-stop docker-clean db-update db-reset

# Default target
help:
	@echo "BlogApp - Available Commands:"
	@echo ""
	@echo "Development:"
	@echo "  build        - Build the solution"
	@echo "  test         - Run all tests"
	@echo "  test-coverage- Run tests with code coverage"
	@echo "  coverage-report - Generate coverage report"
	@echo "  run          - Run the API project"
	@echo "  clean        - Clean build artifacts"
	@echo ""
	@echo "Database:"
	@echo "  db-update    - Update database with latest migrations"
	@echo "  db-reset     - Reset database (drop and recreate)"
	@echo ""
	@echo "Docker:"
	@echo "  docker-build - Build Docker image"
	@echo "  docker-run   - Run Docker containers"
	@echo "  docker-stop  - Stop Docker containers"
	@echo "  docker-clean - Clean Docker containers and images"
	@echo ""
	@echo "Utilities:"
	@echo "  format       - Format code with dotnet format"
	@echo "  analyze      - Run code analysis with SonarQube"
	@echo "  security     - Run security analysis"

# Development commands
build:
	@echo "Building solution..."
	dotnet build

test:
	@echo "Running tests..."
	dotnet test --verbosity normal

test-coverage:
	@echo "Running tests with code coverage..."
	dotnet test \
		--configuration Release \
		--collect:"XPlat Code Coverage" \
		--settings tests/BlogApp.UnitTests/coverlet.runsettings

coverage-report:
	@echo "Generating coverage report..."
	@scripts/generate-coverage-report.sh

run:
	@echo "Running API project..."
	dotnet run --project src/BlogApp.API/BlogApp.API.csproj

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	@echo "Removing bin and obj directories..."
	find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true

# Database commands
db-update:
	@echo "Updating database..."
	dotnet ef database update --project src/BlogApp.Infrastructure/BlogApp.Infrastructure.csproj

db-reset:
	@echo "Resetting database..."
	dotnet ef database drop --project src/BlogApp.Infrastructure/BlogApp.Infrastructure.csproj --force
	dotnet ef database update --project src/BlogApp.Infrastructure/BlogApp.Infrastructure.csproj

# Docker commands
docker-build:
	@echo "Building Docker image..."
	docker build -t blogapp:latest .

docker-run:
	@echo "Starting Docker containers..."
	docker-compose up -d

docker-stop:
	@echo "Stopping Docker containers..."
	docker-compose down

docker-clean:
	@echo "Cleaning Docker containers and images..."
	docker-compose down -v --rmi all
	docker system prune -f

# Utility commands
format:
	@echo "Formatting code..."
	dotnet format

analyze:
	@echo "Running code analysis..."
	@echo "Make sure SonarQube is running on http://localhost:9000"
	dotnet sonarscanner begin /k:"BlogApp" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="admin" /d:sonar.password="admin"
	dotnet build
	dotnet test
	dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="admin"

security:
	@echo "Running security analysis..."
	@echo "Installing security tools..."
	dotnet tool install --global dotnet-security-scan
	@echo "Running security scan..."
	dotnet security-scan

# Windows-specific commands (if using Git Bash or similar)
windows-build:
	@echo "Building solution (Windows)..."
	dotnet build

windows-test:
	@echo "Running tests (Windows)..."
	dotnet test --verbosity normal

windows-test-coverage:
	@echo "Running tests with code coverage (Windows)..."
	dotnet test \
		--configuration Release \
		--collect:"XPlat Code Coverage" \
		--settings tests/BlogApp.UnitTests/coverlet.runsettings

windows-run:
	@echo "Running API project (Windows)..."
	dotnet run --project src\BlogApp.API\BlogApp.API.csproj

windows-clean:
	@echo "Cleaning build artifacts (Windows)..."
	dotnet clean
	@echo "Removing bin and obj directories..."
	rmdir /s /q src\BlogApp.API\bin 2>nul || true
	rmdir /s /q src\BlogApp.API\obj 2>nul || true
	rmdir /s /q src\BlogApp.Application\bin 2>nul || true
	rmdir /s /q src\BlogApp.Application\obj 2>nul || true
	rmdir /s /q src\BlogApp.Domain\bin 2>nul || true
	rmdir /s /q src\BlogApp.Domain\obj 2>nul || true
	rmdir /s /q src\BlogApp.Infrastructure\bin 2>nul || true
	rmdir /s /q src\BlogApp.Infrastructure\obj 2>nul || true
	rmdir /s /q src\BlogApp.Worker\bin 2>nul || true
	rmdir /s /q src\BlogApp.Worker\obj 2>nul || true
	rmdir /s /q tests\BlogApp.UnitTests\bin 2>nul || true
	rmdir /s /q tests\BlogApp.UnitTests\obj 2>nul || true

# Development workflow
dev-setup: docker-run db-update
	@echo "Development environment setup complete!"
	@echo "Services available at:"
	@echo "  - API: https://localhost:5001"
	@echo "  - Swagger: https://localhost:5001/swagger"
	@echo "  - SonarQube: http://localhost:9000"
	@echo "  - Kibana: http://localhost:5601"
	@echo "  - Elasticsearch: http://localhost:9200"
	@echo "  - RabbitMQ: http://localhost:15672"
	@echo "  - MinIO: http://localhost:9003"

dev-clean: docker-stop docker-clean clean
	@echo "Development environment cleaned!"

# Production build
prod-build:
	@echo "Building production version..."
	dotnet publish src/BlogApp.API/BlogApp.API.csproj -c Release -o ./publish
	@echo "Production build complete in ./publish directory"

# Quick development cycle
dev-cycle: clean build test
	@echo "Development cycle complete!"

# Install development tools
install-tools:
	@echo "Installing development tools..."
	dotnet tool install --global dotnet-ef
	dotnet tool install --global dotnet-format
	dotnet tool install --global dotnet-sonarscanner
	@echo "Development tools installed!"
