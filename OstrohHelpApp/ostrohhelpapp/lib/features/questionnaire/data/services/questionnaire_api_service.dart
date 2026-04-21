import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../core/auth/token_storage.dart';
import '../../../../core/auth/user_storage.dart';
import '../models/questionnaire_result.dart';
import '../models/questionnaire.dart';
import 'questionnaire_data.dart';

class QuestionnaireApiService {
  final String baseUrl = 'http://10.0.2.2:5000/api';
  final TokenStorage _tokenStorage = TokenStorage();

  Future<Map<String, String>> _getHeaders() async {
    try {
      final token = await _tokenStorage.getToken();
      print('🔐 Token fetched: ${token?.substring(0, 20)}...');
      final headers = <String, String>{
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      };
      if (token != null) {
        headers['Authorization'] = 'Bearer $token';
      } else {
        print('⚠️ WARNING: Token is null!');
      }
      return headers;
    } catch (e) {
      print('❌ ERROR in _getHeaders: $e');
      rethrow;
    }
  }

  Future<List<Map<String, dynamic>>> getAllQuestionnaires() async {
    try {
      print('📤 Starting getAllQuestionnaires request...');
      final headers = await _getHeaders();
      print('✅ Headers prepared, making HTTP request...');
      
      final response = await http.get(
        Uri.parse('$baseUrl/questionnaire/all'),
        headers: headers,
      );
      
      print('📥 Response received: ${response.statusCode}');
      
      if (response.statusCode == 200) {
        final List<dynamic> data = json.decode(response.body);
        print('✅ Questionnaires loaded: ${data.length} items');
        return data.cast<Map<String, dynamic>>();
      }
      throw Exception('Failed to load questionnaires: ${response.statusCode} - ${response.body}');
    } catch (e) {
      print('❌ ERROR in getAllQuestionnaires: $e');
      throw Exception('Failed to load questionnaires: $e');
    }
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
    try {
      // Отримуємо userId з UserStorage
      final userStorage = UserStorage();
      final user = await userStorage.getUser();
      final userId = user?.id;

      if (userId == null) {
        throw Exception('User ID not found');
      }

      // Додаємо userId до payload
      questionnaire['userId'] = userId;

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
    } catch (e) {
      print('Error creating questionnaire: $e');
      rethrow;
    }
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
      body: json.encode({'id': id, 'statusId': statusId}),
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

  // Відправити результати анкети психічного здоров'я
  Future<void> submitQuestionnaireResult({
    required QuestionnaireResult result,
  }) async {
    try {
      final userStorage = UserStorage();
      final user = await userStorage.getUser();
      final userId = user?.id;

      if (userId == null) {
        throw Exception('User ID not found');
      }

      // Формуємо красивий опис
      final questionnaire = QuestionnaireData.getQuestionnaire();
      final description = _formatQuestionnaireDescription(result, questionnaire);

      final questionnaire_data = {
        'userId': userId,
        'description': description,
        'isAnonymous': false,
        'submittedAt': DateTime.now().toUtc().toIso8601String(),
      };

      await createQuestionnaire(questionnaire_data);
    } catch (e) {
      print('Error submitting questionnaire: $e');
      rethrow;
    }
  }

  // Форматує опис анкети в красивий текст
  String _formatQuestionnaireDescription(
    QuestionnaireResult result,
    Questionnaire questionnaire,
  ) {
    final buffer = StringBuffer();

    // Стан студента
    buffer.writeln('Стан студента: ${_getDepressionDescription(result.depressionLevel)}');
    buffer.writeln('Рівень вигорання: ${_getBurnoutDescription(result.burnoutLevel)}');
    buffer.writeln();

    // Підсумок балів
    buffer.writeln('Підсумок балів:');
    buffer.writeln('- Депресія (1-9): ${result.depressionScore}');
    buffer.writeln('- Вигорання (10-15): ${result.burnoutScore}');
    buffer.writeln();

    // Питання та відповіді
    buffer.writeln('Питання та відповіді:');
    final allQuestions = questionnaire.questions;
    for (var i = 0; i < allQuestions.length; i++) {
      final question = allQuestions[i];
      final answer = result.answers[question.id] ?? 0;
      final optionText = question.options[answer];

      buffer.writeln('${question.id}. ${question.text}');
      buffer.writeln('   Відповідь: $optionText (бал: $answer)');
    }

    return buffer.toString();
  }

  String _getDepressionDescription(String level) {
    switch (level) {
      case 'Норма':
        return 'Стан у межах норми. Депресія відсутня.';
      case 'Легкий':
        return 'Легкі ознаки депресії. Рекомендується звернутися до фахівця для консультації.';
      case 'Помірний':
        return 'Помірні ознаки депресії. Важливо отримати професійну допомогу.';
      case 'Високий':
        return 'Високий рівень депресії. Негайно звернітеся до психолога або психіатра!';
      default:
        return level;
    }
  }

  String _getBurnoutDescription(String level) {
    switch (level) {
      case 'Низький':
        return 'Низький рівень вигорання. Ви в ресурсі.';
      case 'Середній':
        return 'Середній рівень вигорання. Розгляньте способи зняття стресу та відновлення.';
      case 'Високий':
        return 'Високий рівень вигорання. Необхідна невідкладна психологічна допомога.';
      default:
        return level;
    }
  }
} 