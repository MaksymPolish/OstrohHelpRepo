# 🔐 XOR Encryption Implementation Guide for Web & Cross-Platform

## Простий Опис
Ми шифруємо повідомлення за допомогою **XOR** з додатковою **SHA256 перевіркою**.

## Що Робити (3 кроки)

### 1️⃣ При Відправці Повідомлення (Клієнт)

```
Вхід: plaintext = "hello"
      secretKey = "Base64EncodedKey" (32 bytes)

Кроки:
1. Декодуй secretKey з Base64 → 32 байти
2. Візьми plaintext → 5 байтів
3. Генеруй IV → 16 випадкових байтів
4. Для кожного байту:
   - encryptedByte = plaintext[i] XOR key[i % 32] XOR iv[i % 16]
5. Обчисли authTag = SHA256(plaintext + key + iv)
6. Кодуй все в Base64

Вихід:
{
  "encryptedContent": "jNL1sw==",
  "iv": "...base64...",
  "authTag": "...base64..."
}

Відправ на сервер → сервер зберігає всі 3 значення
```

### 2️⃣ При Отриманні Повідомлення (Клієнт)

```
Вхід: encryptedContent = "jNL1sw=="
      iv = "...base64..."
      authTag = "...base64..."
      secretKey = "Base64EncodedKey" (тот же)

Кроки:
1. Декодуй secretKey з Base64 → 32 байти
2. Декодуй encryptedContent з Base64 → байти
3. Декодуй iv з Base64 → 16 байтів
4. Для кожного байту:
   - plainByte = encryptedByte XOR key[i % 32] XOR iv[i % 16]
5. Перевір: authTag == SHA256(plaintext + key + iv)
   - Якщо не совпадает → помилка, не показуй
   - Якщо совпадает → показуй повідомлення

Вихід: "hello"
```

### 3️⃣ Ключ з Сервера (Один раз при Вході)

```
Сервер надсилає Клієнту:
{
  "consultationId": "123abc",
  "key": "Base64EncodedKey", // 44 символи (32 bytes encoded)
  "algorithm": "AES-256-GCM"
}

Клієнт зберігає key і використовує для всіх повідомлень цієї консультації
```

## Конфігурація для АІ (Prompt)

Коли просиш AI імплементувати:

```
Мені потрібна функція шифрування для веб (JavaScript/TypeScript).

Алгоритм:
- XOR cipher: encryptedByte = plaintext[i] XOR key[i%32] XOR iv[i%16]
- IV: 16 випадкових байтів для кожного повідомлення
- Auth Tag: SHA256(plaintext + key + iv) для перевірки цілісності

Параметри:
- secretKey: Base64-encoded 256-bit key (32 bytes, 44 chars)
- plaintext: текст повідомлення
- iv: 16 випадкових байтів в Base64
- authTag: SHA256 хеш в Base64

Функції:
1. encryptMessage(plaintext, secretKey) → {encryptedContent, iv, authTag}
2. decryptMessage(encryptedContent, iv, authTag, secretKey) → plaintext

Все повинно бути в Base64. На помилку auth tag - викинути exception.
```

## Структура Файлів Для Веб

```
src/
├── services/
│   └── encryption.service.ts (або .js)
│       ├── encryptMessage(plaintext, key)
│       ├── decryptMessage(encrypted, iv, tag, key)
│       └── validateAuthTag(computed, provided)
│
├── components/
│   └── ChatWindow.tsx
│       ├── onSendMessage() → call encryptMessage()
│       └── onReceiveMessage() → call decryptMessage()
│
└── types/
    └── encryption.types.ts
        ├── EncryptedMessage
        ├── EncryptionKey
        └── DecryptResult
```

## Конфігурація Сервера

**C# .NET сервер вже робить:**
```
POST /api/consultation/join
Response:
{
  "consultationId": "uuid",
  "key": "Base64Key", // HKDF-SHA256(masterKey + consultationId)
  "algorithm": "AES-256-GCM"
}
```

**База даних зберігає:**
```
Message {
  id: UUID
  encryptedContent: Base64String (from client)
  iv: Base64String (from client)
  authTag: Base64String (from client)
  sentAt: DateTime
}
```

**SignalR evento:**
```
on('ReceiveMessage', (message) => {
  // message містить encryptedContent, iv, authTag
  // Клієнт самостійно дешифрує
})
```

## Перевірка Безпеки

✅ Кожне повідомлення має унікальний IV  
✅ Auth tag запобігає підробці  
✅ Ключ передається один раз при вході  
✅ Все кодується в Base64 для передачі  
✅ Сервер НЕ дешифрує (end-to-end encryption)  

## Помилки & Обробка

| Помилка | Причина | Рішення |
|---------|---------|---------|
| Auth tag mismatch | Повідомлення було зіпсовано | Не показуй, логируй |
| Key is null | Сервер не надіслав ключ | Чекай evento ReceiveConsultationKey |
| IV length != 16 | Невірний формат IV | Відхили повідомлення |
| encryptedContent empty | Шифрування не вдалось | Логируй помилку, повтори |

