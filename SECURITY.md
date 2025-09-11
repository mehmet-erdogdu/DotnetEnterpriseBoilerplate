# Güvenlik Dokümantasyonu

Bu dokümantasyon, BlogApp API'sinin güvenlik özelliklerini ve önlemlerini detaylandırır.

## 🔒 Güvenlik Özellikleri

### 1. Kimlik Doğrulama ve Yetkilendirme

#### JWT Token Güvenliği

- **Token Süresi**: Production'da 60 dakika, Development'da 120 dakika
- **Refresh Token**: 7 gün geçerli
- **Güçlü Şifreleme**: HMAC-SHA256 algoritması
- **Clock Skew**: Sıfır tolerans (token süresi sıkı kontrol)

#### Şifre Politikası

- **Minimum Uzunluk**: 12 karakter
- **Karmaşıklık**: Büyük/küçük harf, rakam ve özel karakter zorunlu
- **Benzersiz Karakter**: En az 1 benzersiz karakter
- **Şifre Geçmişi**: Son 5 şifre tekrar kullanılamaz

#### Hesap Kilitleme

- **Maksimum Deneme**: 5 başarısız giriş (Production), 10 (Development)
- **Kilitleme Süresi**: 15 dakika (Production), 5 dakika (Development)

### 2. API Güvenliği

#### Rate Limiting

- **Global Limit**: 100 istek/dakika
- **Auth Endpoints**: 10 istek/5 dakika
- **Queue Limit**: 2 bekleyen istek
- **429 Status**: Rate limit aşıldığında

#### CORS Politikası

- **Sadece Belirli Origin**: https://localhost:7266
- **HTTP Metodları**: GET, POST, PUT, DELETE
- **Headers**: Authorization, Content-Type, X-Correlation-ID
- **Credentials**: Allow
- **Preflight Cache**: 1 saat

#### Input Validation

- **Content-Type Kontrolü**: Sadece JSON ve form-data
- **Payload Boyutu**: Maksimum 1MB
- **SQL Injection Koruması**: Tehlikeli pattern'ler filtrelenir
- **XSS Koruması**: Script tag'leri ve event handler'lar filtrelenir
- **Path Traversal Koruması**: ../ pattern'leri filtrelenir

### 3. HTTP Güvenlik Headers

#### Güvenlik Başlıkları

- **X-Content-Type-Options**: nosniff
- **X-Frame-Options**: DENY
- **X-XSS-Protection**: 1; mode=block
- **Referrer-Policy**: strict-origin-when-cross-origin
- **Permissions-Policy**: Geolocation, microphone, camera, payment, USB erişimi engellendi

#### Content Security Policy (CSP)

```
default-src 'self';
script-src 'self';
style-src 'self';
img-src 'self' data:;
font-src 'self';
connect-src 'self';
frame-ancestors 'none';
base-uri 'self';
form-action 'self';
upgrade-insecure-requests;
```

#### Ek Güvenlik Headers

- **Strict-Transport-Security**: HTTPS zorunlu (1 yıl)
- **X-Permitted-Cross-Domain-Policies**: none
- **Cross-Origin-Embedder-Policy**: require-corp
- **Cross-Origin-Opener-Policy**: same-origin
- **Cross-Origin-Resource-Policy**: same-origin

### 4. CSRF Koruması

#### Anti-Forgery Middleware

- **Origin Kontrolü**: Sadece güvenilir origin'ler
- **Referer Kontrolü**: Geçerli referer header'ı zorunlu
- **State-Changing Operations**: POST, PUT, DELETE, PATCH için kontrol

### 5. Logging ve Monitoring

#### Güvenli Logging

- **Hassas Veri Filtreleme**: Password, token, secret alanları maskeleme
- **Auth Endpoints**: Tamamen redacted
- **IP Adresi**: Proxy-aware IP tespiti
- **Request/Response Body**: Sanitized logging

#### Audit Logging

- **Kullanıcı Eylemleri**: Tüm CRUD operasyonları loglanır
- **Timestamp**: UTC zaman damgası
- **User ID**: Kimlik doğrulama bilgisi
- **IP Adresi**: Client IP adresi

### 6. Veri Güvenliği

#### Authorization Kontrolü

- **Resource Ownership**: Sadece kaynak sahibi düzenleyebilir/silebilir
- **User ID Validation**: Her istek için kullanıcı kimliği doğrulanır
- **Forbidden Access**: Yetkisiz erişim 403 ile reddedilir

#### Input Sanitization

