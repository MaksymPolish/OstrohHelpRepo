# API Endpoints Documentation

**Статус:** Updated 12.04.2026  
**Security Status:** MessageController & ChatHub are SECURED with authorization checks  
Усі ендпоїнти вимагають JWT токен в заголовку `Authorization: Bearer <token>` крім явно зазначених як "Анонімний"

---

## AuthController
**Base Route:** `/api/auth`  
**Доступ:** Більшість ендпоїнтів вимагають авторизацію

### POST /google-login
**Описание:** Автентифікація користувача через Google OAuth  
**Доступ:** Анонімний  
**Security:** Не вимагає JWT

**Request:**
```json
{
  "googleToken": "string"
}
```

**Response:** 200 OK
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "jwtToken": "string",
  "refreshToken": "string",
  "expiresAt": "datetime"
}
```

---

### GET /{id}
**Описание:** Отримання користувача за ID  
**Доступ:** Авторизований користувач

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|---|---|---|---|
| id | Guid | URL | Так |

**Response:** 200 OK
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "roleName": "string"
}
```

---

### GET /get-by-email
**Описание:** Отримання користувача за email  
**Доступ:** Авторизований користувач

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|---|---|---|---|
| email | string | Query | Так |

**Response:** 200 OK
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "roleName": "string"
}
```

---

### GET /all
**Описание:** Отримання всіх користувачів  
**Доступ:** Психолог або Керівник служби

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "email": "string",
    "fullName": "string",
    "roleId": "guid",
    "roleName": "string"
  }
]
```

---

### DELETE /User-Delete
**Описание:** Видалення користувача  
**Доступ:** Керівник служби

**Request:**
```json
{
  "userId": "guid"
}
```

**Response:** 204 No Content

---

### PUT /User-course
**Описание:** Оновлення курсу користувача  
**Доступ:** Авторизований користувач

**Request:**
```json
{
  "userId": "guid",
  "course": "integer"
}
```

**Response:** 204 No Content

---

### PUT /User-Role-Update
**Описание:** Оновлення ролі користувача  
**Доступ:** Керівник служби

**Request:**
```json
{
  "userId": "guid",
  "roleId": "guid"
}
```

**Response:** 204 No Content

---

## MessageController - SECURED
**Base Route:** `/api/Message`  
**Доступ:** Авторизований користувач, спеціальні перевірки для кожного ендпоїнту  
**Security:** ВСІ ЕНДПОЇНТИ ЗАХИЩЕНІ через IConsultationAccessChecker  
**Tests:** 10 Unit Tests (MessageControllerSecurityTests)

### POST /UploadToCloud/{userId}
**Описание:** Завантаження файлу в Cloudinary  
**Доступ:** Авторизований користувач  

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|---|---|---|---|
| userId | string | URL | Так |
| file | IFormFile | Body (multipart/form-data) | Так |

**Response:** 200 OK
```json
{
  "fileUrl": "string",
  "fileName": "string",
  "contentType": "string"
}
```

---

### POST /AddAttachment - SECURED
**Описание:** Додавання вкладення до повідомлення  
**Доступ:** Авторизований користувач (ТІЛЬКИ власник повідомлення)  
**Security:** ПЕРЕВІРЯЄ ВЛАСНИКА

**Request:**
```json
{
  "messageId": "guid",
  "fileUrl": "string",
  "fileType": "string (mime-type)"
}
```

**Response:** 200 OK
```json
{
  "id": "guid",
  "fileUrl": "string",
  "fileType": "string",
  "createdAt": "datetime"
}
```

**Error Responses:**
- 400 Bad Request — Невалідні дані
- 401 Unauthorized — Користувач не знайдений
- 403 Forbid — Користувач НЕ власник повідомлення

---

