import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../../../features/auth/presentation/bloc/auth_state.dart';
import '../../../../features/auth/presentation/bloc/auth_event.dart';
import '../../../../features/home/presentation/widgets/bottom_nav_bar.dart';
import '../../../auth/data/services/auth_api_service.dart';
import '../../../../core/auth/role_checker.dart';
import 'admin_panel_page.dart';
import '../../../../core/theme/app_theme_controller.dart';

class ProfilePage extends StatefulWidget {
  const ProfilePage({super.key});

  @override
  State<ProfilePage> createState() => _ProfilePageState();
}

class _ProfilePageState extends State<ProfilePage> {
  Map<String, dynamic>? userData;
  bool isLoading = false;

  @override
  void initState() {
    super.initState();
    _fetchUserData();
  }

  Future<void> _fetchUserData() async {
    final state = context.read<AuthBloc>().state;
    if (state is Authenticated && state.user.id != null) {
      setState(() => isLoading = true);
      try {
        final data = await AuthApiService().getUserById(state.user.id!);
        setState(() {
          userData = data;
          isLoading = false;
        });
      } catch (e) {
        setState(() => isLoading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Профіль'),
        centerTitle: true,
      ),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is AuthLoading || isLoading) {
            return const Center(child: CircularProgressIndicator());
          }
          if (state is! Authenticated) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text('Please log in to view profile'),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () {
                      Navigator.pushReplacementNamed(context, '/');
                    },
                    child: const Text('Sign In'),
                  ),
                ],
              ),
            );
          }

          final user = state.user;
          final fullName = userData?['fullName'] ?? user.displayName ?? 'Anonymous User';
          final roleName = userData?['roleName'] ?? user.roleName;
          final email = userData?['email'] ?? user.email;
          final course = userData?['course'] ?? user.course;

          return Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  Card(
                    elevation: 6,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
                    color: Colors.white,
                    child: Padding(
                      padding: const EdgeInsets.symmetric(vertical: 32, horizontal: 24),
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          CircleAvatar(
                            radius: 48,
                            backgroundImage: user.photoUrl != null
                                ? NetworkImage(user.photoUrl!)
                                : null,
                            backgroundColor: const Color(0xFFE3F2FD),
                            child: user.photoUrl == null
                                ? const Icon(Icons.person, size: 48, color: Color(0xFF7FB3D5))
                                : null,
                          ),
                          const SizedBox(height: 20),
                          Text(
                            fullName,
                            style: const TextStyle(
                              fontSize: 24,
                              fontWeight: FontWeight.bold,
                              color: Color(0xFF2C3E50),
                            ),
                            textAlign: TextAlign.center,
                          ),
                          const SizedBox(height: 8),
                          if (roleName != null)
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                const Icon(Icons.verified_user, color: Color(0xFF7FB3D5), size: 20),
                                const SizedBox(width: 6),
                                Text(
                                  roleName,
                                  style: const TextStyle(
                                    fontSize: 16,
                                    color: Color(0xFF7FB3D5),
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                              ],
                            ),
                          const SizedBox(height: 12),
                          if (email != null)
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                const Icon(Icons.email, color: Colors.grey, size: 18),
                                const SizedBox(width: 6),
                                Flexible(
                                  child: Text(
                                    email,
                                    style: const TextStyle(
                                      color: Colors.grey,
                                      fontSize: 15,
                                    ),
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ),
                              ],
                            ),
                          const SizedBox(height: 8),
                          if (course != null && course.isNotEmpty)
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                const Icon(Icons.school, color: Colors.green, size: 18),
                                const SizedBox(width: 6),
                                Text(
                                  course,
                                  style: const TextStyle(
                                    color: Colors.green,
                                    fontSize: 15,
                                  ),
                                ),
                              ],
                            ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 32),
                  ValueListenableBuilder<ThemeMode>(
                    valueListenable: AppThemeController.instance.themeMode,
                    builder: (context, mode, _) {
                      return Card(
                        child: Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                          child: Column(
                            children: [
                              ListTile(
                                contentPadding: EdgeInsets.zero,
                                title: const Text('Тема застосунку'),
                                subtitle: Text(
                                  'Оберіть режим, який менше напружує очі',
                                  style: Theme.of(context).textTheme.bodyMedium,
                                ),
                              ),
                              RadioListTile<ThemeMode>(
                                value: ThemeMode.light,
                                groupValue: mode,
                                onChanged: (value) {
                                  if (value != null) {
                                    AppThemeController.instance.setTheme(value);
                                  }
                                },
                                title: const Text('Академічна теплота'),
                                subtitle: const Text('М\'які світлі відтінки для денного режиму'),
                              ),
                              RadioListTile<ThemeMode>(
                                value: ThemeMode.dark,
                                groupValue: mode,
                                onChanged: (value) {
                                  if (value != null) {
                                    AppThemeController.instance.setTheme(value);
                                  }
                                },
                                title: const Text('Нічний режим без тиску'),
                                subtitle: const Text('М\'які темні відтінки для вечора'),
                              ),
                            ],
                          ),
                        ),
                      );
                    },
                  ),
                  const SizedBox(height: 24),
                  // 🔐 ПЕРЕВІРКА РОЛІ: Показати кнопку адмін панелі для Адміністратора або Психолога
                  if (RoleChecker.isAdminOrPsychologist(user.roleId))
                    ElevatedButton.icon(
                      onPressed: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) => const AdminPanelPage(),
                          ),
                        );
                      },
                      icon: const Icon(Icons.admin_panel_settings),
                      label: const Text('Адмін панель'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.blueGrey,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 32,
                          vertical: 12,
                        ),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        elevation: 2,
                      ),
                    ),
                  const SizedBox(height: 16),
                  ElevatedButton.icon(
                    onPressed: () {
                      context.read<AuthBloc>().add(SignOutRequested());
                    },
                    icon: const Icon(Icons.logout),
                    label: const Text('Sign Out'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.red,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(
                        horizontal: 32,
                        vertical: 12,
                      ),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                      elevation: 2,
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
      bottomNavigationBar: const CustomBottomNavBar(currentIndex: 3),
    );
  }
} 