import 'dart:async';
import 'package:signalr_netcore/signalr_client.dart';
import 'package:ostrohhelpapp/features/message/data/models/message.dart';

class ChatAttachment {
  final String fileUrl;
  final String fileType;

  ChatAttachment({
    required this.fileUrl,
    required this.fileType,
  });

  Map<String, dynamic> toJson() {
    return {
      'fileUrl': fileUrl,
      'fileType': fileType,
    };
  }
}

class UserOnlineEvent {
  final String userId;
  final bool isOnline;

  UserOnlineEvent({
    required this.userId,
    required this.isOnline,
  });
}

class ChatService {
  HubConnection? _hubConnection;
  String? _currentConsultationId;
  String? _currentUserId;
  String? _serverUrl;
  String? _accessToken;

  final _messageController = StreamController<Message>.broadcast();
  final _typingController = StreamController<String>.broadcast();
  final _messageReadController = StreamController<String>.broadcast();
  final _messageDeletedController = StreamController<String>.broadcast();
  final _userOnlineController = StreamController<UserOnlineEvent>.broadcast();
  final _messagesLoadedController = StreamController<List<Message>>.broadcast();
  final _connectionStateController = StreamController<HubConnectionState>.broadcast();
  final _errorController = StreamController<String>.broadcast();
  final _consultationKeyController = StreamController<String>.broadcast();

  Stream<Message> get messages => _messageController.stream;
  Stream<String> get typingUsers => _typingController.stream;
  Stream<String> get messageRead => _messageReadController.stream;
  Stream<String> get messageDeleted => _messageDeletedController.stream;
  Stream<UserOnlineEvent> get userOnline => _userOnlineController.stream;
  Stream<List<Message>> get messagesLoaded => _messagesLoadedController.stream;
  Stream<HubConnectionState> get connectionState => _connectionStateController.stream;
  Stream<String> get errors => _errorController.stream;
  Stream<String> get consultationKey => _consultationKeyController.stream;

  bool get isConnected => _hubConnection?.state == HubConnectionState.Connected;
  String? get currentUserId => _currentUserId;

  Future<void> initialize({
    required String serverUrl,
    required String accessToken,
    required String currentUserId,
  }) async {
    await _hubConnection?.stop();
    _serverUrl = serverUrl;
    _accessToken = accessToken;
    _currentUserId = currentUserId;

    await _connectWithFallback();
    _connectionStateController.add(HubConnectionState.Connected);
  }

  Future<void> _connectWithFallback() async {
    if (_serverUrl == null || _accessToken == null) {
      throw Exception('SignalR connection config is missing');
    }

    final primaryHubUrl = '$_serverUrl/chat?access_token=${Uri.encodeComponent(_accessToken!)}';
    final fallbackHubUrl = '$_serverUrl/hubs/chat?access_token=${Uri.encodeComponent(_accessToken!)}';

    try {
      await _connectToHub(primaryHubUrl, _accessToken!);
      print('✅ Connected to SignalR hub: /chat');
    } catch (primaryError) {
      print('⚠️ Failed to connect to /chat: $primaryError');
      print('↩️ Trying fallback hub URL: /hubs/chat');
      await _connectToHub(fallbackHubUrl, _accessToken!);
      print('✅ Connected to SignalR hub: /hubs/chat (fallback)');
    }
  }

  Future<void> _connectToHub(String hubUrl, String accessToken) async {
    await _hubConnection?.stop();

    final httpConnectionOptions = HttpConnectionOptions(
      accessTokenFactory: () async => accessToken,
      transport: HttpTransportType.WebSockets,
      logMessageContent: true,
      skipNegotiation: false,
    );

    _hubConnection = HubConnectionBuilder()
        .withUrl(
          hubUrl,
          options: httpConnectionOptions,
        )
        .withAutomaticReconnect(
          retryDelays: [0, 2000, 5000, 10000, 30000],
        )
        .build();

    _setupEventHandlers();

    await _hubConnection!.start();
  }