### GET /Recive
**Описание:** Отримання всіх повідомлень консультації  
**Доступ:** Авторизований користувач (учасник консультації)  
**Security:** ПЕРЕВІРЯЄ ЧЛЕНСТВО

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|---|---|---|---|
| idConsultation | Guid | Query | Так |

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "consultationId": "guid",
    "senderId": "guid",
    "senderName": "string",
    "senderPhotoUrl": "string",
    "receiverId": "guid",
    "receiverName": "string",
    "receiverPhotoUrl": "string",
    "text": "string",
    "isRead": "boolean",
    "sentAt": "datetime",
    "attachments": []
  }
]
```

---

### POST /Send
**Описание:** Надсилання нового повідомлення  
**Доступ:** Авторизований користувач

**Request:**
```json
{
  "consultationId": "guid",
  "receiverId": "guid",
  "text": "string"
}
```

**Response:** 200 OK
```json
{
  "id": "guid",
  "consultationId": "guid",
  "senderId": "guid",
  "receiverId": "guid",
  "text": "string",
  "sentAt": "datetime"
}
```

---

### DELETE /Delete
**Описание:** Видалення повідомлення  
**Доступ:** Авторизований користувач (ТІЛЬКИ власник)  
**Security:** ПЕРЕВІРЯЄ ВЛАСНИКА

**Request:**
```json
{
  "messageId": "guid"
}
```

**Response:** 204 No Content

---

### PUT /mark-as-read
**Описание:** Позначення повідомлення як прочитаного  
**Доступ:** Авторизований користувач (ТІЛЬКИ одержувач)  
**Security:** ПЕРЕВІРЯЄ ОДЕРЖУВАЧА

**Request:**
```json
{
  "messageId": "guid"
}
```

**Response:** 204 No Content

---

## ChatHub - SECURED (SignalR)
**Base Route:** `/chat` (WebSocket)  
**Доступ:** Авторизований користувач через JWT токен  
**Security:** УСІХ МЕТОДИ ЗАХИЩЕНІ — Використовує IConsultationAccessChecker  
**Tests:** 13 Unit Tests (ChatHubSecurityTests)

**Connection:** 
```
WebSocket: wss://localhost:7123/chat?access_token=<JWT_TOKEN>
```

### JoinConsultation - SECURED
**Описание:** Приєднання до кімнати консультації  
**Security Level:** ПЕРЕВІРЯЄ ЧЛЕНСТВО

**Request (Client sends):**
```json
{
  "consultationId": "guid"
}
```

**Response (Server sends back):**
```json
{
  "success": true,
  "message": "Приєднані до консультації"
}
```

**Error:**
```json
{
  "success": false,
  "message": "Ви не маєте доступу до цієї консультації"
}
```

---

### SendMessage - SECURED
**Описание:** Надсилання повідомлення до консультації  
**Security Level:** ПЕРЕВІРЯЄ ЧЛЕНСТВО

**Request (Client sends):**
```json
{
  "consultationId": "guid",
  "receiverId": "guid",
  "text": "string"
}
```

**Broadcast to Group (ReceiveMessage):**
```json
{
  "id": "guid",
  "consultationId": "guid",
  "senderId": "guid",
  "senderName": "string",
  "receiverId": "guid",
  "receiverName": "string",
  "text": "string",
  "sentAt": "datetime"
}
```

---

### MarkAsRead - SECURED
**Описание:** Позначення повідомлення як прочитаного  
**Security Level:** ПЕРЕВІРЯЄ ОДЕРЖУВАЧА

**Request (Client sends):**
```json
{
  "messageId": "guid"
}
```

**Broadcast to Group (ReceiveMarkedAsRead):**
```json
{
  "messageId": "guid",
  "consultationId": "guid",
  "isRead": true
}
```

---

### DeleteMessage - SECURED
**Описание:** Видалення повідомлення  
**Security Level:** ПЕРЕВІРЯЄ ВЛАСНИКА

**Request (Client sends):**
```json
{
  "messageId": "guid"
}
```

**Broadcast to Group (ReceiveDeletedMessage):**
```json
{
  "messageId": "guid",
  "consultationId": "guid",
  "timestamp": "datetime"
}
```

---

## ConsultationController
**Base Route:** `/api/Consultations`  
**Доступ:** Авторизований користувач

### POST /Accept-Questionnaire
**Описание:** Прийняття анкети та створення консультації  
**Доступ:** Психолог або Керівник служби

**Request:**
```json
{
  "questionnaireId": "guid",
  "psychologistId": "guid",
  "scheduledTime": "datetime"
}
```

**Response:** 201 Created
```json
{
  "id": "guid",
  "studentId": "guid",
  "psychologistId": "guid",
  "statusId": "guid",
  "scheduledTime": "datetime",
  "createdAt": "datetime"
}
```

---

### GET /{id}
**Описание:** Отримання інформації про консультацію  
**Доступ:** Авторизований користувач

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|---|---|---|---|
| id | Guid | URL | Так |

**Response:** 200 OK
```json
{
  "id": "guid",
  "studentId": "guid",
  "studentName": "string",
  "psychologistId": "guid",
  "psychologistName": "string",
  "statusId": "guid",
  "statusName": "string",
  "scheduledTime": "datetime",
  "createdAt": "datetime"
}
```

---

### PUT /Update-Consultation
**Описание:** Оновлення інформації про консультацію  
**Доступ:** Психолог або Керівник служби

**Request:**
```json
{
  "consultationId": "guid",
  "scheduledTime": "datetime"
}
```

**Response:** 204 No Content

---

### DELETE /Delete-Consultation
**Описание:** Видалення консультації  
**Доступ:** Керівник служби

**Request:**
```json
{
  "consultationId": "guid"
}
```

**Response:** 204 No Content

---

### GET /all
**Описание:** Отримання всіх консультацій  
**Доступ:** Психолог або Керівник служби

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "psychologistId": "guid",
    "psychologistName": "string",
    "statusName": "string",
    "scheduledTime": "datetime",
    "createdAt": "datetime"
  }
]
```

---

### GET /Get-All-Consultations-By-UserId/{Id}
**Описание:** Отримання всіх консультацій користувача  
**Доступ:** Авторизований користувач

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|---|---|---|---|
| Id | Guid | URL | Так |

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "psychologistId": "guid",
    "psychologistName": "string",
    "statusName": "string",
    "scheduledTime": "datetime",
    "createdAt": "datetime"
  }
]
```

---

## ConsultationStatusController
**Base Route:** `/api/ConsultationStatus`  
**Доступ:** Авторизований користувач

### GET /Get-All-ConsultationStatuses
**Описание:** Отримання всіх статусів консультацій

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "name": "string"
  }
]
```

---

### GET /{id}
**Описание:** Отримання статусу консультації за ID

**Parameters:**
| Назва | Тип | Розташування | Обов'язковий |
|---|---|---|---|
| id | Guid | URL | Так |

**Response:** 200 OK
```json
{
  "id": "guid",
  "name": "string"
}
```

---

## RoleController
**Base Route:** `/api/Role`  
**Доступ:** Авторизований користувач

