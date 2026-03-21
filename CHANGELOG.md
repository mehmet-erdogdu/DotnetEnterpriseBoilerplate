# Changelog

Bu dosya, BlogApp projesinde yapılan tüm önemli değişiklikleri belgeler.

## [Unreleased]

### Added
- Modern .NET 10 desteği
- Clean Architecture yapısı
- JWT Authentication sistemi
- PostgreSQL veritabanı desteği
- Redis cache entegrasyonu
- MinIO S3 dosya depolama
- Firebase Notifications
- RabbitMQ mesaj kuyruğu
- Elasticsearch arama motoru
- SonarQube kod kalite analizi
- Docker container desteği
- Rate limiting güvenlik özelliği
- Audit logging sistemi
- Çoklu dil desteği (Türkçe/İngilizce)
- Comprehensive unit test coverage
- Security headers ve middleware'ler
- Anti-forgery protection
- Input validation middleware
- Global exception handling
- Correlation ID tracking
- Request logging
- Password history validation
- Refresh token sistemi

### Changed
- Proje yapısı Clean Architecture prensiplerine göre yeniden düzenlendi
- Tüm uygulama .NET 10'a yükseltildi
- Entity Framework Core 9 kullanılıyor
- Serilog logging sistemi entegre edildi
- Vault secret management sistemi eklendi

### Fixed
- Build uyarıları temizlendi
- Async/await kullanımı optimize edildi
- Exception handling iyileştirildi
- Null reference uyarıları giderildi

### Security
- JWT token güvenliği artırıldı
- Rate limiting eklendi
- CORS policy sıkılaştırıldı
- Security headers eklendi
- Input validation güçlendirildi
- Anti-forgery protection eklendi

## [1.0.0] - 2024-12-19

### Added
- İlk sürüm
- Temel blog uygulaması özellikleri
- Kullanıcı yönetimi
- Post yönetimi
- Todo yönetimi
- Dosya yükleme sistemi
- Push notification sistemi

---

## Sürüm Numaralandırma

Bu proje [Semantic Versioning](https://semver.org/) kullanır:

- **MAJOR**: Uyumsuz API değişiklikleri
- **MINOR**: Geriye uyumlu yeni özellikler
- **PATCH**: Geriye uyumlu hata düzeltmeleri

## Değişiklik Türleri

- **Added**: Yeni özellikler
- **Changed**: Mevcut özelliklerde değişiklikler
- **Deprecated**: Kullanımdan kaldırılacak özellikler
- **Removed**: Kaldırılan özellikler
- **Fixed**: Hata düzeltmeleri
- **Security**: Güvenlik iyileştirmeleri
