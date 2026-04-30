# ostrohhelpapp

OstrohHelpApp / OA Mind Care is a Flutter client for student support, questionnaires, consultations, and real-time chat with psychologists.

Full backend endpoint reference lives in [ENDPOINTS.md](ENDPOINTS.md).

## Що це за застосунок

Застосунок дозволяє:

1. Увійти через Google.
2. Заповнювати та переглядати анкети.
3. Отримувати консультації та спілкуватися в чаті.
4. Бачити онлайн-статус співрозмовника в реальному часі.
5. Для психологів і керівників служби - керувати анкетами, користувачами та консультаціями.

## Ролі користувачів

| Роль | Що може робити |
|---|---|
| Student | Створювати анкети, переглядати власні анкети, відкривати консультації, писати в чаті |
| Psychologist | Переглядати анкети, приймати анкети, керувати консультаціями, спілкуватися з студентами |
| HeadOfService | Усе з доступу психолога плюс керування користувачами та ролями |

## Короткий user flow

### 1. Вхід у застосунок

1. Відкривається `AuthRoot`.
2. Якщо користувач не автентифікований, показується `LoginPage`.
3. Після входу через Google зберігаються токени і користувач потрапляє на `HomePage`.
4. Паралельно запускається presence-сервіс для online/offline статусів.

### 2. Потік студента

1. На `HomePage` користувач переходить до анкет або консультацій.
2. На `QuestionnairesListPage` можна:
	- створити нову анкету,
	- заповнити health questionnaire,
	- переглянути власні анкети.
3. На `QuestionnaireDetailsPage` можна переглянути деталі конкретної анкети.
4. На `ConsultationListPage` можна відкрити чат з призначеним психологом.
5. На `ChatPage` можна писати повідомлення, редагувати власні, видаляти власні, додавати вкладення, бачити read receipts та online/offline статус.

### 3. Потік психолога

1. Після входу психолог бачить ті ж основні вкладки.
2. Через `ConsultationListPage` відкривається чат зі студентом.
3. Через `AdminPanelPage` можна перейти до:
	- `AdminQuestionnairesPage`,
	- `AdminUsersPage`.
4. У `AdminQuestionnairesPage` психолог може приймати анкети та створювати консультації.

### 4. Потік керівника служби

1. Має доступ до адмін-панелі.
2. Може керувати анкетами та користувачами.
3. Може змінювати ролі, курс і видаляти користувачів.

## Карта маршрутів

| Route / entry | Page | Як відкривається | Примітка |
|---|---|---|---|
| `/` | `AuthRoot` | Старт застосунку | Вирішує, показати `LoginPage` або `HomePage` |
| `/` (після входу) | `HomePage` | Через `AuthRoot` | Головний дашборд |
| `/consultations` | `ConsultationListPage` | Named route або кнопка на Home | Список консультацій користувача |
| `/chat` | `ChatPage` | Named route з `consultationId` | Чат для конкретної консультації |
| `/admin-panel` | `AdminPanelPage` | Named route | Панель адміністрування |
| `/admin-questionnaires` | `AdminQuestionnairesPage` | З адмін-панелі | Список анкет для психолога/адміністратора |
| `/admin-users` | `AdminUsersPage` | З адмін-панелі | Управління користувачами |
| in-app | `QuestionnairesListPage` | Кнопка на Home / bottom nav | Список власних анкет |
| in-app | `QuestionnairePage` | З `QuestionnairesListPage` | Створення нової анкети |
| in-app | `HealthQuestionnairePage` | З `QuestionnairesListPage` | Заповнення health questionnaire |
| in-app | `QuestionnaireDetailsPage` | З `QuestionnairesListPage` | Перегляд деталей анкети |
| in-app | `ProfilePage` | Bottom nav | Профіль, тема застосунку, admin entry |

## Сторінка → що робить → які API використовує

