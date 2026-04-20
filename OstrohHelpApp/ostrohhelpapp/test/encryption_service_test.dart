import 'package:flutter_test/flutter_test.dart';
import 'package:ostrohhelpapp/core/services/encryption_service.dart';

void main() {
  group('EncryptionService', () {
    /// Тести для генерування ключів
    group('Key Generation', () {
      test('generateRandomKey creates a valid 256-bit key', () {
        final key = EncryptionService.generateRandomKey();
        
        expect(key, isNotEmpty);
        expect(key.length, greaterThan(0));
        expect(EncryptionService.isValidKey(key), isTrue);
      });

      test('generateRandomKey creates different keys each time', () {
        final key1 = EncryptionService.generateRandomKey();
        final key2 = EncryptionService.generateRandomKey();
        
        expect(key1, isNot(equals(key2)));
      });

      test('isValidKey returns true for valid keys', () {
        final key = EncryptionService.generateRandomKey();
        expect(EncryptionService.isValidKey(key), isTrue);
      });

      test('isValidKey returns false for invalid keys', () {
        expect(EncryptionService.isValidKey('invalid'), isFalse);
        expect(EncryptionService.isValidKey(''), isFalse);
        expect(EncryptionService.isValidKey('shortkey'), isFalse);
      });
    });

    /// Тести для шифрування та дешифрування
    group('Encryption and Decryption', () {
      late String secretKey;

      setUp(() {
        secretKey = EncryptionService.generateRandomKey();
      });

      test('encrypt produces valid encrypted data with all required fields', () {
        const plaintext = 'Hello, World!';
        
        final result = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        expect(result, containsPair('encryptedContent', isNotNull));
        expect(result, containsPair('iv', isNotNull));
        expect(result, containsPair('authTag', isNotNull));
        
        expect(result['encryptedContent'], isNotEmpty);
        expect(result['iv'], isNotEmpty);
        expect(result['authTag'], isNotEmpty);
      });

      test('encrypt produces different ciphertexts for same plaintext', () {
        const plaintext = 'Same text';
        
        final result1 = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );
        
        final result2 = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        // Should have different encrypted content and IVs
        expect(result1['encryptedContent'], isNot(equals(result2['encryptedContent'])));
        expect(result1['iv'], isNot(equals(result2['iv'])));
      });

      test('decrypt can recover original plaintext', () {
        const originalText = 'This is a secret message!';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: originalText,
          secretKey: secretKey,
        );

        final decrypted = EncryptionService.decryptMessage(
          encryptedContent: encrypted['encryptedContent']!,
          iv: encrypted['iv']!,
          authTag: encrypted['authTag']!,
          secretKey: secretKey,
        );

        expect(decrypted, equals(originalText));
      });

      test('decrypt fails with wrong secret key', () {
        const plaintext = 'Secret data';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        final wrongKey = EncryptionService.generateRandomKey();

        expect(
          () => EncryptionService.decryptMessage(
            encryptedContent: encrypted['encryptedContent']!,
            iv: encrypted['iv']!,
            authTag: encrypted['authTag']!,
            secretKey: wrongKey,
          ),
          throwsException,
        );
      });

      test('decrypt fails with modified encrypted content', () {
        const plaintext = 'Tampered data';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        // Modify the first character
        final modifiedContent = encrypted['encryptedContent']!.replaceFirst(
          encrypted['encryptedContent']![0],
          encrypted['encryptedContent']![0] == 'A' ? 'B' : 'A',
        );

        expect(
          () => EncryptionService.decryptMessage(
            encryptedContent: modifiedContent,
            iv: encrypted['iv']!,
            authTag: encrypted['authTag']!,
            secretKey: secretKey,
          ),
          throwsException,
        );
      });

      test('decrypt fails with modified auth tag', () {
        const plaintext = 'Authenticated data';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        // Modify the first character of auth tag
        final modifiedTag = encrypted['authTag']!.replaceFirst(
          encrypted['authTag']![0],
          encrypted['authTag']![0] == 'A' ? 'B' : 'A',
        );

        expect(
          () => EncryptionService.decryptMessage(
            encryptedContent: encrypted['encryptedContent']!,
            iv: encrypted['iv']!,
            authTag: modifiedTag,
            secretKey: secretKey,
          ),
          throwsException,
        );
      });
    });

    /// Тести для різних типів даних
    group('Data Type Handling', () {
      late String secretKey;

      setUp(() {
        secretKey = EncryptionService.generateRandomKey();
      });

      test('encrypt handles empty string', () {
        const plaintext = '';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        final decrypted = EncryptionService.decryptMessage(
          encryptedContent: encrypted['encryptedContent']!,
          iv: encrypted['iv']!,
          authTag: encrypted['authTag']!,
          secretKey: secretKey,
        );

        expect(decrypted, equals(''));
      });

      test('encrypt handles very long text', () {
        const plaintext = '''
          Lorem ipsum dolor sit amet, consectetur adipiscing elit. 
          Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
          Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris 
          nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in 
          reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
        ''';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        final decrypted = EncryptionService.decryptMessage(
          encryptedContent: encrypted['encryptedContent']!,
          iv: encrypted['iv']!,
          authTag: encrypted['authTag']!,
          secretKey: secretKey,
        );

        expect(decrypted, equals(plaintext));
      });

      test('encrypt handles special characters and emojis', () {
        const plaintext = 'Hello! 👋 Special chars: @#\$%^&*()_+-=[]{}|;:,.<>?';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        final decrypted = EncryptionService.decryptMessage(
          encryptedContent: encrypted['encryptedContent']!,
          iv: encrypted['iv']!,
          authTag: encrypted['authTag']!,
          secretKey: secretKey,
        );

        expect(decrypted, equals(plaintext));
      });

      test('encrypt handles Ukrainian text', () {
        const plaintext = 'Привіт, це українский текст! Нам потрібна приватність! 🔐';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        final decrypted = EncryptionService.decryptMessage(
          encryptedContent: encrypted['encryptedContent']!,
          iv: encrypted['iv']!,
          authTag: encrypted['authTag']!,
          secretKey: secretKey,
        );

        expect(decrypted, equals(plaintext));
      });

      test('encrypt handles newlines and special whitespace', () {
        const plaintext = 'Line 1\nLine 2\r\nLine 3\tTabbed';
        
        final encrypted = EncryptionService.encryptMessage(
          plaintext: plaintext,
          secretKey: secretKey,
        );

        final decrypted = EncryptionService.decryptMessage(
          encryptedContent: encrypted['encryptedContent']!,
          iv: encrypted['iv']!,
          authTag: encrypted['authTag']!,
          secretKey: secretKey,
        );

        expect(decrypted, equals(plaintext));
      });
    });

    /// Тести для хешування ключів
    group('Key Hashing', () {
      test('hashKey produces consistent hash for same key', () {
        final key = EncryptionService.generateRandomKey();
        
        final hash1 = EncryptionService.hashKey(key);
        final hash2 = EncryptionService.hashKey(key);

        expect(hash1, equals(hash2));
      });

      test('hashKey produces different hashes for different keys', () {
        final key1 = EncryptionService.generateRandomKey();
        final key2 = EncryptionService.generateRandomKey();
        
        final hash1 = EncryptionService.hashKey(key1);
        final hash2 = EncryptionService.hashKey(key2);

        expect(hash1, isNot(equals(hash2)));
      });
    });

    /// Інтеграційні тести
    group('Integration Tests', () {
      test('Full message encryption and decryption cycle', () {
        // Симулюємо повний цикл надсилання повідомлення
        final senderKey = EncryptionService.generateRandomKey();
        const message = 'This is my secret message for the consultation!';

        // Надсилання: шифруємо
        final encrypted = EncryptionService.encryptMessage(
          plaintext: message,
          secretKey: senderKey,
        );

        // Має бути три компоненти для відправки на сервер
        expect(encrypted.keys.length, equals(3));
        expect(
          encrypted.keys.toList(),
          containsAll(['encryptedContent', 'iv', 'authTag']),
        );

        // Отримання: дешифруємо
        final decrypted = EncryptionService.decryptMessage(
          encryptedContent: encrypted['encryptedContent']!,
          iv: encrypted['iv']!,
          authTag: encrypted['authTag']!,
          secretKey: senderKey,
        );

        expect(decrypted, equals(message));
      });

      test('Multiple messages with same key produce different ciphertexts', () {
        final key = EncryptionService.generateRandomKey();
        
        final messages = [
          'First message',
          'Second message',
          'Third message',
        ];

        final encrypted = messages
            .map((msg) => EncryptionService.encryptMessage(
              plaintext: msg,
              secretKey: key,
            ))
            .toList();

        // All encrypted contents should be different
        final contents = encrypted.map((e) => e['encryptedContent']).toList();
        expect(contents[0], isNot(equals(contents[1])));
        expect(contents[1], isNot(equals(contents[2])));
        expect(contents[0], isNot(equals(contents[2])));
      });

      test('Decryption preserves message order in conversation', () {
        final key = EncryptionService.generateRandomKey();
        
        final messages = ['Hi', 'How are you?', 'I am fine!', 'Great!'];

        // Encrypt all messages
        final encryptedMessages = messages
            .map((msg) => EncryptionService.encryptMessage(
              plaintext: msg,
              secretKey: key,
            ))
            .toList();

        // Decrypt all messages
        final decryptedMessages = encryptedMessages
            .map((enc) => EncryptionService.decryptMessage(
              encryptedContent: enc['encryptedContent']!,
              iv: enc['iv']!,
              authTag: enc['authTag']!,
              secretKey: key,
            ))
            .toList();

        // Should match original order
        expect(decryptedMessages, equals(messages));
      });
    });
  });
}


