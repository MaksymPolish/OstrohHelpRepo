# OstrohHelp Web

Веб-версія платформи психологічної підтримки **OstrohHelp** для студентів, психологів та адміністратора служби.

README нижче описує сайт на рівні продукту: що це за система, які є ролі, які сторінки доступні користувачу, як побудовані основні user flows і які API/SignalR ендпоїнти використовуються. Детальна технічна специфікація контрактів API винесена в [ENDPOINTS.md](ENDPOINTS.md), а дорожня карта та стан модулів описані в [SITE_DOCUMENTATION.md](SITE_DOCUMENTATION.md).

## Що це за сайт

OstrohHelp Web - це клієнтська веб-платформа для:

- швидкої автентифікації через Google OAuth;
- заповнення анкет на консультацію;
- прийняття анкет психологом і створення консультацій;
- перегляду списку своїх консультацій та чатів;
- безпечного чату між студентом і психологом;
- завантаження вкладень у повідомленнях;
- real-time обміну повідомленнями через SignalR;
- роботи на desktop і mobile.

Основна ідея сайту - дати студенту зрозумілий шлях: увійти в систему, заповнити анкету, потрапити в консультацію і спілкуватися з психологом у захищеному чаті.

## Технологічний стек

- React 19
- react-router-dom 7
- Axios
- SignalR (`@microsoft/signalr`)
- Google OAuth (`@react-oauth/google`)
- Tailwind CSS
- react-icons
- lucide-react
- react-scripts

## Ролі користувачів

### Student
Студент може:

- зайти через Google OAuth;
- заповнити анкету на консультацію;
- переглядати свої анкети та їх статуси;
- переглядати список консультацій;
- відкривати чат із психологом;
- надсилати повідомлення та вкладення;
- бачити статус прочитання своїх повідомлень;
- оновлювати профільні дані, які приходять з backend.

### Psychologist
Психолог може:

- увійти в систему;
- бачити анкети студентів;
- приймати анкету і створювати консультацію;
- змінювати статус консультації;
- спілкуватися зі студентами в чаті;
- отримувати real-time повідомлення та статусы читання;
- працювати зі вкладеннями в чаті.

### HeadOfService
Керівник служби може:

- виконувати всі дії психолога;
- керувати користувачами;
- змінювати ролі;
- видаляти користувачів;
- видаляти консультації;
- використовувати адміністративні функції, які передбачені backend.

## Основні сторінки сайту

Нижче - основні екрани, які є у фронтенді.

- **HomePageClean** - головна сторінка з вступом, CTA-кнопками, блоком консультацій/сесій та основними переходами.
- **LoginPage** - вхід у систему через Google OAuth або mock auth залежно від env.
- **QuestionnairesPage** - список анкет, які користувач може переглядати або створювати.
- **QuestionnaireDetailPage** - детальний перегляд анкети.
- **MyQuestionnairesPage** - анкети поточного користувача.
- **ConsultationsPage** - список консультацій і чат між учасниками консультації.
- **ChatPage** - чатова сторінка або окремий chat flow, якщо використовується у конкретному маршруті.
- **ProfilePage** - профіль користувача, включно з розбиттям ПІБ.
- **ResourcesPage** - матеріали та ресурси для користувачів.
- **AdminPanelPage** - адміністративні інструменти.
- **TermsOfUse** - умови використання.
- **PrivacyPolicy** - політика приватності.
- **CookiePolicy** - політика cookies.
- **NotFoundPage** - сторінка 404.

## Карта маршрутів

Нижче показані маршрути, які реально використовуються у `src/App.js`:

