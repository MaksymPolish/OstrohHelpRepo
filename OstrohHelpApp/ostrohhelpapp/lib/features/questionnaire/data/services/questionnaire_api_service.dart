import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../core/auth/token_storage.dart';

class QuestionnaireApiService {
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

  Future<List<Map<String, dynamic>>> getAllQuestionnaires() async {
    final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/questionnaire/all'),
      headers: headers,
    );
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      return data.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to load questionnaires: ${response.statusCode} - ${response.body}');
  }

  Future<Map<String, dynamic>> getQuestionnaireById(String id) async {
    final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/questionnaire/$id'),
      headers: headers,
    );
    if (response.statusCode == 200) {
      return json.decode(response.body);
    }
    throw Exception('Failed to load questionnaire: ${response.statusCode} - ${response.body}');
  }

  Future<List<Map<String, dynamic>>> getQuestionnairesByUserId(String userId) async {
    final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/questionnaire/Get-All-Questionnaire-By-UserId/$userId'),
      headers: headers,
    );
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      print('Questionnaires for user $userId: $data');
      return data.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to load user questionnaires: ${response.statusCode} - ${response.body}');
  }

  Future<Map<String, dynamic>> createQuestionnaire(Map<String, dynamic> questionnaire) async {
    final headers = await _getHeaders();
    final response = await http.post(
      Uri.parse('$baseUrl/questionnaire/Create-Questionnaire'),
      headers: headers,
      body: json.encode(questionnaire),
    );
    if (response.statusCode == 201 || response.statusCode == 200) {
      return json.decode(response.body);
    }
    throw Exception('Failed to create questionnaire: ${response.statusCode} - ${response.body}');
  }

  Future<void> updateQuestionnaire(String id, Map<String, dynamic> questionnaire) async {
    final headers = await _getHeaders();
    final response = await http.put(
      Uri.parse('$baseUrl/questionnaire/Update-Questionnaire'),
      headers: headers,
      body: json.encode(questionnaire),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to update questionnaire: ${response.statusCode} - ${response.body}');
    }
  }

  Future<void> updateQuestionnaireStatus(String id, String statusId) async {
    final headers = await _getHeaders();
    final response = await http.put(
      Uri.parse('$baseUrl/questionnaire/Update-StatusQuestionnaire'),
      headers: headers,
      body: json.encode({'questionnaireId': id, 'statusId': statusId}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to update questionnaire status: ${response.statusCode} - ${response.body}');
    }
  }

  Future<void> deleteQuestionnaire(String id) async {
    final headers = await _getHeaders();
    final response = await http.delete(
      Uri.parse('$baseUrl/questionnaire/Delete-Questionnaire'),
      headers: headers,
      body: json.encode(id),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to delete questionnaire: ${response.statusCode} - ${response.body}');
    }
  }
} 