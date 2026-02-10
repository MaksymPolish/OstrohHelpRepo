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

  Future<Map<String, dynamic>> sendMessage(Map<String, dynamic> message) async {
    final headers = await _getHeaders();
    final response = await http.post(
      Uri.parse('$baseUrl/Message/Send'),
      headers: headers,
      body: json.encode(message),
    );
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
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to delete message');
    }
  }

  Future<void> markAsRead(String messageId) async {
    final headers = await _getHeaders();
    final response = await http.put(
      Uri.parse('$baseUrl/Message/mark-as-read'),
      headers: headers,
      body: json.encode({'id': messageId}),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to mark message as read');
    }
  }
} 