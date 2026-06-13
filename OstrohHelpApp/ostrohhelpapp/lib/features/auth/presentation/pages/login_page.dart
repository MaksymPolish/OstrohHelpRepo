import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:easy_localization/easy_localization.dart';
import '../bloc/auth_bloc.dart';
import '../bloc/auth_state.dart';
import '../bloc/auth_event.dart';
import 'package:shared_preferences/shared_preferences.dart';

class LoginPage extends StatelessWidget {
  const LoginPage({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [
              colorScheme.surface,
              colorScheme.surface,
            ],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: SafeArea(
          child: BlocBuilder<AuthBloc, AuthState>(
            builder: (context, state) {
              if (state is AuthLoading) {
                return const Center(
                  child: CircularProgressIndicator(),
                );
              }
              if (state is AuthError) {
                return Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        'login.error'.tr(args: [state.message]),
                        style: TextStyle(color: colorScheme.error),
                        textAlign: TextAlign.center,
                      ),
                      const SizedBox(height: 16),
                      ElevatedButton(
                        onPressed: () {
                          context.read<AuthBloc>().add(SignInWithGoogleRequested());
                        },
                        child: Text('login.retry'.tr()),
                      ),
                    ],
                  ),
                );
              }

              return Stack(
                children: [
                  Center(
                    child: SingleChildScrollView(
                      padding: const EdgeInsets.all(24),
                      child: ConstrainedBox(
                        constraints: const BoxConstraints(maxWidth: 420),
                        child: Card(
                          child: Padding(
                            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 28),
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Image.asset(
                                  'assets/OAMindCare_logo.png',
                                  height: 88,
                                ),
                                const SizedBox(height: 20),
                                Text(
                                  'app.title'.tr(),
                                  style: theme.textTheme.headlineLarge,
                                  textAlign: TextAlign.center,
                                ),
                                const SizedBox(height: 8),
                                Text(
                                  'login.subtitle'.tr(),
                                  style: theme.textTheme.bodyMedium?.copyWith(
                                    color: colorScheme.onSurface.withOpacity(0.75),
                                  ),
                                  textAlign: TextAlign.center,
                                ),
                                const SizedBox(height: 24),
                                ElevatedButton.icon(
                                  onPressed: () {
                                    context.read<AuthBloc>().add(SignInWithGoogleRequested());
                                  },
                                  icon: const Icon(Icons.g_mobiledata),
                                  label: Text('login.signInWithGoogle'.tr()),
                                  style: ElevatedButton.styleFrom(
                                    minimumSize: const Size.fromHeight(52),
                                  ),
                                ),
                                const SizedBox(height: 16),
                                Text(
                                  'login.confidentialNote'.tr(),
                                  style: theme.textTheme.bodyMedium?.copyWith(
                                    color: colorScheme.onSurface.withOpacity(0.65),
                                  ),
                                  textAlign: TextAlign.center,
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                  Positioned(
                    top: 16,
                    right: 16,
                    child: DropdownButton<Locale>(
                      value: context.locale,
                      underline: const SizedBox(),
                      icon: const Icon(Icons.language),
                      onChanged: (Locale? newLocale) async {
                        if (newLocale != null) {
                          await context.setLocale(newLocale);
                          final prefs = await SharedPreferences.getInstance();
                          await prefs.setString('app_locale', newLocale.languageCode);
                        }
                      },
                      items: const [
                        DropdownMenuItem(
                          value: Locale('uk'),
                          child: Text('Укр'),
                        ),
                        DropdownMenuItem(
                          value: Locale('en'),
                          child: Text('Eng'),
                        ),
                      ],
                    ),
                  ),
                ],
              );
            },
          ),
        ),
      ),
    );
  }
} 
