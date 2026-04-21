import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
import '../../../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_state.dart';
import '../../../../features/home/presentation/widgets/bottom_nav_bar.dart';
import '../../../../core/auth/token_storage.dart';
import '../../data/services/questionnaire_api_service.dart';
import 'questionnaire_page.dart';
import 'questionnaire_details_page.dart';
import 'health_questionnaire_page.dart';

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
      floatingActionButton: Stack(
        children: [
          // Кнопка "Своя анкета" - внизу справа
          Positioned(
            bottom: 16,
            right: 16,
            child: FloatingActionButton.extended(
              onPressed: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const _CustomQuestionnaireForm(),
                  ),
                );
              },
              icon: const Icon(Icons.create),
              label: const Text('Своя анкета'),
              backgroundColor: Colors.brown,
            ),
          ),
          // Кнопка "Заповнити анкету" - вище
          Positioned(
            bottom: 88,
            right: 16,
            child: FloatingActionButton.extended(
              onPressed: () async {
                final tokenStorage = TokenStorage();
                final token = await tokenStorage.getToken();
                
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => HealthQuestionnairePage(token: token),
                  ),
                );
              },
              icon: const Icon(Icons.assignment),
              label: const Text('Заповнити анкету'),
            ),
          ),
        ],
      ),
      bottomNavigationBar: const CustomBottomNavBar(currentIndex: 1),
    );
  }
}

class _CustomQuestionnaireForm extends StatefulWidget {
  const _CustomQuestionnaireForm();

  @override
  State<_CustomQuestionnaireForm> createState() => _CustomQuestionnaireFormState();
}

class _CustomQuestionnaireFormState extends State<_CustomQuestionnaireForm> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _themeController;
  late TextEditingController _descriptionController;
  String _urgency = 'Середня';
  bool _isSubmitting = false;

  final List<String> _urgencyOptions = [
    'Низька',
    'Середня',
    'Висока',
    'Критична',
  ];

  @override
  void initState() {
    super.initState();
    _themeController = TextEditingController();
    _descriptionController = TextEditingController();
  }

  @override
  void dispose() {
    _themeController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  Future<void> _submitForm() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSubmitting = true);

    try {
      // Формуємо опис у потрібному форматі
      final description = '''Тема: ${_themeController.text}
Терміновість: $_urgency
Опис: ${_descriptionController.text}''';

      final apiService = QuestionnaireApiService();
      await apiService.createQuestionnaire({
        'description': description,
        'isAnonymous': false,
        'submittedAt': DateTime.now().toUtc().toIso8601String(),
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Анкета успішно відправлена'),
            backgroundColor: Colors.green,
          ),
        );
        Navigator.pop(context);
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Помилка: $e'),
          backgroundColor: Colors.red,
        ),
      );
    } finally {
      setState(() => _isSubmitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Своя анкета'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Форма заявки',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              const Text(
                'Опишіть вашу проблему, і ми передамо її спеціалісту.',
                style: TextStyle(color: Colors.grey),
              ),
              const SizedBox(height: 24),

              // Тема звернення
              const Text(
                'Тема звернення',
                style: TextStyle(fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 8),
              TextFormField(
                controller: _themeController,
                decoration: InputDecoration(
                  hintText: 'Тривожність, вигорання, конфлікти у групі',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                validator: (value) {
                  if (value?.isEmpty ?? true) {
                    return 'Будь ласка, введіть тему';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 20),

              // Терміновість
              const Text(
                'Терміновість',
                style: TextStyle(fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 8),
              DropdownButtonFormField<String>(
                value: _urgency,
                decoration: InputDecoration(
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                items: _urgencyOptions.map((String value) {
                  return DropdownMenuItem<String>(
                    value: value,
                    child: Text(value),
                  );
                }).toList(),
                onChanged: (String? newValue) {
                  setState(() => _urgency = newValue ?? 'Середня');
                },
              ),
              const SizedBox(height: 20),

              // Детальний опис
              const Text(
                'Детальний опис',
                style: TextStyle(fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 8),
              TextFormField(
                controller: _descriptionController,
                decoration: InputDecoration(
                  hintText: 'Опишіть ситуацію детальніше',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                maxLines: 6,
                validator: (value) {
                  if (value?.isEmpty ?? true) {
                    return 'Будь ласка, введіть опис';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 32),

              // Кнопка відправки
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: _isSubmitting ? null : _submitForm,
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    backgroundColor: Colors.blue,
                  ),
                  child: _isSubmitting
                      ? const SizedBox(
                          height: 20,
                          width: 20,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            valueColor: AlwaysStoppedAnimation(Colors.white),
                          ),
                        )
                      : const Text(
                          'Відправити заявку',
                          style: TextStyle(color: Colors.white, fontSize: 16),
                        ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
} 