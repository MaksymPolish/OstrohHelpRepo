import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'package:intl/intl.dart';
import '../../data/services/questionnaire_api_service.dart';
import '../../../../core/status/status_constants.dart';

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

  Color _statusColor(BuildContext context, Map<String, dynamic>? questionnaire) {
    final colorScheme = Theme.of(context).colorScheme;
    if (questionnaire == null) return colorScheme.secondary;
    final statusId = questionnaire['statusId'] as String?;
    switch (statusId) {
      case QuestionnaireStatusIds.pending:
        return Colors.orange;
      case QuestionnaireStatusIds.accepted:
        return Colors.green;
      case QuestionnaireStatusIds.rejected:
        return Colors.red;
      default:
        return colorScheme.secondary;
    }
  }

  String _localizedStatusName(BuildContext context, Map<String, dynamic>? questionnaire) {
    if (questionnaire == null) return 'common.unknown'.tr();
    final statusId = questionnaire['statusId'] as String?;
    if (statusId != null) {
      if (statusId == QuestionnaireStatusIds.pending) return 'questionnaires.status.processing'.tr();
      if (statusId == QuestionnaireStatusIds.accepted) return 'questionnaires.status.accepted'.tr();
      if (statusId == QuestionnaireStatusIds.rejected) return 'questionnaires.status.rejected'.tr();
    }
    final raw = questionnaire['statusName'] as String?;
    if (raw == null || raw.isEmpty) return 'common.unknown'.tr();
    if (raw.contains('.')) {
      final translated = raw.tr();
      return translated != raw ? translated : raw;
    }
    if (context.locale.languageCode == 'en') {
      switch (raw.trim()) {
        case 'Обробляється':
          return 'questionnaires.status.processing'.tr();
        case 'Прийнято':
          return 'questionnaires.status.accepted'.tr();
        case 'Відхилено':
          return 'questionnaires.status.rejected'.tr();
      }
    }
    return raw;
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
          final statusColor = _statusColor(context, questionnaire);
          final statusName = _localizedStatusName(context, questionnaire);
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
                              label: Text(statusName),
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
