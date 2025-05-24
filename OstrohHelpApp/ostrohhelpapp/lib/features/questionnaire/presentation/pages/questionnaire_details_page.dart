import 'package:flutter/material.dart';
import '../../data/services/questionnaire_api_service.dart';

class QuestionnaireDetailsPage extends StatelessWidget {
  final String questionnaireId;
  final QuestionnaireApiService _apiService = QuestionnaireApiService();

  QuestionnaireDetailsPage({
    super.key,
    required this.questionnaireId,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Questionnaire Details'),
      ),
      body: FutureBuilder<Map<String, dynamic>>(
        future: _apiService.getQuestionnaireById(questionnaireId),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(
              child: Text('Error: ${snapshot.error}'),
            );
          }
          if (!snapshot.hasData) {
            return const Center(child: Text('No data available'));
          }

          final questionnaire = snapshot.data!;
          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Questionnaire #${questionnaire['id']}',
                          style: const TextStyle(
                            fontSize: 24,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 16),
                        Text(
                          'Description:',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                        const SizedBox(height: 8),
                        Text(questionnaire['description'] ?? 'No description'),
                        const SizedBox(height: 16),
                        Text(
                          'Status:',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                        const SizedBox(height: 8),
                        Text(
                          questionnaire['status'] ?? 'Unknown',
                          style: TextStyle(
                            color: questionnaire['status'] == 'Under Review'
                                ? Colors.orange
                                : Colors.green,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 16),
                        Text(
                          'Submitted on:',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                        const SizedBox(height: 8),
                        Text(questionnaire['submittedDate'] ?? 'Unknown date'),
                        if (questionnaire['isAnonymous'] == true) ...[
                          const SizedBox(height: 16),
                          const Text(
                            'Submitted anonymously',
                            style: TextStyle(
                              fontStyle: FontStyle.italic,
                              color: Colors.grey,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
} 