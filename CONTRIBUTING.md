# Katkıda Bulunma Rehberi

BlogApp projesine katkıda bulunmak istediğiniz için teşekkürler! Bu rehber, projeye nasıl katkıda bulunacağınızı açıklar.

## 🚀 Başlamadan Önce

1. Projeyi fork edin
2. Yerel makinenizde clone edin
3. Gerekli araçları yükleyin:
   - .NET 10 SDK
   - Docker & Docker Compose
   - Git

## 🔧 Geliştirme Ortamı Kurulumu

### 1. Projeyi Clone Edin
```bash
git clone https://github.com/YOUR_USERNAME/ai-blogApp.git
cd ai-blogApp
```

### 2. Gerekli Servisleri Başlatın
```bash
docker-compose up -d
```

### 3. Veritabanını Güncelleyin
```bash
dotnet ef database update --project src/BlogApp.Infrastructure/BlogApp.Infrastructure.csproj
```

### 4. Projeyi Build Edin
```bash
dotnet build
```

### 5. Testleri Çalıştırın
```bash
dotnet test
```

## 📝 Kod Yazma Kuralları

### C# Kod Standartları
- **Naming**: PascalCase (sınıflar, methodlar), camelCase (değişkenler)
- **Indentation**: 4 spaces
- **Line Length**: Maksimum 120 karakter
- **Comments**: XML documentation kullanın

### Örnek Kod Formatı
```csharp
/// <summary>
/// Kullanıcı bilgilerini getirir
/// </summary>
/// <param name="userId">Kullanıcı ID'si</param>
/// <returns>Kullanıcı bilgileri</returns>
public async Task<UserDto> GetUserAsync(int userId)
{
    // Implementation
}
```

### Architecture Kuralları
- Clean Architecture prensiplerine uyun
- Dependency Injection kullanın
- SOLID prensiplerini takip edin
- Unit test yazın

## 🧪 Test Yazma

### Test Kuralları
- Her public method için test yazın
- Test method isimleri açıklayıcı olmalı
- Arrange-Act-Assert pattern'ini kullanın
- Mock'ları doğru şekilde kullanın

### Test Örneği
```csharp
[Fact]
public async Task Handle_ValidRequest_ReturnsSuccessResponse()
{
    // Arrange
    var command = new CreatePostCommand { Title = "Test Post" };
    var handler = new CreatePostCommandHandler(mockService.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
}
```

## 🔄 Pull Request Süreci

### 1. Feature Branch Oluşturun
```bash
git checkout -b feature/your-feature-name
```

### 2. Değişikliklerinizi Commit Edin
```bash
git add .
git commit -m "feat: add new feature description"
```

### 3. Commit Message Formatı
```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat`: Yeni özellik
- `fix`: Hata düzeltmesi
- `docs`: Dokümantasyon
- `style`: Kod formatı
- `refactor`: Refactoring
- `test`: Test ekleme/düzenleme
- `chore`: Genel bakım

### 4. Push Edin ve PR Oluşturun
```bash
git push origin feature/your-feature-name
```

### 5. Pull Request Açın
- Açıklayıcı başlık kullanın
- Değişiklikleri detaylı açıklayın
- Test sonuçlarını ekleyin
- Screenshot'lar ekleyin (UI değişiklikleri için)

## 📋 Code Review Süreci

### Review Checklist
- [ ] Kod standartlarına uygun mu?
- [ ] Unit testler yazıldı mı?
- [ ] Testler geçiyor mu?
- [ ] Dokümantasyon güncellendi mi?
- [ ] Performance etkisi var mı?
- [ ] Security riski var mı?

### Review Yorumları
- Yapıcı olun
- Spesifik öneriler verin
- Kod kalitesini artırın
- Öğrenme fırsatı yaratın

## 🐛 Bug Report

### Bug Report Formatı
```
**Bug Description:**
[Kısa açıklama]

**Steps to Reproduce:**
1. [Adım 1]
2. [Adım 2]
3. [Adım 3]

**Expected Behavior:**
[Ne olması gerekiyordu]

**Actual Behavior:**
[Ne oldu]

**Environment:**
- OS: [Windows/Mac/Linux]
- .NET Version: [Version]
- Browser: [Browser ve version]

**Additional Context:**
[Ek bilgiler, screenshot'lar, log'lar]
```

## 💡 Feature Request

### Feature Request Formatı
```
**Feature Description:**
[Özellik açıklaması]

**Use Case:**
[Ne zaman kullanılacak]

**Proposed Solution:**
[Nasıl implement edilebilir]

**Alternatives Considered:**
[Alternatif çözümler]

**Additional Context:**
[Ek bilgiler]
```

## 📚 Faydalı Linkler

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

## 🤝 İletişim

- **Issues**: GitHub Issues kullanın
- **Discussions**: GitHub Discussions kullanın
- **Email**: [Your Email]

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Katkıda bulunarak, kodunuzun da bu lisans altında yayınlanacağını kabul etmiş olursunuz.

---

Tekrar teşekkürler! 🎉