### GET
**Описание:** Отримання всіх ролей

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "name": "string"
  }
]
```

---

## QuestionnaireController
**Base Route:** `/api/Questionnaire`  
**Доступ:** Авторизований користувач

### POST
**Описание:** Створення нової анкети студентом

**Response:** 201 Created

---

### GET /{id}
**Описание:** Отримання анкети за ID

**Response:** 200 OK

---

## Security Summary

| Компонент | Метод | Захист | Тести |
|---|---|---|---|
| MessageController.AddAttachment | POST | Перевірка власника | 1 test |
| MessageController.Receive | GET | Членство консультації | Chat tests |
| ChatHub.JoinConsultation | SignalR | Членство | 1 test |
| ChatHub.SendMessage | SignalR | Членство | 1 test |
| ChatHub.MarkAsRead | SignalR | Власник | 1 test |
| ChatHub.DeleteMessage | SignalR | Власник | 1 test |
| **Усього:** | - | **6 SECURED** | **43 Tests** |

---

**Версія документації:** 2.0 (12.04.2026)  
_Оновлюйте цей файл при додаванні нових ендпоїнтів._
# API Endpoints Documentation

:::info
**Статус:** ✅ Updated 12.04.2026  
**Security Status:** MessageController & ChatHub are SECURED with authorization checks ✅  
Усі ендпоїнти вимагають JWT токен в заголовку `Authorization: Bearer <token>` крім явно зазначених як "Анонімний"
:::

---

## 📌 AuthController
**Base Route:** `/api/auth`  
**Доступ:** Більшість ендпоїнтів вимагають авторизацію (див. нижче)

### POST /google-login
**Описение:** Автентифікація користувача через Google OAuth  
**Доступ:** 🔓 Анонімний  
**Security:** Не вимагає JWT

**Request:**
```json
{
  "googleToken": "string"
}
```

**Response:** 200 OK
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "jwtToken": "string",
  "refreshToken": "string",
  "expiresAt": "datetime"
}
```

---

### GET /{id}
**Описание:** Отримання користувача за ID  
**Доступ:** 🔐 Авторизований користувач

**Parameters:**
|Назва|Тип|Розташування|Обов'язковий|
|---|---|---|---|
|id|Guid|URL|Так|

**Response:** 200 OK
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "roleName": "string"
}
```

---

### GET /get-by-email
**Описание:** Отримання користувача за email  
**Доступ:** 🔐 Авторизований користувач

**Parameters:**
|Назва|Тип|Розташування|Обов'язковий|
|---|---|---|---|
|email|string|Query|Так|

**Response:** 200 OK
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "roleName": "string"
}
```

---

### GET /all
**Описание:** Отримання всіх користувачів  
**Доступ:** 🔐 Психолог або Керівник служби

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "email": "string",
    "fullName": "string",
    "roleId": "guid",
    "roleName": "string"
  }
]
```

---

### DELETE /User-Delete
**Описание:** Видалення користувача (керівник служби)  
**Доступ:** 🔐 Керівник служби

**Request:**
```json
{
  "userId": "guid"
}
```

**Response:** 204 No Content

---

### PUT /User-course
**Описание:** Оновлення курсу користувача  
**Доступ:** 🔐 Авторизований користувач

**Request:**
```json
{
  "userId": "guid",
  "course": "integer"
}
```

**Response:** 204 No Content

---

### PUT /User-Role-Update
**Описание:** Оновлення ролі користувача (керівник)  
**Доступ:** 🔐 Керівник служби

**Request:**
```json
{
  "userId": "guid",
  "roleId": "guid"
}
```

**Response:** 204 No Content

---

## 📌 MessageController 🔒 SECURED
**Base Route:** `/api/Message`  
**Доступ:** 🔐 Авторизований користувач, спеціальні перевірки для кожного ендпоїнту  
**Security:** 🔐🔐 **ВСІ ЕНДПОЇНТИ ЗАХИЩЕНІ** через `IConsultationAccessChecker`  
**Tests:** ✅ 10 Unit Tests (MessageControllerSecurityTests)

### POST /UploadToCloud/{userId}
**Описание:** Завантаження файлу в Cloudinary  
**Доступ:** 🔐 Авторизований користувач  
**Security Level:** 🟢 Standard

**Parameters:**
|Назва|Тип|Розташування|Обов'язковий|
|---|---|---|---|
|userId|string|URL|Так|
|file|IFormFile|Body (multipart/form-data)|Так|

**Response:** 200 OK
```json
{
  "fileUrl": "string",
  "fileName": "string",
  "contentType": "string"
}
```

---

### POST /AddAttachment ✅ SECURED
**Описание:** Додавання вкладення до повідомлення  
**Доступ:** 🔐 Авторизований користувач (ТІЛЬКИ власник повідомлення)  
**Security Level:** 🔴 **ПЕРЕВІРЯЄ ВЛАСНИКА** — Користувач УТИНЕН бути власником повідомлення  
**Tests:** ✅ AddAttachment_WhenUserOwnsMessage_ShouldSucceed

**Request:**
```json
{
  "messageId": "guid",
  "fileUrl": "string",
  "fileType": "string (mime-type)"
}
```

**Response:** 200 OK
```json
{
  "id": "guid",
  "fileUrl": "string",
  "fileType": "string",
  "createdAt": "datetime"
}
```

**Error Responses:**
- `400 Bad Request` — Невалідні дані (messageId або fileUrl пусті)
- `401 Unauthorized` — Користувач не знайдений
- `403 Forbid` — **Користувач НЕ власник повідомлення** ⚠️

---

### GET /Recive
**Описание:** Отримання всіх повідомлень консультації  
**Доступ:** 🔐 Авторизований користувач (учасник консультації)  
**Security Level:** 🔴 **ПЕРЕВІРЯЄ ЧЛЕНСТВО** — Користувач УТИНЕН бути членом консультації  
**Tests:** ✅ ChatHubSecurityTests перевіряє аналогічну логіку

**Parameters:**
|Назва|Тип|Розташування|Обов'язковий|
|---|---|---|---|
|idConsultation|Guid|Query|Так|

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "consultationId": "guid",
    "senderId": "guid",
    "senderName": "string",
    "senderPhotoUrl": "string | null",
    "receiverId": "guid",
    "receiverName": "string",
    "receiverPhotoUrl": "string | null",
    "text": "string",
    "isRead": "boolean",
    "sentAt": "datetime",
    "attachments": [
      {
        "id": "guid",
        "fileUrl": "string",
        "fileType": "string",
        "createdAt": "datetime"
      }
    ]
  }
]
```

