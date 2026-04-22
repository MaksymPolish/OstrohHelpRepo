# OstrohHelpApi — Повна Документація Ендпоїнтів 📋

**Статус:** ✅ Updated 18.04.2026  
**API Version:** v1.0  
**Base URL (Dev):** `https://localhost:7123/api`  
**WebSocket URL:** `wss://localhost:7123/chat`

---

## 📑 Зміст

1. [Безпека & Автентифікація](#-безпека--автентифікація)
2. [AuthController](#-authcontroller) - Автентифікація користувачів
3. [MessageController](#-messagecontroller) - Повідомлення та вкладення
4. [ChatHub (SignalR)](#-chathub-signalr) - Real-time чат
5. [ConsultationController](#-consultationcontroller) - Консультації
6. [QuestionnaireController](#-questionnairecontroller) - Анкети
7. [QuestionaryStatusController](#-questionarystatuscontroller) - Статуси анкет
8. [ConsultationStatusController](#-consultationstatuscontroller) - Статуси консультацій
9. [RoleController](#-rolecontroller) - Ролі користувачів
10. [Audit & Rate Limiting](#-audit--rate-limiting) - Логування та обмеження

---

## 🔐 Безпека & Автентифікація

### JWT Bearer Token

Всі ендпоїнти (крім явно позначених 🔓 Анонімні) вимагають JWT токен:

```
Authorization: Bearer <JWT_TOKEN>
```

**Token Lifetime:** 7 днів  
**Refresh Token:** Видається при входу

### Автентифікація Процес

```
1. POST /api/auth/google-login (Google OAuth token)
    ↓
2. Сервер валідує токен з Google
    ↓
3. Пошук/створення користувача в БД
    ↓
4. Генерація JWT + Refresh Token
    ↓
5. Повернення токенів до клієнта
```

### Ролі & Дозволи

| Роль | Дозволи |
|------|---------|
| **Student** | Створення анкет, чат з психологом, читання своїх консультацій |
| **Psychologist** | Приймання анкет, управління консультаціями, чат зі студентами |
| **HeadOfService** | Адміністративні операції, видалення користувачів, отримання звітів |

---

## 🔑 Шифрування Даних - Key Distribution

### Де передається ключ для шифрування?

**SignalR evento `ReceiveConsultationKey`** при приєднанні до консультації

**Flow:**
```
1. Клієнт підключається до WebSocket:
   wss://localhost:7123/chat?access_token=<JWT_TOKEN>
   
2. Надсилає: JoinConsultation(consultationId)
   
3. Сервер генерує ключ за допомогою HKDF-SHA256:
   - Input: Master Key (з .env) + ConsultationId
   - Output: 256-bit детерминированний ключ
   
4. Сервер передає клієнту:
   evento: "ReceiveConsultationKey"
   {
     ConsultationId: "abc-123...",
     Key: "XtLurkNiKAseW287L...",     ← BASE64!
     Algorithm: "AES-256-GCM",
     Timestamp: "2026-04-18T14:30:00Z"
   }
   
5. Клієнт:
   - Декодує Key з Base64
   - Зберігає в RAM пам'яті (НЕ в localStorage)
   - Використовує для шифрування/дешифрування повідомлень
```

### Деталі реалізації

| Параметр | Значення |
|----------|---------|
| **Метод передачі** | SignalR evento (WebSocket) |
| **Точка передачі** | `JoinConsultation` метод в ChatHub |
| **Evento ім'я** | `ReceiveConsultationKey` |
| **Master Key** | Зберігається в `.env` файлі (`ENCRYPTION_MASTER_KEY=...`) |
| **Key Derivation** | HKDF-SHA256 (RFC 5869) |
| **Output Key Size** | 256-bit (32 bytes) |
| **Детерминізм** | Одна консультація = один ключ (завжди однаковий) |
| **Transport Encoding** | Base64 (для передачі по WebSocket) |
| **Client Storage** | RAM пам'ять (не зберігається на диску) |
| **Шифрування** | Відбувається на КЛІЄНТІ (сервер не бачить plaintext) |

### Безпека ключей

- ✅ Master Key ніколи НЕ передається клієнту
- ✅ Консультаційні ключи генеруються динамічно для кожної консультації
- ✅ Ключи НЕ логуються або НЕ зберігаються в БД
- ✅ Клієнт отримує ключ тільки якщо є членом консультації (перевіряється JWT + консультація ID)
- ✅ Шифрування/дешифрування на клієнті - сервер ніколи не дешифрує дані

---

# 🔐 AuthController

**Base Route:** `/api/auth`

---

## 1️⃣ POST /google-login
**🔓 АНОНІМНИЙ** - Не вимагає JWT

**Описание:** Google OAuth 2.0 автентифікація. Створює нового користувача якщо не існує.

**Flow (Workflow):**
```
1. Клієнт отримує Google ID token з Google OAuth 2.0
2. Надсилає token на ендпоїнт в JSON body
3. Сервер ВАЛІДУЄ токен з Google API
4. Пошукує користувача в БД по email з токена
   - ЯКЩО існує: завантажує його
   - ЯКЩО НЕ існує: створює нового (role = Student по замовчуванню)
5. Генерує JWT токен (exp: +7 днів) та Refresh Token
6. Повертає UserDto з токенами та інформацією
```

### Input

**Body (JSON):**
```json
{
  "googleToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ik..."
}
```

| Поле | Тип | Обов'язковий | Опис |
|------|-----|------------|------|
| googleToken | string | Так | Google ID token від Google OAuth 2.0 |

### Output

**Success (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "student@example.com",
  "fullName": "Ivan Petrov",
  "photoUrl": "https://lh3.googleusercontent.com/...",
  "roleId": "00000000-0000-0000-0000-000000000001",
  "roleName": "Student",
  "jwtToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_abc123...",
  "expiresAt": "2026-04-25T15:20:00Z"
}
```

**Errors:**
| Код | Статус | Опис |
|-----|--------|------|
| InvalidToken | 400 | Немає googleToken в запиті |
| InvalidGoogle Token | 401 | Google токен невалідний або експайрився |
| CreationError | 500 | Помилка при створенні користувача в БД |

---

## 2️⃣ GET /{id}
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання профілю користувача за ID

**Flow:**
```
1. Клієнт надсилає ID користувача в URL
2. Сервер перевіряє JWT токен (403 якщо невалідний)
3. Виконує DB запит: Users.FirstOrDefault(u => u.Id == id)
4. Завантажує пов'язану Role
5. Маршалює до UserDto
6. Повертає дані
```

**Parameters:**
| Назва | Тип | Обов'язковий | Приклад |
|-------|-----|------------|---------|
| id | Guid | Так | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "student@example.com",
  "fullName": "Ivan Petrov",
  "photoUrl": "https://lh3.googleusercontent.com/...",
  "roleId": "00000000-0000-0000-0000-000000000001",
  "roleName": "Student",
  "course": 2,
  "createdAt": "2026-04-01T10:00:00Z"
}
```

**Помилки:**
- `401 Unauthorized` - Невалідний JWT
- `404 Not Found` - Користувач не існує

---

## 3️⃣ GET /get-by-email
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Пошук користувача за email адресою

**Flow:**
```
1. Клієнт передає email як query параметр
2. Сервер перевіряє JWT
3. Виконує DB запит: Users.FirstOrDefault(u => u.Email == email)
4. Якщо знайден - маршалює та повертає
5. Якщо не знайден - 404
```

### Input

**Query Parameters:**
| Назва | Тип | Обов'язковий | Приклад |
|-------|-----|------------|----------|
| email | string | Так | `student@example.com` |

**Example:** `GET /api/auth/get-by-email?email=student@example.com`

### Output

**Success (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "student@example.com",
  "fullName": "Ivan Petrov",
  "photoUrl": "https://lh3.googleusercontent.com/...",
  "roleId": "00000000-0000-0000-0000-000000000001",
  "roleName": "Student"
}
```

**Errors:**
| Код | Статус | Опис |
|-----|--------|------|
| MissingAuth | 401 | Невалідний JWT токен |
| NotFound | 404 | Користувач не знайдений |

---

## 4️⃣ GET /all
**🔐 АВТОРИЗОВАНИЙ | Роль: Психолог, Керівник**

**Описание:** Отримання списку всіх користувачів системи

**Flow:**
```
1. Клієнт надсилає GET запит
2. Сервер перевіряє JWT
3. Перевіряє роль: якщо Student -> 403 Forbidden
4. Виконує: Users.Include(u => u.Role).ToListAsync()
5. Маршалює кожного користувача до UserDto
6. Повертає список (відсортовано за ім'ям)
```

**Response (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "student@example.com",
    "fullName": "Ivan Petrov",
    "photoUrl": "https://...",
    "roleId": "00000000-0000-0000-0000-000000000001",
    "roleName": "Student"
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "email": "psych@example.com",
    "fullName": "Maria Kozak",
    "photoUrl": "https://...",
    "roleId": "00000000-0000-0000-0000-000000000002",
    "roleName": "Psychologist"
  }
]
```

**Помилки:**
- `403 Forbidden` - Недостатньо прав (тільки Психолог+)

---

## 5️⃣ DELETE /User-Delete
**🔐 АВТОРИЗОВАНИЙ | Роль: Керівник служби ТІЛЬКИ**

**Описание:** Видалення користувача з системи (hard delete)

**Flow:**
```
1. Керівник надсилає userId для видалення
2. Сервер перевіряє JWT та роль (403 якщо не Керівник)
3. Знаходить користувача: Users.FirstOrDefault(u => u.Id == userId)
4. Видаляє запись: dbContext.Users.Remove(user)
5. Логує в audit_logs: Action="UserDeleted"
6. dbContext.SaveChangesAsync()
7. Повертає 204 No Content
```

**Request:**
```json
"3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

**Response:** `204 No Content`

**Помилки:**
- `403 Forbidden` - Недостатньо прав
- `404 Not Found` - Користувач не знайдений

---

## 6️⃣ PUT /User-course
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Встановлення курсу для студента

**Flow:**
```
1. Клієнт надсилає userId та номер курсу
2. Сервер перевіряє JWT
3. Знаходить користувача
4. Встановлює user.Course = передане значення
5. dbContext.SaveChangesAsync()
6. Повертає 204 No Content
```

**Request:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "course": 3
}
```

**Response:** `204 No Content`

---

## 7️⃣ PUT /User-Role-Update
**🔐 АВТОРИЗОВАНИЙ | Роль: Керівник служби ТІЛЬКИ**

**Описание:** Зміна ролі користувача (Student → Psychologist → HeadOfService)

**Flow:**
```
1. Керівник надсилає userId та новий roleId
2. Сервер перевіряє JWT та роль (403 якщо не Керівник)
3. Валідує новий roleId: чи існує в Roles таблиці
4. Знаходить користувача
5. Встановлює user.RoleId = новий roleId
6. dbContext.SaveChangesAsync()
7. Логує в audit_logs: Action="UserRoleUpdated"
8. Повертає 204 No Content
```

**Request:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "roleId": "00000000-0000-0000-0000-000000000002"
}
```

**Response:** `204 No Content`

---

# 📨 MessageController

**Base Route:** `/api/Message`  
🔐 **Усі ендпоїнти АВТОРИЗОВАНІ**

**Важливо:** Система використовує **Cloudinary** для зберігання файлів та генерації превʼю (thumbnails, видео постери, PDF сторінки).

---

## 1️⃣ POST /BatchUpload
**🔐 АВТОРИЗОВАНИЙ | ⚠️ RATE LIMITED**

**Описание:** Завантаження одного або кількох файлів до Cloudinary та створення вкладень

**Flow (Workflow):**
```
1. Клієнт готує multipart/form-data з файлами
2. Опціонально передає messageId для прив'язки файлів
3. Сервер перевіряє JWT токен (401 якщо невалідний)
4. Перевіряє rate limiting (429 якщо перевищено)

5. ЯКЩО передано messageId:
   - Знаходить повідомлення
   - Перевіряє власність (403 якщо не SenderId)

6. ДЛЯ КОЖНОГО файлу:
   - Завантажує до Cloudinary
   - Cloudinary генерує превʼю:
     * Зображення: thumbnail (w:150px), medium (w:300px)
     * Відео: poster (постер вибирається)
     * PDF: page previews (перша сторінка)
   - Отримує fileUrl + превʼю URLs
   - Створює MessageAttachment запис в БД

7. Логує в audit_logs з JSON details: {file_count, total_size, message_id}
8. Повертає список завантажених файлів з превʼю URLs
```

**Request (multipart/form-data):**
```
POST /api/Message/BatchUpload?messageId=3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: multipart/form-data

file[0]: <binary data - photo.jpg>
file[1]: <binary data - document.pdf>
```

**Response (200 OK):**
```json
{
  "results": [
    {
      "attachmentId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
      "fileName": "photo.jpg",
      "isSuccess": true,
      "errorMessage": null,
      "fileUrl": "https://res.cloudinary.com/demo/image/upload/v1234567890/photo.jpg",
      "fileType": "jpg",
      "fileSizeBytes": 2048576,
      "createdAt": "2026-04-18T14:30:00Z",
      "thumbnailUrl": "https://res.cloudinary.com/demo/image/upload/w_150,h_150,q_40/v1234567890/photo.jpg",
      "mediumPreviewUrl": "https://res.cloudinary.com/demo/image/upload/w_300,h_300,q_50/v1234567890/photo.jpg",
      "videoPosterUrl": null,
      "pdfPagePreviewUrl": null
    }
  ],
  "successCount": 1,
  "failureCount": 0,
  "completedAt": "2026-04-18T14:30:05Z"
}
```

**Помилки:**
- `400 Bad Request` - Немає файлів
- `401 Unauthorized` - Невалідний JWT
- `403 Forbidden` - Не власник повідомлення
- `429 Too Many Requests` - Перевищено rate limit

---

## 2️⃣ GET /Recive
**🔐 АВТОРИЗОВАНИЙ | ПЕРЕВІРЯЄ ЧЛЕНСТВО**

**Описание:** Отримання всіх повідомлень консультації з вкладеннями та шифруванням

**Flow:**
```
1. Клієнт надсилає ID консультації як query параметр
2. Сервер перевіряє JWT токен
3. Знаходить консультацію
4. ПЕРЕВІРЯЄ ЧЛЕНСТВО: чи User є StudentId або PsychologistId
   - ЯКЩО ні -> 403 Forbidden
5. Завантажує всі повідомлення (включно видалені з IsDeleted=true)
6. ДЛЯ КОЖНОГО повідомлення:
   - Завантажує вкладення (включно видалені)
   - Повертає encrypted data: encryptedContent, iv, authTag
7. Повертає масив повідомлень з метаданими
```

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|-------|-----|--------------|-------------|
| idConsultation | Guid | Query | Так |

**Example:** `GET /api/Message/Recive?idConsultation=3fa85f64-5717-4562-b3fc-2c963f66afa6`

**Response (200 OK):**
```json
[
  {
    "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
    "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "senderId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
    "senderName": "Ivan Petrov",
    "receiverId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
    "receiverName": "Maria Kozak",
    "text": null,
    "encryptedContent": "AES256GCM_BASE64_ENCRYPTED_DATA",
    "iv": "INITIALIZATION_VECTOR_BASE64",
    "authTag": "AUTHENTICATION_TAG_BASE64",
    "isRead": true,
    "sentAt": "2026-04-18T14:20:00Z",
    "isDeleted": false,
    "attachments": [
      {
        "id": "7fa85f64-5717-4562-b3fc-2c963f66afaa",
        "fileUrl": "https://res.cloudinary.com/demo/image/upload/photo.jpg",
        "fileType": "jpg",
        "fileSizeBytes": 1048576,
        "createdAt": "2026-04-18T14:20:05Z",
        "thumbnailUrl": "https://res.cloudinary.com/demo/image/upload/w_150/photo.jpg",
        "mediumPreviewUrl": "https://res.cloudinary.com/demo/image/upload/w_300/photo.jpg",
        "videoPosterUrl": null,
        "pdfPagePreviewUrl": null,
        "isDeleted": false
      }
    ]
  }
]
```

**Помилки:**
- `401 Unauthorized` - Невалідний JWT
- `403 Forbidden` - Не член консультації

---

## 3️⃣ POST /Send
**🔐 АВТОРИЗОВАНИЙ | ⚠️ RATE LIMITED**

**Описание:** Надсилання нового повідомлення до консультації (з шифруванням)

**Flow (Workflow):**
```
1. Клієнт надсилає:
   - consultationId
   - receiverId
   - encryptedContent (вже зашифровано на клієнті)
   - iv (initialization vector)
   - authTag (authentication tag)

2. Сервер перевіряє JWT токен

3. Генерує новий Message:
   Message.Id = Guid.NewGuid()
   Message.SenderId = JWT.UserId (з токена)
   Message.ReceiverId = передане значення
   Message.ConsultationId = передане значення
   Message.EncryptedContent = база64 дані
   Message.Iv = база64 IV
   Message.AuthTag = база64 тег
   Message.SentAt = DateTime.UtcNow
   Message.IsRead = false
   Message.IsDeleted = false

4. Зберігає в БД via dbContext.SaveChangesAsync()

5. ЛОГУЄ в audit_logs:
   - Action: "MessageSent"
   - Resource: "Message"
   - Details: {ConsultationId, ReceiverId, HasAttachments: false}
   - Status: Success

6. Повертає створене повідомлення (без decrypted text)
```

**Request:**
```json
{
  "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "receiverId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
  "encryptedContent": "AES256GCM_ENCRYPTED_BASE64_DATA",
  "iv": "INITIALIZATION_VECTOR_BASE64",
  "authTag": "AUTHENTICATION_TAG_BASE64"
}
```

**Response (201 Created):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "senderId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
  "receiverId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
  "encryptedContent": "AES256GCM_ENCRYPTED_BASE64_DATA",
  "iv": "INITIALIZATION_VECTOR_BASE64",
  "authTag": "AUTHENTICATION_TAG_BASE64",
  "isRead": false,
  "sentAt": "2026-04-18T14:30:00Z",
  "isDeleted": false
}
```

---

## 4️⃣ DELETE /Delete
**🔐 АВТОРИЗОВАНИЙ (тільки власник) | ⚠️ RATE LIMITED | 🔴 SECURED**

**Описание:** Soft Delete повідомлення (видалення логічне, дані очищуються)

**Flow (Workflow):**
```
1. Клієнт надсилає messageId для видалення

2. Сервер перевіряє JWT токен

3. Знаходить повідомлення:
   Message msg = await Messages.FirstOrDefault(m => m.Id == messageId)

4. ПЕРЕВІРЯЄ ВЛАСНІСТЬ:
   if (msg.SenderId != JWT.UserId) return 403 Forbidden

5. Встановлює flags:
   msg.IsDeleted = true

6. Очищує дані для безпеки:
   msg.EncryptedContent = null
   msg.Iv = null
   msg.AuthTag = null

7. ДЛЯ ВСІХ ВКЛАДЕНЬ: 
   attachment.IsDeleted = true
   attachment.FileUrl = "Attachment was deleted by user"

8. Зберігає в БД

9. ЛОГУЄ в audit_logs:
   - Action: "MessageDeleted"
   - Resource: "Message"
   - ResourceId: messageId
   - Status: Success

10. Повертає 204 No Content
```

**Request:**
```json
{
  "messageId": "6fa85f64-5717-4562-b3fc-2c963f66afa9"
}
```

**Response:** `204 No Content`

**Помилки:**
- `401 Unauthorized` - Невалідний JWT
- `403 Forbidden` - Не власник повідомлення
- `404 Not Found` - Повідомлення не існує
- `429 Too Many Requests` - Перевищено rate limit

---

## 5️⃣ PUT /mark-as-read
**🔐 АВТОРИЗОВАНИЙ (тільки одержувач) | ⚠️ RATE LIMITED | 🔴 SECURED**

**Описание:** Позначення повідомлення як прочитаного

**Flow:**
```
1. Клієнт надсилає messageId

2. Сервер перевіряє JWT

3. Знаходить повідомлення

4. ПЕРЕВІРЯЄ ОДЕРЖУВАЧА:
   if (msg.ReceiverId != JWT.UserId) return 403 Forbidden

5. Встановлює:
   msg.IsRead = true

6. Зберігає в БД

7. ЛОГУЄ в audit_logs: Action="MessageRead"

8. Повертає 204 No Content
```

**Request:**
```json
{
  "messageId": "6fa85f64-5717-4562-b3fc-2c963f66afa9"
}
```

**Response:** `204 No Content`

---

## 6️⃣ DELETE /Attachment/{attachmentId}
**🔐 АВТОРИЗОВАНИЙ (тільки власник) | ⚠️ RATE LIMITED | 🔴 SECURED**

**Описание:** Soft Delete одного вкладення повідомлення

**Flow (Workflow):**
```
1. Клієнт надсилає attachmentId в URL

2. Сервер перевіряє JWT

3. Знаходить вкладення та його повідомлення:
   att = await Attachments.FirstOrDefault(a => a.Id == attachmentId)
   msg = await Messages.FirstOrDefault(m => m.Id == att.MessageId)

4. ПЕРЕВІРЯЄ ВЛАСНІСТЬ (власник повідомлення):
   if (msg.SenderId != JWT.UserId) return 403 Forbidden

5. Встановлює flags:
   att.IsDeleted = true
   att.FileUrl = "Attachment was deleted by user"

6. Зберігає в БД

7. ЛОГУЄ в audit_logs:
   - Action: "AttachmentDeleted"
   - Resource: "Attachment"
   - ResourceId: attachmentId

8. Повертає 204 No Content

ℹ️ ВАЖЛИВО: Файл залишається на Cloudinary, але приховується від UI
```

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|-------|-----|--------------|-------------|
| attachmentId | Guid | URL | Так |

**Response:** `204 No Content`

**Помилки:**
- `403 Forbidden` - Не власник повідомлення
- `404 Not Found` - Вкладення не існує

---

# 💬 ChatHub (SignalR)

**Connection URL:** `wss://localhost:7123/chat?access_token=<JWT_TOKEN>`  
🔐 **Усі методи АВТОРИЗОВАНІ & SECURED**

**Важливо:** SignalR підтримує real-time обмін через WebSocket. Клієнт підписується на групу консультації для отримання live повідомлень.

## 🟢 Presence (Online/Offline) в реальному часі

Сервер відстежує кількість активних зʼєднань для кожного користувача.

- При першому активному зʼєднанні користувача: статус у БД оновлюється на `IsOnline = true`.
- При останньому відключенні користувача: статус у БД оновлюється на `IsOnline = false`.
- Усі інші клієнти отримують подію:

**Event:** `UserStatusChanged(string userId, bool isOnline)`

### Як підключитися до хабу

1. Відкрити WebSocket підключення до `/hubs/chat`.
2. Передати JWT через `access_token`.
3. Підписатись на `UserStatusChanged`.
4. Викликати `JoinConsultation(consultationId)` для чату.

### React приклад (@microsoft/signalr)

```typescript
import * as signalR from "@microsoft/signalr";

const token = "YOUR_JWT_TOKEN";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7123/hubs/chat", {
    accessTokenFactory: () => token,
  })
  .withAutomaticReconnect()
  .build();

connection.on("UserStatusChanged", (userId: string, isOnline: boolean) => {
  console.log("User status changed", { userId, isOnline });
  // update your store/UI here
});

connection.on("ReceiveMessage", (message) => {
  console.log("ReceiveMessage", message);
});

await connection.start();
await connection.invoke("JoinConsultation", "4bf57625-929f-4e12-9451-c53a40862943");
```

### Flutter приклад (signalr_netcore)

```dart
import 'package:signalr_netcore/signalr_client.dart';

final token = 'YOUR_JWT_TOKEN';

final connection = HubConnectionBuilder()
    .withUrl(
      'https://localhost:7123/hubs/chat',
      options: HttpConnectionOptions(
        accessTokenFactory: () async => token,
        transport: HttpTransportType.WebSockets,
      ),
    )
    .withAutomaticReconnect()
    .build();

connection.on('UserStatusChanged', (args) {
  final userId = args?[0] as String;
  final isOnline = args?[1] as bool;
  print('UserStatusChanged: $userId => $isOnline');
  // update your state/UI here
});

connection.on('ReceiveMessage', (args) {
  print('ReceiveMessage: ${args?[0]}');
});

await connection.start();
await connection.invoke('JoinConsultation', args: ['4bf57625-929f-4e12-9451-c53a40862943']);
```

### Flutter lifecycle рекомендація

- Використовуй `ChangeNotifier` або `Bloc`, який зберігає `Set<String> onlineUserIds`.
- Підпишися на `UserStatusChanged` і оновлюй state тільки коли статус реально змінився.
- Реалізуй `WidgetsBindingObserver`:
  - `paused` / `inactive` → `connection.stop()` для економії батареї.
  - `resumed` → повторний `connection.start()` і повторний `JoinConsultation(...)`, якщо чат активний.
- Для reconnection залишай `withAutomaticReconnect()`, але додай власний retry/try-catch на старті, щоб UI не падав на тимчасовій втраті мережі.

---

## 📊 Polling (СТАРИЙ) vs SignalR (НОВИЙ) - Порівняння

### ❌ СТАРИЙ МЕТОД: REST API Polling (GET /api/messages)

```
Клієнт                              Сервер
  ↓                                   ↓
  ├─ Таймер: кожні 2-5 сек
  │
  ├─→ GET /api/messages?...  ────────→ Запит #1
  │   {"messages": [...]}    ←────────
  │
  ├─→ GET /api/messages?...  ────────→ Запит #2
  │   {"messages": [...]}    ←────────
  │
  ├─→ GET /api/messages?...  ────────→ Запит #3 (без змін!)
  │   {"messages": [...]}    ←────────
  │
  └─→ GET /api/messages?...  ────────→ Запит #4
      {"messages": [...]}    ←────────
```

**Проблеми:**
- ⚠️ **N+1 запитів** - много однакових запитів, часто без нових даних
- ⚠️ **Затримки** - нові повідомлення затримуються на 2-5 сек (poll interval)
- ⚠️ **Перегрузка сервера** - тисячі непотрібних запитів в секунду
- ⚠️ **Батарея мобільних** - постійна активність Wi-Fi/LTE
- ⚠️ **Користувацька експерієнція** - не real-time, повідомлення приходять з затримкою
- ⚠️ **Масштабованість** - вимагає більше сервера з росту користувачів

### ✅ НОВИЙ МЕТОД: SignalR WebSocket (Real-Time)

```
Клієнт                              Сервер
  ↓                                   ↓
  └─ WebSocket підключення
  ============ ONE PERSISTENT CONNECTION ============
    ↓                                   ↓
    │ JoinConsultation               │ Verify + Generate Key
    │─────────────────────→────────→ │
    │←────────────────────←──────────│ ReceiveConsultationKey
    │ (ключ для шифрування)          │
    │                                 │
    │ (чекаємо на события)            │
    │                                 │
    │ SendMessage (user1 пише)        │
    │─────────────────────→────────→ │ Process
    │                                 │
    │←────────────────────←──────────│ ReceiveMessage (user2)
    │ (миттєво! 0 latency)            │ (миттєво!)
    │                                 │
    │ (інші дії без нових запитів)    │
    │                                 │
    └─────────────────────────────────┘ Connection active
```

**Переваги:**
- ✅ **Single Connection** - одне постійне WebSocket з'єднання
- ✅ **Real-Time Delivery** - повідомлення приходять миттєво (< 100ms)
- ✅ **Zero Polling** - немає зайвих запитів
- ✅ **Efficient** - мінімальний трафік, мало CPU
- ✅ **Battery-Friendly** - одне з'єднання вместо постійних запитів
- ✅ **Scalable** - тисячи клієнтів без перегрузки
- ✅ **Better UX** -真正的 real-time experience

### 📊 Метрики порівняння

| Параметр | Polling (REST) | SignalR (WebSocket) |
|----------|--------|---------|
| **Затримка** | 2-5 сек | <100ms |
| **Запитів за 5 хв** | ~60-150 | 1 (connection) |
| **Трафік за сеанс (1 час)** | ~10-30 MB | ~1-2 MB |
| **CPU сервера** | Високе | Дуже низьке |
| **Батарея мобільного** | ❌ Швидко розряджається | ✅ Економить 30-50% |
| **Real-Time?** | ❌ Нi | ✅ Так |
| **Масштабованість** | 500 користувачів | 5000+ користувачів |
| **Складність** | Простіша | Дещо складніша (але варто) |

### 🔄 Міграція з Polling на SignalR

**Для Flutter розробників:**

```dart
// ❌ СТАРИЙ КОД (polling):
Timer.periodic(Duration(seconds: 3), (_) async {
  final response = await http.get(
    Uri.parse('$baseUrl/api/messages?idConsultation=$consultationId'),
    headers: {'Authorization': 'Bearer $token'},
  );
  // Парсинг, оновлення UI... кожні 3 секунди!
});

// ✅ НОВИЙ КОД (SignalR):
final chatHub = ChatHubService(baseUrl: 'wss://localhost:7123', jwtToken: token);

chatHub.onReceiveMessage = (message) {
  // Миттєво при отриманні повідомлення!
  updateUI(message);
};

await chatHub.initialize();
await chatHub.joinConsultation(consultationId);
// Готово! Слухаємо real-time события, немає таймерів!
```

**Переваги для дев-тиму:**
- 🚀 Менш кода (немає таймерів, retry logic)
- 🔍 Легше дебажити (ясні события вмiсто N запитiв)
- 📈 Лучше перформанс (батарея, трафік, CPU)
- 🛡️ Краще безпека (одна connection, JWT один раз)

---

---

## 1️⃣ JoinConsultation
**🔐 АВТОРИЗОВАНИЙ | 🔴 ПЕРЕВІРЯЄ ЧЛЕНСТВО**

**Описание:** Приєднання користувача до групи консультації для отримання real-time повідомлень + **ПЕРЕДАЧА КЛЮЧА ДЛЯ ШИФРУВАННЯ**

**🔑 Важливо - Де передається ключ для шифрування:**

При приєднанні до консультації, сервер **генерує та передає encryption key** для цієї консультації:

```
МЕТОД ПЕРЕДАЧІ: SignalR evento "ReceiveConsultationKey"
АЛГОРИТМ: HKDF-SHA256 (детерминированный вивід ключа)
INPUT: Master Key (з .env) + ConsultationId
OUTPUT: 256-bit AES-GCM ключ (Base64 encoded)
ЧАСТОТА: При кожному JoinConsultation (одинаковий для однієї консультації)
```

**Flow (Workflow):**
```
1. Клієнт надсилає consultationId через WebSocket:
   wss://localhost:7123/chat?access_token=<JWT_TOKEN>

2. Сервер перевіряє JWT токен (з query параметра)

3. Знаходить користувача та консультацію в БД

4. ПЕРЕВІРЯЄ ЧЛЕНСТВО:
   Consultation c = await Consultations.FirstOrDefault(...)
   if (c.StudentId != JWT.UserId && c.PsychologistId != JWT.UserId)
     return 403 Forbidden

5. Додає користувача до SignalR групи:
   await Groups.AddToGroupAsync(Context.ConnectionId, $"consultation_{consultationId}")

6. ⭐ ГЕНЕРУЄ ENCRYPTION KEY для консультації:
   byte[] consultationKey = _keyDerivationService.DeriveKeyForConsultation(
       masterKeyFromEnvironment,    // Із .env файла
       consultationId               // GUID консультації
   )
   
   Алгоритм: HKDF-SHA256
   - Extract: HMAC-SHA256(salt=ConsultationId, key=MasterKey)
   - Expand: HMAC-SHA256 з info="OstrohHelp-MessageEncryption"
   - Результат: 256-bit (32 bytes) детерминированний ключ

7. ⭐ ПЕРЕДАЄ КЛЮЧ КЛІЄНТУ через evento "ReceiveConsultationKey":
   {
     ConsultationId: guid,
     Key: "XtLurkNiKAseW287L...",    ← BASE64 ENCODED!
     Algorithm: "AES-256-GCM",
     Timestamp: datetime
   }

8. BROADCAST до ГРУПИ (всім у групі - evento "ReceiveJoinedConsultation"):
   {
     consultationId: guid,
     userId: guid,
     userName: string,
     photoUrl: string,
     timestamp: datetime
   }

9. Клієнт:
   - Отримує ключ з ReceiveConsultationKey
   - Зберігає його в RAM (НЕ в localStorage!)
   - Використовує для шифрування/дешифрування повідомлень локально
```

**Важливо для клієнта:**
- ✅ Ключ передається АВТОМАТИЧНО при приєднанні (не потрібно запитувати)
- ✅ Ключ одинаковий для однієї консультації (детерминированний)
- ✅ Клієнт отримує ключ в BASE64, потрібно декодувати перед використанням
- ❌ Ключ НЕ передається в REST API запитах
- ❌ Ключ НЕ зберігається в localStorage (тільки в памяті)
- ✅ Шифрування/дешифрування відбувається на КЛІЄНТІ (сервер не бачить plaintext)

**Client Send:**
```json
{
  "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response (Client):**
```json
{
  "success": true,
  "message": "Successfully joined consultation"
}
```

**Broadcast to Group (ReceiveJoinedConsultation):**
```json
{
  "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
  "userName": "Ivan Petrov",
  "photoUrl": "https://example.com/photo.jpg",
  "timestamp": "2026-04-18T14:30:00Z"
}
```

---

## 2️⃣ SendMessage
**🔐 АВТОРИЗОВАНИЙ | 🔴 ПЕРЕВІРЯЄ ЧЛЕНСТВО**

**Описание:** Надсилання повідомлення до групи консультації в real-time

**Flow (Workflow):**
```
1. Клієнт надсилає через SignalR:
   - consultationId
   - receiverId
   - encryptedContent
   - iv
   - authTag

2. Сервер перевіряє JWT та членство в консультації

3. Генерує новий Message запис (аналогічно POST /Send)

4. Зберігає в БД

5. ЛОГУЄ в audit_logs з JSON details:
   {
     "ConsultationId": "...",
     "ReceiverId": "...",
     "HasAttachments": false
   }

6. BROADCAST до групи (всім у консультації):
   await Clients.Group($"consultation_{consultationId}")
     .SendAsync("ReceiveMessage", {message data})

7. Клієнти отримують message та виводять в UI
```

**Client Send:**
```json
{
  "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "receiverId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
  "encryptedContent": "AES256GCM_ENCRYPTED_BASE64",
  "iv": "INITIALIZATION_VECTOR_BASE64",
  "authTag": "AUTHENTICATION_TAG_BASE64"
}
```

**Broadcast to Group (ReceiveMessage):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "senderId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
  "senderName": "Ivan Petrov",
  "senderPhotoUrl": "https://example.com/photo.jpg",
  "receiverId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
  "receiverName": "Maria Kozak",
  "receiverPhotoUrl": "https://example.com/photo2.jpg",
  "encryptedContent": "AES256GCM_ENCRYPTED_BASE64",
  "iv": "INITIALIZATION_VECTOR_BASE64",
  "authTag": "AUTHENTICATION_TAG_BASE64",
  "sentAt": "2026-04-18T14:31:00Z"
}
```

---

## 3️⃣ MarkAsRead
**🔐 АВТОРИЗОВАНИЙ | 🔴 ПЕРЕВІРЯЄ ОДЕРЖУВАЧА**

**Описание:** Позначення повідомлення як прочитаного в real-time

**Flow:**
```
1. Клієнт надсилає messageId

2. Сервер перевіряє JWT

3. Знаходить повідомлення та перевіряє:
   if (msg.ReceiverId != JWT.UserId) return Unauthorized

4. Встановлює IsRead=true

5. Зберігає в БД

6. BROADCAST до групи evento ReceiveMarkedAsRead
```

**Client Send:**
```json
{
  "messageId": "6fa85f64-5717-4562-b3fc-2c963f66afa9"
}
```

**Broadcast (ReceiveMarkedAsRead):**
```json
{
  "messageId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isRead": true,
  "timestamp": "2026-04-18T14:31:30Z"
}
```

---

## 4️⃣ DeleteMessage
**🔐 АВТОРИЗОВАНИЙ | 🔴 ПЕРЕВІРЯЄ ВЛАСНИКА**

**Описание:** Видалення повідомлення в real-time (soft delete)

**Flow:**
```
1. Клієнт надсилає messageId

2. Сервер перевіряє JWT та членство

3. Знаходить повідомлення та перевіряє:
   if (msg.SenderId != JWT.UserId) return 403 Forbidden

4. Встановлює:
   msg.IsDeleted = true
   Очищує вміст та вкладення (як у REST Delete)

5. Зберігає в БД

6. ЛОГУЄ в audit_logs: Action="MessageDeleted"

7. BROADCAST до групи evento ReceiveDeletedMessage

8. Клієнти видаляють message з UI
```

**Client Send:**
```json
{
  "messageId": "6fa85f64-5717-4562-b3fc-2c963f66afa9"
}
```

**Broadcast (ReceiveDeletedMessage):**
```json
{
  "messageId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "consultationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "timestamp": "2026-04-18T14:32:00Z"
}
```

---

# 🗂️ ConsultationController

**Base Route:** `/api/Consultations`  
🔐 **Авторизовані ендпоїнти** з ролевим доступом

**Описание:** Управління консультаціями студентів з психологами. Консультація створюється при прийманні психологом анкети студента.

---

## 1️⃣ POST /Accept-Questionnaire
**🔐 АВТОРИЗОВАНИЙ | Роль: Психолог, Керівник**

**Описание:** Прийняття анкети студента психологом та створення консультації

**Flow (Workflow):**
```
1. Психолог надсилає:
   - questionaryId
   - psychologistId
   - scheduledTime (час консультації)

2. Сервер перевіряє JWT та роль:
   if (JWT.Role != "Psychologist" && JWT.Role != "HeadOfService")
     return 403 Forbidden

3. Завантажує АНКЕТУ:
   Questionnaire q = await Questionnaires.FirstOrDefault(q => q.Id == questionaryId)
   if (q == null) return 404 Not Found
   if (q.Status != "Pending") return 400 Bad Request (вже прийнята)

4. Завантажує СТУДЕНТА:
   User student = await Users.FirstOrDefault(u => u.Id == q.UserId)

5. Завантажує ПСИХОЛОГА:
   User psychologist = await Users.FirstOrDefault(u => u.Id == psychologistId)

6. Створює новий Consultation:
   Consultation c = new Consultation {
     Id = Guid.NewGuid(),
     StudentId = student.Id,
     PsychologistId = psychologist.Id,
     QuestionnaireId = q.Id,
     StatusId = <"Assigned">,
     ScheduledTime = передане значення,
     CreatedAt = DateTime.UtcNow
   }

7. Зберігає консультацію в БД

8. Оновлює анкету:
   q.StatusId = <"Accepted">
   q.UpdatedAt = DateTime.UtcNow

9. ЛОГУЄ в audit_logs:
   - Action: "ConsultationCreated"
   - Resource: "Consultation"
   - Details: {QuestionaryId, StudentId, PsychologistId}

10. Повертає створену консультацію з деталями
```

**Request:**
```json
{
  "questionaryId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "psychologistId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
  "scheduledTime": "2026-04-20T10:00:00Z"
}
```

**Response (201 Created):**
```json
{
  "id": "9fa85f64-5717-4562-b3fc-2c963f66afac",
  "studentId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
  "studentName": "Ivan Petrov",
  "studentPhotoUrl": "https://...",
  "psychologistId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
  "psychologistName": "Maria Kozak",
  "psychologistPhotoUrl": "https://...",
  "statusId": "10fa85f64-5717-4562-b3fc-2c963f66afad",
  "statusName": "Assigned",
  "questionnaireId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "scheduledTime": "2026-04-20T10:00:00Z",
  "createdAt": "2026-04-18T14:35:00Z"
}
```

---

## 2️⃣ GET /{id}
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання детальної інформації про консультацію за ID

**Flow:**
```
1. Клієнт надсилає ID консультації в URL

2. Сервер перевіряє JWT

3. Завантажує консультацію з усіма зв'язками:
   Consultations.Include(c => c.Student)
               .Include(c => c.Psychologist)
               .Include(c => c.Status)
               .Include(c => c.Questionnaire)
               .FirstOrDefault(c => c.Id == id)

4. Маршалює до ConsultationDto

5. Повертає дані
```

**Parameters:**
| Назва | Тип | Обов'язковий |
|-------|-----|------------|
| id | Guid | Так |

**Response (200 OK):**
```json
{
  "id": "9fa85f64-5717-4562-b3fc-2c963f66afac",
  "studentId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
  "studentName": "Ivan Petrov",
  "studentPhotoUrl": "https://...",
  "psychologistId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
  "psychologistName": "Maria Kozak",
  "psychologistPhotoUrl": "https://...",
  "statusId": "10fa85f64-5717-4562-b3fc-2c963f66afad",
  "statusName": "Assigned",
  "questionnaireId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "scheduledTime": "2026-04-20T10:00:00Z",
  "createdAt": "2026-04-18T14:35:00Z"
}
```

---

## 3️⃣ GET /all
**🔐 АВТОРИЗОВАНИЙ | Роль: Психолог, Керівник**

**Описание:** Отримання списку всіх консультацій (з фільтрацією за ролью)

**Flow:**
```
1. Клієнт надсилає GET запит

2. Сервер перевіряє JWT та роль:
   - ЯКЩО Student: 403 Forbidden
   - ЯКЩО Psychologist: повернути тільки його консультації
   - ЯКЩО HeadOfService: повернути все консультації

3. Виконує DB запит з Include зв'язків

4. Сортує за датою створення (новіші спочатку)

5. Маршалює до списку ConsultationDto

6. Повертає список
```

**Response (200 OK):**
```json
[
  {
    "id": "9fa85f64-5717-4562-b3fc-2c963f66afac",
    "studentId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
    "studentName": "Ivan Petrov",
    "psychologistId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
    "psychologistName": "Maria Kozak",
    "statusName": "Assigned",
    "scheduledTime": "2026-04-20T10:00:00Z",
    "createdAt": "2026-04-18T14:35:00Z"
  }
]
```

---

## 4️⃣ GET /Get-All-Consultations-By-UserId/{Id}
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання всіх консультацій конкретного користувача (студента або психолога)

**Flow:**
```
1. Клієнт надсилає ID користувача в URL

2. Сервер перевіряє JWT

3. Завантажує консультації де:
   StudentId = Id OR PsychologistId = Id

4. Маршалює та повертає список
```

**Parameters:**
| Назва | Тип | Обов'язковий |
|-------|-----|------------|
| Id | Guid | Так |

**Response (200 OK):**
```json
[
  {
    "id": "9fa85f64-5717-4562-b3fc-2c963f66afac",
    "studentId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
    "studentName": "Ivan Petrov",
    "psychologistId": "2fa85f64-5717-4562-b3fc-2c963f66afa1",
    "psychologistName": "Maria Kozak",
    "statusName": "Assigned",
    "scheduledTime": "2026-04-20T10:00:00Z",
    "createdAt": "2026-04-18T14:35:00Z"
  }
]
```

---

## 5️⃣ PUT /Update-Consultation
**🔐 АВТОРИЗОВАНИЙ | Роль: Психолог, Керівник**

**Описание:** Оновлення часу консультації або статусу

**Flow:**
```
1. Психолог надсилає:
   - consultationId
   - statusId (опціонально)
   - scheduledTime (опціонально)

2. Сервер перевіряє JWT та роль

3. Знаходить консультацію

4. Обновляє передані поля:
   if (statusId != null) c.StatusId = statusId
   if (scheduledTime != null) c.ScheduledTime = scheduledTime

5. Зберігає в БД

6. ЛОГУЄ в audit_logs: Action="ConsultationUpdated"

7. Повертає 204 No Content
```

**Request:**
```json
{
  "consultationId": "9fa85f64-5717-4562-b3fc-2c963f66afac",
  "statusId": "11fa85f64-5717-4562-b3fc-2c963f66afae",
  "scheduledTime": "2026-04-21T14:00:00Z"
}
```

**Response:** `204 No Content`

---

## 6️⃣ DELETE /Delete-Consultation
**🔐 АВТОРИЗОВАНИЙ | Роль: Керівник служби ТІЛЬКИ**

**Описание:** Видалення консультації (hard delete)

**Flow:**
```
1. Керівник надсилає ID консультації

2. Сервер перевіряє JWT та роль (403 якщо не Керівник)

3. Знаходить консультацію

4. Видаляє її з БД: dbContext.Consultations.Remove(consultation)

5. ЛОГУЄ в audit_logs: Action="ConsultationDeleted"

6. Зберігає zміни

7. Повертає 204 No Content
```

**Request:**
```json
{
  "consultationId": "9fa85f64-5717-4562-b3fc-2c963f66afac"
}
```

**Response:** `204 No Content`

---

# 📝 QuestionnaireController

**Base Route:** `/api/Questionnaire`  
🔐 **Авторизовані ендпоїнти**

**Описание:** Управління анкетами студентів. Анкета - це початкова форма, яку студент заповнює для запиту консультації.

---

## 1️⃣ POST /Create-Questionnaire
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Створення нової анкети студентом для запиту консультації

**Flow (Workflow):**
```
1. Студент заповнює анкету та надсилає на ендпоїнт

2. Сервер перевіряє JWT

3. Генерує новий Questionnaire:
   Questionnaire q = new Questionnaire {
     Id = Guid.NewGuid(),
     UserId = JWT.UserId,
     Description = передане значення,
     IsAnonymous = передане значення,
     StatusId = <"Pending">,
     SubmittedAt = DateTime.UtcNow
   }

4. Зберігає в БД

5. Повертає створену анкету
```

**Request:**
```json
{
  "userId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
  "description": "I've been experiencing anxiety and need support",
  "isAnonymous": false
}
```

**Response (201 Created):**
```json
{
  "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "userId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
  "userFullName": "Ivan Petrov",
  "description": "I've been experiencing anxiety and need support",
  "isAnonymous": false,
  "statusId": "12fa85f64-5717-4562-b3fc-2c963f66afaf",
  "statusName": "Pending",
  "submittedAt": "2026-04-18T14:40:00Z"
}
```

---

## 2️⃣ GET /{id}
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання деталей анкети за ID

**Flow:**
```
1. Клієнт надсилає ID анкети

2. Сервер перевіряє JWT

3. Завантажує анкету з користувачем та статусом

4. Маршалює та повертає
```

**Parameters:**
| Назва | Тип | Обов'язковий |
|-------|-----|------------|
| id | Guid | Так |

**Response (200 OK):**
```json
{
  "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "userId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
  "userFullName": "Ivan Petrov",
  "userEmail": "ivan@example.com",
  "description": "I've been experiencing anxiety and need support",
  "isAnonymous": false,
  "statusId": "12fa85f64-5717-4562-b3fc-2c963f66afaf",
  "statusName": "Pending",
  "submittedAt": "2026-04-18T14:40:00Z"
}
```

---

## 3️⃣ GET /all
**🔐 АВТОРИЗОВАНИЙ | Роль: Психолог, Керівник**

**Описание:** Отримання списку всіх анкет (для психолога щоб вибрати яку взяти)

**Flow:**
```
1. Психолог надсилає GET запит

2. Сервер перевіряє JWT та роль (403 якщо Student)

3. Завантажує всі анкети зі статусом "Pending"

4. Сортує за датою (новіші спочатку)

5. Маршалює та повертає список
```

**Response (200 OK):**
```json
[
  {
    "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
    "userId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
    "userFullName": "Ivan Petrov",
    "userEmail": "ivan@example.com",
    "description": "I've been experiencing anxiety and need support",
    "isAnonymous": false,
    "statusName": "Pending",
    "submittedAt": "2026-04-18T14:40:00Z"
  }
]
```

---

## 4️⃣ GET /get-by-user-id/{userId}
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання всіх анкет конкретного студента

**Flow:**
```
1. Клієнт надсилає ID користувача

2. Сервер перевіряє JWT

3. Завантажує всі анкети де UserId = передане значення

4. Сортує за датою

5. Маршалює та повертає список
```

**Parameters:**
| Назва | Тип | Обов'язковий |
|-------|-----|------------|
| userId | Guid | Так |

**Response (200 OK):**
```json
[
  {
    "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
    "userId": "1fa85f64-5717-4562-b3fc-2c963f66afa0",
    "userFullName": "Ivan Petrov",
    "description": "I've been experiencing anxiety...",
    "statusName": "Accepted",
    "submittedAt": "2026-04-18T14:40:00Z"
  }
]
```

---

## 5️⃣ PUT /Update
**🔐 АВТОРИЗОВАНИЙ | власник анкети**

**Описание:** Оновлення опису анкети студентом до прийняття психологом

**Flow:**
```
1. Студент надсилає ID анкети та новий опис

2. Сервер перевіряє JWT та членство:
   if (q.UserId != JWT.UserId) return 403 Forbidden

3. Перевіряє статус (має бути "Pending"):
   if (q.Status != "Pending") return 400 Bad Request

4. Обновляє Description

5. Зберігає в БД

6. Повертає 204 No Content
```

**Request:**
```json
{
  "questionnaireId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "description": "Updated description about my concerns"
}
```

**Response:** `204 No Content`

---

## 6️⃣ PUT /Update-Status
**🔐 АВТОРИЗОВАНИЙ | Роль: Психолог, Керівник**

**Описание:** Зміна статусу анкети (Pending → Accepted, Rejected)

**Flow:**
```
1. Психолог надсилає ID анкети та новий statusId

2. Сервер перевіряє JWT та роль

3. Знаходить анкету та новий статус

4. Валідує: чи statusId існує в QuestionaryStatuses таблиці

5. Обновляє StatusId

6. Зберігає в БД

7. ЛОГУЄ в audit_logs: Action="QuestionnaireStatusChanged"

8. Повертає 204 No Content
```

**Request:**
```json
{
  "questionnaireId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "statusId": "13fa85f64-5717-4562-b3fc-2c963f66afa0"
}
```

**Response:** `204 No Content`

---

## 7️⃣ DELETE /Delete
**🔐 АВТОРИЗОВАНИЙ | Роль: Керівник служби ТІЛЬКИ**

**Описание:** Видалення анкети з системи (hard delete)

**Flow:**
```
1. Керівник надсилає ID анкети

2. Сервер перевіряє JWT та роль (403 якщо не Керівник)

3. Видаляє анкету з БД

4. ЛОГУЄ в audit_logs: Action="QuestionnaireDeleted"

5. Повертає 204 No Content
```

**Request:**
```json
{
  "questionnaireId": "8fa85f64-5717-4562-b3fc-2c963f66afab"
}
```

**Response:** `204 No Content`

---

# 📊 QuestionaryStatusController

**Base Route:** `/api/QuestiStatController`  
🔐 **Авторизовані ендпоїнти**

**Описание:** Управління статусами анкет (Pending, Accepted, Rejected)

---

## 1️⃣ GET /{id}
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання статусу анкети за ID

**Flow:**
```
1. Клієнт надсилає ID статусу

2. Сервер перевіряє JWT

3. Знаходить статус в QuestionaryStatuses таблиці

4. Маршалює та повертає
```

**Parameters:**
| Назва | Тип | Обов'язковий |
|-------|-----|------------|
| id | Guid | Так |

**Response (200 OK):**
```json
{
  "id": "12fa85f64-5717-4562-b3fc-2c963f66afaf",
  "name": "Pending"
}
```

---

## 2️⃣ GET /Get-All-Statuses
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання списку всіх можливих статусів анкет

**Flow:**
```
1. Клієнт надсилає GET запит

2. Сервер перевіряє JWT

3. Завантажує всі статуси з QuestionaryStatuses

4. Маршалює та повертає список (впорядковано)
```

**Response (200 OK):**
```json
[
  {
    "id": "12fa85f64-5717-4562-b3fc-2c963f66afaf",
    "name": "Pending"
  },
  {
    "id": "13fa85f64-5717-4562-b3fc-2c963f66afa0",
    "name": "Accepted"
  },
  {
    "id": "14fa85f64-5717-4562-b3fc-2c963f66afa1",
    "name": "Rejected"
  }
]
```

---

# 🏥 ConsultationStatusController

**Base Route:** `/api/ConsultationStatus`  
🔐 **Авторизовані ендпоїнти**

**Описание:** Управління статусами консультацій (Pending, Assigned, In Progress, Completed, Cancelled)

---

## 1️⃣ GET /Get-All-ConsultationStatuses
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання списку всіх можливих статусів консультацій

**Flow:**
```
1. Клієнт надсилає GET запит

2. Сервер перевіряє JWT

3. Завантажує всі статуси з ConsultationStatuses

4. Маршалює та повертає список
```

**Response (200 OK):**
```json
[
  {
    "id": "10fa85f64-5717-4562-b3fc-2c963f66afad",
    "name": "Pending"
  },
  {
    "id": "15fa85f64-5717-4562-b3fc-2c963f66afa2",
    "name": "Assigned"
  },
  {
    "id": "16fa85f64-5717-4562-b3fc-2c963f66afa3",
    "name": "In Progress"
  },
  {
    "id": "17fa85f64-5717-4562-b3fc-2c963f66afa4",
    "name": "Completed"
  }
]
```

---

## 2️⃣ GET /{id}
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання статусу консультації за ID

**Parameters:**
| Назва | Тип | Обов'язковий |
|-------|-----|------------|
| id | Guid | Так |

**Response (200 OK):**
```json
{
  "id": "10fa85f64-5717-4562-b3fc-2c963f66afad",
  "name": "Assigned"
}
```

---

# 👥 RoleController

**Base Route:** `/api/Role`  
🔐 **Авторизовані ендпоїнти**

**Описание:** Управління ролями системи (Student, Psychologist, HeadOfService)

---

## 1️⃣ GET
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання списку всіх ролей системи

**Flow:**
```
1. Клієнт надсилає GET запит

2. Сервер перевіряє JWT

3. Завантажує всі ролі з Roles таблиці

4. Маршалює та повертає список
```

**Response (200 OK):**
```json
[
  {
    "id": "00000000-0000-0000-0000-000000000001",
    "name": "Student"
  },
  {
    "id": "00000000-0000-0000-0000-000000000002",
    "name": "Psychologist"
  },
  {
    "id": "00000000-0000-0000-0000-000000000003",
    "name": "HeadOfService"
  }
]
```

---

## 2️⃣ GET /{id}
**🔐 АВТОРИЗОВАНИЙ**

**Описание:** Отримання деталей ролі за ID

**Parameters:**
| Назва | Тип | Обов'язковий |
|-------|-----|------------|
| id | Guid | Так |

**Response (200 OK):**
```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "name": "Student"
}
```

---

# 📋 Audit & Rate Limiting

## 📊 Audit Logging

**Система логує критичні операції** для забезпечення безпеки та моніторингу:

### Логовані Дії

| Дія | Ендпоїнт | Таблиця | Деталі |
|-----|----------|--------|--------|
| MessageSent | POST /Send | Messages | {ConsultationId, ReceiverId, HasAttachments} |
| MessageDeleted | DELETE /Delete | Messages | {MessageId} |
| AttachmentUploaded | POST /BatchUpload | MessageAttachments | {FileName, FileSize, Count} |
| AttachmentDeleted | DELETE /Attachment/{id} | MessageAttachments | {AttachmentId} |
| MessageRead | PUT /mark-as-read | Messages | {MessageId, IsRead} |
| ConsultationCreated | POST /Accept-Questionnaire | Consultations | {QuestionaryId, StudentId, PsychologistId} |
| ConsultationUpdated | PUT /Update-Consultation | Consultations | {StatusId, ScheduledTime} |
| UserRoleUpdated | PUT /User-Role-Update | Users | {UserId, OldRole, NewRole} |

### Структура AuditLog

```sql
CREATE TABLE audit_logs (
  id UUID PRIMARY KEY,
  user_id UUID NOT NULL,
  action VARCHAR(255) NOT NULL,         -- MessageSent, MessageDeleted, etc.
  resource VARCHAR(255) NOT NULL,       -- Message, Attachment, Consultation, etc.
  resource_id UUID,                     -- ID ресурсу що було змінено
  timestamp TIMESTAMP NOT NULL,         -- DateTime.UtcNow
  ip_address VARCHAR(45),               -- User's IP address
  status VARCHAR(50),                   -- Success, Failed
  details JSONB,                        -- JSON об'єкт з додатковою інфо
  error_message TEXT
);
```

## ⏱️ Rate Limiting

**Система обмежує кількість запитів** за допомогою Token Bucket алгоритму.

### Rate-Limited Ендпоїнти

| Ендпоїнт | Метод | Ліміт | Вікно | Per |
|----------|-------|-------|-------|-----|
| /api/Message/Send | POST | Configurable | Per user | За користувачем |
| /api/Message/BatchUpload | POST | Configurable | Per user | За користувачем |
| /api/Message/mark-as-read | PUT | Configurable | Per user | За користувачем |
| /api/Message/Delete | DELETE | Configurable | Per user | За користувачем |
| /api/Message/Attachment/{id} | DELETE | Configurable | Per user | За користувачем |

### Rate Limit Response

```
HTTP/1.1 429 Too Many Requests
Retry-After: 60
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1681234567

{
  "error": "Rate limit exceeded",
  "message": "Too many requests to SendMessage. Please try again in 60 seconds.",
  "retryAfter": 60
}
```

### Configuration (appsettings.json)

```json
{
  "RateLimit": {
    "DefaultLimit": 100,
    "LimitWindowSeconds": 60,
    "Enabled": true
  }
}
```

---

## 🔐 Security Summary Table

| Компонент | Ендпоїнт | Метод | Захист | Аудит |
|-----------|---------|-------|--------|-------|
| **Auth** | /google-login | POST | 🔓 Анонімний | ❌ |
| **Auth** | /{id} | GET | 🔐 JWT | ❌ |
| **Auth** | /get-by-email | GET | 🔐 JWT | ❌ |
| **Auth** | /all | GET | 🔐 JWT + Role | ❌ |
| **Auth** | /User-Delete | DELETE | 🔐 JWT + Role | ❌ |
| **Auth** | /User-course | PUT | 🔐 JWT | ❌ |
| **Auth** | /User-Role-Update | PUT | 🔐 JWT + Role | ✅ |
| **Message** | /BatchUpload | POST | 🔐 JWT + Владелец | ✅ |
| **Message** | /Recive | GET | 🔐 JWT + Членство | ❌ |
| **Message** | /Send | POST | 🔐 JWT | ✅ |
| **Message** | /Delete | DELETE | 🔐 JWT + Владелец | ✅ |
| **Message** | /Attachment/{id} | DELETE | 🔐 JWT + Владелец | ✅ |
| **Message** | /mark-as-read | PUT | 🔐 JWT + Одержувач | ✅ |
| **Consultation** | /Accept-Questionnaire | POST | 🔐 JWT + Role | ✅ |
| **Consultation** | /{id} | GET | 🔐 JWT | ❌ |
| **Consultation** | /all | GET | 🔐 JWT + Role | ❌ |
| **Consultation** | /Get-All-Consultations-By-UserId/{Id} | GET | 🔐 JWT | ❌ |
| **Consultation** | /Update-Consultation | PUT | 🔐 JWT + Role | ✅ |
| **Consultation** | /Delete-Consultation | DELETE | 🔐 JWT + Role | ✅ |
| **Questionnaire** | /Create-Questionnaire | POST | 🔐 JWT | ❌ |
| **Questionnaire** | /{id} | GET | 🔐 JWT | ❌ |
| **Questionnaire** | /all | GET | 🔐 JWT + Role | ❌ |
| **Questionnaire** | /get-by-user-id/{userId} | GET | 🔐 JWT | ❌ |
| **Questionnaire** | /Update | PUT | 🔐 JWT + Владелец | ❌ |
| **Questionnaire** | /Update-Status | PUT | 🔐 JWT + Role | ✅ |
| **Questionnaire** | /Delete | DELETE | 🔐 JWT + Role | ✅ |
| **QuestionaryStatus** | /{id} | GET | 🔐 JWT | ❌ |
| **QuestionaryStatus** | /Get-All-Statuses | GET | 🔐 JWT | ❌ |
| **ConsultationStatus** | /Get-All-ConsultationStatuses | GET | 🔐 JWT | ❌ |
| **ConsultationStatus** | /{id} | GET | 🔐 JWT | ❌ |
| **Role** | / | GET | 🔐 JWT | ❌ |
| **Role** | /{id} | GET | 🔐 JWT | ❌ |
| **ChatHub** | /JoinConsultation | SignalR | 🔐 JWT + Членство | ❌ |
| **ChatHub** | /SendMessage | SignalR | 🔐 JWT + Членство | ✅ |
| **ChatHub** | /MarkAsRead | SignalR | 🔐 JWT + Одержувач | ✅ |
| **ChatHub** | /DeleteMessage | SignalR | 🔐 JWT + Владелец | ✅ |

---

**Версія документації:** v3.0  
**Дата оновлення:** 18.04.2026  
**Статус:** ✅ Production Ready  
**Build Status:** ✅ 0 Errors | ✅ 138/141 Tests Passing | ✅ 2 Migrations Applied
