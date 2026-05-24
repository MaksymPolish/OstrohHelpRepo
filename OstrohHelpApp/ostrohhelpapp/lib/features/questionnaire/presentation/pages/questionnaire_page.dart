import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:easy_localization/easy_localization.dart';
import '../../../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_state.dart';
import '../../data/services/questionnaire_api_service.dart';

class QuestionnairePage extends StatefulWidget {
  const QuestionnairePage({super.key});

  @override
  State<QuestionnairePage> createState() => _QuestionnairePageState();
}

class _QuestionnairePageState extends State<QuestionnairePage> {
  final _descriptionController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _isAnonymous = false;
  bool _isSubmitting = false;
  final QuestionnaireApiService _apiService = QuestionnaireApiService();

  @override
  void dispose() {
    _descriptionController.dispose();
    super.dispose();
  }

  Future<void> _submitQuestionnaire() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isSubmitting = true;
    });

    try {
      final state = context.read<AuthBloc>().state;
      if (state is! Authenticated) {
        throw Exception('questionnaire.error.notAuthenticated'.tr());
      }

      final questionnaire = {
        'description': _descriptionController.text,
        'isAnonymous': _isAnonymous,
        'userId': state.user.id,
        'status': 'Under Review',
        'submittedDate': DateTime.now().toIso8601String(),
      };

      await _apiService.createQuestionnaire(questionnaire);
      if (mounted) {
        Navigator.pop(context);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('questionnaire.success'.tr()),
            backgroundColor: Colors.green,
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('common.errorWithDetails'.tr(args: [e.toString()])),
            backgroundColor: Colors.red,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Scaffold(
      appBar: AppBar(
        title: Text('questionnaire.newTitle'.tr()),
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(24),
          children: [
            Text(
              'questionnaire.prompt'.tr(),
              style: theme.textTheme.headlineSmall,
            ),
            const SizedBox(height: 8),
            Text(
              'questionnaire.privacyNote'.tr(),
              style: theme.textTheme.bodyMedium?.copyWith(
                color: colorScheme.onSurface.withOpacity(0.7),
              ),
            ),
            const SizedBox(height: 20),
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    TextFormField(
                      controller: _descriptionController,
                      maxLines: 6,
                      decoration: InputDecoration(
                        labelText: 'questionnaire.descriptionLabel'.tr(),
                        hintText: 'questionnaire.descriptionHint'.tr(),
                      ),
                      validator: (value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'questionnaire.validators.descriptionRequired'.tr();
                        }
                        if (value.trim().length < 10) {
                          return 'questionnaire.validators.descriptionTooShort'.tr();
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 12),
                    SwitchListTile.adaptive(
                      title: Text('questionnaire.anonymous'.tr()),
                      subtitle: Text('questionnaire.anonymousSubtitle'.tr()),
                      value: _isAnonymous,
                      onChanged: (value) {
                        setState(() {
                          _isAnonymous = value;
                        });
                      },
                      contentPadding: EdgeInsets.zero,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: _isSubmitting
                        ? null
                        : () {
                            Navigator.pop(context);
                          },
                    child: Text('common.cancel'.tr()),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: ElevatedButton(
                    onPressed: _isSubmitting ? null : _submitQuestionnaire,
                    child: _isSubmitting
                        ? const SizedBox(
                            width: 20,
                            height: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                            ),
                          )
                        : Text('common.submit'.tr()),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
} 