| Маршрут | Сторінка | Призначення |
|---|---|---|
| `/` | HomePageClean | Головна сторінка сайту |
| `/homepage` | HomePageClean | Альтернативний шлях до головної |
| `/consultations` | ConsultationsPage | Консультації та чат |
| `/questionnaires` | QuestionnairesPage | Список анкет |
| `/my-questionnaires` | MyQuestionnairesPage | Анкети поточного користувача |
| `/profile` | ProfilePage | Профіль користувача |
| `/admin` | AdminPanelPage | Адмін-панель |
| `/privacy` | PrivacyPolicy | Політика приватності |
| `/terms` | TermsOfUse | Умови використання |
| `/cookies` | CookiePolicy | Політика cookies |
| `*` | LoginPage / NotFoundPage | Для неавторизованих - login, для авторизованих - 404 на невідомий шлях |

## Сторінки та API

Таблиця нижче показує, що робить кожна сторінка і які основні API або SignalR точки вона використовує.

| Сторінка | Що робить | Які API використовує |
|---|---|---|
| HomePageClean | Показує головний екран, CTA та вступні блоки | Залежить від поточної реалізації; зазвичай використовує дані користувача і може показувати консультації або інформаційні блоки |
| LoginPage | Дає вхід у систему через Google OAuth або mock login | `POST /api/auth/google-login` |
| QuestionnairesPage | Показує список анкет і дає створити нову анкету | `GET /api/Questionnaire/all`, `POST /api/Questionnaire/Create-Questionnaire` |
| QuestionnaireDetailPage | Відкриває деталі конкретної анкети | `GET /api/Questionnaire/{id}` |
| MyQuestionnairesPage | Показує анкети поточного користувача | `GET /api/Questionnaire/get-by-user-id/{userId}` |
| ConsultationsPage | Показує консультації, історію повідомлень і чат | `GET /api/Consultations/Get-All-Consultations-By-UserId/{Id}`, `GET /api/Message/Recive?idConsultation={consultationId}`, `POST /api/Message/Send`, `POST /api/Message/BatchUpload`, `PUT /api/Message/EditMessage`, `DELETE /api/Message/Delete`, `DELETE /api/Message/Attachment/{attachmentId}`, SignalR `JoinConsultation`, `ReceiveMessage`, `MarkAsRead`, `ReceiveConsultationKey`, `UserStatusChanged` |
| ChatPage | Якщо використовується окремо, показує чат або чат-інтерфейс | Ті самі точки, що й ConsultationsPage, залежно від маршруту |
| ProfilePage | Показує профіль користувача і його дані з backend | `GET /api/auth/{id}`, `GET /api/auth/get-by-email` |
| ResourcesPage | Показує довідкові матеріали та контент | Якщо підключений backend-контент, використовує відповідні read-only ендпоїнти; у базовому вигляді може працювати без API |
| AdminPanelPage | Адміністрування користувачів, ролей і службових даних | `GET /api/auth/all`, `DELETE /api/auth/User-Delete`, `PUT /api/auth/User-Role-Update`, `PUT /api/auth/User-course`, `GET /api/Role`, `GET /api/Consultations/all`, `DELETE /api/Consultations/Delete-Consultation` |
| TermsOfUse | Статична сторінка з умовами використання | API не потрібен |
| PrivacyPolicy | Статична сторінка з політикою приватності | API не потрібен |
| CookiePolicy | Статична сторінка з політикою cookies | API не потрібен |
| NotFoundPage | Показує 404 для невідомих маршрутів | API не потрібен |

## Що користувач може робити на сайті

### 1. Увійти в систему
Користувач натискає кнопку логіну і проходить Google OAuth. Після успішного входу фронтенд отримує JWT, refresh token та основні дані профілю.

### 2. Переглядати головну сторінку
На головній сторінці користувач бачить:

- вступний блок;
- основні CTA-кнопки;
- інформацію про сервіс;
- блоки, пов'язані з консультаціями або майбутніми сесіями;
- навігацію до анкети, чатів і ресурсів.

### 3. Заповнити анкету
Студент може створити анкету для запиту консультації. Після створення анкета зберігається на backend і далі доступна в списку анкет користувача.

