# Infrastructure projesinde çalıştırılır

cd src/BlogApp.Infrastructure

# Yeni migration ekleme

dotnet ef migrations add Initial --startup-project ../BlogApp.API

# Tüm migration'ları uygulama

dotnet ef database update --startup-project ../BlogApp.API

