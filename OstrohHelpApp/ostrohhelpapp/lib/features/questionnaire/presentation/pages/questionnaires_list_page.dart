import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:easy_localization/easy_localization.dart';
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
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Scaffold(
      appBar: AppBar(
        title: Text('questionnaires.title'.tr()),
      ),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is! Authenticated) {
            return Center(child: Text('questionnaires.signInPrompt'.tr()));
          }

          return FutureBuilder<List<Map<String, dynamic>>>(
            future: QuestionnaireApiService().getQuestionnairesByUserId(state.user.id ?? ''),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                return Center(
                  child: Text('common.errorWithDetails'.tr(args: [snapshot.error.toString()])),
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
                          'questionnaires.empty.title'.tr(),
                          style: theme.textTheme.headlineSmall,
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: 8),
                        Text(
                          'questionnaires.empty.subtitle'.tr(),
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
                          label: Text('questionnaires.create'.tr()),
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
                                    'questionnaires.itemPrefix'.tr(args: [questionnaire['id'].toString()]),
                                    style: theme.textTheme.headlineSmall,
                                  ),
                                ),
                                Chip(
                                  label: Text(statusName ?? 'common.unknown'.tr()),
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
                                  'questionnaires.viewDetails'.tr(),
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
          Positioned(
            bottom: 16,
            right: 16,
            child: FloatingActionButton.extended(
              heroTag: 'questionnairesCustomFormFab',
              onPressed: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const _CustomQuestionnaireForm(),
                  ),
                );
              },
              icon: const Icon(Icons.create),
              label: Text('questionnaires.customForm'.tr()),
              backgroundColor: Colors.brown,
            ),
          ),
          Positioned(
            bottom: 88,
            right: 16,
            child: FloatingActionButton.extended(
              heroTag: 'questionnairesHealthFab',
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
              label: Text('questionnaires.fillHealth'.tr()),
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
          SnackBar(
            content: Text('questionnaire.success'.tr()),
            backgroundColor: Colors.green,
          ),
        );
        Navigator.pop(context);
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('common.errorWithDetails'.tr(args: [e.toString()])),
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
        title: Text('questionnaires.customForm'.tr()),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'questionnaires.formTitle'.tr(),
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              Text(
                'questionnaires.formSubtitle'.tr(),
                style: TextStyle(color: Colors.grey),
              ),
              const SizedBox(height: 24),

              Text(
                'questionnaires.themeLabel'.tr(),
                style: TextStyle(fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 8),
              TextFormField(
                controller: _themeController,
                decoration: InputDecoration(
                  hintText: 'questionnaires.themeHintCustom'.tr(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                validator: (value) {
                  if (value?.isEmpty ?? true) {
                    return 'questionnaires.validators.themeRequired'.tr();
                  }
                  return null;
                },
              ),
              const SizedBox(height: 20),

              Text(
                'questionnaires.urgencyLabel'.tr(),
                style: TextStyle(fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 8),
              DropdownButtonFormField<String>(
                initialValue: _urgency,
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

              Text(
                'questionnaires.descriptionLabel'.tr(),
                style: TextStyle(fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 8),
              TextFormField(
                controller: _descriptionController,
                decoration: InputDecoration(
                  hintText: 'questionnaires.descriptionHintCustom'.tr(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                maxLines: 6,
                validator: (value) {
                  if (value?.isEmpty ?? true) {
                    return 'questionnaires.validators.descriptionRequired'.tr();
                  }
                  return null;
                },
              ),
              const SizedBox(height: 32),

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
                      : Text(
                          'questionnaires.submitApplication'.tr(),
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
