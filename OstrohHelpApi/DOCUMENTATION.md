# Документація для OstrohHelpApi

## Міграції та робота з базою даних

### Запуск PostgreSQL через Docker

```
docker run --name BimbaApi-postgres -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=4321 -e POSTGRES_DB=OstrohHelp-DB -d postgres
```

### Міграції Entity Framework Core

1. Створення міграції:
```
dotnet ef migrations add --project src/Infrastructure/Infrastructure.csproj --startup-project src/Api/Api.csproj Initial --output-dir Persistence/Migrations
```
2. Застосування міграції до бази:
```
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Api/Api.csproj
```

---

## Google OAuth 2.0 Playground

- Сервіс: [OAuth 2.0 Playground](https://developers.google.com/oauthplayground)
- Скоупи для отримання профілю та email:

```
openid email profile
```

---

## Структура проекту
- src/Api — Web API, контролери, маппери
- src/Application — бізнес-логіка, сервіси, інтерфейси
- src/Domain — доменні моделі
- src/Infrastructure — робота з БД, репозиторії, міграції
- tests/ — інтеграційні та unit-тести

---

_Оновлюйте цей файл при зміні процесів або структури проекту._
