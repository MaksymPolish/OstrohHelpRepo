import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../questionnaire/data/services/questionnaire_api_service.dart';
import '../../../auth/presentation/bloc/auth_bloc.dart';
import '../../../auth/presentation/bloc/auth_state.dart';
import 'package:intl/intl.dart';
import '../../../consultation/data/services/consultation_api_service.dart';
import 'dart:convert';

class AdminQuestionnairesPage extends StatefulWidget {
  const AdminQuestionnairesPage({super.key});

  @override
  State<AdminQuestionnairesPage> createState() => _AdminQuestionnairesPageState();
}

class _AdminQuestionnairesPageState extends State<AdminQuestionnairesPage> {
  final QuestionnaireApiService _apiService = QuestionnaireApiService();
  final ConsultationApiService _consultationApiService = ConsultationApiService();

  Future<void> _acceptQuestionnaire({
    required String questionaryId,
    required String psychologistId,
    required DateTime scheduledTime,
  }) async {
    final data = {
      'questionaryId': questionaryId,
      'psychologistId': psychologistId,
      'scheduledTime': scheduledTime.toIso8601String(),
    };
    try {
      await _consultationApiService.acceptConsultation(data);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Анкету прийнято!')),
      );
      setState(() {});
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Помилка при прийнятті: $e')),
      );
    }
  }

  Future<void> _showAcceptDialog(BuildContext context, String questionnaireId, String psychologistId) async {
    DateTime? selectedDateTime;
    await showDialog(
      context: context,
      builder: (ctx) {
        DateTime tempDate = DateTime.now().add(const Duration(days: 1));
        TimeOfDay tempTime = const TimeOfDay(hour: 12, minute: 0);
        return StatefulBuilder(
          builder: (context, setState) {
            return AlertDialog(
              title: const Text('Виберіть дату та час консультації'),
              content: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  ElevatedButton(
                    onPressed: () async {
                      final date = await showDatePicker(
                        context: context,
                        initialDate: tempDate,
                        firstDate: DateTime.now(),
                        lastDate: DateTime.now().add(const Duration(days: 365)),
                      );
                      if (date != null) setState(() => tempDate = date);
                    },
                    child: Text('Дата: ${DateFormat('dd.MM.yyyy').format(tempDate)}'),
                  ),
                  ElevatedButton(
                    onPressed: () async {
                      final time = await showTimePicker(
                        context: context,
                        initialTime: tempTime,
                      );
                      if (time != null) setState(() => tempTime = time);
                    },
                    child: Text('Час: ${tempTime.format(context)}'),
                  ),
                ],
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(ctx),
                  child: const Text('Скасувати'),
                ),
                ElevatedButton(
                  onPressed: () {
                    selectedDateTime = DateTime(
                      tempDate.year,
                      tempDate.month,
                      tempDate.day,
                      tempTime.hour,
                      tempTime.minute,
                    );
                    Navigator.pop(ctx);
                  },
                  child: const Text('Прийняти'),
                ),
              ],
            );
          },
        );
      },
    );
    if (selectedDateTime != null) {
      await _acceptQuestionnaire(
        questionaryId: questionnaireId,
        psychologistId: psychologistId,
        scheduledTime: selectedDateTime!,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Всі анкети')),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is! Authenticated) {
            return const Center(child: Text('Увійдіть як адміністратор або психолог.'));
          }
          final user = state.user;
          return FutureBuilder<List<Map<String, dynamic>>>(
            future: _apiService.getAllQuestionnaires(),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                return Center(child: Text('Помилка: ${snapshot.error}'));
              }
              final questionnaires = snapshot.data ?? [];
              if (questionnaires.isEmpty) {
                return const Center(child: Text('Анкет немає.'));
              }
              return ListView.builder(
                itemCount: questionnaires.length,
                itemBuilder: (context, index) {
                  final q = questionnaires[index];
                  return Card(
                    margin: const EdgeInsets.all(8),
                    child: ListTile(
                      title: Text('Анкета #${q['id']}'),
                      subtitle: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('Опис: ${q['description'] ?? ''}'),
                          Text('Статус: ${q['statusName'] ?? ''}'),
                          if (q['submittedAt'] != null)
                            Text('Дата: ${DateFormat('dd.MM.yyyy HH:mm').format(DateTime.parse(q['submittedAt']))}'),
                        ],
                      ),
                      trailing: (user.roleId == '0c79cd0c-86a8-4a02-803d-d4af6f6ef266' ||
                                user.roleId == 'cf9e7046-d455-480c-970e-0dc55f5ef42c')
                          ? ElevatedButton(
                              onPressed: () => _showAcceptDialog(context, q['id'], user.id ?? ''),
                              child: const Text('Прийняти'),
                            )
                          : null,
                    ),
                  );
                },
              );
            },
          );
        },
      ),
    );
  }
} 