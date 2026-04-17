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

### Phase 2: Message Encryption (COMPLETE ✅)
- [x] Implement AES-256-GCM encryption
- [x] Add message encryption/decryption in SendMessageCommand
- [x] Store encrypted messages in database
- [x] Key derivation via HKDF-SHA256 (per-consultation)
- [x] Create comprehensive encryption unit tests (83 tests)

### Phase 2.1: Message Retrieval with Encryption (COMPLETE ✅)
- [x] Update MessageDto with encrypted fields (EncryptedContent, Iv, AuthTag)
- [x] Update MessageDtoProfile mapper to convert byte arrays to base64
- [x] MessageController returns encrypted messages for client-side decryption

### Phase 3: Attachment Management (COMPLETE ✅)

**Status:** COMPLETED 17.04.2026  
**Components:** Hangfire Jobs, Preview URLs, Batch Upload, Soft Delete  
**Build:** 0 Errors

- [x] Hangfire integration for background job processing (v1.8.14)
- [x] Daily orphaned attachment cleanup job (2AM UTC)
- [x] Cloudinary integration with preview URL generation
- [x] Batch file upload support (AddMultipleAttachmentsCommand)
- [x] Automatic preview generation (thumbnails, medium preview, video poster, PDF pages)
- [x] MessageAttachment entity extended (preview URL fields)
- [x] Soft delete system for messages and attachments (IsDeleted flag)
- [x] File type normalization utility (FileTypeNormalizer)
- [x] CloudinaryService refactoring (eliminated URL building duplicates)
- [x] Database migration for IsDeleted flags
- [x] DELETE /api/Message/Delete endpoint (soft delete messages)
- [x] DELETE /api/Message/Attachment/{id} endpoint (soft delete single attachment)
- [x] Frontend visibility for deleted content (IsDeleted flag in DTOs)

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
| AuthController | 6 | ✅ Active | ⏳ Pending |
| ConsultationController | 8+ | ✅ Active | ⏳ Pending |
| MessageController | 6 | ✅ Active + Secure | ✅ 10 tests |
| ChatHub (SignalR) | 4 | ✅ Active + Secure | ✅ 13 tests |
| ConsultationStatusController | 3+ | ✅ Active | ⏳ Pending |
| RoleController | 3+ | ✅ Active | ⏳ Pending |

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
├── Tests.Common/           # Unit-тести (126 total) ✅
└── Api.Tests.Integration/  # Інтеграційні тести (заглушені)
```

---

## 🔐 Message Encryption Architecture (Phase 2)

### Encryption Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                       CLIENT (Browser)                       │
├─────────────────────────────────────────────────────────────┤
│  1. User joins consultation via SignalR                      │
│     ↓                                                         │
│  2. Server sends derived encryption key (base64)             │
│     ↓                                                         │
│  3. Client encrypts message using AES-256-GCM:              │
│     - plaintext message                                      │
│     - encryption key (from step 2)                           │
│     → generates: EncryptedContent, IV, AuthTag               │
│     ↓                                                         │
│  4. Client sends encrypted data via SignalR:                │
│     SendMessage(consultationId, encryptedContent,           │
│                 iv, authTag, attachments)                    │
└─────────────────────────────────────────────────────────────┘
                           ↓ HTTPS/TLS
┌─────────────────────────────────────────────────────────────┐
│                   SERVER (ASP.NET Core)                      │
├─────────────────────────────────────────────────────────────┤
│  1. ChatHub.SendMessage receives encrypted data              │
│  2. Validates user authorization (IConsultationAccessChecker)│
│  3. Decodes base64 strings → byte arrays                     │
│  4. Creates SendMessageCommand with encrypted fields:        │
│     SendMessageCommand(consultationId, senderId,             │
│                        encryptedContent, iv, authTag)        │
│  5. Handler stores in database (NO decryption):              │
│     Message {                                                │
│       EncryptedContent: byte[],                              │
│       Iv: byte[],                                            │
│       AuthTag: byte[]                                        │
│     }                                                        │
│  6. Broadcasts to all consultation members via SignalR       │
│  7. Client decrypts using stored key                         │
└─────────────────────────────────────────────────────────────┘
```

### Key Derivation Strategy

- **Algorithm:** HKDF-SHA256 (RFC 5869)
- **Master Key:** 256-bit, stored in `.env` (never in database)
- **Consultation Key:** Deterministic derivation from `consultationId`
  - Same consultation ID always produces the same key
  - Enables multi-session consistency
  - No key storage required (generated on demand)

### Encryption Parameters

| Parameter | Value | Notes |
|-----------|-------|-------|
| Cipher | AES-256-GCM | 256-bit encryption |
| IV Size | 96 bits (12 bytes) | Random per message |
| Auth Tag | 128 bits (16 bytes) | Tamper detection |
| Key Size | 256 bits (32 bytes) | Per-consultation |

### Server-Side Services

