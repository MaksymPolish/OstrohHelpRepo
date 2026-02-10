import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
import '../../../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_state.dart';
import '../../../../features/home/presentation/widgets/bottom_nav_bar.dart';
import '../../data/services/questionnaire_api_service.dart';
import 'questionnaire_page.dart';
import 'questionnaire_details_page.dart';

class QuestionnairesListPage extends StatelessWidget {
  const QuestionnairesListPage({super.key});

  String _formatDate(String? raw) {
    if (raw == null || raw.trim().isEmpty) return 'Невідома дата';
    try {
      final parsed = DateTime.parse(raw).toLocal();
      return DateFormat('dd.MM.yyyy, HH:mm').format(parsed);
    } catch (_) {
      return 'Невідома дата';
    }
  }

  Color _statusColor(BuildContext context, String? status) {
    final colorScheme = Theme.of(context).colorScheme;
    if (status == null) return colorScheme.secondary;
    if (status == 'Обробляється') return Colors.orange;
    return Colors.green;
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Мої анкети'),
      ),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is! Authenticated) {
            return const Center(child: Text('Увійдіть, щоб переглянути анкети'));
          }

          return FutureBuilder<List<Map<String, dynamic>>>(
            future: QuestionnaireApiService().getQuestionnairesByUserId(state.user.id ?? ''),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                return Center(
                  child: Text('Помилка: ${snapshot.error}'),
                );
              }
              if (!snapshot.hasData || snapshot.data!.isEmpty) {
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(Icons.inbox_outlined, size: 56, color: colorScheme.primary),
                        const SizedBox(height: 16),
                        Text(
                          'Поки що немає анкет',
                          style: theme.textTheme.headlineSmall,
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: 8),
                        Text(
                          'Створіть першу анкету для звернення до психолога',
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: colorScheme.onSurface.withOpacity(0.7),
                          ),
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: 20),
                        ElevatedButton.icon(
                          onPressed: () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (context) => const QuestionnairePage(),
                              ),
                            );
                          },
                          icon: const Icon(Icons.add),
                          label: const Text('Створити анкету'),
                        ),
                      ],
                    ),
                  ),
                );
              }

              return ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: snapshot.data!.length,
                itemBuilder: (context, index) {
                  final questionnaire = snapshot.data![index];
                  final statusName = questionnaire['statusName'] as String?;
                  final statusColor = _statusColor(context, statusName);
                  final submittedAt = _formatDate(questionnaire['submittedAt'] as String?);

                  return Card(
                    margin: const EdgeInsets.only(bottom: 16),
                    child: InkWell(
                      borderRadius: BorderRadius.circular(20),
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) => QuestionnaireDetailsPage(
                              questionnaireId: questionnaire['id'].toString(),
                            ),
                          ),
                        );
                      },
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              children: [
                                Expanded(
                                  child: Text(
                                    'Анкета #${questionnaire['id']}',
                                    style: theme.textTheme.headlineSmall,
                                  ),
                                ),
                                Chip(
                                  label: Text(statusName ?? 'Невідомо'),
                                  labelStyle: theme.textTheme.bodyMedium?.copyWith(
                                    color: statusColor,
                                    fontWeight: FontWeight.w600,
                                  ),
                                  backgroundColor: statusColor.withOpacity(0.12),
                                  side: BorderSide(color: statusColor.withOpacity(0.3)),
                                ),
                              ],
                            ),
                            const SizedBox(height: 8),
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
                            const SizedBox(height: 12),
                            Row(
                              children: [
                                Text(
                                  'Переглянути деталі',
                                  style: theme.textTheme.bodyMedium?.copyWith(
                                    color: colorScheme.primary,
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                                const SizedBox(width: 6),
                                Icon(Icons.chevron_right, color: colorScheme.primary),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ),
                  );
                },
              );
            },
          );
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => const QuestionnairePage(),
            ),
          );
        },
        icon: const Icon(Icons.add),
        label: const Text('Нова анкета'),
      ),
      bottomNavigationBar: const CustomBottomNavBar(currentIndex: 1),
    );
  }
} 