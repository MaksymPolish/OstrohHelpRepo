# OstrohHelp Web

Веб-версія першого додатку OstrohHelp для допомоги студентам.

## Структура проекту

```
ostrohhelpweb/
├── public/           # Статичні файли
│   └── index.html    # Головний HTML файл
├── src/              # Вихідний код
│   ├── components/   # React компоненти
│   ├── pages/        # Сторінки додатку
│   ├── services/     # API сервіси
│   ├── App.js        # Головний компонент
│   ├── App.css       # Стилі додатку
│   ├── index.js      # Entry point
│   └── index.css     # Глобальні стилі
├── .env              # Конфігурація середовища
├── .gitignore        # Git ignore файл
└── package.json      # Залежності проекту
```

## Документація сайту та ToDo

- Повна документація по поточному стану сайту і roadmap імплементації: `SITE_DOCUMENTATION.md`

## Встановлення

1. Встановіть залежності:
```bash
npm install
```

2. Створіть `.env` на основі `.env.example` і налаштуйте змінні:
```
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_SIGNALR_HUB_URL=ws://localhost:5000/hubs/chat
REACT_APP_GOOGLE_CLIENT_ID=your_google_client_id_here
REACT_APP_USE_MOCK_AUTH=true
REACT_APP_ENABLE_TOKEN_REFRESH=false
REACT_APP_TOKEN_REFRESH_ENDPOINT=/auth/refresh-token
```

Пояснення змінних:
- `REACT_APP_API_URL` - базовий URL REST API.
- `REACT_APP_SIGNALR_HUB_URL` - URL SignalR хабу для майбутнього чату в реальному часі.
- `REACT_APP_GOOGLE_CLIENT_ID` - публічний Google OAuth Client ID (без client secret).
- `REACT_APP_USE_MOCK_AUTH` - якщо `true`, кнопка логіну використовує тестовий вхід; якщо `false`, вмикається реальний Google OAuth потік.
- `REACT_APP_ENABLE_TOKEN_REFRESH` - вмикає автооновлення JWT по refresh token при відповіді 401.
- `REACT_APP_TOKEN_REFRESH_ENDPOINT` - endpoint оновлення токена, за замовчуванням `/auth/refresh-token`.

## Розробка

Запустіть розробницький сервер:
```bash
npm start
```

Додаток буде доступний за адресою: `http://localhost:3000`

## Вибудованість

Для вибудованості для продакшену:
```bash
npm build
```

Це створить оптимізовану версію вашого додатку в папці `build/`.

## Основні залежності

- **React** ^19.2.4 - Головна бібліотека для UI
- **react-router-dom** ^7.13.1 - Маршрутизація
- **axios** ^1.13.6 - HTTP клієнт для API запитів
- **react-scripts** ^5.0.1 - Build скрипти

## Конфігурація API

API клієнт налаштований в `src/services/api.js` і підтримує:
- Автоматичне додавання токена авторизації
- Обробку помилок (401 редирект на логін)
- Автоматичний refresh access token (коли увімкнено env-прапор)
- Налаштовування базового URL через env контейнер (`src/config/env.js`)

Google login інтеграція:
- UI логіну: `src/pages/LoginPage.js`
- Backend exchange: `src/services/authApi.js` -> `POST /auth/google-login`
- Валідація env на старті: `src/config/validateEnv.js`

## Переносимість на мобільні пристрої

Проект налаштований для мобільної версії за допомогою:
- Meta viewport тегу в HTML
- Адаптивного CSS з media queries
- Mobile-first design підходу

## Розробка

- Додавайте нові компоненти в папку `src/components/`
- Додавайте нові сторінки в папку `src/pages/`
- Додавайте нові API вызови в `src/services/api.js`

## Комунікація з Backend

При комунікації з .NET API переконайтеся що:
1. CORS налаштований на backend для дозволу запитів з вашої веб-адреси
2. Authentication токен передається в заголовку `Authorization`
3. API URL встановлений правильно в `.env` файлі

## Ліцензія

MIT
