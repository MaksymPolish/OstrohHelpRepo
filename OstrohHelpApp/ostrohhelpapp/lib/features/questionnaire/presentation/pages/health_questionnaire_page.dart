import 'package:flutter/material.dart';
import '../../data/models/questionnaire_question.dart';
import '../../data/models/questionnaire_result.dart';
import '../../data/services/questionnaire_data.dart';
import '../../data/services/questionnaire_calculation_service.dart';
import '../../data/services/questionnaire_api_service.dart';
import 'questionnaire_page.dart';

class HealthQuestionnairePage extends StatefulWidget {
  final String? token;

  const HealthQuestionnairePage({this.token, super.key});

  @override
  State<HealthQuestionnairePage> createState() => _HealthQuestionnairePageState();
}

class _HealthQuestionnairePageState extends State<HealthQuestionnairePage> {
  late List<QuestionnaireQuestion> questions;
  late Map<int, int> answers;
  int currentBlock = 1;
  bool isSubmitting = false;
  QuestionnaireResult? result;

  @override
  void initState() {
    super.initState();
    final questionnaire = QuestionnaireData.getQuestionnaire();
    questions = questionnaire.questions;
    answers = {};
  }

  void _selectAnswer(int questionId, int score) {
    setState(() {
      answers[questionId] = score;
    });
  }

  void _nextBlock() {
    if (currentBlock < 2) {
      setState(() {
        currentBlock++;
      });
      _scrollToTop();
    }
  }

  void _previousBlock() {
    if (currentBlock > 1) {
      setState(() {
        currentBlock--;
      });
      _scrollToTop();
    }
  }

  void _scrollToTop() {
    // Прокрутити вверх
  }

