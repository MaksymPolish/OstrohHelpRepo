# OstrohHelpApi — Документація

## Огляд проекту

**OstrohHelpApi** — це кросс-платформне ASP.NET Core 8.0 API для системи психологічного консультування зі змішаною навчанням (Ostrog National University).

**Дата закінчення Phase 1:** 12.04.2026  
**Статус:** Authorization & Testing Complete

---

## Архітектура

### Tech Stack
- Framework: ASP.NET Core 8.0
- ORM: Entity Framework Core 8.0
- Database: PostgreSQL 16
- Authentication: Google OAuth 2.0
- Real-time: SignalR (WebSocket)
- Testing: xUnit, NSubstitute, EF Core In-Memory
- API Documentation: Swagger/OpenAPI
- Containerization: Docker, Docker Compose
- CI/CD: GitHub Actions

### Структура проекту
```
src/
├── Api/                    # REST контролери, SignalR Hub, Middleware
├── Application/            # Бізнес-логіка, CQRS Commands/Queries
├── Domain/                 # Доменні моделі (User, Message, Consultation)
└── Infrastructure/         # EF Core, Репозиторії, Міграції

tests/
├── Tests.Common/           # Unit-тести (43 test cases)
└── Api.Tests.Integration/  # Інтеграційні тести (заглушені)
```

---

## Безпека (Phase 1 — Complete)

### Архітектура авторизації

Усі месседжі та консультації захищені через IConsultationAccessChecker інтерфейс:

| Компонент | Методи Захисту | Статус |
|---|---|---|
| ChatHub (SignalR) | JoinConsultation, SendMessage, MarkAsRead, DeleteMessage | 13 тестів |
| MessageController | AddAttachment (перевірка власника) | 10 тестів |
| ConsultationAccessChecker | 5 Authorization методів | 20 тестів |

### Запуск тестів безпеки

Локально:
```bash
dotnet test tests/Tests.Common/Tests.Common.csproj -c Release
```

У Docker:
```bash
docker-compose --profile tests run all-tests
```

**Результати:** 43/43 PASS (0 failures)

---

## Запуск проекту

### Вимоги
- .NET 8.0 SDK
- Docker & Docker Compose (рекомендовано)
- PostgreSQL 16 (або Docker container)

### 1. Local Development

**Запуск PostgreSQL:**
```bash
docker run --name ostrog-db -p 5432:5432 \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=4321 \
  -e POSTGRES_DB=OstrohHelp-DB \
  -d postgres:16
```

**Міграції:**
```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

**Запуск API:**
```bash
cd src/Api && dotnet run --configuration Development
```

API буде доступна на `https://localhost:7123`

### 2. Docker Compose (Recommended)

```bash
# Запуск API + PostgreSQL
docker-compose up api db

# Запуск тестів
docker-compose --profile tests run all-tests

# Запуск усього
docker-compose --profile tests up
```

---

## Робота з БД

### Міграції Entity Framework Core

Створення нової міграції:
```bash
dotnet ef migrations add MigrationName \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj \
  --output-dir Persistence/Migrations
```

Застосування міграції:
```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

---

## Google OAuth 2.0

### Налаштування

1. Перейдіть на [Google Cloud Console](https://console.cloud.google.com/)
2. Створіть OAuth 2.0 Credentials (Web Application)
3. Додайте googleToken в клієнтські конфігурації

### Скоупи для отримання профілю та email:
```
openid email profile
```

### Playground:
- [OAuth 2.0 Playground](https://developers.google.com/oauthplayground)

---

## ToDo & Roadmap

### Phase 1: Authorization & Testing (COMPLETE)
- [x] Design authorization architecture
- [x] Implement IConsultationAccessChecker interface
- [x] Secure ChatHub (4 methods with access checks)
- [x] Secure MessageController.AddAttachment
- [x] Create 43 unit tests (all passing)
- [x] Docker containerization
- [x] GitHub Actions CI/CD

### Phase 2: Message Encryption (TODO)
- [ ] Implement AES-256-GCM encryption
- [ ] Add message encryption/decryption in MessageController.Send
- [ ] Store encrypted messages in database
- [ ] Client-side decryption
- [ ] Create encryption tests

### Phase 3: Attachment Security & Validation (TODO)
- [ ] File type validation (whitelist: pdf, jpg, png, docx)
- [ ] File size limits (max 50MB)
- [ ] Malware scanning integration (ClamAV or VirusTotal)
- [ ] Secure file storage (encrypted in Cloudinary)
- [ ] Create attachment security tests

### Phase 4: Rate Limiting & Audit Logging (TODO)
- [ ] Implement rate limiting (X messages per user per minute)
- [ ] Add audit logging (who accessed what, when)
- [ ] Prepare audit log database schema
- [ ] Create rate limiting tests

### Phase 5: Production Deployment (TODO)
- [ ] Environment-specific configuration (dev/staging/prod)
- [ ] SSL/TLS certificates for HTTPS
- [ ] Production database backup strategy
- [ ] Monitoring & logging (Serilog, ELK stack)
- [ ] Performance optimization
- [ ] Load testing & stress testing

---

## API Status

| Controller | Endpoints | Статус | Тести |
|---|---|---|---|
| AuthController | 6 | Active | Pending |
| ConsultationController | 8+ | Active | Pending |
| MessageController | 5 | Active + Secure | 10 tests |
| ChatHub (SignalR) | 4 | Active + Secure | 13 tests |
| ConsultationStatusController | 3+ | Active | Pending |
| RoleController | 3+ | Active | Pending |

**Деталі ендпоїнтів:** див. [ENDPOINTS.md](ENDPOINTS.md)

---

## Розробка

### Додавання нового ендпоїнту

1. Створіть Command/Query в Application/ папці
2. Реалізуйте Handler (CQRS pattern)
3. Додайте ендпоїнт в контролер з [Authorize] атрибутом
4. Якщо потрібна авторизація — додайте перевірку IConsultationAccessChecker
5. Напишіть unit-тести

### Додавання міграції

```bash
# 1. Оновіть Domain моделі
# 2. Створіть міграцію
dotnet ef migrations add YourMigrationName --project src/Infrastructure --startup-project src/Api --output-dir Persistence/Migrations
# 3. Перевірте згенерований код в Persistence/Migrations/
# 4. Застосуйте міграцію
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

