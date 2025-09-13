# BlogApp - Modern .NET 9 Blog Uygulaması

Bu proje, modern .NET 9 teknolojileri kullanılarak geliştirilmiş, güvenli ve ölçeklenebilir bir blog uygulamasıdır.

## 🚀 Özellikler

- **Modern .NET 9** - En güncel .NET teknolojileri
- **Clean Architecture** - Katmanlı mimari yapısı
- **JWT Authentication** - Güvenli kimlik doğrulama
- **PostgreSQL** - Güçlü veritabanı desteği
- **Redis Cache** - Hızlı önbellekleme
- **MinIO S3** - Dosya depolama çözümü
- **Firebase Notifications** - Push notification desteği
- **RabbitMQ** - Mesaj kuyruğu sistemi
- **Elasticsearch** - Arama ve log analizi
- **Docker Support** - Container desteği
- **Rate Limiting** - API güvenliği
- **Audit Logging** - Kapsamlı loglama
- **Multi-language Support** - Çoklu dil desteği

## 🏗️ Proje Yapısı

```
src/
├── BlogApp.API/           # Web API katmanı
├── BlogApp.Application/   # İş mantığı katmanı
├── BlogApp.Domain/        # Domain model katmanı
├── BlogApp.Infrastructure/# Altyapı katmanı
└── BlogApp.Worker/        # Background worker

tests/
└── BlogApp.UnitTests/     # Birim testler
```

## 🛠️ Teknolojiler

- **Backend**: .NET 9, ASP.NET Core
- **Database**: PostgreSQL, Entity Framework Core
- **Cache**: Redis
- **File Storage**: MinIO S3
- **Message Queue**: RabbitMQ
- **Search**: Elasticsearch
- **Monitoring**: SonarQube
- **Container**: Docker, Docker Compose

## 📋 Gereksinimler

- .NET 9 SDK
- Docker & Docker Compose
- PostgreSQL (opsiyonel - Docker ile gelir)
- Redis (opsiyonel - Docker ile gelir)

## 🚀 Kurulum

### 1. Projeyi Klonlayın
```bash
git clone <repository-url>
cd ai-blogApp
```

### 2. Docker ile Çalıştırın
```bash
docker-compose up -d
```

### 3. Veritabanını Güncelleyin ve Seed Data Ekleyin
```bash
dotnet ef database update --project src/BlogApp.Infrastructure/BlogApp.Infrastructure.csproj

dotnet run --project src/BlogApp.Seeder/BlogApp.Seeder.csproj
```

### 4. Uygulamayı Çalıştırın
```bash
dotnet run --project src/BlogApp.API/BlogApp.API.csproj
```

## 🔧 Konfigürasyon

Proje, HashiCorp Vault kullanarak güvenli konfigürasyon yönetimi yapar. Gerekli environment variable'lar:

- `VAULT_ADDR`: Vault sunucu adresi
- `VAULT_TOKEN`: Vault authentication token
- `BLOG_APP`: Vault secret path

## 📚 API Dokümantasyonu

Uygulama çalıştıktan sonra Swagger UI'a erişebilirsiniz:
- **Swagger UI**: `https://localhost:5001/swagger`
- **API Base URL**: `https://localhost:5001/api`

## 🧪 Test

```bash
dotnet test
```

## 🌱 Veri Doldurma (Seeding)

Uygulama, geliştirme ortamında kullanılmak üzere örnek verilerle veritabanını doldurma özelliğine sahiptir. Bu özellik sayesinde uygulamayı çalıştırdığınızda otomatik olarak örnek kullanıcılar, blog yazıları ve yapılacaklar listesi oluşturulur.

### Seed Edilen Veriler

- **Kullanıcılar**: Admin ve normal kullanıcı hesapları
- **Blog Yazıları**: Örnek blog yazıları
- **Yapılacaklar**: Örnek yapılacaklar listesi

### Seed İşlemini Manuel Olarak Çalıştırma

```bash
dotnet run --project src/BlogApp.Seeder/BlogApp.Seeder.csproj
```

Bu işlem, veritabanında henüz veri yoksa örnek verileri oluşturacaktır.

## 📊 Monitoring

