import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../../../features/auth/domain/entities/user.dart';
import '../bloc/user_bloc.dart';

class ProfilePage extends StatelessWidget {
  const ProfilePage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<UserBloc, UserState>(
      builder: (context, state) {
        return Scaffold(
          backgroundColor: Colors.transparent,
          appBar: AppBar(
            backgroundColor: Colors.transparent,
            elevation: 0,
            title: const Text(
              'Profile',
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.bold,
                color: Color(0xFF046380),
              ),
            ),
            centerTitle: true,
          ),
          body: state is UserLoading
              ? const Center(child: CircularProgressIndicator())
              : state is UserLoaded
                  ? _buildProfileContent(context, state.user)
                  : state is UserError
                      ? Center(child: Text(state.message))
                      : const Center(child: Text('No user data available')),
        );
      },
    );
  }

  Widget _buildProfileContent(BuildContext context, User user) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        // Profile Header
        CircleAvatar(
          radius: 50,
          backgroundColor: const Color(0xFF046380),
          backgroundImage: user.photoUrl != null ? NetworkImage(user.photoUrl!) : null,
          child: user.photoUrl == null
              ? const Icon(
                  Icons.person,
                  size: 50,
                  color: Colors.white,
                )
              : null,
        ),
        const SizedBox(height: 16),
        Text(
          user.displayName ?? 'User',
          textAlign: TextAlign.center,
          style: const TextStyle(
            fontSize: 24,
            fontWeight: FontWeight.bold,
            color: Color(0xFF046380),
          ),
        ),
        Text(
          user.email ?? '',
          textAlign: TextAlign.center,
          style: const TextStyle(
            fontSize: 16,
            color: Colors.grey,
          ),
        ),
        const SizedBox(height: 32),

        // Profile Options
        _buildProfileOption(
          icon: Icons.person_outline,
          title: 'Personal Information',
          onTap: () {
            // Navigate to personal info page
          },
        ),
        _buildProfileOption(
          icon: Icons.notifications_outlined,
          title: 'Notifications',
          onTap: () {
            // Navigate to notifications settings
          },
        ),
        _buildProfileOption(
          icon: Icons.lock_outline,
          title: 'Privacy & Security',
          onTap: () {
            // Navigate to privacy settings
          },
        ),
        _buildProfileOption(
          icon: Icons.help_outline,
          title: 'Help & Support',
          onTap: () {
            // Navigate to help page
          },
        ),
        _buildProfileOption(
          icon: Icons.info_outline,
          title: 'About',
          onTap: () {
            // Navigate to about page
          },
        ),
        const SizedBox(height: 16),
        
        // Sign Out Button
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 16),
          child: ElevatedButton(
            onPressed: () {
              context.read<AuthBloc>().add(SignOutRequested());
              Navigator.of(context).pushReplacementNamed('/');
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF046380),
              foregroundColor: Colors.white,
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(30),
              ),
            ),
            child: const Text(
              'Sign Out',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildProfileOption({
    required IconData icon,
    required String title,
    required VoidCallback onTap,
  }) {
    return ListTile(
      leading: Icon(
        icon,
        color: const Color(0xFF046380),
        size: 28,
      ),
      title: Text(
        title,
        style: const TextStyle(
          fontSize: 16,
          fontWeight: FontWeight.w500,
        ),
      ),
      trailing: const Icon(
        Icons.chevron_right,
        color: Colors.grey,
      ),
      onTap: onTap,
    );
  }
} 