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

  Future<void> sendMessage(Map<String, dynamic> message) async {
      final headers = await _getHeaders();
    final response = await http.post(
      Uri.parse('$baseUrl/Message/Send'),
      headers: headers,
      body: json.encode(message),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to send message');
    }
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