| Сторінка | Що робить | API / сервіси |
|---|---|---|
| `LoginPage` | Логін через Google | `AuthBloc` → `AuthApiService.googleLogin()` |
| `AuthRoot` | Визначає auth state і запускає presence | `AuthBloc`, `TokenStorage`, `PresenceService` |
| `HomePage` | Головний хаб з швидкими діями | Прямий API не викликає, лише навігація |
| `QuestionnairesListPage` | Показує власні анкети, відкриває створення анкети / health questionnaire | `QuestionnaireApiService.getQuestionnairesByUserId()` |
| `QuestionnairePage` | Створює нову анкету | `QuestionnaireApiService.createQuestionnaire()` |
| `HealthQuestionnairePage` | Розраховує результат і відправляє анкету | `QuestionnaireCalculationService`, `QuestionnaireApiService.submitQuestionnaireResult()` |
| `QuestionnaireDetailsPage` | Показує деталі анкети | `QuestionnaireApiService.getQuestionnaireById()` |
| `ConsultationListPage` | Показує консультації користувача та онлайн-статус співрозмовника | `ConsultationApiService.getConsultationsByUserId()`, `OnlineUsersNotifier` |
| `ChatPage` | Real-time чат, вкладення, read receipts, edit/delete власних повідомлень | `ConsultationApiService.getConsultationById()`, `MessageApiService.getMessages()`, `MessageApiService.sendMessage()`, `MessageApiService.editMessage()`, `MessageApiService.deleteMessage()`, `MessageApiService.batchUpload()`, `ChatService` (SignalR) |
| `ProfilePage` | Показує профіль, перемикає тему, веде до адмін-панелі | `AuthApiService.getUserById()`, `AuthBloc` sign out, `AppThemeController` |
| `AdminPanelPage` | Дає доступ до адмін-розділів | Навігація |
| `AdminQuestionnairesPage` | Переглядає та приймає анкети | `QuestionnaireApiService.getAllQuestionnaires()`, `QuestionnaireApiService.updateQuestionnaireStatus()`, `ConsultationApiService.acceptConsultation()` |
| `AdminUsersPage` | Шукає, видаляє, змінює роль і курс користувачів | `AuthApiService.getAllUsers()`, `AuthApiService.deleteUser()`, `AuthApiService.updateUserRole()`, `AuthApiService.updateUserCourse()` |

## Технологічна карта API

### AuthApiService

| Метод | Endpoint | Призначення |
|---|---|---|
| `googleLogin()` | `POST /api/auth/google-login` | Google sign-in |
| `getUserById()` | `GET /api/auth/{id}` | Профіль користувача |
| `getUserByEmail()` | `GET /api/auth/get-by-email` | Пошук користувача |
| `getAllUsers()` | `GET /api/auth/all` | Список усіх користувачів |
| `deleteUser()` | `DELETE /api/auth/User-Delete` | Видалення користувача |
| `updateUserCourse()` | `PUT /api/auth/User-course` | Оновлення курсу |
| `updateUserRole()` | `PUT /api/auth/User-Role-Update` | Оновлення ролі |

### ConsultationApiService

| Метод | Endpoint | Призначення |
|---|---|---|
| `getAllConsultations()` | `GET /api/Consultations/all` | Усі консультації |
| `getConsultationById()` | `GET /api/Consultations/Get-Consultation-ById/{id}` | Деталі консультації |
| `getConsultationsByUserId()` | `GET /api/Consultations/Get-All-Consultations-By-UserId/{userId}` | Консультації користувача |
| `acceptConsultation()` | `POST /api/Consultations/Accept-Questionnaire` | Створення консультації з анкети |
| `updateConsultation()` | `PUT /api/Consultations/Update-Consultation` | Оновлення консультації |
| `deleteConsultation()` | `DELETE /api/Consultations/Delete-Consultation` | Видалення консультації |

### QuestionnaireApiService