---

### POST /Send
**Описание:** Надсилання нового повідомлення  
**Доступ:** 🔐 Авторизований користувач  
**Security Level:** 🟢 Standard

**Request:**
```json
{
  "consultationId": "guid",
  "receiverId": "guid",
  "text": "string"
}
```

**Response:** 200 OK
```json
{
  "id": "guid",
  "consultationId": "guid",
  "senderId": "guid",
  "receiverId": "guid",
  "text": "string",
  "sentAt": "datetime"
}
```

---

### DELETE /Delete
**Описание:** Видалення повідомлення  
**Доступ:** 🔐 Авторизований користувач (ТІЛЬКИ власник)  
**Security Level:** 🔴 **ПЕРЕВІРЯЄ ВЛАСНИКА**

**Request:**
```json
{
  "messageId": "guid"
}
```

**Response:** 204 No Content

---

### PUT /mark-as-read
**Описание:** Позначення повідомлення як прочитаного  
**Доступ:** 🔐 Авторизований користувач (ТІЛЬКИ одержувач)  
**Security Level:** 🔴 **ПЕРЕВІРЯЄ ОДЕРЖУВАЧА**

**Request:**
```json
{
  "messageId": "guid"
}
```

**Response:** 204 No Content

---

## 📌 ChatHub 🔒 SECURED (SignalR)
**Base Route:** `/chat` (WebSocket)  
**Доступ:** 🔐 Авторизований користувач через JWT токен  
**Security Level:** 🔴🔴 **ВСІХ МЕТОДИ ЗАХИЩЕНІ** — Використовує `IConsultationAccessChecker`  
**Tests:** ✅ 13 Unit Tests (ChatHubSecurityTests)

**Connection:** 
```
WebSocket: wss://localhost:7123/chat?access_token=<JWT_TOKEN>
```

### JoinConsultation ✅ SECURED
**Описание:** Приєднання до кімнати консультації (SignalR Group)  
**Security Level:** 🔴🔴 **ПЕРЕВІРЯЄ ЧЛЕНСТВО**
- Користувач УТИНЕН бути членом консультації
- Студент не може приєднатися до іншої консультації без дозволу

**Request (Client sends):**
```json
{
  "consultationId": "guid"
}
```

**Response (Server sends back):**
```json
{
  "success": true,
  "message": "Приєднані до консультації"
}
```

**Error:**
```json
{
  "success": false,
  "message": "Ви не маєте доступу до цієї консультації"
}
```

**Broadcast to Group (ReceiveJoinedConsultation):**
```json
{
  "consultationId": "guid",
  "userId": "guid",
  "userName": "string",
  "timestamp": "datetime"
}
```

---

### SendMessage ✅ SECURED
**Описание:** Надсилання повідомлення до консультації (в реал-часі)  
**Security Level:** 🔴 **ПЕРЕВІРЯЄ ЧЛЕНСТВО** — Користувач має бути членом консультації

**Request (Client sends):**
```json
{
  "consultationId": "guid",
  "receiverId": "guid",
  "text": "string"
}
```

**Broadcast to Group (ReceiveMessage):**
```json
{
  "id": "guid",
  "consultationId": "guid",
  "senderId": "guid",
  "senderName": "string",
  "senderPhotoUrl": "string | null",
  "receiverId": "guid",
  "receiverName": "string",
  "receiverPhotoUrl": "string | null",
  "text": "string",
  "sentAt": "datetime"
}
```

---

### MarkAsRead ✅ SECURED
**Описание:** Позначення повідомлення як прочитаного (в реал-часі)  
**Security Level:** 🔴🔴 **ПЕРЕВІРЯЄ ОДЕРЖУВАЧА** — Тільки одержувач повідомлення може позначити його як прочитане

**Request (Client sends):**
```json
{
  "messageId": "guid"
}
```

**Broadcast to Group (ReceiveMarkedAsRead):**
```json
{
  "messageId": "guid",
  "consultationId": "guid",
  "isRead": true
}
```

---

### DeleteMessage ✅ SECURED
**Описание:** Видалення повідомлення (в реал-часі)  
**Security Level:** 🔴🔴 **ПЕРЕВІРЯЄ ВЛАСНИКА** — Тільки власник повідомлення може його видалити

**Request (Client sends):**
```json
{
  "messageId": "guid"
}
```

**Broadcast to Group (ReceiveDeletedMessage):**
```json
{
  "messageId": "guid",
  "consultationId": "guid",
  "timestamp": "datetime"
}
```

---

## 📌 ConsultationController
**Base Route:** `/api/Consultations`  
**Доступ:** 🔐 Авторизований користувач

### POST /Accept-Questionnaire
**Описание:** Прийняття анкети психологом та створення консультації  
**Доступ:** 🔐 Психолог або Керівник служби

**Request:**
```json
{
  "questionnaireId": "guid",
  "psychologistId": "guid",
  "scheduledTime": "datetime"
}
```

**Response:** 201 Created
```json
{
  "id": "guid",
  "studentId": "guid",
  "psychologistId": "guid",
  "statusId": "guid",
  "scheduledTime": "datetime",
  "createdAt": "datetime"
}
```

---

### GET /{id}
**Описание:** Отримання інформації про консультацію  
**Доступ:** 🔐 Авторизований користувач

