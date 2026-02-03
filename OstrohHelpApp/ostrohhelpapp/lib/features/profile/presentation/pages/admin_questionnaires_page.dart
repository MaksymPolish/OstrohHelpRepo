import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../questionnaire/data/services/questionnaire_api_service.dart';
import '../../../auth/presentation/bloc/auth_bloc.dart';
import '../../../auth/presentation/bloc/auth_state.dart';
import 'package:intl/intl.dart';
import '../../../consultation/data/services/consultation_api_service.dart';

class AdminQuestionnairesPage extends StatefulWidget {
  const AdminQuestionnairesPage({super.key});

  @override
  State<AdminQuestionnairesPage> createState() => _AdminQuestionnairesPageState();
}

class _AdminQuestionnairesPageState extends State<AdminQuestionnairesPage> {
  final QuestionnaireApiService _apiService = QuestionnaireApiService();
  final ConsultationApiService _consultationApiService = ConsultationApiService();

  Future<DateTime?> showDateTimePicker(BuildContext context) async {
    DateTime? selectedDate;
    TimeOfDay? selectedTime;

    selectedDate = await showDatePicker(
      context: context,
      initialDate: DateTime.now().add(const Duration(days: 1)),
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 365)),
      builder: (context, child) {
        return child!;
      },
    );
    if (selectedDate == null) return null;

    selectedTime = await showTimePicker(
      context: context,
      initialTime: const TimeOfDay(hour: 12, minute: 0),
      builder: (context, child) {
        return MediaQuery(
          data: MediaQuery.of(context).copyWith(alwaysUse24HourFormat: true),
          child: child!,
        );
      },
    );
    if (selectedTime == null) return null;

    return DateTime(
      selectedDate.year,
      selectedDate.month,
      selectedDate.day,
      selectedTime.hour,
      selectedTime.minute,
    );
  }

  Future<void> _acceptQuestionnaire(String questionnaireId, String psychologistId) async {
    const acceptedStatusId = 'c71269cb-f0a3-4020-b25a-e423e7daa398';
    try {
      final scheduledTime = await showDateTimePicker(context);
      if (scheduledTime == null) return;

      await _consultationApiService.acceptConsultation({
        'questionaryId': questionnaireId,
        'psychologistId': psychologistId,
        'scheduledTime': scheduledTime.toIso8601String(),
      });

      await _apiService.updateQuestionnaireStatus(questionnaireId, acceptedStatusId);

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
              final questionnaires = (snapshot.data ?? [])
                  .where((q) => q['statusId'] != 'c71269cb-f0a3-4020-b25a-e423e7daa398')
                  .toList();
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
                              onPressed: () => _acceptQuestionnaire(q['id'], user.id ?? ''),
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