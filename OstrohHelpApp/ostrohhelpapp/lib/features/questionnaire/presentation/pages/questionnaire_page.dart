import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
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
        throw Exception('User not authenticated');
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
          const SnackBar(
            content: Text('Questionnaire submitted successfully'),
            backgroundColor: Colors.green,
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Error: ${e.toString()}'),
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
    return Scaffold(
      appBar: AppBar(
        title: const Text('Submit Questionnaire'),
      ),
      body: Form(
        key: _formKey,
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              TextFormField(
                controller: _descriptionController,
                maxLines: 5,
                decoration: const InputDecoration(
                  labelText: 'Description',
                  hintText: 'Please describe your situation...',
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  if (value == null || value.trim().isEmpty) {
                    return 'Please enter a description';
                  }
                  if (value.trim().length < 10) {
                    return 'Description must be at least 10 characters long';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 24),
              SwitchListTile(
                title: const Text('Submit anonymously'),
                value: _isAnonymous,
                onChanged: (value) {
                  setState(() {
                    _isAnonymous = value;
                  });
                },
              ),
              const SizedBox(height: 32),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: [
                  ElevatedButton(
                    onPressed: _isSubmitting ? null : () {
                      Navigator.pop(context);
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.grey[300],
                      foregroundColor: Colors.black87,
                    ),
                    child: const Text('Cancel'),
                  ),
                  ElevatedButton(
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
                        : const Text('Send'),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
} 