| Метод | Endpoint | Призначення |
|---|---|---|
| `getAllQuestionnaires()` | `GET /api/questionnaire/all` | Усі анкети |
| `getQuestionnaireById()` | `GET /api/questionnaire/{id}` | Деталі анкети |
| `getQuestionnairesByUserId()` | `GET /api/questionnaire/Get-All-Questionnaire-By-UserId/{userId}` | Анкети користувача |
| `createQuestionnaire()` | `POST /api/questionnaire/Create-Questionnaire` | Створення анкети |
| `updateQuestionnaire()` | `PUT /api/questionnaire/Update-Questionnaire` | Редагування анкети |
| `updateQuestionnaireStatus()` | `PUT /api/questionnaire/Update-StatusQuestionnaire` | Зміна статусу |
| `deleteQuestionnaire()` | `DELETE /api/questionnaire/Delete-Questionnaire` | Видалення анкети |
| `submitQuestionnaireResult()` | обгортка над `createQuestionnaire()` | Надсилання health questionnaire |

### MessageApiService

| Метод | Endpoint | Призначення |
|---|---|---|
| `getMessages()` | `GET /api/Message/Recive?idConsultation=...` | Історія повідомлень |
| `sendMessage()` | `POST /api/Message/Send` | Надсилання текстового повідомлення |
| `editMessage()` | `PUT /api/Message/EditMessage` | Редагування повідомлення |
| `deleteMessage()` | `DELETE /api/Message/Delete` | Видалення повідомлення |
| `batchUpload()` | `POST /api/Message/BatchUpload?messageId=...` | Завантаження вкладень |

### ChatService (SignalR)

| Метод / event | Що робить |
|---|---|
| `JoinConsultation` | Підписує користувача на групу консультації та передає key |
| `SendMessage` | Відправляє encrypted message |
| `MarkAsRead` | Позначає окреме повідомлення як прочитане |
| `DeleteMessage` | Видаляє повідомлення real-time |
| `Typing` / `StopTyping` | Індикація набору тексту |
| `ReceiveMessage` | Нове повідомлення |
| `MessageRead` / `ReceiveMarkedAsRead` | Оновлення read receipts |
| `MessageUpdated` / `ReceiveUpdatedMessage` | Оновлення редагованого повідомлення |
| `MessageDeleted` / `ReceiveDeletedMessage` | Оновлення видаленого повідомлення |
| `ReceiveConsultationKey` | Ключ для шифрування |
| `UserStatusChanged` / `UserOnline` | Online/offline presence |

## Що користувач може робити на сторінках

| Сторінка | Дії користувача |
|---|---|
| `LoginPage` | Увійти через Google |
| `HomePage` | Перейти до анкет, консультацій, профілю |
| `QuestionnairesListPage` | Перегляд власних анкет, створення нової анкети, запуск health questionnaire |
| `QuestionnairePage` | Надіслати нову анкету |
| `HealthQuestionnairePage` | Відповісти на 15 питань і отримати результат |
| `QuestionnaireDetailsPage` | Перегляд опису, статусу та дати подання анкети |
| `ConsultationListPage` | Перегляд консультацій, відкриття чату, бачити online/offline |
| `ChatPage` | Писати повідомлення, редагувати та видаляти власні, додавати файли, бачити read receipts |
| `ProfilePage` | Перегляд профілю, зміна теми, перехід в адмін-панель для дозволених ролей |
| `AdminPanelPage` | Відкрити анкети або користувачів |
| `AdminQuestionnairesPage` | Приймати анкети, створювати консультацію |
| `AdminUsersPage` | Шукати, видаляти, змінювати роль і курс користувачів |

## Примітки по реалізації

1. Чат працює через SignalR, а не polling.
2. Історія повідомлень завантажується через REST, а нові події приходять через WebSocket.
3. Read receipts відправляються як окремий `MarkAsRead(messageId, consultationId)` для кожного повідомлення.
4. Дати в чаті відображаються через `intl` з локаллю `uk_UA`.
5. Ключ шифрування консультації передається під час `JoinConsultation`.

## Де дивитися backend-контракт

Якщо потрібні точні request/response приклади або повний список ендпоїнтів, відкрий [ENDPOINTS.md](ENDPOINTS.md).
