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
    _currentUserId = currentUserId;

    final httpConnectionOptions = HttpConnectionOptions(
      accessTokenFactory: () async => accessToken,
      transport: HttpTransportType.WebSockets,
      logMessageContent: true,
      skipNegotiation: false,
    );

    _hubConnection = HubConnectionBuilder()
        .withUrl(
          '$serverUrl/hubs/chat?access_token=${Uri.encodeComponent(accessToken)}',
          options: httpConnectionOptions,
        )
        .withAutomaticReconnect(
          retryDelays: [0, 2000, 5000, 10000, 30000],
        )
        .build();

    _setupEventHandlers();

    await _hubConnection!.start();
    _connectionStateController.add(HubConnectionState.Connected);
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

    // ReceiveMessage - отримання нового повідомлення зі зашифрованими даними
    _hubConnection!.on('ReceiveMessage', (arguments) {
      print('📨 [ReceiveMessage] evento received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final messageJson = arguments[0] as Map<String, dynamic>;
          print('   messageId: ${messageJson['id']}, senderId: ${messageJson['senderId']}');
          final message = Message.fromJson(messageJson);
          _messageController.add(message);
          print('   ✅ Message added');
        } catch (e) {
          print('   ❌ Error parsing message: $e');
          _errorController.add('Error parsing message: $e');
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
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          print('   UserId: ${data['UserId']}, IsOnline: ${data['IsOnline']}');
          _userOnlineController.add(UserOnlineEvent(
            userId: data['UserId']?.toString() ?? '',
            isOnline: data['IsOnline'] ?? false,
          ));
        } catch (e) {
          print('   ❌ Error parsing UserOnline: $e');
          _errorController.add('Error parsing UserOnline: $e');
        }
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

    _hubConnection!.on('LoadMessagesResult', (arguments) {
      print('📨 [LoadMessagesResult] evento received');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final messages = (arguments[0] as List?)
                  ?.map((m) => Message.fromJson(m as Map<String, dynamic>))
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
      
      print('   Loading messages...');
      await loadMessages(consultationId);
      print('   ✅ Messages loaded');
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

  Future<void> loadMessages(String consultationId) async {
    if (!isConnected) return;

    try {
      await _hubConnection!.invoke('LoadMessages', args: [consultationId]);
    } catch (e) {
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

  Future<void> markAsRead({
    required String messageId,
  }) async {
    if (!isConnected) return;

    try {
      await _hubConnection!.invoke(
        'MarkAsRead',
        args: [messageId],
      );
    } catch (e) {
      _errorController.add('Failed to mark message as read: $e');
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
