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

  Stream<Message> get messages => _messageController.stream;
  Stream<String> get typingUsers => _typingController.stream;
  Stream<String> get messageRead => _messageReadController.stream;
  Stream<String> get messageDeleted => _messageDeletedController.stream;
  Stream<UserOnlineEvent> get userOnline => _userOnlineController.stream;
  Stream<List<Message>> get messagesLoaded => _messagesLoadedController.stream;
  Stream<HubConnectionState> get connectionState => _connectionStateController.stream;
  Stream<String> get errors => _errorController.stream;

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

    _hubConnection!.on('ReceiveMessage', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final messageJson = arguments[0] as Map<String, dynamic>;
          final message = Message.fromJson(messageJson);
          _messageController.add(message);
        } catch (e) {
          _errorController.add('Error parsing message: $e');
        }
      }
    });

    _hubConnection!.on('UserOnline', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          _userOnlineController.add(UserOnlineEvent(
            userId: data['UserId']?.toString() ?? '',
            isOnline: data['IsOnline'] ?? false,
          ));
        } catch (e) {
          _errorController.add('Error parsing UserOnline: $e');
        }
      }
    });

    _hubConnection!.on('MessageRead', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final data = arguments[0] as Map<String, dynamic>;
          final messageId = data['MessageId']?.toString();
          if (messageId != null) {
            _messageReadController.add(messageId);
          }
        } catch (e) {
          _errorController.add('Error parsing MessageRead: $e');
        }
      }
    });

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
          final data = arguments[0] as Map<String, dynamic>;
          final messageId = data['MessageId']?.toString();
          if (messageId != null) {
            _messageDeletedController.add(messageId);
          }
        } catch (e) {
          _errorController.add('Error parsing MessageDeleted: $e');
        }
      }
    });

    _hubConnection!.on('LoadMessagesResult', (arguments) {
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final messages = (arguments[0] as List?)
                  ?.map((m) => Message.fromJson(m as Map<String, dynamic>))
                  .toList() ??
              [];
          _messagesLoadedController.add(messages);
        } catch (e) {
          _errorController.add('Error parsing LoadMessagesResult: $e');
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
      await loadMessages(consultationId);
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
    required String text,
    List<ChatAttachment>? attachments,
  }) async {
    if (!isConnected) {
      throw Exception('Not connected to chat hub');
    }

    if (text.isEmpty && (attachments == null || attachments.isEmpty)) {
      throw Exception('Message cannot be empty');
    }

    final attachmentsJson = attachments?.map((a) => a.toJson()).toList() ?? [];

    try {
      await _hubConnection!.invoke(
        'SendMessage',
        args: [consultationId, text, attachmentsJson],
      );
    } catch (e) {
      _errorController.add('Failed to send message: $e');
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
    required String consultationId,
  }) async {
    if (!isConnected) {
      throw Exception('Not connected to chat hub');
    }

    try {
      await _hubConnection!.invoke(
        'DeleteMessage',
        args: [messageId, consultationId],
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
  }
}
