import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_state.dart';
import '../../../../features/home/presentation/widgets/bottom_nav_bar.dart';
import '../../data/services/questionnaire_api_service.dart';
import 'questionnaire_page.dart';
import 'questionnaire_details_page.dart';

class QuestionnairesListPage extends StatelessWidget {
  const QuestionnairesListPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('My Questionnaires'),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => const QuestionnairePage(),
                ),
              );
            },
          ),
        ],
      ),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is! Authenticated) {
            return const Center(child: Text('Please log in to view questionnaires'));
          }

          return FutureBuilder<List<Map<String, dynamic>>>(
            future: QuestionnaireApiService().getQuestionnairesByUserId(state.user.id ?? ''),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                return Center(
                  child: Text('Error: ${snapshot.error}'),
                );
              }
              if (!snapshot.hasData || snapshot.data!.isEmpty) {
                return const Center(
                  child: Text('No questionnaires found'),
                );
              }

              return ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: snapshot.data!.length,
                itemBuilder: (context, index) {
                  final questionnaire = snapshot.data![index];
                  return Card(
                    margin: const EdgeInsets.only(bottom: 16),
                    child: ListTile(
                      contentPadding: const EdgeInsets.all(16),
                      title: Text(
                        'Questionnaire #${questionnaire['id']}',
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 18,
                        ),
                      ),
                      subtitle: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const SizedBox(height: 8),
                          Text(
                            'Submitted on: ${questionnaire['submittedDate']?.toString().split('T')[0] ?? 'Unknown date'}',
                            style: const TextStyle(color: Colors.grey),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            'Status: ${questionnaire['status'] ?? 'Unknown'}',
                            style: TextStyle(
                              color: questionnaire['status'] == 'Under Review'
                                  ? Colors.orange
                                  : Colors.green,
                            ),
                          ),
                        ],
                      ),
                      trailing: const Icon(Icons.chevron_right),
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
                    ),
                  );
                },
              );
            },
          );
        },
      ),
      bottomNavigationBar: const CustomBottomNavBar(currentIndex: 1),
    );
  }
} 