import '../models/questionnaire_result.dart';

// Сервіс для обчислення результатів анкети
class QuestionnaireCalculationService {
  // Обчислює результати на основі відповідей
  static QuestionnaireResult calculateResult(Map<int, int> answers) {
    // Питання 1-9: Блок депресії (0-3 бали)
    // Питання 10-15: Блок вигорання (0-4 бали)

    int depressionScore = 0;
    int burnoutScore = 0;

    // Суюємо бали для блоку депресії (питання 1-9)
    for (int i = 1; i <= 9; i++) {
      depressionScore += answers[i] ?? 0;
    }

    // Суюємо бали для блоку вигорання (питання 10-15)
    for (int i = 10; i <= 15; i++) {
      burnoutScore += answers[i] ?? 0;
    }

    // Визначаємо рівні
    final depressionLevel = QuestionnaireResult.getDepressionLevel(depressionScore);
    final burnoutLevel = QuestionnaireResult.getBurnoutLevel(burnoutScore);

    // Генеруємо рекомендацію
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

  // Отримати детальний звіт
  static String getDetailedReport(QuestionnaireResult result) {
    final buffer = StringBuffer();

    buffer.writeln('═══════════════════════════════════════');
    buffer.writeln('📋 РЕЗУЛЬТАТИ АНКЕТИ');
    buffer.writeln('═══════════════════════════════════════\n');

    buffer.writeln('📊 СТАН СТУДЕНТА:\n');

    // Депресія
    buffer.writeln('1️⃣ Загальний емоційний стан (Депресія)');
    buffer.writeln('   Рівень: ${result.depressionLevel}');
    buffer.writeln('   Бали: ${result.depressionScore}/27');
    buffer.writeln();

    // Вигорання
    buffer.writeln('2️⃣ Академічне вигорання');
    buffer.writeln('   Рівень: ${result.burnoutLevel}');
    buffer.writeln('   Бали: ${result.burnoutScore}/24');
    buffer.writeln();

    // Рекомендації
    buffer.writeln('💡 РЕКОМЕНДАЦІЇ:');
    buffer.writeln(result.recommendation);

    return buffer.toString();
  }

  // Преобразувати бали в відсотки
  static int getDepressionPercentage(int score) {
    return ((score / 27) * 100).toStringAsFixed(0) as int;
  }

  static int getBurnoutPercentage(int score) {
    return ((score / 24) * 100).toStringAsFixed(0) as int;
  }
}
