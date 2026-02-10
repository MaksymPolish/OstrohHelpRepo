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
  final String? psychologistPhotoUrl;
  final DateTime scheduledTime;
  final DateTime createdAt;

  Consultation({
    required this.id,
    required this.studentId,
    required this.psychologistId,
    required this.statusName,
    required this.studentName,
    required this.psychologistName,
    required this.psychologistPhotoUrl,
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
      psychologistPhotoUrl: json['psychologistPhotoUrl'],
      scheduledTime: DateTime.parse(json['scheduledTime']),
      createdAt: DateTime.parse(json['createdAt']),
    );
  }
}

class ConsultationListPage extends StatelessWidget {
  const ConsultationListPage({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

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
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(Icons.forum_outlined, size: 56, color: colorScheme.primary),
                        const SizedBox(height: 16),
                        Text(
                          'Поки що немає консультацій',
                          style: theme.textTheme.headlineSmall,
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: 8),
                        Text(
                          'Подайте анкету, щоб отримати підтримку психолога',
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: colorScheme.onSurface.withOpacity(0.7),
                          ),
                          textAlign: TextAlign.center,
                        ),
                      ],
                    ),
                  ),
                );
              }

              return ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: consultations.length,
                itemBuilder: (context, index) {
                  final consultation = Consultation.fromJson(consultations[index]);
                  final now = DateTime.now();
                  final isChatAvailable = now.isAfter(consultation.scheduledTime);
                  final initials = consultation.psychologistName.isNotEmpty
                      ? consultation.psychologistName
                          .trim()
                          .split(' ')
                          .map((part) => part.isNotEmpty ? part[0] : '')
                          .take(2)
                          .join()
                      : 'P';

                  return Card(
                    margin: const EdgeInsets.only(bottom: 16),
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          CircleAvatar(
                            radius: 26,
                            backgroundColor: colorScheme.primary.withOpacity(0.15),
                            backgroundImage: consultation.psychologistPhotoUrl != null
                                ? NetworkImage(consultation.psychologistPhotoUrl!)
                                : null,
                            child: consultation.psychologistPhotoUrl == null
                                ? Text(
                                    initials.toUpperCase(),
                                    style: theme.textTheme.titleMedium?.copyWith(
                                      color: colorScheme.primary,
                                      fontWeight: FontWeight.w700,
                                    ),
                                  )
                                : null,
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  consultation.psychologistName,
                                  style: theme.textTheme.headlineSmall,
                                ),
                                const SizedBox(height: 6),
                                Text(
                                  'Статус: ${consultation.statusName}',
                                  style: theme.textTheme.bodyMedium?.copyWith(
                                    color: colorScheme.onSurface.withOpacity(0.7),
                                  ),
                                ),
                                const SizedBox(height: 6),
                                Row(
                                  children: [
                                    Icon(Icons.calendar_today, size: 14, color: colorScheme.primary),
                                    const SizedBox(width: 6),
                                    Text(
                                      DateFormat('dd.MM.yyyy HH:mm').format(consultation.scheduledTime),
                                      style: theme.textTheme.bodyMedium?.copyWith(
                                        color: colorScheme.onSurface.withOpacity(0.7),
                                      ),
                                    ),
                                  ],
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(width: 12),
                          ElevatedButton.icon(
                            onPressed: isChatAvailable
                                ? () {
                                    Navigator.pushNamed(
                                      context,
                                      '/chat',
                                      arguments: consultation.id,
                                    );
                                  }
                                : null,
                            icon: const Icon(Icons.chat_bubble_outline),
                            label: const Text('Чат'),
                          ),
                        ],
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