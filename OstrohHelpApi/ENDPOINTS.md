# API Endpoints Documentation

## AuthController
**Base Route:** `/api/auth`

### - POST /google-login
**Що робить:** Автентифікація користувача через Google OAuth

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

### - POST /Accept-Questionnaire
**Що робить:** Прийняття анкети та створення консультації (тільки для психологів та керівників)

**Що приймає:**
```json
{
  "questionnaireId": "guid",
  "psychologistId": "guid"
}
```

**Що видає:** 204 No Content або 400 Bad Request з помилкою

---

### - PUT /Update-Consultation
**Що робить:** Оновлення інформації про консультацію (тільки для психологів та керівників)

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

**Що приймає:** Нічого

**Що видає:**
```json
[
  {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "psychologistId": "guid",
    "psychologistName": "string",
    "statusId": "guid",
    "statusName": "string",
    "dateTime": "datetime",
    "note": "string"
  }
]
```

---

### - GET /Get-Consultation-ById/{id}
**Що робить:** Отримання консультації за ID

**Що приймає:** `id` (Guid) - ID консультації в URL

**Що видає:**
```json
{
  "id": "guid",
  "studentId": "guid",
  "studentName": "string",
  "psychologistId": "guid",
  "psychologistName": "string",
  "statusId": "guid",
  "statusName": "string",
  "dateTime": "datetime",
  "note": "string"
}
```

---

### - GET /Get-All-Consultations-By-UserId/{Id}
**Що робить:** Отримання всіх консультацій користувача

**Що приймає:** `Id` (Guid) - ID користувача в URL

**Що видає:**
```json
[
  {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "psychologistId": "guid",
    "psychologistName": "string",
    "statusId": "guid",
    "statusName": "string",
    "dateTime": "datetime",
    "note": "string"
  }
]
```

---

## ConsultationStatusController
**Base Route:** `/api/ConsultationStatus`

### - GET /Get-All-ConsultationStatuses
**Що робить:** Отримання всіх статусів консультацій

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

### - POST /UploadToCloud/{userId}
**Що робить:** Завантаження файлу (зображення/відео/інші) в Cloudinary для конкретного користувача

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

### - GET /{id}Get-By-Id
**Що робить:** Отримання статусу анкети за ID

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

### - POST /Create-Questionnaire
**Що робить:** Створення нової анкети

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

**Що приймає:**
```json
"guid" // ID анкети
```

**Що видає:** 204 No Content або 400 Bad Request з помилкою

---

### - PUT /Update-Questionnaire
**Що робить:** Оновлення анкети

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