---

## Тестування

### Запуск специфічного тесту

```bash
dotnet test tests/Tests.Common/Tests.Common.csproj -c Release \
  --logger "console;verbosity=normal" \
  -v "Tests.Common.Security.ChatHubSecurityTests"
```

### Тестові профілі

- ConsultationAccessCheckerTests (20) — Repository-level authorization
- ChatHubSecurityTests (13) — SignalR Hub security
- MessageControllerSecurityTests (10) — REST API authorization

---

## Контакти & Помічні посилання

- Database: PostgreSQL 16 docs: https://www.postgresql.org/docs/16/
- EF Core: https://learn.microsoft.com/en-us/ef/core/
- ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/
- SignalR: https://learn.microsoft.com/en-us/aspnet/core/signalr/

---

**Версія документації:** 2.0 (12.04.2026)  
_Оновлюйте цей файл при зміні архітектури, ендпоїнтів або процесів._
# OstrohHelpApi — Документація

## Огляд проекту

**OstrohHelpApi** — це кросс-платформне ASP.NET Core 8.0 API для системи психологічного консультування зі змішаною навчанням (Ostrog National University).

**Дата закінчення Phase 1:** 12.04.2026  
**Статус:** Authorization & Testing Complete

---

## Архітектура

### Tech Stack
- Framework: ASP.NET Core 8.0
- ORM: Entity Framework Core 8.0
- Database: PostgreSQL 16
- Authentication: Google OAuth 2.0
- Real-time: SignalR (WebSocket)
- Testing: xUnit, NSubstitute, EF Core In-Memory
- API Documentation: Swagger/OpenAPI
- Containerization: Docker, Docker Compose
- CI/CD: GitHub Actions

### Структура проекту
```
src/
├── Api/                    # REST контролери, SignalR Hub, Мiddleware
├── Application/            # Бізнес-логіка, CQRS Commands/Queries
├── Domain/                 # Доменні моделі (User, Message, Consultation)
└── Infrastructure/         # EF Core, Репозиторії, Міграції

tests/
├── Tests.Common/           # Unit-тести (43 test cases) ✅
└── Api.Tests.Integration/  # Інтеграційні тести (заглушені)
```

---

## 🔒 Безпека (Phase 1 — Complete)

### Архітектура авторизації

Усі месседжі та консультації захищені через `IConsultationAccessChecker` інтерфейс:

| Компонент | Методи Захисту | Статус |
|---|---|---|
| ChatHub (SignalR) | JoinConsultation, SendMessage, MarkAsRead, DeleteMessage | 13 тестів |
| MessageController | AddAttachment (перевірка власника) | 10 тестів |
| ConsultationAccessChecker | 5 Authorization методів | 20 тестів |

### Запуск тестів

Локально:
```bash
dotnet test tests/Tests.Common/Tests.Common.csproj -c Release
```

У Docker:
```bash
docker-compose --profile tests run all-tests
```

**Результати:** 43/43 PASS (0 failures)

---

## Запуск проекту

### Вимоги
- .NET 8.0 SDK
- Docker & Docker Compose (рекомендовано)
- PostgreSQL 16 (або Docker container)

### 1️⃣ LocalDevelopment

**Запуск PostgreSQL:**
```bash
docker run --name ostrog-db -p 5432:5432 \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=4321 \
  -e POSTGRES_DB=OstrohHelp-DB \
  -d postgres:16
```

**Міграції:**
```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

**Запуск API:**
```bash
cd src/Api && dotnet run --configuration Development
```

API буде доступна на `https://localhost:7123`

