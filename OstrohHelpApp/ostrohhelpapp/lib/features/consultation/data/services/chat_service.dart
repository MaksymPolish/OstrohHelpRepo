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
  final _messageUpdatedController = StreamController<Message>.broadcast();
  final _userOnlineController = StreamController<UserOnlineEvent>.broadcast();
  final _messagesLoadedController = StreamController<List<Message>>.broadcast();
  final _connectionStateController = StreamController<HubConnectionState>.broadcast();
  final _errorController = StreamController<String>.broadcast();
  final _consultationKeyController = StreamController<String>.broadcast();

  Stream<Message> get messages => _messageController.stream;
  Stream<String> get typingUsers => _typingController.stream;
  Stream<String> get messageRead => _messageReadController.stream;
  Stream<String> get messageDeleted => _messageDeletedController.stream;
  Stream<Message> get messageUpdated => _messageUpdatedController.stream;
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
    } catch (primaryError) {
      await _connectToHub(fallbackHubUrl, _accessToken!);
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


    _hubConnection!.on('ReceiveConsultationKey', (arguments) {
      
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          
          final consultationKey = data['key']?.toString();
          
          if (consultationKey != null && consultationKey.isNotEmpty) {
            _consultationKeyController.add(consultationKey);
          } else {
          }
        } catch (e) {
          _errorController.add('Error parsing ReceiveConsultationKey: $e');
        }
      } else {
      }
    });

    void handleReceiveMessage(List<Object?>? arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final raw = arguments[0];
          if (raw is! Map) {
            return;
          }

          final messageJson = raw.map((key, value) => MapEntry(key.toString(), value));
          final message = Message.fromJson(messageJson);
          if (message.id.isEmpty) {
            return;
          }
          _messageController.add(message);
        } catch (e) {
          _errorController.add('Error parsing message: $e');
        }
      }
    }

    _hubConnection!.on('ReceiveMessage', handleReceiveMessage);
    _hubConnection!.on('ReceiveNewMessage', handleReceiveMessage);
    _hubConnection!.on('MessageReceived', handleReceiveMessage);

    void handleMessageUpdated(List<Object?>? arguments) {
      if (arguments == null || arguments.isEmpty) return;
      try {
        final raw = arguments[0];
        if (raw is! Map) {
          return;
        }

        final messageJson = raw.map((key, value) => MapEntry(key.toString(), value));
        final message = Message.fromJson(messageJson);
        if (message.id.isEmpty) return;
        _messageUpdatedController.add(message);
      } catch (e) {
        _errorController.add('Error parsing MessageUpdated: $e');
      }
    }

    _hubConnection!.on('MessageUpdated', handleMessageUpdated);
    _hubConnection!.on('ReceiveUpdatedMessage', handleMessageUpdated);
    _hubConnection!.on('MessageEdited', handleMessageUpdated);

    _hubConnection!.on('LoadMessagesResult', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final messages = (arguments[0] as List?)
                  ?.map((m) => Message.fromJson((m as Map).map((k, v) => MapEntry(k.toString(), v))))
                  .toList() ??
              [];
          _messagesLoadedController.add(messages);
        } catch (e) {
          _errorController.add('Error parsing LoadMessagesResult: $e');
        }
      }
    });

    _hubConnection!.on('ReceiveJoinedConsultation', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          _userOnlineController.add(UserOnlineEvent(
            userId: data['userId']?.toString() ?? '',
            isOnline: true,
          ));
        } catch (e) {
          _errorController.add('Error parsing ReceiveJoinedConsultation: $e');
        }
      }
    });

    _hubConnection!.on('UserOnline', (arguments) {
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

        if (userId.isNotEmpty) {
          _userOnlineController.add(UserOnlineEvent(
            userId: userId,
            isOnline: isOnline,
          ));
        }
      } catch (e) {
        _errorController.add('Error parsing UserOnline: $e');
      }
    });

    _hubConnection!.on('UserStatusChanged', (arguments) {
      if (arguments == null || arguments.length < 2) {
        return;
      }

      try {
        final userId = arguments[0]?.toString() ?? '';
        final isOnline = arguments[1] == true;

        if (userId.isNotEmpty) {
          _userOnlineController.add(UserOnlineEvent(
            userId: userId,
            isOnline: isOnline,
          ));
        }
      } catch (e) {
        _errorController.add('Error parsing UserStatusChanged: $e');
      }
    });

    void handleMessageRead(List<Object?>? arguments) {
      if (arguments == null || arguments.isEmpty) return;
      try {
        String messageId = '';

        if (arguments[0] is String) {
          messageId = arguments[0]?.toString() ?? '';
        } else {
          final raw = arguments[0];
          if (raw is Map) {
            final data = raw.map((k, v) => MapEntry(k.toString(), v));
            messageId = data['messageId']?.toString() ?? data['MessageId']?.toString() ?? '';
          }
        }

        if (messageId.isNotEmpty) {
          _messageReadController.add(messageId);
        }
      } catch (e) {
        _errorController.add('Error parsing MessageRead: $e');
      }
    }

    _hubConnection!.on('MessageRead', handleMessageRead);
    _hubConnection!.on('ReceiveMarkedAsRead', handleMessageRead);

    _hubConnection!.on('UserTyping', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          final userId = data['UserId']?.toString();
          if (userId != null) {
            _typingController.add(userId);
          }
        } catch (e) {
          _errorController.add('Error parsing UserTyping: $e');
        }
      }
    });

    _hubConnection!.on('MessageDeleted', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final raw = arguments[0];
          String messageId = '';

          if (raw is String) {
            messageId = raw;
          } else if (raw is Map) {
            final data = raw.map((k, v) => MapEntry(k.toString(), v));
            messageId = data['messageId']?.toString() ?? data['MessageId']?.toString() ?? '';
          }

          if (messageId != null) {
            _messageDeletedController.add(messageId);
          }
        } catch (e) {
          _errorController.add('Error parsing MessageDeleted: $e');
        }
      }
    });

    _hubConnection!.on('Error', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        _errorController.add(arguments[0].toString());
      }
    });

    _hubConnection!.onreconnecting(({error}) {
      _connectionStateController.add(HubConnectionState.Reconnecting);
    });

    _hubConnection!.onreconnected(({connectionId}) {
      _connectionStateController.add(HubConnectionState.Connected);
      if (_currentConsultationId != null) {
        joinConsultation(_currentConsultationId!);
      }
    });

    _hubConnection!.onclose(({error}) {
      _connectionStateController.add(HubConnectionState.Disconnected);
    });
  }

  Future<void> joinConsultation(String consultationId) async {
    
    if (!isConnected) {
      throw Exception('Not connected to chat hub');
    }

    _currentConsultationId = consultationId;

    try {
      await _hubConnection!.invoke('JoinConsultation', args: [consultationId]);
    } catch (e) {
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
    if (!isConnected) {
      return;
    }

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

  Future<void> markAsRead({
    required String messageId,
    required String consultationId,
  }) async {
    if (!isConnected) return;

    try {
      await _hubConnection!.invoke(
        'MarkAsRead',
        args: [messageId, consultationId],
      );
    } catch (e) {
      _errorController.add('Failed to mark message as read: $e');
    }
  }

  Future<void> dispose() async {
    await _hubConnection?.stop();
    await _messageController.close();
    await _typingController.close();
    await _messageReadController.close();
    await _messageDeletedController.close();
    await _messageUpdatedController.close();
    await _userOnlineController.close();
    await _messagesLoadedController.close();
    await _connectionStateController.close();
    await _errorController.close();
    await _consultationKeyController.close();
  }
}

