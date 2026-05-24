import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'package:intl/intl.dart';
import '../../data/services/questionnaire_api_service.dart';

class QuestionnaireDetailsPage extends StatelessWidget {
  final String questionnaireId;
  final QuestionnaireApiService _apiService = QuestionnaireApiService();

  QuestionnaireDetailsPage({
    super.key,
    required this.questionnaireId,
  });

  String _formatDate(String? raw) {
    if (raw == null || raw.trim().isEmpty) return 'common.unknownDate'.tr();
    try {
      final parsed = DateTime.parse(raw).toLocal();
      return DateFormat('dd.MM.yyyy, HH:mm').format(parsed);
    } catch (_) {
      return 'common.unknownDate'.tr();
    }
  }

  Color _statusColor(BuildContext context, String? status) {
    final colorScheme = Theme.of(context).colorScheme;
    if (status == null) return colorScheme.secondary;
    if (status == 'questionnaires.status.processing'.tr()) return Colors.orange;
    return Colors.green;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('questionnaires.detailsTitle'.tr()),
      ),
      body: FutureBuilder<Map<String, dynamic>>(
        future: _apiService.getQuestionnaireById(questionnaireId),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(
              child: Text('common.errorWithDetails'.tr(args: [snapshot.error.toString()])),
            );
          }
          if (!snapshot.hasData) {
            return Center(child: Text('common.noData'.tr()));
          }

          final questionnaire = snapshot.data!;
          final theme = Theme.of(context);
          final colorScheme = theme.colorScheme;
          final statusName = questionnaire['statusName'] as String?;
          final statusColor = _statusColor(context, statusName);
          final submittedAt = _formatDate(questionnaire['submittedAt'] as String?);

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
                        Row(
                          children: [
                            Expanded(
                              child: Text(
                                'questionnaires.itemPrefix'.tr(args: [questionnaire['id'].toString()]),
                                style: theme.textTheme.headlineSmall,
                              ),
                            ),
                            Chip(
                              label: Text(statusName ?? 'РќРµРІС–РґРѕРјРѕ'),
                              labelStyle: theme.textTheme.bodyMedium?.copyWith(
                                color: statusColor,
                                fontWeight: FontWeight.w600,
                              ),
                              backgroundColor: statusColor.withOpacity(0.12),
                              side: BorderSide(color: statusColor.withOpacity(0.3)),
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),
                        Text(
                          'questionnaires.descriptionTitle'.tr(),
                          style: theme.textTheme.titleMedium,
                        ),
                        const SizedBox(height: 8),
                        Text(
                          questionnaire['description'] ?? 'questionnaires.descriptionMissing'.tr(),
                          style: theme.textTheme.bodyLarge,
                        ),
                        const SizedBox(height: 16),
                        Row(
                          children: [
                            Icon(Icons.calendar_today, size: 16, color: colorScheme.primary),
                            const SizedBox(width: 6),
                            Text(
                              submittedAt,
                              style: theme.textTheme.bodyMedium?.copyWith(
                                color: colorScheme.onSurface.withOpacity(0.7),
                              ),
                            ),
                          ],
                        ),
                        if (questionnaire['isAnonymous'] == true) ...[
                          const SizedBox(height: 16),
                          Text(
                            'questionnaires.submittedAnonymous'.tr(),
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