- **SonarQube**: `http://localhost:9000`
- **Kibana**: `http://localhost:5601`
- **Elasticsearch**: `http://localhost:9200`
- **RabbitMQ Management**: `http://localhost:15672`
- **MinIO Console**: `http://localhost:9003`

## 🔒 Güvenlik

- JWT token tabanlı authentication
- Rate limiting
- CORS policy
- Security headers
- Input validation
- Anti-forgery protection
- Password history validation

---

# Elasticsearch & Vault Setup

Bu proje Serilog ile Elasticsearch entegrasyonu içerir.

## 🚀 Kurulum

### 1. Docker Compose ile Vault, Elasticsearch ve Kibana Başlatma

```bash
# Vault, Elasticsearch ve Kibana'yı başlat
docker compose up -d

# Servislerin durumunu kontrol et
docker-compose ps
```

### 2. Servisler

- **Elasticsearch**: http://localhost:9200
- **Kibana**: http://localhost:5601
- **Vault (UI)**: http://localhost:8200 (Token: `root` — sadece local/dev içindir)

### 3. Vault KV v2 ve Secret Ekleme

```bash
export VAULT_ADDR=http://127.0.0.1:8200
export VAULT_TOKEN=root

# KV v2 mount (varsa hata verirse ignore edebilirsiniz)
curl -H "X-Vault-Token: $VAULT_TOKEN" -H "Content-Type: application/json" \
  -X POST $VAULT_ADDR/v1/sys/mounts/secret -d '{"type":"kv","options":{"version":"2"}}' || true

# Uygulama secret'larını yaz (JWT:Secret)
curl -H "X-Vault-Token: $VAULT_TOKEN" -H "Content-Type: application/json" \
  -X POST $VAULT_ADDR/v1/secret/data/blogapp \
  -d '{"data":{"JWT:Secret":"ReplaceWithStrongRandomBase64Key"}}'
```

### 4. API'yi Çalıştırma

```bash
cd src/BlogApp.API
dotnet run --urls "https://localhost:7266"
```

## 📊 Log Yapısı

### Elasticsearch Index Formatı

- Index: `blogapp-logs-YYYY-MM`
- Örnek: `blogapp-logs-2025-08`

### Log Alanları

- `@timestamp`: Log zamanı
- `level`: Log seviyesi (Warning, Error, Information)
- `message`: Log mesajı
- `fields`: Ek alanlar
    - `MachineName`: Makine adı
    - `ThreadId`: Thread ID
    - `CorrelationId`: Request correlation ID
    - `UserId`: Kullanıcı ID (varsa)
    - `RequestPath`: Request path
    - `RequestId`: Request ID

## 🔍 Log Sorgulama

### Elasticsearch API ile

```bash
# Tüm logları getir
curl "http://localhost:9200/blogapp-logs-*/_search?pretty"

# Belirli seviyedeki logları getir
curl "http://localhost:9200/blogapp-logs-*/_search?pretty" -H 'Content-Type: application/json' -d'
{
  "query": {
    "match": {
      "level": "Warning"
    }
  }
}'

# Son 1 saatteki logları getir
curl "http://localhost:9200/blogapp-logs-*/_search?pretty" -H 'Content-Type: application/json' -d'
{
  "query": {
    "range": {
      "@timestamp": {
        "gte": "now-1h"
      }
    }
  }
}'
```

### Kibana ile

1. http://localhost:5601 adresine git
2. "Discover" sekmesine tıkla
3. Index pattern: `blogapp-logs-*` seç
4. Logları görüntüle ve filtrele

---

# File Upload System with MinIO S3

Bu dokümantasyon, BlogApp'e eklenen dosya yükleme sistemi hakkında bilgi verir. Sistem MinIO S3 uyumlu object storage kullanır.

## Özellikler

- **Dosya Yükleme**: Kullanıcılar dosya yükleyebilir
- **Dosya İndirme**: Yüklenen dosyalar indirilebilir
- **Dosya Yönetimi**: Kullanıcılar kendi dosyalarını görüntüleyebilir ve silebilir
- **Güvenlik**: Dosyalar sadece sahipleri tarafından silinebilir
- **Pre-signed URL**: Geçici erişim linkleri oluşturulabilir
- **Post Banner Images**: Post'lara banner resmi eklenebilir

## Teknoloji Stack

