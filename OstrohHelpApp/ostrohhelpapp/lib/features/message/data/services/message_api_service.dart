import 'dart:convert';
import 'package:http/http.dart' as http;

class MessageApiService {
  final String baseUrl = 'http://10.0.2.2:5132/api';

  Future<List<Map<String, dynamic>>> getMessages(String consultationId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/Message/Recive?idConsultation=$consultationId'),
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
    final response = await http.post(
      Uri.parse('$baseUrl/Message/Send'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode(message),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to send message');
    }
  }

  Future<void> deleteMessage(String messageId) async {
    final response = await http.delete(
      Uri.parse('$baseUrl/Message/Delete'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'messageId': messageId}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to delete message');
    }
  }

  Future<void> markAsRead(String messageId) async {
    final response = await http.put(
      Uri.parse('$baseUrl/Message/mark-as-read'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'id': messageId}),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to mark message as read');
    }
  }
} 