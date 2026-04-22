import 'package:signalr_netcore/signalr_client.dart';
import 'package:ostrohhelpapp/features/consultation/presentation/notifiers/online_users_notifier.dart';

class PresenceService {
  PresenceService._();
  static final PresenceService instance = PresenceService._();

  HubConnection? _connection;
  String? _serverUrl;
  String? _accessToken;
  String? _currentUserId;

  bool get isConnected => _connection?.state == HubConnectionState.Connected;

  Future<void> start({
    required String serverUrl,
    required String accessToken,
    required String currentUserId,
  }) async {
    _serverUrl = serverUrl;
    _accessToken = accessToken;
    _currentUserId = currentUserId;

    await _connectWithFallback();
  }

  Future<void> _connectWithFallback() async {
    if (_serverUrl == null || _accessToken == null) {
      throw Exception('Presence connection config is missing');
    }

    final primaryUrl = '$_serverUrl/hubs/chat?access_token=${Uri.encodeComponent(_accessToken!)}';
    final fallbackUrl = '$_serverUrl/chat?access_token=${Uri.encodeComponent(_accessToken!)}';

    try {
      await _connect(primaryUrl, _accessToken!);
      print('✅ Presence connected: /hubs/chat');
    } catch (primaryError) {
      print('⚠️ Presence failed /hubs/chat: $primaryError');
      print('↩️ Presence trying fallback /chat');
      await _connect(fallbackUrl, _accessToken!);
      print('✅ Presence connected: /chat (fallback)');
    }
  }

  Future<void> _connect(String hubUrl, String accessToken) async {
    await _connection?.stop();

    final options = HttpConnectionOptions(
      accessTokenFactory: () async => accessToken,
      transport: HttpTransportType.WebSockets,
      skipNegotiation: false,
      logMessageContent: true,
    );

    _connection = HubConnectionBuilder()
        .withUrl(hubUrl, options: options)
        .withAutomaticReconnect(retryDelays: [0, 2000, 5000, 10000, 30000])
        .build();

    _setupHandlers();
    await _connection!.start();
  }

  void _setupHandlers() {
    if (_connection == null) return;

    _connection!.on('UserStatusChanged', (args) {
      print('📨 [Presence.UserStatusChanged] received: $args');
      if (args == null || args.length < 2) return;

      final userId = args[0]?.toString() ?? '';
      final isOnline = args[1] == true;
      OnlineUsersNotifier.instance.setUserStatus(userId: userId, isOnline: isOnline);
    });

    _connection!.on('UserOnline', (args) {
      print('📨 [Presence.UserOnline] received: $args');
      if (args == null || args.isEmpty) return;

      String userId = '';
      bool isOnline = false;

      if (args.length >= 2) {
        userId = args[0]?.toString() ?? '';
        isOnline = args[1] == true;
      } else {
        final raw = args[0];
        if (raw is Map) {
          final data = raw.map((k, v) => MapEntry(k.toString(), v));
          userId = data['userId']?.toString() ?? data['UserId']?.toString() ?? '';
          isOnline = data['isOnline'] == true || data['IsOnline'] == true;
        }
      }

      if (userId.isNotEmpty) {
        OnlineUsersNotifier.instance.setUserStatus(userId: userId, isOnline: isOnline);
      }
    });

    _connection!.onreconnected(({connectionId}) {
      print('✅ Presence reconnected ($connectionId)');
    });

    _connection!.onreconnecting(({error}) {
      print('🔄 Presence reconnecting: $error');
    });

    _connection!.onclose(({error}) {
      print('❌ Presence connection closed: $error');
    });
  }

  Future<void> pause() async {
    try {
      await _connection?.stop();
    } catch (e) {
      print('⚠️ Presence pause failed: $e');
    }
  }

  Future<void> resume() async {
    if (isConnected) return;
    if (_serverUrl == null || _accessToken == null || _currentUserId == null) return;

    try {
      await _connectWithFallback();
    } catch (e) {
      print('⚠️ Presence resume failed: $e');
    }
  }

  Future<void> stop() async {
    try {
      await _connection?.stop();
    } finally {
      _connection = null;
      _serverUrl = null;
      _accessToken = null;
      _currentUserId = null;
    }
  }
}
