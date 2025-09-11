# Deployment Guide

Bu doküman, BlogApp projesini farklı ortamlarda nasıl deploy edeceğinizi açıklar.

## 🚀 Deployment Seçenekleri

### 1. Docker ile Deployment
### 2. Azure App Service
### 3. AWS ECS/Fargate
### 4. Kubernetes
### 5. On-Premises

## 🐳 Docker ile Deployment

### Production Dockerfile
```dockerfile
# Multi-stage build for production
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BlogApp.sln", "./"]
COPY ["src/BlogApp.Domain/BlogApp.Domain.csproj", "src/BlogApp.Domain/"]
COPY ["src/BlogApp.Application/BlogApp.Application.csproj", "src/BlogApp.Application/"]
COPY ["src/BlogApp.Infrastructure/BlogApp.Infrastructure.csproj", "src/BlogApp.Infrastructure/"]
COPY ["src/BlogApp.API/BlogApp.API.csproj", "src/BlogApp.API/"]
COPY ["src/BlogApp.Worker/BlogApp.Worker.csproj", "src/BlogApp.Worker/"]

RUN dotnet restore "BlogApp.sln"
COPY . .
RUN dotnet build "BlogApp.sln" -c Release -o /app/build
RUN dotnet publish "src/BlogApp.API/BlogApp.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "BlogApp.API.dll"]
```

### Production Docker Compose
```yaml
version: '3.8'

services:
  postgres:
    image: postgres:18beta2-alpine
    environment:
      - POSTGRES_USER=${POSTGRES_USER}
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
      - POSTGRES_DB=${POSTGRES_DB}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - app_network
    restart: unless-stopped

  redis:
    image: redis:8.2.0-alpine3.22
    command: redis-server --appendonly yes --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis_data:/data
    networks:
      - app_network
    restart: unless-stopped

  minio:
    image: minio/minio:latest
    environment:
      - MINIO_ROOT_USER=${MINIO_ROOT_USER}
      - MINIO_ROOT_PASSWORD=${MINIO_ROOT_PASSWORD}
    volumes:
      - minio_data:/data
    command: server /data --console-address ":9001"
    networks:
      - app_network
    restart: unless-stopped

  rabbitmq:
    image: rabbitmq:4.1.3-management
    environment:
      - RABBITMQ_DEFAULT_USER=${RABBITMQ_USER}
      - RABBITMQ_DEFAULT_PASS=${RABBITMQ_PASSWORD}
    networks:
      - app_network
    restart: unless-stopped

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:9.1.1
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms1g -Xmx1g"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data
    networks:
      - app_network
    restart: unless-stopped

  kibana:
    image: docker.elastic.co/kibana/kibana:9.1.1
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    networks:
      - app_network
    restart: unless-stopped
    depends_on:
      - elasticsearch

  app:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80;https://+:443
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./ssl:/app/ssl:ro
    networks:
      - app_network
    depends_on:
      - postgres
      - redis
      - minio
      - rabbitmq
      - elasticsearch
    restart: unless-stopped

volumes:
  postgres_data:
  redis_data:
  minio_data:
  elasticsearch_data:

networks:
  app_network:
    driver: bridge
```

### Environment Variables (.env)
```bash
# Database
POSTGRES_USER=blogapp
POSTGRES_PASSWORD=your-secure-password
POSTGRES_DB=blogapp

# Redis
REDIS_PASSWORD=your-secure-redis-password

# MinIO
MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=your-secure-minio-password

# RabbitMQ
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=your-secure-rabbitmq-password

# JWT
JWT_SECRET=your-super-secret-key-here-minimum-32-characters
JWT_VALID_AUDIENCE=https://yourdomain.com
JWT_VALID_ISSUER=https://yourdomain.com

# API
API_URL=https://yourdomain.com
```

## ☁️ Azure App Service Deployment

