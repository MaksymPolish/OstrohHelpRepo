import 'package:http/http.dart' as http;
import 'dart:convert';
import '../models/questionnaire.dart';
import '../models/questionnaire_result.dart';

class QuestionnaireService {
  final String baseUrl = 'https://localhost:7123';

  // Отримати анкету з сервера
  Future<Questionnaire> getQuestionnaire(String token) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/api/questionnaire'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return Questionnaire.fromJson(data);
      } else {
        throw Exception('Failed to load questionnaire: ${response.statusCode}');
      }
    } catch (e) {
      print('Error loading questionnaire: $e');
      rethrow;
    }
  }

  // Відправити результати анкети на сервер
  Future<void> submitQuestionnaireResult({
    required String token,
    required QuestionnaireResult result,
  }) async {
    try {
      final payload = {
        'depressionScore': result.depressionScore,
        'burnoutScore': result.burnoutScore,
        'depressionLevel': result.depressionLevel,
        'burnoutLevel': result.burnoutLevel,
        'answers': result.answers,
      };

      final response = await http.post(
        Uri.parse('$baseUrl/api/questionnaire/submit'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(payload),
      );

      if (response.statusCode != 200 && response.statusCode != 201) {
        throw Exception('Failed to submit questionnaire: ${response.statusCode}');
      }

      print('Questionnaire submitted successfully');
    } catch (e) {
      print('Error submitting questionnaire: $e');
      rethrow;
    }
  }

  // Отримати попередні результати анкети
  Future<List<QuestionnaireResult>> getPreviousResults(String token) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/api/questionnaire/results'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data
            .map((r) =>
                QuestionnaireResult.fromJson(r as Map<String, dynamic>))
            .toList();
      } else {
        return [];
      }
    } catch (e) {
      print('Error loading previous results: $e');
      return [];
    }
  }
}
