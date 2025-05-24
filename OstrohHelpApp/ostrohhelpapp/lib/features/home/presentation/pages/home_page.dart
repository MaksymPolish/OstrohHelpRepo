import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../auth/presentation/bloc/auth_bloc.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Home'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () {
              context.read<AuthBloc>().add(SignOutRequested());
            },
          ),
        ],
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Text(
              'Welcome to OA Mind Care!',
              style: TextStyle(fontSize: 24),
            ),
            const SizedBox(height: 20),
            BlocBuilder<AuthBloc, AuthState>(
              builder: (context, state) {
                if (state is Authenticated) {
                  return Column(
                    children: [
                      if (state.user.photoUrl != null)
                        CircleAvatar(
                          radius: 50,
                          backgroundImage: NetworkImage(state.user.photoUrl!),
                        ),
                      const SizedBox(height: 16),
                      Text(
                        'Hello, ${state.user.displayName}!',
                        style: const TextStyle(fontSize: 18),
                      ),
                      Text(
                        state.user.email ?? 'No email provided',
                        style: const TextStyle(fontSize: 16),
                      ),
                    ],
                  );
                }
                return const CircularProgressIndicator();
              },
            ),
          ],
        ),
      ),
    );
  }
} 