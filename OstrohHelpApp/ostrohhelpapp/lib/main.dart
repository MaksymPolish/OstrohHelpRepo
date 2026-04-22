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
import 'features/profile/presentation/pages/admin_users_page.dart';
import 'core/theme/app_theme.dart';
import 'core/theme/app_theme_controller.dart';
import 'core/auth/token_storage.dart';
import 'core/services/presence_service.dart';
import 'features/consultation/presentation/notifiers/online_users_notifier.dart';

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

  await AppThemeController.instance.loadTheme();

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
          return ValueListenableBuilder<ThemeMode>(
            valueListenable: AppThemeController.instance.themeMode,
            builder: (context, mode, _) {
              return MaterialApp(
                theme: AppTheme.light(),
                darkTheme: AppTheme.dark(),
                themeMode: mode,
                home: Scaffold(
                  body: Center(
                    child: Text('Error: ${snapshot.error}'),
                  ),
                ),
              );
            },
          );
        }

        if (snapshot.connectionState == ConnectionState.done) {
          return MultiBlocProvider(
            providers: [
              BlocProvider<AuthBloc>(
                create: (context) => di.sl<AuthBloc>(param1: context)..add(CheckAuthStatus()),
              ),
            ],
            child: ValueListenableBuilder<ThemeMode>(
              valueListenable: AppThemeController.instance.themeMode,
              builder: (context, mode, _) {
                return MaterialApp(
                  title: 'OA Mind Care',
                  debugShowCheckedModeBanner: false,
                  theme: AppTheme.light(),
                  darkTheme: AppTheme.dark(),
                  themeMode: mode,
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
                );
              },
            ),
          );
        }

        return ValueListenableBuilder<ThemeMode>(
          valueListenable: AppThemeController.instance.themeMode,
          builder: (context, mode, _) {
            return MaterialApp(
              theme: AppTheme.light(),
              darkTheme: AppTheme.dark(),
              themeMode: mode,
              home: const Scaffold(
                body: Center(
                  child: CircularProgressIndicator(),
                ),
              ),
            );
          },
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
  final TokenStorage _tokenStorage = TokenStorage();
  final PresenceService _presenceService = PresenceService.instance;
  bool _presenceEnabled = false;

  static const String _hubBaseUrl = 'http://10.0.2.2:5000';

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(_lifecycleObserver);
  }

  final WidgetsBindingObserver _lifecycleObserver = _PresenceLifecycleObserver();

  Future<void> _startPresence(Authenticated state) async {
    final token = await _tokenStorage.getToken();
    final userId = state.user.id ?? '';
    if (token == null || token.isEmpty || userId.isEmpty) return;

    await _presenceService.start(
      serverUrl: _hubBaseUrl,
      accessToken: token,
      currentUserId: userId,
    );
    _presenceEnabled = true;
  }

  Future<void> _stopPresence() async {
    if (!_presenceEnabled) return;
    await _presenceService.stop();
    _presenceEnabled = false;
    OnlineUsersNotifier.instance.clear();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(_lifecycleObserver);
    _presenceService.stop();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocListener<AuthBloc, AuthState>(
      listener: (context, state) async {
        if (state is Authenticated) {
          await _startPresence(state);
          (_lifecycleObserver as _PresenceLifecycleObserver).onResume = () {
            _presenceService.resume();
          };
          (_lifecycleObserver as _PresenceLifecycleObserver).onPause = () {
            _presenceService.pause();
          };
        } else {
          (_lifecycleObserver as _PresenceLifecycleObserver).onResume = null;
          (_lifecycleObserver as _PresenceLifecycleObserver).onPause = null;
          await _stopPresence();
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

class _PresenceLifecycleObserver with WidgetsBindingObserver {
  VoidCallback? onPause;
  VoidCallback? onResume;

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    switch (state) {
      case AppLifecycleState.paused:
      case AppLifecycleState.inactive:
      case AppLifecycleState.detached:
        onPause?.call();
        break;
      case AppLifecycleState.resumed:
        onResume?.call();
        break;
      case AppLifecycleState.hidden:
        break;
    }
  }
}

