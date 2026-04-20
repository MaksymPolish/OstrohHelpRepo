import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../questionnaire/data/services/questionnaire_api_service.dart';
import '../../../auth/presentation/bloc/auth_bloc.dart';
import '../../../auth/presentation/bloc/auth_state.dart';
import 'package:intl/intl.dart';
import '../../../consultation/data/services/consultation_api_service.dart';
import '../../../../core/auth/role_checker.dart';
import '../../../../core/status/status_constants.dart';

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
    const acceptedStatusId = QuestionnaireStatusIds.accepted;
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
            // 🔐 ПЕРЕВІРКА РОЛІ: Тільки автентифіковані користувачі
            return const Center(child: Text('Увійдіть як адміністратор або психолог.'));
          }
          final user = state.user;
          print('👤 Current user: RoleId=${user.roleId}, RoleName=${user.roleName}');
          print('🔍 isAdminOrPsychologist=${RoleChecker.isAdminOrPsychologist(user.roleId)}');
          print('🔍 isAdminOrPsychologistByName=${RoleChecker.isAdminOrPsychologistByName(user.roleName)}');
          
          if (!RoleChecker.isAdminOrPsychologist(user.roleId) &&
              !RoleChecker.isAdminOrPsychologistByName(user.roleName)) {
            print('❌ User does not have required role');
            return Center(
              child: Text(
                'Недостатньо прав для перегляду анкет. RoleId: ${user.roleId ?? 'null'}, RoleName: ${user.roleName ?? 'null'}',
              ),
            );
          }
          print('✅ User has required role, loading questionnaires...');
          return FutureBuilder<List<Map<String, dynamic>>>(
            future: _apiService.getAllQuestionnaires(),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                print('⏳ FutureBuilder: waiting...');
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                print('❌ FutureBuilder error: ${snapshot.error}');
                return Center(
                  child: Text(
                    'Помилка: ${snapshot.error}\nRoleId: ${user.roleId ?? 'null'}\nRoleName: ${user.roleName ?? 'null'}\nСпробуйте вийти та увійти знову.',
                    textAlign: TextAlign.center,
                  ),
                );
              }
                  final questionnaires = (snapshot.data ?? [])
                    .where((q) => q['statusId'] != QuestionnaireStatusIds.accepted)
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
                      // 🔐 ПЕРЕВІРКА РОЛІ: Показати кнопку прийняття анкети тільки для Адміністратора або Психолога
                      trailing: RoleChecker.isAdminOrPsychologist(user.roleId)
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