import 'dart:convert';
import 'package:http/http.dart' as http;

class QuestionnaireApiService {
  final String baseUrl = 'http://10.0.2.2:5132/api';

  Future<List<Map<String, dynamic>>> getAllQuestionnaires() async {
    final response = await http.get(Uri.parse('$baseUrl/questionnaire/all'));
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      return data.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to load questionnaires');
  }

  Future<Map<String, dynamic>> getQuestionnaireById(String id) async {
    final response = await http.get(Uri.parse('$baseUrl/questionnaire/$id'));
    if (response.statusCode == 200) {
      return json.decode(response.body);
    }
    throw Exception('Failed to load questionnaire');
  }

  Future<List<Map<String, dynamic>>> getQuestionnairesByUserId(String userId) async {
    final response = await http.get(Uri.parse('$baseUrl/questionnaire/Get-All-Questionnaire-By-UserId/$userId'));
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      print('Questionnaires for user $userId: $data');
      return data.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to load user questionnaires');
  }

  Future<Map<String, dynamic>> createQuestionnaire(Map<String, dynamic> questionnaire) async {
    final response = await http.post(
      Uri.parse('$baseUrl/questionnaire/Create-Questionnaire'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode(questionnaire),
    );
    if (response.statusCode == 201) {
      return json.decode(response.body);
    }
    throw Exception('Failed to create questionnaire');
  }

  Future<void> updateQuestionnaire(String id, Map<String, dynamic> questionnaire) async {
    final response = await http.put(
      Uri.parse('$baseUrl/questionnaire/Update-Questionnaire'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode(questionnaire),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to update questionnaire');
    }
  }

  Future<void> updateQuestionnaireStatus(String id, String status) async {
    final response = await http.put(
      Uri.parse('$baseUrl/questionnaire/Update-StatusQuestionnaire'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'id': id, 'status': status}),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to update questionnaire status');
    }
  }

  Future<void> deleteQuestionnaire(String id) async {
    final response = await http.delete(
      Uri.parse('$baseUrl/questionnaire/Delete-Questionnaire'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode({'id': id}),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to delete questionnaire');
    }
  }
} 