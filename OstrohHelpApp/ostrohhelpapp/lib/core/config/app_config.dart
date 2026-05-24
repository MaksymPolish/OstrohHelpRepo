import 'package:flutter_dotenv/flutter_dotenv.dart';

class AppConfig {
  static String get apiBaseUrl => dotenv.env['API_BASE_URL'] ?? 'http://10.0.2.2:5000/api';

  static String get signalRHubBaseUrl => dotenv.env['SIGNALR_HUB_URL'] ?? 'http://10.0.2.2:5000';

  static String get googleServerClientId => dotenv.env['GOOGLE_SERVER_CLIENT_ID'] ?? '';
}
