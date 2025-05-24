import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:firebase_core/firebase_core.dart';
import 'firebase_options.dart';
import 'core/di/injection_container.dart' as di;
import 'features/auth/data/repositories/auth_repository_impl.dart';
import 'features/auth/domain/repositories/auth_repository.dart';
import 'features/auth/presentation/bloc/auth_bloc.dart';
import 'features/auth/presentation/bloc/auth_state.dart';
import 'features/auth/presentation/bloc/auth_event.dart';
import 'features/auth/presentation/pages/login_page.dart';
import 'features/home/presentation/pages/home_page.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // 👇 Add this block to catch any early Firebase access
  try {
    await Firebase.initializeApp(
      name: "ostrohhelpapp-e9a56",
      options: DefaultFirebaseOptions.currentPlatform,
    );
    debugPrint("Firebase initialized successfully");
  } catch (e) {
    debugPrint('Error initializing Firebase: $e');
  }

  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return FutureBuilder(
      future: _initializeApp(),
      builder: (context, snapshot) {
        if (snapshot.hasError) {
          return MaterialApp(
            home: Scaffold(
              body: Center(
                child: Text('Error: ${snapshot.error}'),
              ),
            ),
          );
        }

        if (snapshot.connectionState == ConnectionState.done) {
          return MultiRepositoryProvider(
            providers: [
              RepositoryProvider<AuthRepository>(
                create: (context) => AuthRepositoryImpl(),
              ),
            ],
            child: BlocProvider(
              create: (context) => AuthBloc(
                authRepository: context.read<AuthRepository>(),
              )..add(CheckAuthStatus()),
              child: MaterialApp(
                title: 'OA Mind Care',
                debugShowCheckedModeBanner: false,
                theme: ThemeData(
                  primaryColor: const Color(0xFF7FB3D5),
                  scaffoldBackgroundColor: const Color(0xFFF5F5F5),
                  colorScheme: ColorScheme.fromSeed(
                    seedColor: const Color(0xFF7FB3D5),
                    primary: const Color(0xFF7FB3D5),
                    secondary: const Color(0xFF98D8C8),
                    surface: const Color(0xFFF5F5F5),
                  ),
                  elevatedButtonTheme: ElevatedButtonThemeData(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF7FB3D5),
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                  ),
                  textTheme: const TextTheme(
                    headlineLarge: TextStyle(
                      fontSize: 32,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF2C3E50),
                    ),
                    bodyLarge: TextStyle(
                      fontSize: 16,
                      color: Color(0xFF2C3E50),
                    ),
                  ),
                ),
                home: BlocBuilder<AuthBloc, AuthState>(
                  builder: (context, state) {
                    if (state is AuthLoading) {
                      return const Scaffold(
                        body: Center(
                          child: CircularProgressIndicator(),
                        ),
                      );
                    }
                    if (state is Authenticated) {
                      return const HomePage();
                    }
                    if (state is AuthError) {
                      return Scaffold(
                        body: Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Text(
                                'Error: ${state.message}',
                                style: const TextStyle(color: Colors.red),
                              ),
                              const SizedBox(height: 16),
                              ElevatedButton(
                                onPressed: () {
                                  context.read<AuthBloc>().add(CheckAuthStatus());
                                },
                                child: const Text('Retry'),
                              ),
                            ],
                          ),
                        ),
                      );
                    }
                    return const LoginPage();
                  },
                ),
              ),
            ),
          );
        }

        return const MaterialApp(
          home: Scaffold(
            body: Center(
              child: CircularProgressIndicator(),
            ),
          ),
        );
      },
    );
  }

  Future<void> _initializeApp() async {
    await di.init(); // Initialize dependency injection
    // Ensure Firebase is initialized
    await Firebase.initializeApp(
      name: "ostrohhelpapp-e9a56",
      options: DefaultFirebaseOptions.currentPlatform,
    );
  }
}