- **MinIO**: S3 uyumlu object storage
- **AWS SDK S3**: MinIO ile iletişim için
- **Entity Framework**: Dosya metadata'sı için veritabanı
- **JWT Authentication**: Güvenli erişim

## Veritabanı Yapısı

### FileEntity Tablosu

```sql
CREATE TABLE Files (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    FileName NVARCHAR(MAX) NOT NULL,
    OriginalFileName NVARCHAR(MAX) NOT NULL,
    ContentType NVARCHAR(MAX) NOT NULL,
    FileSize BIGINT NOT NULL,
    FilePath NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX),
    UploadedById NVARCHAR(450),
    CreatedById NVARCHAR(450) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedById NVARCHAR(450),
    UpdatedAt DATETIME2,
    CreatedByIp NVARCHAR(MAX),
    UpdatedByIp NVARCHAR(MAX),
    CreatedByUserAgent NVARCHAR(MAX),
    UpdatedByUserAgent NVARCHAR(MAX)
);
```

## API Endpoints

### Dosya Yükleme

```http
POST /api/files/upload
Content-Type: multipart/form-data
Authorization: Bearer {token}

{
  "file": [binary data],
  "description": "Optional description"
}
```

### Dosya Bilgisi Alma

```http
GET /api/files/{fileId}
Authorization: Bearer {token}
```

### Dosya İndirme

```http
GET /api/files/{fileId}/download
Authorization: Bearer {token}
```

### Pre-signed URL Alma

```http
GET /api/files/{fileId}/url
Authorization: Bearer {token}
```

### Dosya Silme

```http
DELETE /api/files/{fileId}
Authorization: Bearer {token}
```

### Kullanıcının Dosyalarını Listeleme

```http
GET /api/files/my-files
Authorization: Bearer {token}
```

## Docker Kurulumu

### MinIO Container

```yaml
minio:
  image: minio/minio:latest
  container_name: minio
  environment:
    - MINIO_ROOT_USER=minioadmin
    - MINIO_ROOT_PASSWORD=minioadmin123
  ports:
    - "9002:9000"
    - "9003:9001"
  volumes:
    - minio_data:/data
  command: server /data --console-address ":9001"
```

### MinIO Console Erişimi

- **URL**: http://localhost:9003
- **Kullanıcı**: minioadmin
- **Şifre**: minioadmin123

---

# Firebase Push Notification Setup

Bu dokümanda Firebase Push Notification servisinin BlogApp projesine nasıl kurulduğu açıklanmaktadır.

## 🚀 Kurulum Adımları

### 1. Firebase Console'da Proje Oluşturma

