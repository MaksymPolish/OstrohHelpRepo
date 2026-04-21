import 'package:http/http.dart' as http;
import 'dart:convert';
import '../models/questionnaire.dart';
import '../models/questionnaire_result.dart';
import '../../../../core/auth/user_storage.dart';
import 'questionnaire_data.dart';

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
      final userStorage = UserStorage();
      final user = await userStorage.getUser();
      final userId = user?.id;

      if (userId == null) {
        throw Exception('User ID not found');
      }

      // Формуємо красивий опис
      final questionnaire = QuestionnaireData.getQuestionnaire();
      final description = _formatQuestionnaireDescription(result, questionnaire);

      final payload = {
        'userId': userId,
        'description': description,
        'isAnonymous': false,
        'submittedAt': DateTime.now().toUtc().toIso8601String(),
      };

      final response = await http.post(
        Uri.parse('$baseUrl/api/questionnaire/Create-Questionnaire'),
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
