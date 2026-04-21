// Модель для питання анкети
class QuestionnaireQuestion {
  final int id;
  final int blockId;
  final String blockName;
  final String text;
  final List<String> options;
  final int maxScore;

  QuestionnaireQuestion({
    required this.id,
    required this.blockId,
    required this.blockName,
    required this.text,
    required this.options,
    required this.maxScore,
  });

  factory QuestionnaireQuestion.fromJson(Map<String, dynamic> json) {
    return QuestionnaireQuestion(
      id: json['id'] as int,
      blockId: json['blockId'] as int,
      blockName: json['blockName'] as String,
      text: json['text'] as String,
      options: List<String>.from(json['options'] as List),
      maxScore: json['maxScore'] as int,
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'blockId': blockId,
    'blockName': blockName,
    'text': text,
    'options': options,
    'maxScore': maxScore,
  };
}