**Parameters:**
|Назва|Тип|Розташування|Обов'язковий|
|---|---|---|---|
|id|Guid|URL|Так|

**Response:** 200 OK
```json
{
  "id": "guid",
  "studentId": "guid",
  "studentName": "string",
  "psychologistId": "guid",
  "psychologistName": "string",
  "statusId": "guid",
  "statusName": "string",
  "scheduledTime": "datetime",
  "createdAt": "datetime"
}
```

---

### PUT /Update-Consultation
**Описание:** Оновлення інформації про консультацію  
**Доступ:** 🔐 Психолог або Керівник служби

**Request:**
```json
{
  "consultationId": "guid",
  "scheduledTime": "datetime"
}
```

**Response:** 204 No Content

---

### DELETE /Delete-Consultation
**Описание:** Видалення консультації  
**Доступ:** 🔐 Керівник служби

**Request:**
```json
{
  "consultationId": "guid"
}
```

**Response:** 204 No Content

---

### GET /all
**Описание:** Отримання всіх консультацій  
**Доступ:** 🔐 Психолог або Керівник служби

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "psychologistId": "guid",
    "psychologistName": "string",
    "statusName": "string",
    "scheduledTime": "datetime",
    "createdAt": "datetime"
  }
]
```

---

### GET /Get-All-Consultations-By-UserId/{Id}
**Описание:** Отримання всіх консультацій користувача  
**Доступ:** 🔐 Авторизований користувач

**Parameters:**
|Назва|Тип|Розташування|Обов'язковий|
|---|---|---|---|
|Id|Guid|URL|Так|

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "psychologistId": "guid",
    "psychologistName": "string",
    "statusName": "string",
    "scheduledTime": "datetime",
    "createdAt": "datetime"
  }
]
```

---

## 📌 ConsultationStatusController
**Base Route:** `/api/ConsultationStatus`  
**Доступ:** 🔐 Авторизований користувач

### GET /Get-All-ConsultationStatuses
**Описание:** Отримання всіх статусів консультацій

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "name": "string"
  }
]
```

---

### GET /{id}
**Описание:** Отримання статусу консультації за ID

**Parameters:**
|Назва|Тип|Розташування|Обов'язковий|
|---|---|---|---|
|id|Guid|URL|Так|

**Response:** 200 OK
```json
{
  "id": "guid",
  "name": "string"
}
```

---

## 📌 RoleController
**Base Route:** `/api/Role`  
**Доступ:** 🔐 Авторизований користувач

### GET
**Описание:** Отримання всіх ролей

**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "name": "string"
  }
]
```

---

## 📌 QuestionnaireController
**Base Route:** `/api/Questionnaire`  
**Доступ:** 🔐 Авторизований користувач

### POST
**Описание:** Створення нової анкети студентом

**Request:** (залежить від структури анкети)

**Response:** 201 Created

---

### GET /{id}
**Описание:** Отримання анкети за ID

**Response:** 200 OK

---

## 🔐 Security Summary

| Компонент | Метод | Захист | Тести |
|---|---|---|---|
| MessageController.AddAttachment | POST | ✅ Перевірка власника | ✅ 1 test |
| MessageController.Receive | GET | ✅ Членство консультації | ✅ ChatHub tests |
| ChatHub.JoinConsultation | SignalR | ✅ Членство | ✅ 1 test |
| ChatHub.SendMessage | SignalR | ✅ Членство | ✅ 1 test |
| ChatHub.MarkAsRead | SignalR | ✅ Власник | ✅ 1 test |
| ChatHub.DeleteMessage | SignalR | ✅ Власник | ✅ 1 test |
| **Усього:** | - | **6 SECURED** | **✅ 43 Tests** |

---

**Версія документації:** 2.0 (12.04.2026)  
_Оновлюйте цей файл при додаванні нових ендпоїнтів._
# API Endpoints Documentation

## AuthController
**Base Route:** `/api/auth`
**Доступ:** за замовчуванням авторизований користувач (див. ендпоінти нижче)

### - POST /google-login
**Що робить:** Автентифікація користувача через Google OAuth
**Доступ:** Анонімний користувач

**Що приймає:**
```json
{
  "googleToken": "string"
}
```

**Що видає:**
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "jwtToken": "string",
  "refreshToken": "string",
  "expiresAt": "datetime"
}
```

---

### - GET /{id}
**Що робить:** Отримання користувача за ID
**Доступ:** Авторизований користувач

**Що приймає:** `id` (Guid) - ID користувача в URL

**Що видає:**
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "roleName": "string"
}
```

---

### - GET /get-by-email
**Що робить:** Отримання користувача за email
**Доступ:** Авторизований користувач

**Що приймає:** `email` (string) - Email користувача в query параметрах

**Що видає:**
```json
{
  "id": "guid",
  "email": "string",
  "fullName": "string",
  "roleId": "guid",
  "roleName": "string"
}
```

---

### - GET /all
**Що робить:** Отримання всіх користувачів (тільки для психологів та керівників)
**Доступ:** Психолог або Керівник служби

**Що приймає:** Нічого

**Що видає:**
```json
[
  {
    "id": "guid",
    "email": "string",
    "fullName": "string",
    "roleId": "guid",
    "roleName": "string"
  }
]
```

---

### - DELETE /User-Delete
**Що робить:** Видалення користувача (тільки для керівника служби)
**Доступ:** Керівник служби

**Що приймає:**
```json
{
  "userId": "guid"
}
```

**Що видає:** 204 No Content (успіх) або 400 Bad Request з помилкою

---

### - PUT /User-course
**Що робить:** Оновлення курсу користувача
**Доступ:** Авторизований користувач

