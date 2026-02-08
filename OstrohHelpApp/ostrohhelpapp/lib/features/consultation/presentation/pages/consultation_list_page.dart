import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
import '../../../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_state.dart';
import '../../../../features/home/presentation/widgets/bottom_nav_bar.dart';
import '../../data/services/consultation_api_service.dart';

class Consultation {
  final String id;
  final String studentId;
  final String psychologistId;
  final String statusName;
  final String studentName;
  final String psychologistName;
  final DateTime scheduledTime;
  final DateTime createdAt;

  Consultation({
    required this.id,
    required this.studentId,
    required this.psychologistId,
    required this.statusName,
    required this.studentName,
    required this.psychologistName,
    required this.scheduledTime,
    required this.createdAt,
  });

  factory Consultation.fromJson(Map<String, dynamic> json) {
    return Consultation(
      id: json['id'],
      studentId: json['studentId'],
      psychologistId: json['psychologistId'],
      statusName: json['statusName'],
      studentName: json['studentName'],
      psychologistName: json['psychologistName'],
      scheduledTime: DateTime.parse(json['scheduledTime']),
      createdAt: DateTime.parse(json['createdAt']),
    );
  }
}

class ConsultationListPage extends StatelessWidget {
  const ConsultationListPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Консультації'),
      ),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is! Authenticated) {
            return const Center(child: Text('Будь ласка, увійдіть в систему'));
          }

          return FutureBuilder<List<Map<String, dynamic>>>(
            future: ConsultationApiService().getConsultationsByUserId(state.user.id ?? ''),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }

              if (snapshot.hasError) {
                return Center(child: Text('Помилка: ${snapshot.error}'));
              }

              final consultations = snapshot.data ?? [];

              if (consultations.isEmpty) {
                return const Center(child: Text('Список консультацій порожній'));
              }

              return ListView.builder(
                itemCount: consultations.length,
                itemBuilder: (context, index) {
                  final consultation = Consultation.fromJson(consultations[index]);
                  final now = DateTime.now();
                  final isChatAvailable = now.isAfter(consultation.scheduledTime);

                  return Card(
                    margin: const EdgeInsets.all(8.0),
                    child: ListTile(
                      title: Text(consultation.psychologistName),
                      subtitle: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('Статус: ${consultation.statusName}'),
                          Text(
                            'Заплановано: ${DateFormat('dd.MM.yyyy HH:mm').format(consultation.scheduledTime)}',
                          ),
                        ],
                      ),
                      trailing: ElevatedButton(
                        onPressed: isChatAvailable
                            ? () {
                                Navigator.pushNamed(
                                  context,
                                  '/chat',
                                  arguments: consultation.id,
                                );
                              }
                            : null,
                        child: const Text('Чат'),
                      ),
                    ),
                  );
                },
              );
            },
          );
        },
      ),
      bottomNavigationBar: const CustomBottomNavBar(currentIndex: 2),
    );
  }
} 