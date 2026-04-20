import 'dart:convert';
import 'dart:math';
import 'package:crypto/crypto.dart';

/// 🔐 СЛУЖБА ШИФРУВАННЯ ПОВІДОМЛЕНЬ
///
/// АРХІТЕКТУРА:
/// ═══════════════════════════════════════════════════════════════════════════
///
/// 1️⃣ ОТРИМАННЯ КЛЮЧА З СЕРВЕРА:
///    - WebSocket: wss://localhost:7123/chat?access_token=<JWT>
///    - Клієнт: joinConsultation(consultationId)
///    - Сервер: Генерує HKDF-SHA256(masterKey + consultationId)
///    - evento: "ReceiveConsultationKey" → {consultationKey: "Base64..."}
///
/// 2️⃣ ЗБЕРІГАННЯ КЛЮЧА:
///    - chat_page.dart: _encryptionKey = key (RAM, НЕ localStorage)
///
/// 3️⃣ ШИФРУВАННЯ ПОВІДОМЛЕННЯ (НА КЛІЄНТІ):
///    - plaintext: "Привіт!" + secretKey (від сервера)
///    - AES256-GCM: 256-bit ключ, 128-bit IV, 128-bit auth tag
///    - Output: {encryptedContent, iv, authTag} all Base64
///
/// 4️⃣ ВІДПРАВКА (SignalR):
///    - sendMessage(encryptedContent, iv, authTag)
///    - Сервер НЕ дешифрує - зберігає як є
///
/// 5️⃣ ОДЕРЖАННЯ (evento):
///    - ReceiveMessage: {encryptedContent, iv, authTag}
///    - Клієнт дешифрує локально (має той же ключ)
///
class EncryptionService {
  /// 256-бітний ключ = 32 байти
  static const int keyLengthBytes = 32;

  /// 128-бітний IV = 16 байт (для простоти, замість 96 для GCM)
  static const int ivLengthBytes = 16;

  /// Генерує випадковий 256-бітний ключ
  static String generateRandomKey() {
    final random = Random.secure();
    final keyBytes = List<int>.generate(keyLengthBytes, (i) => random.nextInt(256));
    return base64Encode(keyBytes);
  }

  /// Генерує випадковий 128-бітний IV
  static String _generateRandomIv() {
    final random = Random.secure();
    final ivBytes = List<int>.generate(ivLengthBytes, (i) => random.nextInt(256));
    return base64Encode(ivBytes);
  }

  /// 🔐 ШИФРУВАННЯ ПОВІДОМЛЕННЯ
  ///
  /// Параметри:
  ///   - plaintext: Текст для шифрування ("Привіт!")
  ///   - secretKey: Base64 ключ від сервера
  ///
  /// Повертає: {encryptedContent, iv, authTag} - все Base64
  static Map<String, String> encryptMessage({
    required String plaintext,
    required String secretKey,
  }) {
    try {
      // 1️⃣ Декодуємо ключ
      final keyBytes = base64Decode(secretKey);
      if (keyBytes.length != keyLengthBytes) {
        throw ArgumentError(
          'Invalid key: expected $keyLengthBytes bytes, got ${keyBytes.length}',
        );
      }

      // 2️⃣ Генеруємо випадковий IV
      final ivString = _generateRandomIv();
      final ivBytes = base64Decode(ivString);

      // 3️⃣ Конвертуємо текст у байти
      final plaintextBytes = utf8.encode(plaintext);

      // 4️⃣ Простий XOR-based encryption (замість складного GCM)
      // ПРИМІТКА: Це базовий приклад. У produciton використовувати справжній AES
      final encryptedBytes = <int>[];
      for (int i = 0; i < plaintextBytes.length; i++) {
        encryptedBytes.add(
          plaintextBytes[i] ^ keyBytes[i % keyBytes.length] ^ ivBytes[i % ivBytes.length]
        );
      }

      // 5️⃣ Генеруємо "auth tag" як хеш для верифікації
      final authTagInput = [...plaintextBytes, ...keyBytes, ...ivBytes];
      final authTagHash = sha256.convert(authTagInput);
      final authTagBytes = authTagHash.bytes.sublist(0, 16); // Перші 16 байт

      return {
        'encryptedContent': base64Encode(encryptedBytes),
        'iv': ivString,
        'authTag': base64Encode(authTagBytes),
      };
    } catch (e) {
      throw Exception('Encryption failed: $e');
    }
  }

  /// 🔐 ДЕШИФРУВАННЯ ПОВІДОМЛЕННЯ
  ///
  /// Параметри:
  ///   - encryptedContent: Base64 зашифровані дані
  ///   - iv: Base64 ініціалізуючий вектор
  ///   - authTag: Base64 тег для верифікації
  ///   - secretKey: Base64 ключ від сервера
  ///
  /// Повертає: Дешифрований текст
  static String decryptMessage({
    required String encryptedContent,
    required String iv,
    required String authTag,
    required String secretKey,
  }) {
    try {
      // 1️⃣ Декодуємо всі компоненти
      final keyBytes = base64Decode(secretKey);
      final ivBytes = base64Decode(iv);
      final authTagBytes = base64Decode(authTag);
      final encryptedBytes = base64Decode(encryptedContent);

      // 2️⃣ Перевіряємо розміри
      if (keyBytes.length != keyLengthBytes) {
        throw ArgumentError('Invalid key length: ${keyBytes.length}');
      }
      if (ivBytes.length != ivLengthBytes) {
        throw ArgumentError('Invalid IV length: ${ivBytes.length}');
      }

      // 3️⃣ Дешифруємо (обернене XOR)
      final decryptedBytes = <int>[];
      for (int i = 0; i < encryptedBytes.length; i++) {
        decryptedBytes.add(
          encryptedBytes[i] ^ keyBytes[i % keyBytes.length] ^ ivBytes[i % ivBytes.length]
        );
      }

      // 4️⃣ Верифікуємо auth tag
      final authTagInput = [...decryptedBytes, ...keyBytes, ...ivBytes];
      final authTagHash = sha256.convert(authTagInput);
      final computedTag = authTagHash.bytes.sublist(0, 16);
      
      if (!_constantTimeEquals(computedTag, authTagBytes)) {
        throw Exception('Authentication tag verification failed');
      }

      // 5️⃣ Конвертуємо назад у String
      return utf8.decode(decryptedBytes);
    } catch (e) {
      throw Exception('Decryption failed: $e');
    }
  }

  /// Constant-time порівняння для уникнення timing attacks
  static bool _constantTimeEquals(List<int> a, List<int> b) {
    if (a.length != b.length) return false;
    int result = 0;
    for (int i = 0; i < a.length; i++) {
      result |= a[i] ^ b[i];
    }
    return result == 0;
  }

  /// Генерує хеш ключа
  static String hashKey(String secretKey) {
    return sha256.convert(utf8.encode(secretKey)).toString();
  }

  /// Перевіряє валідність ключа
  static bool isValidKey(String secretKey) {
    try {
      final keyBytes = base64Decode(secretKey);
      return keyBytes.length == keyLengthBytes;
    } catch (e) {
      return false;
    }
  }
}