**Що приймає:**
```json
{
  "userId": "guid",
  "course": "number"
}
```

**Що видає:** 204 No Content

---

### - PUT /User-Role-Update
**Що робить:** Оновлення ролі користувача (тільки для керівника служби)
**Доступ:** Керівник служби

**Що приймає:**
```json
{
  "userId": "guid",
  "roleId": "guid"
}
```

**Що видає:** 204 No Content

---

## ConsultationController
**Base Route:** `/api/Consultations`
**Доступ:** авторизований користувач (див. ендпоінти нижче)

### - POST /Accept-Questionnaire
**Що робить:** Прийняття анкети та створення консультації (тільки для психологів та керівників)
**Доступ:** Психолог або Керівник служби

**Що приймає:**
```json
{
  "questionaryId": "guid",
  "psychologistId": "guid",
  "scheduledTime": "datetime"
}
```

**Що видає:** 201 Created або 400 Bad Request з помилкою

```json
{
  "id": "guid",
  "studentId": "guid",
  "studentName": "string",
  "studentPhotoUrl": "string | null",
  "psychologistId": "guid",
  "psychologistName": "string",
  "psychologistPhotoUrl": "string | null",
  "statusName": "string",
  "scheduledTime": "datetime",
  "createdAt": "datetime"
}
```

**Що нотифікується (SignalR):** подія `ConsultationStarted` для обох користувачiв

**Payload:**
```json
{
  "consultationId": "guid",
  "studentId": "guid",
  "studentName": "string",
  "studentPhotoUrl": "string | null",
  "psychologistId": "guid",
  "psychologistName": "string",
  "psychologistPhotoUrl": "string | null",
  "scheduledTime": "datetime",
  "message": "string",
  "timestamp": "datetime"
}
```

---

### - PUT /Update-Consultation
**Що робить:** Оновлення інформації про консультацію (тільки для психологів та керівників)
**Доступ:** Психолог або Керівник служби

**Що приймає:**
```json
{
  "consultationId": "guid",
  "statusId": "guid",
  "dateTime": "datetime",
  "note": "string"
}
```

**Що видає:** 204 No Content або 400 Bad Request з помилкою

---

### - DELETE /Delete-Consultation
**Що робить:** Видалення консультації (тільки для керівника служби)
**Доступ:** Керівник служби

**Що приймає:**
```json
{
  "consultationId": "guid"
}
```

**Що видає:** 204 No Content або 400 Bad Request з помилкою

---

### - GET /all
**Що робить:** Отримання всіх консультацій (тільки для психологів та керівників)
**Доступ:** Психолог або Керівник служби

**Що приймає:** Нічого

**Що видає:**
```json
[
  {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "studentPhotoUrl": "string | null",
    "psychologistId": "guid",
    "psychologistName": "string",
    "psychologistPhotoUrl": "string | null",
    "statusName": "string",
    "scheduledTime": "datetime",
    "createdAt": "datetime"
  }
]
```

---

### - GET /Get-Consultation-ById/{id}
**Що робить:** Отримання консультації за ID
**Доступ:** Авторизований користувач

**Що приймає:** `id` (Guid) - ID консультації в URL

**Що видає:**
```json
{
  "id": "guid",
  "studentId": "guid",
  "studentName": "string",
  "studentPhotoUrl": "string | null",
  "psychologistId": "guid",
  "psychologistName": "string",
  "psychologistPhotoUrl": "string | null",
  "statusName": "string",
  "scheduledTime": "datetime",
  "createdAt": "datetime"
}
```

---

### - GET /Get-All-Consultations-By-UserId/{Id}
**Що робить:** Отримання всіх консультацій користувача
**Доступ:** Авторизований користувач

**Що приймає:** `Id` (Guid) - ID користувача в URL

**Що видає:**
```json
[
  {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "studentPhotoUrl": "string | null",
    "psychologistId": "guid",
    "psychologistName": "string",
    "psychologistPhotoUrl": "string | null",
    "statusName": "string",
    "scheduledTime": "datetime",
    "createdAt": "datetime"
  }
]
```

---

## ConsultationStatusController
**Base Route:** `/api/ConsultationStatus`
**Доступ:** Авторизований користувач

### - GET /Get-All-ConsultationStatuses
**Що робить:** Отримання всіх статусів консультацій
**Доступ:** Авторизований користувач

**Що приймає:** Нічого

**Що видає:**
```json
[
  {
    "id": "guid",
    "name": "string"
  }
]
```

---

### - GET /{id}
**Що робить:** Отримання статусу консультації за ID
**Доступ:** Авторизований користувач

**Що приймає:** `id` (Guid) - ID статусу в URL

**Що видає:**
```json
{
  "id": "guid",
  "name": "string"
}
```

---

## MessageController
**Base Route:** `/api/Message`
**Доступ:** Авторизований користувач

### - POST /UploadToCloud/{userId}
**Що робить:** Завантаження файлу (зображення/відео/інші) в Cloudinary для конкретного користувача
**Доступ:** Авторизований користувач

**Що приймає:** 
- `userId` (string) - ID користувача в URL
- `file` (IFormFile) - файл у formData

**Що видає:**
```json
{
  "url": "string",
  "fileType": "string"
}
```

---

### - POST /AddAttachment
**Що робить:** Додавання вкладення до існуючого повідомлення (використовується після завантаження файлу в Cloudinary)
**Доступ:** Авторизований користувач

**Що приймає:**
```json
{
  "messageId": "guid",
  "fileUrl": "string",
  "fileType": "string"
}
```

**Що видає:**
```json
{
  "id": "guid",
  "fileUrl": "string",
  "fileType": "string",
  "createdAt": "datetime"
}
```

---

