import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:easy_localization/easy_localization.dart';
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
        SnackBar(content: Text('admin.questionnaires.accepted'.tr())),
      );
      setState(() {});
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('admin.questionnaires.acceptError'.tr(args: [e.toString()]))),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('admin.questionnaires.title'.tr())),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is! Authenticated) {
            return Center(child: Text('admin.questionnaires.signInPrompt'.tr()));
          }
          final user = state.user;
          
          if (!RoleChecker.isAdminOrPsychologist(user.roleId) &&
              !RoleChecker.isAdminOrPsychologistByName(user.roleName)) {
            return Center(
              child: Text(
                'admin.questionnaires.insufficientRole'.tr(args: [user.roleId?.toString() ?? 'null', user.roleName ?? 'null']),
              ),
            );
          }
          return FutureBuilder<List<Map<String, dynamic>>>(
            future: _apiService.getAllQuestionnaires(),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                return Center(
                  child: Text(
                    'admin.questionnaires.loadError'.tr(args: [snapshot.error.toString(), user.roleId?.toString() ?? 'null', user.roleName ?? 'null']),
                    textAlign: TextAlign.center,
                  ),
                );
              }
                  final questionnaires = (snapshot.data ?? [])
                    .where((q) => q['statusId'] != QuestionnaireStatusIds.accepted)
                    .toList();
              if (questionnaires.isEmpty) {
                return Center(child: Text('admin.questionnaires.empty'.tr()));
              }
              return ListView.builder(
                itemCount: questionnaires.length,
                itemBuilder: (context, index) {
                  final q = questionnaires[index];
                  return Card(
                    margin: const EdgeInsets.all(8),
                    child: ListTile(
                      title: Text('admin.questionnaires.itemPrefix'.tr(args: [q['id'].toString()])),
                      subtitle: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('admin.questionnaires.description'.tr(args: [q['description']?.toString() ?? ''])),
                          Text('admin.questionnaires.status'.tr(args: [q['statusName']?.toString() ?? ''])),
                          if (q['submittedAt'] != null)
                            Text('admin.questionnaires.date'.tr(args: [DateFormat('dd.MM.yyyy HH:mm').format(DateTime.parse(q['submittedAt']))])),
                        ],
                      ),
                      trailing: RoleChecker.isAdminOrPsychologist(user.roleId)
                          ? ElevatedButton(
                              onPressed: () => _acceptQuestionnaire(q['id'], user.id ?? ''),
                              child: Text('admin.questionnaires.accept'.tr()),
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
