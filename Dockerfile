# Multi-stage build for .NET 10 application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set working directory
WORKDIR /src

# Copy solution and project files
COPY ["BlogApp.slnx", "./"]
COPY ["src/BlogApp.Domain/BlogApp.Domain.csproj", "src/BlogApp.Domain/"]
COPY ["src/BlogApp.Application/BlogApp.Application.csproj", "src/BlogApp.Application/"]
COPY ["src/BlogApp.Infrastructure/BlogApp.Infrastructure.csproj", "src/BlogApp.Infrastructure/"]
COPY ["src/BlogApp.API/BlogApp.API.csproj", "src/BlogApp.API/"]
COPY ["src/BlogApp.Worker/BlogApp.Worker.csproj", "src/BlogApp.Worker/"]
COPY ["tests/BlogApp.UnitTests/BlogApp.UnitTests.csproj", "tests/BlogApp.UnitTests/"]

# Restore dependencies
RUN dotnet restore "BlogApp.slnx"

# Copy source code (sensitive files excluded by .dockerignore)
# Note: .dockerignore prevents copying of VAULT.json, .env files, and other secrets
COPY . .

# Security validation: Ensure no sensitive files were copied
RUN echo "Security check: Verifying no sensitive files in container..." && \
    ! find . -name "VAULT.json" -o -name "vault.json" -o -name ".env*" -o -name "*.pfx" -o -name "*.key" | grep -q . && \
    echo "✓ No sensitive files found in container"

# Build the application
RUN dotnet build "BlogApp.slnx" -c Release -o /app/build

# Publish the application
RUN dotnet publish "src/BlogApp.API/BlogApp.API.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Security: Install only essential packages without recommended dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean \
    && apt-get autoremove -y

# Set working directory
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Create non-root user and set ownership
RUN groupadd -r appuser && useradd -r -g appuser appuser && \
    chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "BlogApp.API.dll"]