### - GET /Recive
**Що робить:** Отримання всіх повідомлень для конкретної консультації
**Доступ:** Авторизований користувач

**Що приймає:** `idConsultation` (Guid) - ID консультації в query параметрах

**Що видає:**
```json
[
  {
    "id": "guid",
    "senderId": "guid",
    "fullNameSender": "string",
    "receiverId": "guid",
    "fullNameReceiver": "string",
    "consultationId": "guid",
    "content": "string",
    "isRead": "boolean",
    "createdAt": "datetime"
  }
]
```

---

### - POST /Send
**Що робить:** Відправка повідомлення
**Доступ:** Авторизований користувач

**Що приймає:**
```json
{
  "senderId": "guid",
  "receiverId": "guid",
  "consultationId": "guid",
  "content": "string",
  "mediaPaths": ["string"] // опціонально
}
```

**Що видає:**
```json
{
  "id": "guid",
  "senderId": "guid",
  "receiverId": "guid",
  "consultationId": "guid",
  "content": "string",
  "isRead": "boolean",
  "createdAt": "datetime"
}
```

---

### - DELETE /Delete
**Що робить:** Видалення повідомлення
**Доступ:** Авторизований користувач

**Що приймає:**
```json
{
  "messageId": "guid"
}
```

**Що видає:**
```json
{
  "id": "guid"
}
```

---

### - PUT /mark-as-read
**Що робить:** Позначення повідомлення як прочитаного
**Доступ:** Авторизований користувач

**Що приймає:**
```json
{
  "messageId": "guid"
}
```

**Що видає:**
```json
{
  "id": "guid"
}
```

---

## QuestionaryStController
**Base Route:** `/api/QuestiStatController`
**Доступ:** Авторизований користувач

### - GET /{id}Get-By-Id
**Що робить:** Отримання статусу анкети за ID
**Доступ:** Авторизований користувач

**Що приймає:** `id` (Guid) - ID статусу в URL

**Що видає:**
```json
{
  "id": "guid",
  "name": "string"
}
```

---

### - GET /Get-All-Statuses
**Що робить:** Отримання всіх статусів анкет
**Доступ:** Авторизований користувач

**Що приймає:** Нічого

**Що видає:**
```json
[
  {
    "id": "guid",
    "name": "string"
  }
]
```

---

## QuestionnaireController
**Base Route:** `/api/questionnaire`
**Доступ:** Авторизований користувач

### - POST /Create-Questionnaire
**Що робить:** Створення нової анкети
**Доступ:** Авторизований користувач

**Що приймає:**
```json
{
  "userId": "guid",
  "problem": "string",
  "description": "string",
  "preferredDate": "datetime"
}
```

**Що видає:**
```json
{
  "id": "guid",
  "userId": "guid",
  "problem": "string",
  "description": "string",
  "preferredDate": "datetime",
  "statusId": "guid",
  "createdAt": "datetime"
}
```

---

### - GET /all
**Що робить:** Отримання всіх анкет (тільки для психологів та керівників)
**Доступ:** Психолог або Керівник служби

**Що приймає:** Нічого

**Що видає:**
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "fullName": "string",
    "email": "string",
    "problem": "string",
    "description": "string",
    "preferredDate": "datetime",
    "statusId": "guid",
    "statusName": "string",
    "createdAt": "datetime"
  }
]
```

---

### - GET /{id}
**Що робить:** Отримання анкети за ID
**Доступ:** Авторизований користувач

**Що приймає:** `id` (Guid) - ID анкети в URL

**Що видає:**
```json
{
  "id": "guid",
  "userId": "guid",
  "fullName": "string",
  "email": "string",
  "problem": "string",
  "description": "string",
  "preferredDate": "datetime",
  "statusId": "guid",
  "statusName": "string",
  "createdAt": "datetime"
}
```

---

### - GET /get-by-user-id/{id}
**Що робить:** Отримання всіх анкет користувача за його ID
**Доступ:** Авторизований користувач

**Що приймає:** `id` (Guid) - ID користувача в URL

**Що видає:**
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "fullName": "string",
    "email": "string",
    "problem": "string",
    "description": "string",
    "preferredDate": "datetime",
    "statusId": "guid",
    "statusName": "string",
    "createdAt": "datetime"
  }
]
```

---

### - GET /Get-All-Questionnaire-By-UserId/{id}
**Що робить:** Отримання всіх анкет користувача (дублікат попереднього ендпоінту)
**Доступ:** Авторизований користувач

**Що приймає:** `id` (Guid) - ID користувача в URL