1. **IEncryptionService (AesGcmEncryptionService)**
   - Low-level AES-256-GCM encrypt/decrypt operations
   - Validates key/IV/tag sizes
   - Throws `AuthenticationTagMismatchException` on tampering

2. **IKeyDerivationService (HkdfKeyDerivationService)**
   - HKDF-SHA256 deterministic key generation
   - Uses consultation ID as salt
   - Validates master key minimum size (32 bytes)

3. **IConsultationKeyProvider (ConsultationKeyProvider)**
   - Bridges encryption services and ChatHub
   - Returns derived key for transmission to client

4. **IMessageEncryptionService (MessageEncryptionService)**
   - High-level orchestration
   - Used for decryption on message retrieval
   - Not used for initial client-side encryption

### Validation & Error Handling

- **IV Validation:** Must be exactly 12 bytes
- **Auth Tag Validation:** Must be exactly 16 bytes
- **Encryption Validation:** `AuthenticationTagMismatchException` if tampered
- **Decryption Validation:** Same exception if wrong key used

### Testing

- **Unit Tests:** 83 encryption-specific tests (100% pass)
  - AesGcmEncryptionServiceTests (13 tests)
  - HkdfKeyDerivationServiceTests (11 tests)
  - MessageEncryptionServiceTests (16 tests)
- **Coverage:** Encryption, decryption, tampering, key derivation

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

### Phase 2: Message Encryption (COMPLETE ✅)

**Status:** COMPLETED 14.04.2026  
**Architecture:** Client-side encryption (AES-256-GCM + HKDF-SHA256)  
**Test Pass Rate:** 83/83 (100%)

- [x] Implement AES-256-GCM encryption service
- [x] Create HKDF-SHA256 key derivation (per-consultation keys, deterministic)
- [x] Update Message entity with encrypted columns (EncryptedContent, Iv, AuthTag)
- [x] Create EF Core migration for database schema
- [x] Redesigned SendMessageCommand to accept encrypted data (not plaintext)
- [x] Updated ChatHub.SendMessage to receive encrypted content (base64-encoded)
- [x] Add key delivery in ChatHub.JoinConsultation via SignalR
- [x] Create comprehensive encryption unit tests (83 tests, 100% pass rate)
- [x] Master key configuration in .env (256-bit base64)
- [x] AES-256-GCM with 128-bit authentication tags
- [x] HKDF-SHA256 deterministic key derivation (no key storage)
- [x] Build succeeds with 0 errors, 56 warnings (non-critical)
- [x] Client-side encryption implementation (JavaScript library)
- [x] Update MessageController to return encrypted messages
- [x] Update MessageDto mapper for encryption fields (base64-encoded)
- [x] Test end-to-end encryption/decryption flow

### Phase 2.1: Message Retrieval with Encryption (COMPLETE ✅)

**Status:** COMPLETED 14.04.2026  
**Purpose:** Message retrieval returns encrypted data for client-side decryption  
**Test Pass Rate:** 83/83 (100%)

- [x] Updated MessageDto with EncryptedContent, Iv, AuthTag fields (base64-encoded)
- [x] Kept Text field for backward compatibility (nullable)
- [x] Updated MessageDtoProfile to map encrypted byte arrays to base64 strings
- [x] MessageController.Recive endpoint now returns encrypted messages
- [x] All tests passing - no breaking changes
- [x] Build succeeds with 0 errors

**Message Retrieval Flow:**
1. Client requests messages via MessageController.Recive
2. Server returns MessageDto with EncryptedContent, Iv, AuthTag (base64-encoded)
3. Client has encryption key from JoinConsultation
4. Client decrypts messages locally using AES-256-GCM

**Architecture Completed:**
- Encryption: Client-side (before transmission)
- Decryption: Client-side (after retrieval)
- Server: Never sees plaintext (only encrypted blobs)
- E2E Encryption: ✅ For messages (complete)

### Phase 3: Attachment Management (COMPLETE ✅)

**Status:** COMPLETED 17.04.2026  
**Components:** Hangfire Jobs, Preview URLs, Batch Upload, Soft Delete  
**Build:** 0 Errors

- [x] Hangfire integration for background job processing (v1.8.14)
- [x] Daily orphaned attachment cleanup job (2AM UTC)
- [x] Cloudinary integration with preview URL generation
- [x] Batch file upload support (AddMultipleAttachmentsCommand)
- [x] Automatic preview generation (thumbnails, medium preview, video poster, PDF pages)
- [x] MessageAttachment entity extended (preview URL fields)
- [x] Soft delete system for messages and attachments (IsDeleted flag)
- [x] File type normalization utility (FileTypeNormalizer)
- [x] CloudinaryService refactoring (eliminated URL building duplicates)
- [x] Database migration for IsDeleted flags
- [x] DELETE /api/Message/Delete endpoint (soft delete messages)
- [x] DELETE /api/Message/Attachment/{id} endpoint (soft delete single attachment)
- [x] Frontend visibility for deleted content (IsDeleted flag in DTOs)

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
| MessageController | 6 | ✅ Active + Secure | ✅ 10 tests |
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