### 1. Azure CLI ile Deployment
```bash
# Login to Azure
az login

# Create resource group
az group create --name BlogApp-RG --location EastUS

# Create App Service plan
az appservice plan create --name BlogApp-Plan --resource-group BlogApp-RG --sku B1 --is-linux

# Create web app
az webapp create --resource-group BlogApp-RG --plan BlogApp-Plan --name your-blogapp --deployment-local-git

# Configure environment variables
az webapp config appsettings set --resource-group BlogApp-RG --name your-blogapp --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  ConnectionStrings__DefaultConnection="your-connection-string" \
  JWT__Secret="your-jwt-secret"

# Deploy from Git
git remote add azure https://username@your-blogapp.scm.azurewebsites.net:443/your-blogapp.git
git push azure main
```

### 2. Azure DevOps Pipeline
```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

stages:
- stage: Build
  displayName: 'Build and Test'
  jobs:
  - job: Build
    steps:
    - task: UseDotNet@2
      inputs:
        version: '9.0.x'
        includePreviewVersions: false

    - task: DotNetCoreCLI@2
      displayName: 'Restore NuGet packages'
      inputs:
        command: 'restore'
        projects: '$(solution)'

    - task: DotNetCoreCLI@2
      displayName: 'Build solution'
      inputs:
        command: 'build'
        projects: '$(solution)'
        arguments: '--configuration $(buildConfiguration)'

    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        projects: '**/*Tests/*.csproj'
        arguments: '--configuration $(buildConfiguration) --collect "Code coverage"'

    - task: DotNetCoreCLI@2
      displayName: 'Publish'
      inputs:
        command: 'publish'
        publishWebProjects: true
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        zipAfterPublish: true

    - task: PublishBuildArtifacts@1
      displayName: 'Publish artifacts'
      inputs:
        pathToPublish: '$(Build.ArtifactStagingDirectory)'
        artifactName: 'drop'

- stage: Deploy
  displayName: 'Deploy to Azure'
  dependsOn: Build
  condition: succeeded()
  jobs:
  - deployment: Deploy
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'Your-Azure-Subscription'
              appName: 'your-blogapp'
              package: '$(Pipeline.Workspace)/drop/**/*.zip'
```

## 🚢 AWS ECS/Fargate Deployment

### 1. ECS Task Definition
```json
{
  "family": "blogapp",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::account:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::account:role/ecsTaskRole",
  "containerDefinitions": [
    {
      "name": "blogapp",
      "image": "your-account.dkr.ecr.region.amazonaws.com/blogapp:latest",
      "portMappings": [
        {
          "containerPort": 80,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "secrets": [
        {
          "name": "JWT__Secret",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:blogapp/jwt-secret"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/blogapp",
          "awslogs-region": "region",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

### 2. ECS Service
```json
{
  "cluster": "blogapp-cluster",
  "serviceName": "blogapp-service",
  "taskDefinition": "blogapp",
  "desiredCount": 2,
  "launchType": "FARGATE",
  "networkConfiguration": {
    "awsvpcConfiguration": {
      "subnets": ["subnet-12345", "subnet-67890"],
      "securityGroups": ["sg-12345"],
      "assignPublicIp": "ENABLED"
    }
  },
  "loadBalancers": [
    {
      "targetGroupArn": "arn:aws:elasticloadbalancing:region:account:targetgroup/blogapp-tg/12345",
      "containerName": "blogapp",
      "containerPort": 80
    }
  ]
}
```

## ☸️ Kubernetes Deployment

### 1. Namespace
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: blogapp
```

### 2. ConfigMap
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: blogapp-config
  namespace: blogapp
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ConnectionStrings__DefaultConnection: "Host=postgres-service;Database=blogapp;Username=blogapp;Password=blogapp123"
```

### 3. Secret
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: blogapp-secrets
  namespace: blogapp
type: Opaque
data:
  JWT__Secret: <base64-encoded-jwt-secret>
  Redis__Password: <base64-encoded-redis-password>
```