### 4. Переглянути анкети
Користувач може бачити:

- список власних анкет;
- статус анкети;
- деталі анкети;
- історію змін у межах відповідного процесу.

### 5. Прийняти анкету як психолог
Психолог бачить анкети зі статусом очікування, може прийняти анкету, створити консультацію та призначити час.

### 6. Переглядати консультації
У списку консультацій користувач бачить:

- свою активну або минулу консультацію;
- співрозмовника;
- статус консультації;
- прив'язку до анкети;
- дату створення або запланований час.

### 7. Спілкуватися в чаті
У чаті користувач може:

- писати текстові повідомлення;
- редагувати власні повідомлення;
- видаляти власні повідомлення;
- додавати файли;
- отримувати повідомлення в real-time;
- бачити статусы доставки та прочитання;
- бачити online/offline presence співрозмовника.

### 8. Працювати зі вкладеннями
Чат підтримує:

- завантаження файлів;
- preview для зображень і відео;
- безпечне видалення вкладень;
- відображення посилань на файли.

### 9. Переглядати профіль
Профіль показує основні дані користувача та коректно розбиває ПІБ, якщо backend повертає повне ім'я одним рядком.

### 10. Отримувати доступ до ресурсів і довідкових сторінок
Сайт містить сторінки з політиками, ресурсами та іншими довідковими матеріалами.

### 11. Керувати користувачами та ролями
Адміністративні можливості доступні користувачам з відповідними правами.

## Основні user flows

### Flow 1. Вхід у систему
1. Користувач відкриває LoginPage.
2. Натискає кнопку входу через Google.
3. Отримує Google токен.
4. Frontend відправляє токен на backend.
5. Backend повертає JWT, refresh token і дані користувача.
6. Фронтенд зберігає сесію і відкриває основний інтерфейс.

### Flow 2. Заповнення анкети
1. Студент відкриває форму анкети.
2. Заповнює опис проблеми і додаткові параметри.
3. Надсилає анкету на backend.
4. Анкета з'являється у списку.
5. Психолог бачить анкету в загальному списку очікуючих.

### Flow 3. Прийняття анкети психологом
1. Психолог відкриває список анкет.
2. Обирає анкету студента.
3. Призначає час консультації, якщо потрібно.
4. Backend створює consultation record.
5. Анкета переходить у відповідний статус.
6. У студента з'являється нова консультація.

### Flow 4. Відкриття консультації та чату
1. Користувач відкриває ConsultationsPage.
2. Фронтенд завантажує список консультацій користувача.
3. Користувач обирає активну консультацію.
4. Frontend отримує історію повідомлень.
5. Підключається до SignalR group консультації.
6. Отримує ключ шифрування для цієї консультації.

### Flow 5. Надсилання повідомлення
1. Користувач вводить текст.
2. Фронтенд шифрує повідомлення локально.
3. Надсилає encrypted payload на backend.
4. Повідомлення зберігається в БД.
5. Через SignalR інший учасник одразу отримує нове повідомлення.
6. Статус прочитання оновлюється, коли одержувач відкриває чат.

### Flow 6. Вкладення в чаті
1. Користувач обирає файли.
2. Фронтенд відправляє файли в upload endpoint.
3. Backend завантажує їх у Cloudinary.
4. Повертаються URL та прев'ю.
5. Вкладення прив'язуються до повідомлення.

### Flow 7. Read status
1. Одержувач відкриває чат.
2. Непрочитані повідомлення автоматично позначаються як прочитані.
3. Backend оновлює `IsRead = true`.
4. Відправник бачить оновлення через SignalR.
5. У UI відображається одна або дві галочки залежно від стану.

### Flow 8. Presence
1. Користувач підключається до chat hub.
2. Система фіксує, що користувач online.
3. Інші учасники бачать presence update.
4. При виході зі сторінки або втраті з'єднання статус стає offline.

## Які ендпоїнти використовуються