  Future<void> _submitQuestionnaire() async {
    if (answers.length != 15) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Будь ласка, відповідьте на всі питання'),
          backgroundColor: Colors.orange,
        ),
      );
      return;
    }

    setState(() {
      isSubmitting = true;
    });

    try {
      final calculatedResult =
          QuestionnaireCalculationService.calculateResult(answers);

      // Відправити на сервер
      final apiService = QuestionnaireApiService();
      await apiService.submitQuestionnaireResult(result: calculatedResult);

      setState(() {
        result = calculatedResult;
        isSubmitting = false;
      });

      _showResultDialog(calculatedResult);
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Помилка: $e'),
          backgroundColor: Colors.red,
        ),
      );
      setState(() {
        isSubmitting = false;
      });
    }
  }

  void _showResultDialog(QuestionnaireResult result) {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: const Text('Результати анкети'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildResultSection(
                'Загальний емоційний стан',
                result.depressionLevel,
                result.depressionScore,
                27,
              ),
              const SizedBox(height: 16),
              _buildResultSection(
                'Академічне вигорання',
                result.burnoutLevel,
                result.burnoutScore,
                24,
              ),
              const SizedBox(height: 16),
              const Divider(),
              const SizedBox(height: 8),
              const Text(
                'Рекомендації:',
                style: TextStyle(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              Text(result.recommendation),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Закрити'),
          ),
        ],
      ),
    );
  }

  Widget _buildResultSection(
    String title,
    String level,
    int score,
    int maxScore,
  ) {
    final percentage = ((score / maxScore) * 100).toStringAsFixed(0);
    final color = _getLevelColor(level);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: const TextStyle(fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 8),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text('Рівень: $level'),
            Text('$score/$maxScore'),
          ],
        ),
        const SizedBox(height: 8),
        ClipRRect(
          borderRadius: BorderRadius.circular(4),
          child: LinearProgressIndicator(
            value: score / maxScore,
            minHeight: 8,
            backgroundColor: Colors.grey[300],
            valueColor: AlwaysStoppedAnimation<Color>(color),
          ),
        ),
        const SizedBox(height: 4),
        Text('$percentage%', style: TextStyle(color: color)),
      ],
    );
  }

  Color _getLevelColor(String level) {
    switch (level) {
      case 'Норма':
      case 'Низький':
        return Colors.green;
      case 'Легкий':
      case 'Середній':
        return Colors.orange;
      case 'Помірний':
      case 'Високий':
        return Colors.red;
      default:
        return Colors.grey;
    }
  }

  @override
  Widget build(BuildContext context) {
    final blockQuestions = questions.where((q) => q.blockId == currentBlock).toList();
    final answeredCount = blockQuestions.where((q) => answers.containsKey(q.id)).length;
    final progress = answeredCount / blockQuestions.length;

    if (result != null) {
      return Scaffold(
        appBar: AppBar(
          title: const Text('Результати'),
          actions: [
            Padding(
              padding: const EdgeInsets.all(16),
              child: Center(
                child: OutlinedButton(
                  onPressed: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(builder: (context) => const QuestionnairePage()),
                    );
                  },
                  child: const Text('Заповнити форму заявки'),
                ),
              ),
            ),
          ],
        ),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Дякуємо за проходження анкети!',
                    style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 24),
                  _buildResultSection(
                    'Загальний емоційний стан',
                    result!.depressionLevel,
                    result!.depressionScore,
                    27,
                  ),
                  const SizedBox(height: 24),
                  _buildResultSection(
                    'Академічне вигорання',
                    result!.burnoutLevel,
                    result!.burnoutScore,
                    24,
                  ),
                  const SizedBox(height: 24),
                  const Divider(),
                  const SizedBox(height: 16),
                  const Text(
                    'Рекомендації:',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 12),
                  Text(result!.recommendation),
                  const SizedBox(height: 24),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.blue,
                      ),
                      onPressed: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(builder: (context) => const QuestionnairePage()),
                        );
                      },
                      child: const Text(
                        'Заповнити форму заявки',
                        style: TextStyle(color: Colors.white),
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton(
                      onPressed: () => Navigator.pop(context),
                      child: const Text('Повернутися'),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      );
    }

    return Scaffold(
      appBar: AppBar(
        title: Text('Анкета психічного здоров\'я - Блок $currentBlock з 2'),
        leading: IconButton(
          icon: const Icon(Icons.close),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: Column(
        children: [
          // Progress bar
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('Прогрес: $answeredCount/${blockQuestions.length}'),
                    Text('${(progress * 100).toStringAsFixed(0)}%'),
                  ],
                ),
                const SizedBox(height: 8),
                ClipRRect(
                  borderRadius: BorderRadius.circular(4),
                  child: LinearProgressIndicator(
                    value: progress,
                    minHeight: 8,
                  ),
                ),
              ],
            ),
          ),
          // Questions
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              itemCount: blockQuestions.length,
              itemBuilder: (context, index) {
                final question = blockQuestions[index];
                final isAnswered = answers.containsKey(question.id);

                return Card(
                  margin: const EdgeInsets.only(bottom: 16),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Питання ${question.id}',
                          style: TextStyle(
                            fontSize: 12,
                            color: Colors.grey[600],
                          ),
                        ),
                        const SizedBox(height: 8),
                        Text(
                          question.text,
                          style: const TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(height: 12),
                        ...List.generate(
                          question.options.length,
                          (optionIndex) => Padding(
                            padding: const EdgeInsets.symmetric(vertical: 4),
                            child: SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                style: ElevatedButton.styleFrom(
                                  backgroundColor:
                                      answers[question.id] == optionIndex
                                          ? Colors.blue
                                          : Colors.grey[200],
                                  foregroundColor:
                                      answers[question.id] == optionIndex
                                          ? Colors.white
                                          : Colors.black,
                                ),
                                onPressed: () =>
                                    _selectAnswer(question.id, optionIndex),
                                child: Text(question.options[optionIndex]),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
          // Navigation buttons
          Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                if (currentBlock > 1)
                  Expanded(
                    child: OutlinedButton(
                      onPressed: _previousBlock,
                      child: const Text('Назад'),
                    ),
                  ),
                if (currentBlock > 1) const SizedBox(width: 12),
                Expanded(
                  child: ElevatedButton(
                    onPressed: currentBlock < 2 ? _nextBlock : _submitQuestionnaire,
                    child: isSubmitting
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                            ),
                          )
                        : Text(currentBlock < 2 ? 'Далі' : 'Готово'),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
