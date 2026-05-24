import 'questionnaire_question.dart';

class Questionnaire {
  final List<QuestionnaireQuestion> questions;

  Questionnaire({required this.questions});

  List<QuestionnaireQuestion> getBlockQuestions(int blockId) {
    return questions.where((q) => q.blockId == blockId).toList();
  }

  String? getBlockName(int blockId) {
    final question = questions.firstWhere(
      (q) => q.blockId == blockId,
      orElse: () => throw Exception('Block not found'),
    );
    return question.blockName;
  }

  int getBlockQuestionCount(int blockId) {
    return questions.where((q) => q.blockId == blockId).length;
  }

  factory Questionnaire.fromJson(List<dynamic> json) {
    return Questionnaire(
      questions: (json as List)
          .map((q) => QuestionnaireQuestion.fromJson(q as Map<String, dynamic>))
          .toList(),
    );
  }
}

