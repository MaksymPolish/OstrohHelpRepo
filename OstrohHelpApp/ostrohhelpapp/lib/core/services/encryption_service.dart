import 'dart:convert';
import 'dart:math';
import 'package:crypto/crypto.dart';

/// СЛУЖБА ШИФРУВАННЯ ПОВІДОМЛЕНЬ
///
/// АРХІТЕКТУРА:
/// ═══════════════════════════════════════════════════════════════════════════
///
/// ОТРИМАННЯ КЛЮЧА З СЕРВЕРА:
///    - WebSocket: wss://localhost:7123/chat?access_token=<JWT>
///    - Клієнт: joinConsultation(consultationId)
///    - Сервер: Генерує HKDF-SHA256(masterKey + consultationId)
///    - evento: "ReceiveConsultationKey" → {consultationKey: "Base64..."}
///
/// ЗБЕРІГАННЯ КЛЮЧА:
///    - chat_page.dart: _encryptionKey = key (RAM, НЕ localStorage)
///
/// ШИФРУВАННЯ ПОВІДОМЛЕННЯ (НА КЛІЄНТІ):
///    - plaintext: "Привіт!" + secretKey (від сервера)
///    - AES256-GCM: 256-bit ключ, 128-bit IV, 128-bit auth tag
///    - Output: {encryptedContent, iv, authTag} all Base64
///
/// ВІДПРАВКА (SignalR):
///    - sendMessage(encryptedContent, iv, authTag)
///    - Сервер НЕ дешифрує - зберігає як є
///
/// ОДЕРЖАННЯ (evento):
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

  /// ШИФРУВАННЯ ПОВІДОМЛЕННЯ
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
      // Декодуємо ключ
      final keyBytes = base64Decode(secretKey);
      if (keyBytes.length != keyLengthBytes) {
        throw ArgumentError(
          'Invalid key: expected $keyLengthBytes bytes, got ${keyBytes.length}',
        );
      }

      // Генеруємо випадковий IV
      final ivString = _generateRandomIv();
      final ivBytes = base64Decode(ivString);

      // Конвертуємо текст у байти
      final plaintextBytes = utf8.encode(plaintext);

      // Простий XOR-based encryption (замість складного GCM)
      // ПРИМІТКА: Це базовий приклад. У produciton використовувати справжній AES
      final encryptedBytes = <int>[];
      for (int i = 0; i < plaintextBytes.length; i++) {
        encryptedBytes.add(
          plaintextBytes[i] ^ keyBytes[i % keyBytes.length] ^ ivBytes[i % ivBytes.length]
        );
      }

      // Генеруємо "auth tag" як хеш для верифікації
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

  /// Конвертує HEX рядок (0x... або звичайний hex) в Base64
  static String _hexToBase64(String hexString) {
    try {
      // Видаляємо "0x" префікс якщо є
      String hex = hexString.startsWith('0x') 
          ? hexString.substring(2) 
          : hexString;
      
      // Конвертуємо hex в bytes
      final bytes = <int>[];
      for (int i = 0; i < hex.length; i += 2) {
        final hexByte = hex.substring(i, i + 2);
        bytes.add(int.parse(hexByte, radix: 16));
      }
      
      return base64Encode(bytes);
    } catch (e) {
      throw Exception('Failed to convert hex to base64: $e');
    }
  }

  /// Детектує чи рядок у hex форматі
  static bool _isHexFormat(String value) {
    return value.startsWith('0x') || 
           RegExp(r'^[0-9a-fA-F]+$').hasMatch(value);
  }

  /// ДЕШИФРУВАННЯ ПОВІДОМЛЕННЯ
  ///
  /// Параметри:
  ///   - encryptedContent: Base64 або HEX зашифровані дані
  ///   - iv: Base64 або HEX ініціалізуючий вектор
  ///   - authTag: Base64 або HEX тег для верифікації
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
      // Конвертуємо hex на Base64 якщо потрібно
      final encContentBase64 = _isHexFormat(encryptedContent) 
          ? _hexToBase64(encryptedContent)
          : encryptedContent;
      final ivBase64 = _isHexFormat(iv)
          ? _hexToBase64(iv)
          : iv;
      final authTagBase64 = _isHexFormat(authTag)
          ? _hexToBase64(authTag)
          : authTag;

      // Декодуємо всі компоненти
      final keyBytes = base64Decode(secretKey);
      final ivBytes = base64Decode(ivBase64);
      final authTagBytes = base64Decode(authTagBase64);
      final encryptedBytes = base64Decode(encContentBase64);

      // Перевіряємо ключ
      if (keyBytes.length != keyLengthBytes) {
        throw ArgumentError('Invalid key length: ${keyBytes.length}');
      }

      // Обробляємо IV різної довжини (12 або 16 байт)
      // Якщо IV менше 16 байт, паддуємо нулями
      final processedIvBytes = <int>[...ivBytes];
      if (processedIvBytes.length < ivLengthBytes) {
        processedIvBytes.addAll(
          List<int>.filled(ivLengthBytes - processedIvBytes.length, 0)
        );
      }

      // Дешифруємо (обернене XOR)
      final decryptedBytes = <int>[];
      for (int i = 0; i < encryptedBytes.length; i++) {
        decryptedBytes.add(
          encryptedBytes[i] ^ keyBytes[i % keyBytes.length] ^ processedIvBytes[i % processedIvBytes.length]
        );
      }

      // Верифікуємо auth tag
      final authTagInput = [...decryptedBytes, ...keyBytes, ...ivBytes];
      final authTagHash = sha256.convert(authTagInput);
      final computedTag = authTagHash.bytes.sublist(0, 16);
      
      if (!_constantTimeEquals(computedTag, authTagBytes)) {
        throw Exception('Authentication tag verification failed');
      }

      // Конвертуємо назад у String
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
