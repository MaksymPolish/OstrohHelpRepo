import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../core/auth/token_storage.dart';

class MessageApiService {
  final String baseUrl = 'http://10.0.2.2:5000/api';
  final TokenStorage _tokenStorage = TokenStorage();

  Future<Map<String, String>> _getHeaders() async {
    final token = await _tokenStorage.getToken();
    final headers = <String, String>{
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };
    if (token != null) {
      headers['Authorization'] = 'Bearer $token';
    }
    return headers;
  }

  Future<List<Map<String, dynamic>>> getMessages(String consultationId) async {
    final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/Message/Recive?idConsultation=$consultationId'),
      headers: headers,
    );
    if (response.statusCode == 200) {
      final data = json.decode(response.body);
      if (data is List) {
        return data.cast<Map<String, dynamic>>();
      } else if (data is Map<String, dynamic>) {
        return [data];
      }
      return [];
    }
    throw Exception('Failed to load messages');
  }

  Future<Map<String, dynamic>> sendMessage({
    required String consultationId,
    required String receiverId,
    required String encryptedContent,
    required String iv,
    required String authTag,
  }) async {
    final headers = await _getHeaders();
    
    final body = {
      'consultationId': consultationId,
      'receiverId': receiverId,
      'encryptedContent': encryptedContent,
      'iv': iv,
      'authTag': authTag,
    };

    final response = await http.post(
      Uri.parse('$baseUrl/Message/Send'),
      headers: headers,
      body: json.encode(body),
    );

    // Обробка rate limiting (429)
    if (response.statusCode == 429) {
      final retryAfter = response.headers['retry-after'] ?? '60';
      throw Exception(
        'Rate limit exceeded. Please try again in $retryAfter seconds.',
      );
    }

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(
        'Failed to send message (status: ${response.statusCode}): ${response.body}',
      );
    }

    if (response.body.isEmpty) {
      throw Exception('Send message returned empty body');
    }

    final data = json.decode(response.body);
    if (data is Map<String, dynamic>) {
      return data;
    }
    throw Exception('Unexpected send message response');
  }

  /// Завантажує кілька файлів відразу до консультації (BatchUpload)
  /// 
  /// Параметри:
  ///   - [filePaths]: Список шляхів до файлів для завантаження
  ///   - [messageId]: Опціонально ID повідомлення для прив'язки файлів
  ///
  /// Returns: Map з результатами завантаження включая URLs, thumbnails, posterів
  Future<Map<String, dynamic>> batchUpload({
    required List<String> filePaths,
    String? messageId,
  }) async {
    if (filePaths.isEmpty) {
      throw Exception('No files to upload');
    }

    final token = await _tokenStorage.getToken();
    
    var uri = Uri.parse('$baseUrl/Message/BatchUpload');
    if (messageId != null) {
      uri = uri.replace(queryParameters: {'messageId': messageId});
    }
    
    final request = http.MultipartRequest('POST', uri);
    if (token != null) {
      request.headers['Authorization'] = 'Bearer $token';
    }

    // Додаємо файли до запиту
    for (var filePath in filePaths) {
      request.files.add(
        await http.MultipartFile.fromPath('file', filePath),
      );
    }

    final response = await request.send();
    final responseBody = await response.stream.bytesToString();

    // Обробка rate limiting (429)
    if (response.statusCode == 429) {
      throw Exception('Rate limit exceeded. Please try again later.');
    }

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(
        'Failed to batch upload files (status: ${response.statusCode}): $responseBody',
      );
    }

    try {
      final data = json.decode(responseBody);
      if (data is Map<String, dynamic>) {
        return data;
      }
      throw Exception('Unexpected batch upload response format');
    } catch (e) {
      throw Exception('Error parsing batch upload response: $e');
    }
  }

  Future<Map<String, dynamic>> uploadToCloud({
    required String userId,
    required String filePath,
  }) async {
    final token = await _tokenStorage.getToken();
    final uri = Uri.parse('$baseUrl/Message/UploadToCloud/$userId');
    final request = http.MultipartRequest('POST', uri);
    if (token != null) {
      request.headers['Authorization'] = 'Bearer $token';
    }
    request.files.add(await http.MultipartFile.fromPath('file', filePath));

    final response = await request.send();
    final responseBody = await response.stream.bytesToString();

    // Обробка rate limiting (429)
    if (response.statusCode == 429) {
      throw Exception('Rate limit exceeded. Please try again later.');
    }

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(
        'Failed to upload file (status: ${response.statusCode}): $responseBody',
      );
    }
    final data = json.decode(responseBody);
    if (data is Map<String, dynamic>) {
      return data;
    }
    throw Exception('Unexpected upload response');
  }

  Future<Map<String, dynamic>> addAttachment({
    required String messageId,
    required String fileUrl,
    required String fileType,
  }) async {
    final headers = await _getHeaders();
    final response = await http.post(
      Uri.parse('$baseUrl/Message/AddAttachment'),
      headers: headers,
      body: json.encode({
        'messageId': messageId,
        'fileUrl': fileUrl,
        'fileType': fileType,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(
        'Failed to add attachment (status: ${response.statusCode}): ${response.body}',
      );
    }
    if (response.body.isEmpty) {
      throw Exception('Add attachment returned empty body');
    }
    final data = json.decode(response.body);
    if (data is Map<String, dynamic>) {
      return data;
    }
    throw Exception('Unexpected add attachment response');
  }

  Future<void> deleteMessage(String messageId) async {
    final headers = await _getHeaders();
    
    final response = await http.delete(
      Uri.parse('$baseUrl/Message/Delete'),
      headers: headers,
      body: json.encode({'messageId': messageId}),
    );

    // Обробка rate limiting (429)
    if (response.statusCode == 429) {
      throw Exception('Rate limit exceeded. Please try again later.');
    }

    if (response.statusCode != 204 && response.statusCode != 200) {
      throw Exception(
        'Failed to delete message (status: ${response.statusCode}): ${response.body}',
      );
    }
  }

  Future<void> markAsRead(String messageId) async {
    final headers = await _getHeaders();
    
    final response = await http.put(
      Uri.parse('$baseUrl/Message/mark-as-read'),
      headers: headers,
      body: json.encode({'messageId': messageId}),
    );

    // Обробка rate limiting (429)
    if (response.statusCode == 429) {
      throw Exception('Rate limit exceeded. Please try again later.');
    }

    if (response.statusCode != 204 && response.statusCode != 200) {
      throw Exception(
        'Failed to mark message as read (status: ${response.statusCode})',
      );
    }
  }
} 