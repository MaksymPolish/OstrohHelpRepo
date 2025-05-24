import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_state.dart';
import '../../../../features/auth/presentation/bloc/auth_event.dart';

class ProfilePage extends StatelessWidget {
  const ProfilePage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Profile'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () {
              context.read<AuthBloc>().add(SignOutRequested());
            },
          ),
        ],
      ),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is Authenticated) {
            return SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  const SizedBox(height: 20),
                  CircleAvatar(
                    radius: 50,
                    backgroundImage: state.user.photoUrl != null
                        ? NetworkImage(state.user.photoUrl!)
                        : null,
                    child: state.user.photoUrl == null
                        ? const Icon(Icons.person, size: 50)
                        : null,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    state.user.displayName ?? 'User',
                    style: const TextStyle(
                      fontSize: 24,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    state.user.email ?? 'No email provided',
                    style: const TextStyle(
                      fontSize: 16,
                      color: Colors.grey,
                    ),
                  ),
                  const SizedBox(height: 32),
                  const ListTile(
                    leading: Icon(Icons.history),
                    title: Text('Consultation History'),
                    trailing: Icon(Icons.chevron_right),
                  ),
                  const Divider(),
                  const ListTile(
                    leading: Icon(Icons.assignment),
                    title: Text('My Questionnaires'),
                    trailing: Icon(Icons.chevron_right),
                  ),
                  const Divider(),
                  const ListTile(
                    leading: Icon(Icons.settings),
                    title: Text('Settings'),
                    trailing: Icon(Icons.chevron_right),
                  ),
                  const Divider(),
                  const ListTile(
                    leading: Icon(Icons.help),
                    title: Text('Help & Support'),
                    trailing: Icon(Icons.chevron_right),
                  ),
                ],
              ),
            );
          }
          return const Center(child: CircularProgressIndicator());
        },
      ),
    );
  }
} 