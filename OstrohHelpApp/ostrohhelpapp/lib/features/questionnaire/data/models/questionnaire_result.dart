// Результат анкети
class QuestionnaireResult {
  final int depressionScore; // Блок 1: 0-15+
  final int burnoutScore; // Блок 2: 0-24
  final String depressionLevel; // Норма, Легкий, Помірний, Високий
  final String burnoutLevel; // Низький, Середній, Високий
  final String recommendation;
  final Map<int, int> answers; // questionId -> score

  QuestionnaireResult({
    required this.depressionScore,
    required this.burnoutScore,
    required this.depressionLevel,
    required this.burnoutLevel,
    required this.recommendation,
    required this.answers,
  });

  // Визначає рівень депресії
  static String getDepressionLevel(int score) {
    if (score <= 4) return 'Норма';
    if (score <= 9) return 'Легкий';
    if (score <= 14) return 'Помірний';
    return 'Високий';
  }

  // Визначає рівень вигорання
  static String getBurnoutLevel(int score) {
    if (score <= 8) return 'Низький';
    if (score <= 16) return 'Середній';
    return 'Високий';
  }

  // Генерує рекомендацію
  static String generateRecommendation(String depression, String burnout) {
    final recommendations = <String>[];

    if (depression == 'Високий') {
      recommendations.add('⚠️ Потрібна допомога психолога для детальної діагностики депресії');
    } else if (depression == 'Помірний') {
      recommendations.add('⚠️ Рекомендується консультація з психологом');
    } else if (depression == 'Легкий') {
      recommendations.add('💤 Рекомендується повноцінний відпочинок');
    }

    if (burnout == 'Високий') {
      recommendations.add('⚠️ Рекомендовано звернутися до психолога або взяти академічну відпустку');
    } else if (burnout == 'Середній') {
      recommendations.add('⚠️ Потрібен перегляд режиму дня та зниження навантаження');
    } else if (burnout == 'Низький') {
      recommendations.add('✅ Ви в ресурсі, продовжуйте берегти себе');
    }

    return recommendations.join('\n');
  }

  factory QuestionnaireResult.fromJson(Map<String, dynamic> json) {
    return QuestionnaireResult(
      depressionScore: json['depressionScore'] as int,
      burnoutScore: json['burnoutScore'] as int,
      depressionLevel: json['depressionLevel'] as String,
      burnoutLevel: json['burnoutLevel'] as String,
      recommendation: json['recommendation'] as String,
      answers: Map<int, int>.from(json['answers'] as Map),
    );
  }

  Map<String, dynamic> toJson() => {
    'depressionScore': depressionScore,
    'burnoutScore': burnoutScore,
    'depressionLevel': depressionLevel,
    'burnoutLevel': burnoutLevel,
    'recommendation': recommendation,
    'answers': answers,
  };
}