  void _setupEventHandlers() {
    if (_hubConnection == null) return;

    print('🔌 Setting up event handlers...');

    // ReceiveConsultationKey - отримання ключа для шифрування повідомлень
    // Сервер генерує ключ за допомогою HKDF-SHA256:
    //    Input: Master Key (з .env) + ConsultationId
    //    Output: 256-bit Base64-encoded AES-GCM ключ
    _hubConnection!.on('ReceiveConsultationKey', (arguments) {
      print('📨 [ReceiveConsultationKey] evento received');
      print('   arguments: $arguments');
      print('   arguments length: ${arguments?.length}');
      
      if (arguments != null && arguments.isNotEmpty) {
        try {
          print('   Parsing arguments[0]...');
          final data = arguments[0] as Map<String, dynamic>;
          print('   ✅ Parsed data: $data');
          
          // ❗ Сервер надсилає поле 'key', не 'consultationKey'
          final consultationKey = data['key']?.toString();
          print('   consultationKey: ${consultationKey?.substring(0, 20)}...');
          
          if (consultationKey != null && consultationKey.isNotEmpty) {
            print('   🔐 Adding key to consultationKeyController');
            _consultationKeyController.add(consultationKey);
            print('   ✅ Key added successfully');
          } else {
            print('   ❌ consultationKey is null or empty');
          }
        } catch (e) {
          print('   ❌ Error parsing ReceiveConsultationKey: $e');
          _errorController.add('Error parsing ReceiveConsultationKey: $e');
        }
      } else {
        print('   ⚠️ arguments is null or empty');
      }
    });

    void handleReceiveMessage(List<Object?>? arguments) {
      print('📨 [ReceiveMessage] evento received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final raw = arguments[0];
          if (raw is! Map) {
            print('   ❌ Invalid message payload type: ${raw.runtimeType}');
            return;
          }

          final messageJson = raw.map((key, value) => MapEntry(key.toString(), value));
          print('   messageId: ${messageJson['id']}, senderId: ${messageJson['senderId']}');
          final message = Message.fromJson(messageJson);
          if (message.id.isEmpty) {
            print('   ❌ Skipping message with empty id (likely payload shape mismatch)');
            return;
          }
          _messageController.add(message);
          print('   ✅ Message added');
        } catch (e) {
          print('   ❌ Error parsing message: $e');
          _errorController.add('Error parsing message: $e');
        }
      }
    }

    // ReceiveMessage - основний evento
    _hubConnection!.on('ReceiveMessage', handleReceiveMessage);
    // Додаткові aliases для сумісності з різними backend-реалізаціями
    _hubConnection!.on('ReceiveNewMessage', handleReceiveMessage);
    _hubConnection!.on('MessageReceived', handleReceiveMessage);