## JavaScript Реалізація Довідка

```javascript
const crypto = require('crypto');

// Шифрування
function encryptMessage(plaintext, secretKey) {
  const keyBuffer = Buffer.from(secretKey, 'base64');
  const plainBuffer = Buffer.from(plaintext, 'utf-8');
  
  // Генеруємо IV (16 випадкових байтів)
  const iv = crypto.randomBytes(16);
  const ivBase64 = iv.toString('base64');
  
  // XOR шифрування
  const encrypted = Buffer.alloc(plainBuffer.length);
  for (let i = 0; i < plainBuffer.length; i++) {
    encrypted[i] = plainBuffer[i] ^ 
                   keyBuffer[i % keyBuffer.length] ^ 
                   iv[i % iv.length];
  }
  
  // Auth Tag (SHA256)
  const authData = Buffer.concat([plainBuffer, keyBuffer, iv]);
  const authTag = crypto.createHash('sha256').update(authData).digest();
  
  return {
    encryptedContent: encrypted.toString('base64'),
    iv: ivBase64,
    authTag: authTag.toString('base64'),
  };
}

// Дешифрування
function decryptMessage(encryptedContent, iv, authTag, secretKey) {
  const keyBuffer = Buffer.from(secretKey, 'base64');
  const encryptedBuffer = Buffer.from(encryptedContent, 'base64');
  const ivBuffer = Buffer.from(iv, 'base64');
  
  // XOR дешифрування
  const decrypted = Buffer.alloc(encryptedBuffer.length);
  for (let i = 0; i < encryptedBuffer.length; i++) {
    decrypted[i] = encryptedBuffer[i] ^ 
                   keyBuffer[i % keyBuffer.length] ^ 
                   ivBuffer[i % ivBuffer.length];
  }
  
  // Перевіряємо Auth Tag
  const plaintext = decrypted.toString('utf-8');
  const plainBuffer = Buffer.from(plaintext, 'utf-8');
  const authData = Buffer.concat([plainBuffer, keyBuffer, ivBuffer]);
  const expectedAuthTag = crypto.createHash('sha256').update(authData).digest();
  
  const providedAuthTag = Buffer.from(authTag, 'base64');
  if (!expectedAuthTag.equals(providedAuthTag)) {
    throw new Error('Auth tag verification failed');
  }
  
  return plaintext;
}
```

## Dart Реалізація Довідка

```dart
import 'dart:convert';
import 'dart:math';
import 'package:crypto/crypto.dart';

static Map<String, String> encryptMessage({
  required String plaintext,
  required String secretKey,
}) {
  final keyBytes = base64Decode(secretKey);
  final plainBytes = utf8.encode(plaintext);
  
  // Генеруємо IV
  final random = Random.secure();
  final iv = List<int>.generate(16, (i) => random.nextInt(256));
  final ivBase64 = base64Encode(iv);
  
  // XOR шифрування
  final encrypted = <int>[];
  for (int i = 0; i < plainBytes.length; i++) {
    int byte = plainBytes[i] ^ 
               keyBytes[i % keyBytes.length] ^ 
               iv[i % iv.length];
    encrypted.add(byte);
  }
  
  // Auth Tag (SHA256 хеш)
  final authData = plainBytes + keyBytes + iv;
  final authTag = sha256.convert(authData);
  
  return {
    'encryptedContent': base64Encode(encrypted),
    'iv': ivBase64,
    'authTag': base64Encode(authTag.bytes),
  };
}

static String decryptMessage({
  required String encryptedContent,
  required String iv,
  required String authTag,
  required String secretKey,
}) {
  final keyBytes = base64Decode(secretKey);
  final encryptedBytes = base64Decode(encryptedContent);
  final ivBytes = base64Decode(iv);
  
  // XOR дешифрування
  final decrypted = <int>[];
  for (int i = 0; i < encryptedBytes.length; i++) {
    int byte = encryptedBytes[i] ^ 
               keyBytes[i % keyBytes.length] ^ 
               ivBytes[i % ivBytes.length];
    decrypted.add(byte);
  }
  
  final plaintext = utf8.decode(decrypted);
  
  // Перевірка Auth Tag
  final plainBytes = utf8.encode(plaintext);
  final authData = plainBytes + keyBytes + ivBytes;
  final expectedAuthTag = sha256.convert(authData);
  final providedAuthTag = base64Decode(authTag);
  
  if (!ListEquality().equals(expectedAuthTag.bytes, providedAuthTag)) {
    throw Exception('Auth tag verification failed');
  }
  
  return plaintext;
}
```

---

**Для веб розробника:** Копіюй JavaScript код вище і адаптуй під твій фреймворк (React, Vue, Angular).  
**Для мобільного:** Аналогічна логіка на Kotlin, Swift, Flutter.