### 2️⃣ Docker Compose (Recommended)

```bash
# Запуск API + PostgreSQL
docker-compose up api db

# Запуск тестів
docker-compose --profile tests run all-tests

# Запуск усього
docker-compose --profile tests up
```

---

## Робота з БД

### Міграції Entity Framework Core

Створення нової міграції:
```bash
dotnet ef migrations add MigrationName \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj \
  --output-dir Persistence/Migrations
```

Застосування міграції:
```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

---

## Google OAuth 2.0

### Налаштування

1. Перейдіть на [Google Cloud Console](https://console.cloud.google.com/)
2. Створіть OAuth 2.0 Credentials (Web Application)
3. Додайте `googleToken` в клієнтські конфігурації

### Скоупи для отримання профілю та email:
```
openid email profile
```

### Playground:
- [OAuth 2.0 Playground](https://developers.google.com/oauthplayground)

---

## ToDo & Roadmap

### Phase 1: Authorization & Testing (COMPLETE)
- [x] Design authorization architecture
- [x] Implement IConsultationAccessChecker interface
- [x] Secure ChatHub (4 methods with access checks)
- [x] Secure MessageController.AddAttachment
- [x] Create 43 unit tests (all passing)
- [x] Docker containerization
- [x] GitHub Actions CI/CD

### Phase 2: Message Encryption (TODO)
- [ ] Implement AES-256-GCM encryption
- [ ] Add message encryption/decryption in MessageController.Send
- [ ] Store encrypted messages in database
- [ ] Client-side decryption
- [ ] Create encryption tests

### Phase 3: Attachment Security & Validation (TODO)
- [ ] File type validation (whitelist: pdf, jpg, png, docx)
- [ ] File size limits (max 50MB)
- [ ] Malware scanning integration (ClamAV or VirusTotal)
- [ ] Secure file storage (encrypted in Cloudinary)
- [ ] Create attachment security tests

### Phase 4: Rate Limiting & Audit Logging (TODO)
- [ ] Implement rate limiting (X messages per user per minute)
- [ ] Add audit logging (who accessed what, when)
- [ ] Prepare audit log database schema
- [ ] Create rate limiting tests

### Phase 5: Production Deployment (TODO)
- [ ] Environment-specific configuration (dev/staging/prod)
- [ ] SSL/TLS certificates for HTTPS
- [ ] Production database backup strategy
- [ ] Monitoring & logging (Serilog, ELK stack)
- [ ] Performance optimization
- [ ] Load testing & stress testing

---

## 📊 API Status

| Controller | ендпоїнти | Статус | Тести |
|---|---|---|---|
| AuthController | 6 | ✅ Active | ⏳ Pending |
| ConsultationController | 8+ | ✅ Active | ⏳ Pending |
| MessageController | 5 | ✅ Active + Secure | ✅ 10 tests |
| ChatHub (SignalR) | 4 | ✅ Active + Secure | ✅ 13 tests |
| ConsultationStatusController | 3+ | ✅ Active | ⏳ Pending |
| RoleController | 3+ | ✅ Active | ⏳ Pending |

**Деталі ендпоїнтів:** див. [ENDPOINTS.md](ENDPOINTS.md)

---

## 🔧 Розробка

### Додавання нового ендпоїнту

1. Створіть Command/Query в `Application/` папці
2. Реалізуйте Handler (CQRS pattern)
3. Додайте ендпоїнт в контролер з `[Authorize]` атрибутом
4. Якщо потрібна авторизація — додайте перевірку `IConsultationAccessChecker`
5. Напишіть unit-тести

### Додавання міграції

```bash
# 1. Оновіть Domain моделі
# 2. Створіть міграцію
dotnet ef migrations add YourMigrationName --project src/Infrastructure --startup-project src/Api --output-dir Persistence/Migrations
# 3. Перевірте згенерований код в Persistence/Migrations/
# 4. Застосуйте міграцію
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

---

## Тестування

### Запуск специфічного тесту

```bash
dotnet test tests/Tests.Common/Tests.Common.csproj -c Release \
  --logger "console;verbosity=normal" \
  -v "Tests.Common.Security.ChatHubSecurityTests"
```

### Тестові профілі

- **ConsultationAccessCheckerTests** (20) — Repository-level authorization
- **ChatHubSecurityTests** (13) — SignalR Hub security
- **MessageControllerSecurityTests** (10) — REST API authorization

---

## Контакти & Помічні посилання

- **Database:** PostgreSQL 16 docs: https://www.postgresql.org/docs/16/
- **EF Core:** https://learn.microsoft.com/en-us/ef/core/
- **ASP.NET Core:** https://learn.microsoft.com/en-us/aspnet/core/
- **SignalR:** https://learn.microsoft.com/en-us/aspnet/core/signalr/

---

**Версія документацiї:** 2.0 (12.04.2026)  
_Оновлюйте цей файл при зміні архітектури, ендпоїнтів або процесів._