    // Keep old event for compatibility (legacy)
    _hubConnection!.on('LoadMessagesResult', (arguments) {
      print('📨 [LoadMessagesResult] evento received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final messages = (arguments[0] as List?)
                  ?.map((m) => Message.fromJson((m as Map).map((k, v) => MapEntry(k.toString(), v))))
                  .toList() ??
              [];
          print('   Loaded ${messages.length} messages');
          _messagesLoadedController.add(messages);
          print('   ✅ Messages loaded');
        } catch (e) {
          print('   ❌ Error parsing LoadMessagesResult: $e');
          _errorController.add('Error parsing LoadMessagesResult: $e');
        }
      }
    });

    // ReceiveJoinedConsultation - користувач приєднався до консультації
    _hubConnection!.on('ReceiveJoinedConsultation', (arguments) {
      print('📨 [ReceiveJoinedConsultation] evento received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          print('   userId: ${data['userId']}');
          _userOnlineController.add(UserOnlineEvent(
            userId: data['userId']?.toString() ?? '',
            isOnline: true,
          ));
          print('   ✅ User online event added');
        } catch (e) {
          print('   ❌ Error parsing ReceiveJoinedConsultation: $e');
          _errorController.add('Error parsing ReceiveJoinedConsultation: $e');
        }
      }
    });

    _hubConnection!.on('UserOnline', (arguments) {
      print('📨 [UserOnline] evento received');
      if (arguments == null || arguments.isEmpty) return;
      try {
        String userId = '';
        bool isOnline = false;

        if (arguments.length >= 2) {
          userId = arguments[0]?.toString() ?? '';
          isOnline = arguments[1] == true;
        } else {
          final raw = arguments[0];
          if (raw is Map) {
            final data = raw.map((k, v) => MapEntry(k.toString(), v));
            userId = data['userId']?.toString() ?? data['UserId']?.toString() ?? '';
            isOnline = data['isOnline'] == true || data['IsOnline'] == true;
          }
        }

        print('   UserId: $userId, IsOnline: $isOnline');
        if (userId.isNotEmpty) {
          _userOnlineController.add(UserOnlineEvent(
            userId: userId,
            isOnline: isOnline,
          ));
        }
      } catch (e) {
        print('   ❌ Error parsing UserOnline: $e');
        _errorController.add('Error parsing UserOnline: $e');
      }
    });

    _hubConnection!.on('UserStatusChanged', (arguments) {
      print('📨 [UserStatusChanged] evento received');
      if (arguments == null || arguments.length < 2) {
        print('   ⚠️ Invalid UserStatusChanged payload: $arguments');
        return;
      }

      try {
        final userId = arguments[0]?.toString() ?? '';
        final isOnline = arguments[1] == true;
        print('   UserId: $userId, IsOnline: $isOnline');

        if (userId.isNotEmpty) {
          _userOnlineController.add(UserOnlineEvent(
            userId: userId,
            isOnline: isOnline,
          ));
        }
      } catch (e) {
        print('   ❌ Error parsing UserStatusChanged: $e');
        _errorController.add('Error parsing UserStatusChanged: $e');
      }
    });

    _hubConnection!.on('MessageRead', (arguments) {
      print('📨 [MessageRead] evento received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          final messageId = data['MessageId']?.toString();
          print('   messageId: $messageId');
          if (messageId != null) {
            _messageReadController.add(messageId);
            print('   ✅ Message read event added');
          }
        } catch (e) {
          print('   ❌ Error parsing MessageRead: $e');
          _errorController.add('Error parsing MessageRead: $e');
        }
      }
    });

    _hubConnection!.on('UserTyping', (arguments) {
      print('📨 [UserTyping] evento received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          final userId = data['UserId']?.toString();
          print('   userId: $userId');
          if (userId != null) {
            _typingController.add(userId);
          }
        } catch (e) {
          print('   ❌ Error parsing UserTyping: $e');
          _errorController.add('Error parsing UserTyping: $e');
        }
      }
    });

    _hubConnection!.on('MessageDeleted', (arguments) {
      print('📨 [MessageDeleted] evento received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          final messageId = data['MessageId']?.toString();
          print('   messageId: $messageId');
          if (messageId != null) {
            _messageDeletedController.add(messageId);
            print('   ✅ Message deleted event added');
          }
        } catch (e) {
          print('   ❌ Error parsing MessageDeleted: $e');
          _errorController.add('Error parsing MessageDeleted: $e');
        }
      }
    });

    _hubConnection!.on('Error', (arguments) {
      print('📨 [Error] evento received: $arguments');
      if (arguments != null && arguments.isNotEmpty) {
        _errorController.add(arguments[0].toString());
      }
    });

    _hubConnection!.onreconnecting(({error}) {
      print('🔄 Reconnecting... (error: $error)');
      _connectionStateController.add(HubConnectionState.Reconnecting);
    });

    _hubConnection!.onreconnected(({connectionId}) {
      print('✅ Reconnected (connectionId: $connectionId)');
      _connectionStateController.add(HubConnectionState.Connected);
      if (_currentConsultationId != null) {
        print('   Rejoining consultation: $_currentConsultationId');
        joinConsultation(_currentConsultationId!);
      }
    });

    _hubConnection!.onclose(({error}) {
      print('❌ Connection closed (error: $error)');
      _connectionStateController.add(HubConnectionState.Disconnected);
    });
  }

  Future<void> joinConsultation(String consultationId) async {
    print('📍 joinConsultation called with consultationId: $consultationId');
    print('   isConnected: $isConnected');
    
    if (!isConnected) {
      print('   ❌ Not connected to chat hub');
      throw Exception('Not connected to chat hub');
    }

    _currentConsultationId = consultationId;

    try {
      print('   Invoking JoinConsultation...');
      await _hubConnection!.invoke('JoinConsultation', args: [consultationId]);
      print('   ✅ JoinConsultation invoked successfully');
    } catch (e) {
      print('   ❌ Failed to join consultation: $e');
      _errorController.add('Failed to join consultation: $e');
      rethrow;
    }
  }

  Future<void> leaveConsultation(String consultationId) async {
    if (!isConnected) return;

    try {
      await _hubConnection!.invoke('LeaveConsultation', args: [consultationId]);
      _currentConsultationId = null;
    } catch (e) {
      _errorController.add('Failed to leave consultation: $e');
    }
  }

  Future<void> pauseConnection() async {
    try {
      await _hubConnection?.stop();
      _connectionStateController.add(HubConnectionState.Disconnected);
    } catch (e) {
      _errorController.add('Failed to pause SignalR connection: $e');
    }
  }

  Future<void> resumeConnection() async {
    if (isConnected) return;
    if (_serverUrl == null || _accessToken == null || _currentUserId == null) {
      _errorController.add('Cannot resume SignalR: missing connection context');
      return;
    }

    try {
      await _connectWithFallback();
      _connectionStateController.add(HubConnectionState.Connected);

      if (_currentConsultationId != null) {
        await joinConsultation(_currentConsultationId!);
      }
    } catch (e) {
      _errorController.add('Failed to resume SignalR connection: $e');
    }
  }

  Future<void> loadMessages(String consultationId) async {
    // 📌 DEPRECATED - завантаження історії тепер через REST API (chat_page.dart)
    // Цей метод залишається для сумісності, але більше не викликається
    if (!isConnected) {
      print('⚠️ Cannot load messages: not connected to chat hub');
      return;
    }

    try {
      print('📨 Invoking LoadMessages for consultation: $consultationId (deprecated)');
      await _hubConnection!.invoke('LoadMessages', args: [consultationId]);
      print('✅ LoadMessages invoked successfully (but use REST API instead)');
    } catch (e) {
      print('❌ Error invoking LoadMessages: $e');
      _errorController.add('Failed to load messages: $e');
    }
  }



  Future<void> sendMessage({
    required String consultationId,
    required String encryptedContent,
    required String iv,
    required String authTag,
  }) async {
    if (!isConnected) {
      throw Exception('Not connected to chat hub');
    }

    if (encryptedContent.isEmpty) {
      throw Exception('Message cannot be empty');
    }

    try {
      // 🔐 Сервер вычисляет receiverId сам из консультации
      // Отправляем: consultationId, encryptedContent, iv, authTag, attachments ([])
      // 5 параметров (последний - optional List<AttachmentData>)
      await _hubConnection!.invoke('SendMessage',
        args: [consultationId, encryptedContent, iv, authTag, []],
      );
    } catch (e) {
      _errorController.add('Failed to send message: $e');
      rethrow;
    }
  }

  Future<void> typing(String consultationId) async {
    if (!isConnected) return;

    try {
      await _hubConnection!.invoke('Typing', args: [consultationId]);
    } catch (e) {
      _errorController.add('Failed to send typing indicator: $e');
    }
  }

  Future<void> stopTyping(String consultationId) async {
    if (!isConnected) return;

    try {
      await _hubConnection!.invoke('StopTyping', args: [consultationId]);
    } catch (e) {
      _errorController.add('Failed to stop typing indicator: $e');
    }
  }

  Future<void> deleteMessage({
    required String messageId,
  }) async {
    if (!isConnected) {
      throw Exception('Not connected to chat hub');
    }

    try {
      await _hubConnection!.invoke(
        'DeleteMessage',
        args: [messageId],
      );
    } catch (e) {
      _errorController.add('Failed to delete message: $e');
      rethrow;
    }
  }

  Future<void> dispose() async {
    await _hubConnection?.stop();
    await _messageController.close();
    await _typingController.close();
    await _messageReadController.close();
    await _messageDeletedController.close();
    await _userOnlineController.close();
    await _messagesLoadedController.close();
    await _connectionStateController.close();
    await _errorController.close();
    await _consultationKeyController.close();
  }
}