Нижче наведено основні ендпоїнти, які використовує фронтенд. Повний контракт, приклади payload і технічні деталі дивись у [ENDPOINTS.md](ENDPOINTS.md).

### AuthController
- `POST /api/auth/google-login` - логін через Google OAuth.
- `GET /api/auth/{id}` - отримання профілю користувача за ID.
- `GET /api/auth/get-by-email` - пошук користувача за email.
- `GET /api/auth/all` - список користувачів для ролей вище Student.
- `DELETE /api/auth/User-Delete` - видалення користувача.
- `PUT /api/auth/User-course` - встановлення курсу користувачу.
- `PUT /api/auth/User-Role-Update` - оновлення ролі користувача.

### ConsultationController
- `POST /api/Consultations/Accept-Questionnaire` - прийняття анкети та створення консультації.
- `GET /api/Consultations/{id}` - деталі консультації.
- `GET /api/Consultations/all` - список консультацій.
- `GET /api/Consultations/Get-All-Consultations-By-UserId/{Id}` - консультації конкретного користувача.
- `PUT /api/Consultations/Update-Consultation` - оновлення часу або статусу консультації.
- `DELETE /api/Consultations/Delete-Consultation` - видалення консультації.

### QuestionnaireController
- `POST /api/Questionnaire/Create-Questionnaire` - створення анкети.
- `GET /api/Questionnaire/{id}` - деталі анкети.
- `GET /api/Questionnaire/all` - список анкет.
- `GET /api/Questionnaire/get-by-user-id/{userId}` - анкети конкретного користувача.
- `PUT /api/Questionnaire/Update` - оновлення анкети власником.
- `PUT /api/Questionnaire/Update-Status` - зміна статусу анкети.
- `DELETE /api/Questionnaire/Delete` - видалення анкети.

### QuestionaryStatusController
- `GET /api/QuestiStatController/{id}` - отримання статусу анкети.
- `GET /api/QuestiStatController/Get-All-Statuses` - список статусів анкет.

### ConsultationStatusController
- `GET /api/ConsultationStatus/Get-All-ConsultationStatuses` - список статусів консультацій.
- `GET /api/ConsultationStatus/{id}` - отримання статусу консультації.

### MessageController
- `POST /api/Message/BatchUpload` - завантаження вкладень у Cloudinary.
- `GET /api/Message/Recive?idConsultation={consultationId}` - історія повідомлень консультації.
- `POST /api/Message/Send` - надсилання нового повідомлення.
- `PUT /api/Message/EditMessage` - редагування власного повідомлення.
- `DELETE /api/Message/Delete` - soft delete повідомлення.
- `DELETE /api/Message/Attachment/{attachmentId}` - soft delete вкладення.
- `PUT /api/Message/mark-as-read` - у REST-контурі цей endpoint більше не є основним; читання працює через SignalR MarkAsRead.

### RoleController
- `GET /api/Role` - список ролей.
- `GET /api/Role/{id}` - деталізація ролі.

### SignalR / ChatHub
- `JoinConsultation(consultationId)` - приєднання до групи консультації.
- `SendMessage` - real-time надсилання повідомлення.
- `MarkAsRead` - позначення повідомлення як прочитаного.
- `DeleteMessage` - видалення повідомлення.
- `ReceiveConsultationKey` - передача ключа шифрування для конкретної консультації.
- `ReceiveMessage` - нове повідомлення в чаті.
- `MessageUpdated` - оновлення повідомлення.
- `MessageRead` або `ReceiveMarkedAsRead` - оновлення read status.
- `ReceiveJoinedConsultation` - подія при вході учасника в консультацію.
- `UserStatusChanged` - update presence online/offline.

## Як працює чат

Чат у цьому проєкті побудований на двох шарах:

- **REST API** - для завантаження історії, списків і базових CRUD-операцій;
- **SignalR** - для real-time доставки, статусу прочитання і presence.