### 4. Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: blogapp
  namespace: blogapp
spec:
  replicas: 3
  selector:
    matchLabels:
      app: blogapp
  template:
    metadata:
      labels:
        app: blogapp
    spec:
      containers:
      - name: blogapp
        image: your-registry/blogapp:latest
        ports:
        - containerPort: 80
        envFrom:
        - configMapRef:
            name: blogapp-config
        - secretRef:
            name: blogapp-secrets
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
```

### 5. Service
```yaml
apiVersion: v1
kind: Service
metadata:
  name: blogapp-service
  namespace: blogapp
spec:
  selector:
    app: blogapp
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: ClusterIP
```

### 6. Ingress
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: blogapp-ingress
  namespace: blogapp
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  tls:
  - hosts:
    - yourdomain.com
    secretName: blogapp-tls
  rules:
  - host: yourdomain.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: blogapp-service
            port:
              number: 80
```

## 🔒 SSL/TLS Konfigürasyonu

### Let's Encrypt ile SSL
```bash
# Certbot kurulumu
sudo apt-get update
sudo apt-get install certbot

# SSL sertifikası alma
sudo certbot certonly --standalone -d yourdomain.com

# Nginx konfigürasyonu
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com;
    
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## 📊 Monitoring ve Logging

### Application Insights (Azure)
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// appsettings.json
{
  "ApplicationInsights": {
    "ConnectionString": "your-connection-string"
  }
}
```

### Prometheus + Grafana
```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'blogapp'
    static_configs:
      - targets: ['localhost:5000']
    metrics_path: '/metrics'
```

### ELK Stack
```yaml
# docker-compose.monitoring.yml
version: '3.8'
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:9.1.1
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data

  logstash:
    image: docker.elastic.co/logstash/logstash:9.1.1
    ports:
      - "5044:5044"
    volumes:
      - ./logstash/pipeline:/usr/share/logstash/pipeline

  kibana:
    image: docker.elastic.co/kibana/kibana:9.1.1
    ports:
      - "5601:5601"
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200

volumes:
  elasticsearch_data:
```

## 🚨 Security Checklist

- [ ] HTTPS zorunlu
- [ ] JWT secret güvenli
- [ ] Database connection string güvenli
- [ ] Environment variables güvenli
- [ ] Firewall kuralları
- [ ] Rate limiting aktif
- [ ] CORS policy sıkı
- [ ] Security headers aktif
- [ ] Input validation aktif
- [ ] Anti-forgery protection aktif
- [ ] Audit logging aktif
- [ ] Regular security updates
- [ ] Backup strategy
- [ ] Disaster recovery plan

## 📈 Performance Optimization

### 1. Caching
- Redis cache aktif
- Response caching
- Static file caching

### 2. Database
- Connection pooling
- Query optimization
- Index optimization

### 3. Application
- Async/await kullanımı
- Memory optimization
- Garbage collection tuning

### 4. Infrastructure
- Load balancing
- Auto-scaling
- CDN kullanımı

## 🔄 CI/CD Pipeline

### GitHub Actions
```yaml
name: Build and Deploy

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: blogapp
        path: ./publish

  deploy:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
    - name: Download artifact
      uses: actions/download-artifact@v3
      with:
        name: blogapp
    
    - name: Deploy to server
      run: |
        # Deploy logic here
        echo "Deploying to production..."
```

## 📞 Support

Deployment sırasında sorun yaşarsanız:

1. **Logs**: Application ve system loglarını kontrol edin
2. **Health Checks**: `/health` endpoint'ini kontrol edin
3. **Dependencies**: Tüm servislerin çalıştığından emin olun
4. **Configuration**: Environment variables'ları kontrol edin
5. **Network**: Firewall ve network ayarlarını kontrol edin

---

Bu rehber ile BlogApp projenizi başarıyla deploy edebilirsiniz! 🎉
