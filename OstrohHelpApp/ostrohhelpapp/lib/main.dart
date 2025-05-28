import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:firebase_core/firebase_core.dart';
import 'firebase_options.dart';
import 'core/di/injection_container.dart' as di;
import 'features/auth/presentation/bloc/auth_bloc.dart';
import 'features/auth/presentation/bloc/auth_state.dart';
import 'features/auth/presentation/bloc/auth_event.dart';
import 'features/auth/presentation/pages/login_page.dart';
import 'features/home/presentation/pages/home_page.dart';
import 'features/consultation/presentation/pages/consultation_list_page.dart';
import 'features/consultation/presentation/pages/chat_page.dart';
import 'features/profile/presentation/pages/admin_panel_page.dart';
import 'features/profile/presentation/pages/admin_questionnaires_page.dart';
import 'features/auth/presentation/widgets/course_input_dialog.dart';
import 'features/profile/presentation/pages/admin_users_page.dart';

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

  // Initialize dependency injection
  await di.init();

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
          return MultiBlocProvider(
            providers: [
              BlocProvider<AuthBloc>(
                create: (context) => di.sl<AuthBloc>(param1: context)..add(CheckAuthStatus()),
              ),
            ],
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
              routes: {
                '/consultations': (context) => const ConsultationListPage(),
                '/chat': (context) {
                  final consultationId = ModalRoute.of(context)!.settings.arguments as String;
                  return ChatPage(consultationId: consultationId);
                },
                '/admin-panel': (context) => const AdminPanelPage(),
                '/admin-questionnaires': (context) => const AdminQuestionnairesPage(),
                '/admin-users': (context) {
                  return BlocBuilder<AuthBloc, AuthState>(
                    builder: (context, state) {
                      if (state is Authenticated) {
                        return AdminUsersPage(currentUserId: state.user.id ?? '');
                      }
                      return const Scaffold(body: Center(child: Text('Не авторизовано')));
                    },
                  );
                },
              },
              home: const AuthRoot(),
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

// Окремий віджет для root, щоб можна було використовувати контекст з Navigator
class AuthRoot extends StatefulWidget {
  const AuthRoot({super.key});
  @override
  State<AuthRoot> createState() => _AuthRootState();
}

class _AuthRootState extends State<AuthRoot> {
  bool _dialogShown = false;

  @override
  Widget build(BuildContext context) {
    return BlocListener<AuthBloc, AuthState>(
      listenWhen: (previous, current) {
        // Діалог тільки якщо курс порожній і ще не показували
        return current is Authenticated &&
            (current.user.course == null || current.user.course!.trim().isEmpty) &&
            !_dialogShown;
      },
      listener: (context, state) {
        if (state is Authenticated &&
            (state.user.course == null || state.user.course!.trim().isEmpty) &&
            !_dialogShown) {
          _dialogShown = true;
          showDialog(
            context: context,
            barrierDismissible: false,
            builder: (context) => Builder(
              builder: (dialogContext) => CourseInputDialog(
                userId: state.user.id!,
                onSubmit: (userId, course) {
                  dialogContext.read<AuthBloc>().add(UpdateUserCourse(userId, course));
                  _dialogShown = false; // Дозволити показати діалог знову, якщо курс знову стане порожнім
                },
              ),
            ),
          );
        }
        // Скидаємо прапорець, якщо користувач розлогінився
        if (state is! Authenticated) {
          _dialogShown = false;
        }
      },
      child: BlocBuilder<AuthBloc, AuthState>(
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
          return const LoginPage();
        },
      ),
    );
  }
}

