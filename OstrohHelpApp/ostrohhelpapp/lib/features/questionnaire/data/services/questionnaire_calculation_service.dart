import '../models/questionnaire_result.dart';

class QuestionnaireCalculationService {
  static QuestionnaireResult calculateResult(Map<int, int> answers) {

    int depressionScore = 0;
    int burnoutScore = 0;

    for (int i = 1; i <= 9; i++) {
      depressionScore += answers[i] ?? 0;
    }

    for (int i = 10; i <= 15; i++) {
      burnoutScore += answers[i] ?? 0;
    }

    final depressionLevel = QuestionnaireResult.getDepressionLevel(depressionScore);
    final burnoutLevel = QuestionnaireResult.getBurnoutLevel(burnoutScore);

    final recommendation = QuestionnaireResult.generateRecommendation(
      depressionLevel,
      burnoutLevel,
    );

    return QuestionnaireResult(
      depressionScore: depressionScore,
      burnoutScore: burnoutScore,
      depressionLevel: depressionLevel,
      burnoutLevel: burnoutLevel,
      recommendation: recommendation,
      answers: answers,
    );
  }

  static String getDetailedReport(QuestionnaireResult result) {
    final buffer = StringBuffer();

    buffer.writeln('═══════════════════════════════════════');
    buffer.writeln('📋 РЕЗУЛЬТАТИ АНКЕТИ');
    buffer.writeln('═══════════════════════════════════════\n');

    buffer.writeln('📊 СТАН СТУДЕНТА:\n');

    buffer.writeln('1️⃣ Загальний емоційний стан (Депресія)');
    buffer.writeln('   Рівень: ${result.depressionLevel}');
    buffer.writeln('   Бали: ${result.depressionScore}/27');
    buffer.writeln();

    buffer.writeln('2️⃣ Академічне вигорання');
    buffer.writeln('   Рівень: ${result.burnoutLevel}');
    buffer.writeln('   Бали: ${result.burnoutScore}/24');
    buffer.writeln();

    buffer.writeln('💡 РЕКОМЕНДАЦІЇ:');
    buffer.writeln(result.recommendation);

    return buffer.toString();
  }

  static int getDepressionPercentage(int score) {
    return int.parse(((score / 27) * 100).toStringAsFixed(0));
  }

  static int getBurnoutPercentage(int score) {
    return int.parse(((score / 24) * 100).toStringAsFixed(0));
  }
}