### Основні принципи чату

- повідомлення шифруються на фронтенді;
- backend зберігає encrypted payload, iv та authTag;
- ключ консультації передається тільки через SignalR;
- історія чату підтягується через REST;
- нові повідомлення приходять через SignalR без polling;
- статус read/unread синхронізується в реальному часі;
- вкладення зберігаються окремо і прив'язуються до message records.

## Профіль користувача

Сторінка профілю показує дані користувача, що повертаються з backend. Якщо backend віддає повне ПІБ одним рядком, фронтенд розбиває його на більш зручні поля для відображення.

## Навігація та локалізація

- У проєкті використовується централізована локалізація в `src/i18n/translations.js`.
- Тексти кнопок, заголовки та блоки інтерфейсу беруться з перекладів.
- Навігація керується через React Router.

## Конфігурація середовища

Створіть `.env` на основі `.env.example` і заповніть змінні:

```bash
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_SIGNALR_HUB_URL=ws://localhost:5000/hubs/chat
REACT_APP_GOOGLE_CLIENT_ID=your_google_client_id_here
REACT_APP_USE_MOCK_AUTH=true
REACT_APP_ENABLE_TOKEN_REFRESH=false
REACT_APP_TOKEN_REFRESH_ENDPOINT=/auth/refresh-token
```

### Пояснення змінних

- `REACT_APP_API_URL` - базовий URL REST API.
- `REACT_APP_SIGNALR_HUB_URL` - URL SignalR хабу.
- `REACT_APP_GOOGLE_CLIENT_ID` - Google OAuth Client ID.
- `REACT_APP_USE_MOCK_AUTH` - вмикає тестовий mock login.
- `REACT_APP_ENABLE_TOKEN_REFRESH` - вмикає refresh token flow.
- `REACT_APP_TOKEN_REFRESH_ENDPOINT` - endpoint для оновлення JWT.

## Запуск проєкту

### Встановлення залежностей

```bash
npm install
```

### Запуск у режимі розробки

```bash
npm start
```

Додаток буде доступний за адресою `http://localhost:3000`.

### Production build

```bash
npm build
```

### Тести

```bash
npm test
```

## Структура проєкту

```text
ostrohhelpweb/
├── public/
│   └── index.html
├── src/
│   ├── components/
│   │   ├── Common/
│   │   └── Layout/
│   ├── config/
│   ├── hooks/
│   ├── i18n/
│   ├── pages/
│   ├── services/
│   ├── utils/
│   ├── App.js
│   ├── App.css
│   ├── index.js
│   └── index.css
├── ENDPOINTS.md
├── SITE_DOCUMENTATION.md
└── package.json
```

## Важливі технічні примітки

- Авторизація побудована на Google OAuth + backend exchange.
- У клієнта є автоматичне додавання JWT до API запитів.
- У разі 401 може бути запущений refresh token flow, якщо це увімкнено в env.
- Чат працює з encrypted payload, тому формати `encryptedContent`, `iv` і `authTag` мають бути узгоджені з backend.
- Read status використовується для відображення однієї або двох галочок у UI.
- Frontend підтримує normalізацію camelCase/PascalCase полів, тому відповіді backend можуть бути більш ніж в одному форматі.

## Для розробки нового функціоналу

Рекомендований порядок:

1. Описати сценарій у README або `SITE_DOCUMENTATION.md`.
2. Перевірити контракт у `ENDPOINTS.md`.
3. Додати/оновити API service у `src/services/`.
4. Оновити UI у `src/pages/` або `src/components/`.
5. Прогнати build і перевірити повідомлення ESLint.

## Додаткова документація

- [SITE_DOCUMENTATION.md](SITE_DOCUMENTATION.md) - стан сайту, поточні задачі, roadmap.
- [ENDPOINTS.md](ENDPOINTS.md) - повна документація API та SignalR.

## Ліцензія

MIT
