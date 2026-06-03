import 'package:easy_localization/easy_localization.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'core/auth/token_storage.dart';
import 'core/config/app_config.dart';
import 'core/di/injection_container.dart' as di;
import 'core/services/presence_service.dart';
import 'core/theme/app_theme.dart';
import 'core/theme/app_theme_controller.dart';
import 'firebase_options.dart';
import 'features/auth/presentation/bloc/auth_bloc.dart';
import 'features/auth/presentation/bloc/auth_event.dart';
import 'features/auth/presentation/bloc/auth_state.dart';
import 'features/auth/presentation/pages/login_page.dart';
import 'features/consultation/presentation/notifiers/online_users_notifier.dart';
import 'features/consultation/presentation/pages/chat_page.dart';
import 'features/consultation/presentation/pages/consultation_list_page.dart';
import 'features/home/presentation/pages/home_page.dart';
import 'features/profile/presentation/pages/admin_panel_page.dart';
import 'features/profile/presentation/pages/admin_questionnaires_page.dart';
import 'features/profile/presentation/pages/admin_users_page.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await EasyLocalization.ensureInitialized();
  await dotenv.load(fileName: '.env');

  try {
    await Firebase.initializeApp(
      name: 'ostrohhelpapp-e9a56',
      options: DefaultFirebaseOptions.currentPlatform,
    );
  } catch (e) {
  }

  await di.init();
  await initializeDateFormatting('uk_UA', null);
  await AppThemeController.instance.loadTheme();

  final prefs = await SharedPreferences.getInstance();
  final savedLocale = prefs.getString('app_locale');
  final startLocale = savedLocale == null ? const Locale('uk') : Locale(savedLocale);

  runApp(
    EasyLocalization(
      supportedLocales: const [Locale('uk'), Locale('en')],
      fallbackLocale: const Locale('uk'),
      startLocale: startLocale,
      path: 'assets/translations',
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider<AuthBloc>(
      create: (blocContext) => AuthBloc(blocContext)..add(CheckAuthStatus()),
      child: ValueListenableBuilder<ThemeMode>(
        valueListenable: AppThemeController.instance.themeMode,
        builder: (context, mode, _) {
          return MaterialApp(
            title: 'app.title'.tr(),
            debugShowCheckedModeBanner: false,
            theme: AppTheme.light(),
            darkTheme: AppTheme.dark(),
            themeMode: mode,
            locale: context.locale,
            supportedLocales: context.supportedLocales,
            localizationsDelegates: context.localizationDelegates,
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
                    return Scaffold(body: Center(child: Text('common.notAuthorized'.tr())));
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
}

class AuthRoot extends StatefulWidget {
  const AuthRoot({super.key});

  @override
  State<AuthRoot> createState() => _AuthRootState();
}

class _AuthRootState extends State<AuthRoot> with WidgetsBindingObserver {
  final TokenStorage _tokenStorage = TokenStorage();
  final PresenceService _presenceService = PresenceService.instance;
  bool _presenceEnabled = false;

  final String _hubBaseUrl = AppConfig.signalRHubBaseUrl;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
  }

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
  void didChangeAppLifecycleState(AppLifecycleState state) {
    switch (state) {
      case AppLifecycleState.paused:
      case AppLifecycleState.inactive:
      case AppLifecycleState.detached:
        _presenceService.pause();
        break;
      case AppLifecycleState.resumed:
        _presenceService.resume();
        break;
      case AppLifecycleState.hidden:
        break;
    }
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _presenceService.stop();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocListener<AuthBloc, AuthState>(
      listener: (context, state) async {
        if (state is Authenticated) {
          await _startPresence(state);
        } else {
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