- **HTML Encoding**: Özel karakterler encode edilir
- **SQL Injection**: Tehlikeli SQL komutları filtrelenir
- **XSS Prevention**: Script ve event handler'lar temizlenir

## 🚀 Güvenlik Konfigürasyonu

### Environment Variables

```bash
# JWT Configuration
JWT__Secret=YourSuperSecretKeyHereMakeItLongAndComplex123456789
JWT__ValidAudience=https://yourdomain.com
JWT__ValidIssuer=https://yourdomain.com
JWT__TokenExpirationMinutes=60
JWT__RefreshTokenExpirationDays=7

# Security Configuration
Security__RequireHttps=true
Security__MaxLoginAttempts=5
Security__LockoutDurationMinutes=15
Security__PasswordHistoryCount=5

# Rate Limiting
RateLimiting__GlobalLimit=100
RateLimiting__WindowMinutes=1
RateLimiting__QueueLimit=2
RateLimiting__AuthLimit=10
RateLimiting__AuthWindowMinutes=5
```

### Production Güvenlik Ayarları

```json
{
  "Security": {
    "RequireHttps": true,
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 15,
    "PasswordHistoryCount": 5
  },
  "JWT": {
    "TokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

## 🧪 Güvenlik Testleri

### Test Coverage

- **Security Headers**: Tüm güvenlik başlıkları test edilir
- **Input Validation**: Büyük payload'lar ve geçersiz content-type'lar reddedilir
- **CSRF Protection**: Geçersiz origin'ler reddedilir
- **Rate Limiting**: Limit aşımları kontrol edilir
- **Authorization**: Resource ownership kontrol edilir

### Test Çalıştırma

```bash
dotnet test tests/BlogApp.UnitTests/Security/
```

## 🔍 Güvenlik Tarama Araçları

### Önerilen Araçlar

- **OWASP ZAP**: Web uygulama güvenlik tarayıcısı
- **SonarQube**: Kod kalitesi ve güvenlik analizi
- **Snyk**: Dependency güvenlik taraması
- **Bandit**: Python güvenlik linter (eğer Python kullanılıyorsa)

### Güvenlik Tarama Komutları

```bash
# OWASP ZAP ile tarama
zap-cli quick-scan --self-contained https://localhost:7266

# SonarQube analizi
dotnet sonarscanner begin
dotnet build
dotnet sonarscanner end
```

## 📋 Güvenlik Checklist

### Development

- [ ] JWT secret environment variable'da
- [ ] HTTPS development'da aktif
- [ ] Rate limiting test edildi
- [ ] Input validation çalışıyor
- [ ] Security headers set ediliyor

### Production

- [ ] HTTPS zorunlu
- [ ] Güçlü JWT secret
- [ ] Rate limiting aktif
- [ ] CORS policy kısıtlayıcı
- [ ] Audit logging aktif
- [ ] Security headers set ediliyor
- [ ] Input validation aktif
- [ ] CSRF protection aktif

### Monitoring

- [ ] Güvenlik logları izleniyor
- [ ] Rate limiting metrics
- [ ] Failed authentication attempts
- [ ] Suspicious IP addresses
- [ ] Large payload attempts

## 🚨 Güvenlik Uyarıları

### Kritik Güvenlik Önlemleri

1. **JWT Secret**: Asla kod içinde hardcode etmeyin
2. **HTTPS**: Production'da mutlaka kullanın
3. **Input Validation**: Tüm kullanıcı girdilerini validate edin
4. **Authorization**: Her resource access'i kontrol edin
5. **Logging**: Hassas verileri asla loglamayın

### Güvenlik Güncellemeleri

- Düzenli olarak dependency'leri güncelleyin
- Güvenlik patch'lerini takip edin
- OWASP Top 10 güncellemelerini izleyin
- Güvenlik taramalarını düzenli yapın

## 📞 Güvenlik İletişimi

### Güvenlik Sorunları

Güvenlik açığı bulduysanız, lütfen:

1. **Responsible Disclosure** prensibini takip edin
2. Güvenlik ekibi ile iletişime geçin
3. Detaylı bilgi ve repro steps sağlayın
4. Açığın public olmasını bekleyin

### İletişim Bilgileri

- **Email**: security@yourdomain.com
- **PGP Key**: [Güvenlik ekibi PGP anahtarı]
- **Response Time**: 24-48 saat içinde yanıt

---

**Not**: Bu dokümantasyon sürekli güncellenmektedir. En güncel versiyon için repository'yi kontrol edin.