**Що видає:**
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "fullName": "string",
    "email": "string",
    "problem": "string",
    "description": "string",
    "preferredDate": "datetime",
    "statusId": "guid",
    "statusName": "string",
    "createdAt": "datetime"
  }
]
```

---

### - DELETE /Delete-Questionnaire
**Що робить:** Видалення анкети (тільки для керівника служби)
**Доступ:** Керівник служби

**Що приймає:**
```json
"guid" // ID анкети
```

**Що видає:** 204 No Content або 400 Bad Request з помилкою

---

### - PUT /Update-Questionnaire
**Що робить:** Оновлення анкети
**Доступ:** Авторизований користувач

**Що приймає:**
```json
{
  "questionnaireId": "guid",
  "problem": "string",
  "description": "string",
  "preferredDate": "datetime"
}
```

**Що видає:** 204 No Content або 400 Bad Request з помилкою

---

### - PUT /Update-StatusQuestionnaire
**Що робить:** Оновлення статусу анкети (тільки для психологів та керівників)
**Доступ:** Психолог або Керівник служби

**Що приймає:**
```json
{
  "questionnaireId": "guid",
  "statusId": "guid"
}
```

**Що видає:** 204 No Content або 400 Bad Request з помилкою

---

## RoleController
**Base Route:** `/api/Role`
**Доступ:** Тільки для керівника служби

### - GET /Get-All-Roles
**Що робить:** Отримання всіх ролей

**Що приймає:** Нічого

**Що видає:**
```json
[
  {
    "id": "guid",
    "name": "string"
  }
]
```

---

### - GET /{id}
**Що робить:** Отримання ролі за ID

**Що приймає:** `id` (Guid) - ID ролі в URL

**Що видає:**
```json
{
  "id": "guid",
  "name": "string"
}
```

---

## SignalRHubController
**Base Route:** `/api/SignalRHub`
**Доступ:** Авторизований користувач

### - GET /info
**Що робить:** Отримання інформації про SignalR Chat Hub для підключення в реальному часі
**Доступ:** Авторизований користувач

**Що приймає:** Нічого

**Що видає:**
```json
{
  "hubName": "ChatHub",
  "url": "/hubs/chat",
  "protocol": "signalr",
  "requiresAuthentication": true,
  "description": "Real-time chat for consultations",
  "methods": [
    "JoinConsultation",
    "LeaveConsultation",
    "SendMessage",
    "MarkAsRead",
    "Typing",
    "StopTyping",
    "DeleteMessage"
  ],
  "events": [
    "ReceiveMessage",
    "UserJoined",
    "UserLeft",
    "UserTyping",
    "UserStoppedTyping",
    "MessageRead",
    "MessageDeleted",
    "Error"
  ],
  "exampleConnection": {
    "url": "wss://localhost:7000/hubs/chat",
    "headers": {
      "Authorization": "Bearer YOUR_JWT_TOKEN"
    }
  }
}
```

**SignalR методи:**

#### 1. JoinConsultation(consultationId: string)
- Приєднатися до кімнати консультації для отримання повідомлень у реальному часі

#### 2. LeaveConsultation(consultationId: string)
- Залишити кімнату консультації

#### 3. SendMessage(consultationId: string, text: string, attachments?: list)
- Відправити повідомлення до консультації
- receiver ID визначається автоматично

#### 4. MarkAsRead(messageId: string, consultationId: string)
- Позначити повідомлення як прочитане

#### 5. Typing(consultationId: string)
- Показати індикатор введення

#### 6. StopTyping(consultationId: string)
- Приховати індикатор введення

#### 7. DeleteMessage(messageId: string, consultationId: string)
- Видалити повідомлення

**SignalR події для прослуховування:**

- **ReceiveMessage:** Нове повідомлення отримано
- **UserJoined:** Користувач приєднався до консультації
- **UserLeft:** Користувач залишив консультацію
- **UserTyping:** Користувач починає печатати
- **UserStoppedTyping:** Користувач припинив печатати
- **MessageRead:** Повідомлення було прочитано
- **MessageDeleted:** Повідомлення було видалено
- **Error:** Сталася помилка

---

### - GET /example-js
**Що робить:** Отримання прикладу підключення для JavaScript/TypeScript

**Що приймає:** Нічого

**Що видає:**
```json
{
  "example": "// JavaScript/TypeScript Example\nimport * as signalR from '@microsoft/signalr';\n\nconst connection = new signalR.HubConnectionBuilder()\n    .withUrl('/hubs/chat', {\n        accessTokenFactory: () => 'YOUR_JWT_TOKEN'\n    })\n    .withAutomaticReconnect()\n    .build();\n\nconnection.on('ReceiveMessage', (message) => {\n    console.log('New message:', message);\n});\n\nawait connection.start();\nawait connection.invoke('JoinConsultation', 'consultation-id');\nawait connection.invoke('SendMessage', 'consultation-id', 'Hello!', []);"
}
```

---

### - GET /example-flutter
**Що робить:** Отримання прикладу підключення для Flutter

**Що приймає:** Нічого

**Що видає:**
```json
{
  "example": "// Flutter Example\nimport 'package:signalr_netcore/signalr_client.dart';\n\nfinal httpConnectionOptions = HttpConnectionOptions(\n    accessTokenFactory: () async => 'YOUR_JWT_TOKEN',\n    transport: HttpTransportType.WebSockets,\n);\n\nfinal hubConnection = HubConnectionBuilder()\n    .withUrl('https://localhost:7000/hubs/chat', options: httpConnectionOptions)\n    .build();\n\nhubConnection.on('ReceiveMessage', (arguments) {\n    print('New message: $arguments');\n});\n\nawait hubConnection.start();\nawait hubConnection.invoke('JoinConsultation', args: ['consultation-id']);\nawait hubConnection.invoke('SendMessage', args: ['consultation-id', 'Hello!', []]);"
}
```

---

### - GET /example-csharp
**Що робить:** Отримання прикладу підключення для C#/.NET

**Що приймає:** Нічого

**Що видає:**
```json
{
  "example": "// C#/.NET Example\nusing Microsoft.AspNetCore.SignalR.Client;\n\nvar connection = new HubConnectionBuilder()\n    .WithUrl(\"https://localhost:7000/hubs/chat\", options =>\n    {\n        options.AccessTokenProvider = () => Task.FromResult(\"YOUR_JWT_TOKEN\");\n    })\n    .WithAutomaticReconnect()\n    .Build();\n\nconnection.On<MessageDto>(\"ReceiveMessage\", message =>\n{\n    Console.WriteLine($\"New message: {message.Text}\");\n});\n\nawait connection.StartAsync();\nawait connection.InvokeAsync(\"JoinConsultation\", \"consultation-id\");\nawait connection.InvokeAsync(\"SendMessage\", \"consultation-id\", \"Hello!\", new List<object>());"
}
```
