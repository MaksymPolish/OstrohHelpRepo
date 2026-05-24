import 'dart:convert';
import 'dart:math';
import 'package:crypto/crypto.dart';

class EncryptionService {
  static const int keyLengthBytes = 32;

  static const int ivLengthBytes = 12;

  static String generateRandomKey() {
    final random = Random.secure();
    final keyBytes = List<int>.generate(keyLengthBytes, (i) => random.nextInt(256));
    return base64Encode(keyBytes);
  }

  static String _generateRandomIv() {
    final random = Random.secure();
    final ivBytes = List<int>.generate(ivLengthBytes, (i) => random.nextInt(256));
    return base64Encode(ivBytes);
  }

  static Map<String, String> encryptMessage({
    required String plaintext,
    required String secretKey,
  }) {
    try {
      final keyBytes = base64Decode(secretKey);
      if (keyBytes.length != keyLengthBytes) {
        throw ArgumentError(
          'Invalid key: expected $keyLengthBytes bytes, got ${keyBytes.length}',
        );
      }

      final ivString = _generateRandomIv();
      final ivBytes = base64Decode(ivString);

      final plaintextBytes = utf8.encode(plaintext);

      final encryptedBytes = <int>[];
      for (int i = 0; i < plaintextBytes.length; i++) {
        encryptedBytes.add(
          plaintextBytes[i] ^ keyBytes[i % keyBytes.length] ^ ivBytes[i % ivBytes.length]
        );
      }

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

  static String _hexToBase64(String hexString) {
    try {
      String hex = hexString.startsWith('0x') 
          ? hexString.substring(2) 
          : hexString;
      
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

  static bool _isHexFormat(String value) {
    return value.startsWith('0x') || 
           RegExp(r'^[0-9a-fA-F]+$').hasMatch(value);
  }

  static String decryptMessage({
    required String encryptedContent,
    required String iv,
    required String authTag,
    required String secretKey,
  }) {
    try {
      final encContentBase64 = _isHexFormat(encryptedContent) 
          ? _hexToBase64(encryptedContent)
          : encryptedContent;
      final ivBase64 = _isHexFormat(iv)
          ? _hexToBase64(iv)
          : iv;
      final authTagBase64 = _isHexFormat(authTag)
          ? _hexToBase64(authTag)
          : authTag;

      final keyBytes = base64Decode(secretKey);
      final ivBytes = base64Decode(ivBase64);
      final authTagBytes = base64Decode(authTagBase64);
      final encryptedBytes = base64Decode(encContentBase64);

      if (keyBytes.length != keyLengthBytes) {
        throw ArgumentError('Invalid key length: ${keyBytes.length}');
      }

      if (ivBytes.isEmpty) {
        throw ArgumentError('Invalid IV: empty');
      }

      final decryptedBytes = <int>[];
      for (int i = 0; i < encryptedBytes.length; i++) {
        decryptedBytes.add(
          encryptedBytes[i] ^ keyBytes[i % keyBytes.length] ^ ivBytes[i % ivBytes.length]
        );
      }

      final authTagInput = [...decryptedBytes, ...keyBytes, ...ivBytes];
      final authTagHash = sha256.convert(authTagInput);
      final computedTag = authTagHash.bytes.sublist(0, 16);
      
      if (!_constantTimeEquals(computedTag, authTagBytes)) {
        throw Exception('Authentication tag verification failed');
      }

      return utf8.decode(decryptedBytes);
    } catch (e) {
      throw Exception('Decryption failed: $e');
    }
  }

  static bool _constantTimeEquals(List<int> a, List<int> b) {
    if (a.length != b.length) return false;
    int result = 0;
    for (int i = 0; i < a.length; i++) {
      result |= a[i] ^ b[i];
    }
    return result == 0;
  }

  static String hashKey(String secretKey) {
    return sha256.convert(utf8.encode(secretKey)).toString();
  }

  static bool isValidKey(String secretKey) {
    try {
      final keyBytes = base64Decode(secretKey);
      return keyBytes.length == keyLengthBytes;
    } catch (e) {
      return false;
    }
  }
}