1. [Firebase Console](https://console.firebase.google.com/) adresine gidin
2. "Create a project" butonuna tıklayın
3. Proje adını girin (örn: "blog-app-notifications")
4. Google Analytics'i etkinleştirin (isteğe bağlı)
5. "Create project" butonuna tıklayın

### 2. Firebase Admin SDK Kurulumu

1. Firebase Console'da proje seçin
2. Sol menüden "Project settings" seçin
3. "Service accounts" sekmesine gidin
4. "Generate new private key" butonuna tıklayın
5. JSON dosyasını indirin ve güvenli bir yere kaydedin

### 3. Proje Konfigürasyonu

1. İndirilen JSON dosyasını `src/BlogApp.API/` klasörüne kopyalayın
2. `appsettings.json` dosyasına Firebase konfigürasyonunu ekleyin:

```json
{
  "Firebase": {
    "ProjectId": "your-firebase-project-id",
    "ServiceAccountKeyPath": "serviceAccountKey.json"
  }
}
```

## 📱 API Endpoints

### Notification Gönderme

#### Tekil Cihazlara Bildirim

```http
POST /api/firebasenotification/send
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "tokenIds": ["device-token-1", "device-token-2"],
  "notification": {
    "title": "Yeni Blog Yazısı",
    "body": "Yeni bir blog yazısı yayınlandı!",
    "imageUrl": "https://example.com/image.jpg",
    "clickAction": "OPEN_POST",
    "sound": "default",
    "priority": true
  },
  "data": {
    "postId": "123",
    "type": "new_post"
  }
}
```

#### Topic'e Bildirim

```http
POST /api/firebasenotification/send-topic
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "topic": "blog_updates",
  "notification": {
    "title": "Blog Güncellemesi",
    "body": "Blog'da yeni güncellemeler var!"
  },
  "data": {
    "updateType": "maintenance"
  }
}
```

---

# Presigned URL File Upload

Bu dokümanda, S3/MinIO presigned URL kullanarak doğrudan dosya upload'ı yapma süreci açıklanmaktadır.

## Avantajlar

- **Performans**: Dosya trafiği API sunucusundan geçmez
- **Ölçeklenebilirlik**: API sunucusu yükü azalır
- **Güvenlik**: Zaman sınırlı URL'ler ile güvenli upload
- **Bant Genişliği**: API sunucusu bant genişliği tasarrufu

## Upload Süreci

### 1. Presigned URL Alma

```http
POST /api/files/presigned-upload-url
Authorization: Bearer {token}
Content-Type: application/json

{
  "fileName": "example.jpg",
  "contentType": "image/jpeg",
  "fileSize": 1024000,
  "description": "Örnek resim dosyası"
}
```

### 2. Dosyayı S3'e Upload Etme

```javascript
// JavaScript örneği
const uploadFile = async (file, presignedUrl) => {
  const response = await fetch(presignedUrl, {
    method: 'PUT',
    body: file,
    headers: {
      'Content-Type': file.type
    }
  });
  
  if (response.ok) {
    console.log('Dosya başarıyla upload edildi');
  }
};
```

### 3. Upload'ı Tamamlama

```http
POST /api/files/complete-upload
Authorization: Bearer {token}
Content-Type: application/json

{
  "fileId": "123e4567-e89b-12d3-a456-426614174000",
  "actualFileSize": 1024000
}
```

---

# Redis Cache Implementation

Bu proje, BlogApp için Redis cache implementasyonu içerir. Cache sistemi, performansı artırmak ve veritabanı yükünü azaltmak için kullanılır.

## Özellikler

- **Redis Cache Service**: Temel cache operasyonları (Get, Set, Remove, Exists, Increment)
- **Cache Invalidation Service**: Veri değişikliklerinde cache temizleme
- **GetPostById Caching**: Post detayları için cache implementasyonu
- **Automatic Cache Invalidation**: Post oluşturma, güncelleme ve silme işlemlerinde otomatik cache temizleme

## Kurulum

### 1. Docker ile Redis Başlatma

```bash
# Redis container'ını başlat
docker-compose up redis -d

# Tüm servisleri başlat (Redis dahil)
docker-compose up -d
```

### 2. Redis Bağlantı Bilgileri

- **Host**: localhost
- **Port**: 6379
- **Password**: redis123
- **Instance Name**: BlogApp_

## Cache Yapısı

### Cache Keys

Cache key'leri aşağıdaki formatta oluşturulur:

```
posts:{postId}                    # Tekil post cache
posts:list:{page}:{pageSize}      # Post listesi cache
todos:{todoId}                    # Tekil todo cache
todos:list:{page}:{pageSize}      # Todo listesi cache
user:{userId}                     # Kullanıcı cache
user:{userId}:posts               # Kullanıcının postları cache
auth:refresh:{token}              # Refresh token cache
```

### Cache Expiration

- **Posts**: 30 dakika
- **Todos**: 15 dakika
- **User**: 60 dakika
- **Auth**: 5 dakika

---

# API Controller Extensions

This directory contains extension methods for controllers to provide consistent validation error handling across the entire API.

## Features

### Standardized Validation Error Handling

All controllers can now use consistent validation error handling that returns HTTP 200 responses with validation errors embedded in the `ApiResponse` error field, instead of
returning HTTP 400 responses.

## Usage

### Method 1: Direct Validation Check

```csharp
public async Task<ApiResponse<string>> Register([FromBody] RegisterDto model)
{
    if (!ModelState.IsValid)
        return this.CreateValidationErrorResponse<string>(ModelState);
    
    // Continue with business logic...
}
```

### Method 2: Try Pattern (Alternative)

```csharp
public async Task<ApiResponse<string>> Register([FromBody] RegisterDto model)
{
    if (this.TryGetValidationErrorResponse<string>(ModelState, out var validationResponse))
        return validationResponse;
    
    // Continue with business logic...
}
```

## Response Format

When validation fails, the response will be:

```json
{
  "data": null,
  "isSuccess": false,
  "error": {
    "message": "Validation failed",
    "code": "VALIDATION_ERROR",
    "details": {
      "Email": ["The Email field is required."],
      "Password": ["The Password field is required.", "Password must be at least 8 characters long."]
    }
  }
}
```

---

# Frontend React Application

Modern React frontend uygulaması, Blog App API'si ile entegre çalışır.

## Özellikler

- 🔐 **Authentication**: JWT tabanlı kimlik doğrulama
- 📝 **Posts Management**: Blog yazıları oluşturma, düzenleme, silme
- ✅ **Todos Management**: Görev yönetimi
- 📁 **File Management**: Dosya yükleme ve indirme
- 🎨 **Modern UI**: Material-UI ile responsive tasarım
- 🌙 **Theme Support**: Açık/koyu tema desteği
- 📱 **Responsive**: Mobil uyumlu tasarım

## Teknolojiler

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Material-UI (MUI)** - UI component library
- **React Router** - Client-side routing
- **Axios** - HTTP client
- **date-fns** - Date formatting

## Kurulum

### Gereksinimler

- Node.js (v16 veya üzeri)
- npm veya yarn

### Adımlar

1. **Bağımlılıkları yükleyin:**
   ```bash
   npm install
   ```

2. **Environment değişkenlerini ayarlayın:**
   `.env` dosyası oluşturun:
   ```env
   REACT_APP_API_URL=https://localhost:7266/api
   ```

3. **Uygulamayı başlatın:**
   ```bash
   npm start
   ```

4. **Tarayıcıda açın:**
   ```
   http://localhost:3000
   ```

## API Entegrasyonu

Bu frontend, aşağıdaki API endpoint'leri ile entegre çalışır:

### Authentication
- `POST /api/auth/login` - Giriş
- `POST /api/auth/register` - Kayıt
- `POST /api/auth/refresh` - Token yenileme
- `POST /api/auth/change-password` - Şifre değiştirme

### Posts
- `GET /api/posts` - Tüm yazıları listele
- `GET /api/posts/{id}` - Yazı detayı
- `POST /api/posts` - Yeni yazı oluştur
- `PUT /api/posts/{id}` - Yazı güncelle
- `DELETE /api/posts/{id}` - Yazı sil

### Todos
- `GET /api/todos` - Tüm görevleri listele
- `GET /api/todos/{id}` - Görev detayı
- `POST /api/todos` - Yeni görev oluştur
- `PUT /api/todos/{id}` - Görev güncelle
- `DELETE /api/todos/{id}` - Görev sil

### Files
- `GET /api/files/my-files` - Kullanıcının dosyalarını listele
- `POST /api/files/presigned-upload-url` - Upload URL al
- `POST /api/files/complete-upload` - Upload'ı tamamla
- `GET /api/files/{id}/download` - Dosya indir
- `DELETE /api/files/{id}` - Dosya sil

## Proje Yapısı

```
src/
├── components/          # Yeniden kullanılabilir bileşenler
│   ├── Layout.tsx      # Ana layout
│   └── ProtectedRoute.tsx # Korumalı route
├── contexts/           # React context'leri
│   ├── AuthContext.tsx # Kimlik doğrulama context'i
│   └── ThemeContext.tsx # Tema context'i
├── pages/              # Sayfa bileşenleri
│   ├── Login.tsx       # Giriş sayfası
│   ├── Register.tsx    # Kayıt sayfası
│   ├── Dashboard.tsx   # Ana sayfa
│   ├── Posts.tsx       # Yazılar listesi
│   ├── CreatePost.tsx  # Yazı oluşturma
│   ├── EditPost.tsx    # Yazı düzenleme
│   ├── PostDetail.tsx  # Yazı detayı
│   ├── Todos.tsx       # Görevler
│   ├── Files.tsx       # Dosyalar
│   └── Profile.tsx     # Profil
├── services/           # API servisleri
│   └── api.ts         # API client
├── App.tsx            # Ana uygulama bileşeni
└── index.tsx          # Giriş noktası
```

---

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📞 İletişim

Proje hakkında sorularınız için issue açabilirsiniz.

