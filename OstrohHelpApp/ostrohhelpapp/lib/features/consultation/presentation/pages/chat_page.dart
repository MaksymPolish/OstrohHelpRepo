import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:ostrohhelpapp/features/message/data/services/message_api_service.dart';
import 'package:ostrohhelpapp/features/message/data/models/message.dart';
import 'package:ostrohhelpapp/features/auth/presentation/bloc/auth_bloc.dart';
import 'package:ostrohhelpapp/features/auth/presentation/bloc/auth_state.dart';

class ChatPage extends StatefulWidget {
  final String consultationId;
  const ChatPage({super.key, required this.consultationId});

  @override
  State<ChatPage> createState() => _ChatPageState();
}

class _ChatPageState extends State<ChatPage> {
  final MessageApiService _messageApiService = MessageApiService();
  final TextEditingController _controller = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  late Future<List<Message>> _messagesFuture;

  @override
  void initState() {
    super.initState();
    _messagesFuture = _loadMessages();
  }

  Future<List<Message>> _loadMessages() async {
    try {
      final raw = await _messageApiService.getMessages(widget.consultationId);
      return raw.map((json) => Message.fromJson(json)).toList();
    } catch (e) {
      debugPrint('Failed to load messages: $e');
      return [];
    }
  }

  Future<void> _sendMessage(String userId) async {
    final text = _controller.text.trim();
    if (text.isEmpty) return;

    try {
      await _messageApiService.sendMessage({
        'consultationId': widget.consultationId,
        'senderId': userId,
        'text': text,
      });

      // --- Очищення текстового поля ---
      _controller.clear();

      // --- Оновлення списку повідомлень ---
      setState(() {
        _messagesFuture = _loadMessages();
      });
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Не вдалося відправити повідомлення: $e')),
      );
    }
  }

  Future<void> _deleteMessage(String messageId) async {
    try {
      await _messageApiService.deleteMessage(messageId);
      setState(() {
        _messagesFuture = _loadMessages();
      });
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Помилка видалення: $e')),
      );
    }
  }

  void _showDeleteMenu(BuildContext context, String messageId) {
    showModalBottomSheet(
      context: context,
      builder: (ctx) => SafeArea(
        child: ListTile(
          leading: const Icon(Icons.delete, color: Colors.red),
          title: const Text('Видалити'),
          onTap: () {
            Navigator.pop(ctx);
            _deleteMessage(messageId);
          },
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Чат'),
        actions: [
          IconButton(
            icon: const Icon(Icons.arrow_back),
            onPressed: () => Navigator.pop(context),
          ),
        ],
      ),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is! Authenticated) {
            return const Center(child: Text('Будь ласка, увійдіть в систему'));
          }

          final userId = state.user.id ?? '';
          return Column(
            children: [
              Expanded(
                child: FutureBuilder<List<Message>>(
                  future: _messagesFuture,
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Center(child: CircularProgressIndicator());
                    }

                    if (snapshot.hasError) {
                      return Center(child: Text('Помилка: ${snapshot.error}'));
                    }

                    final messages = snapshot.data ?? [];

                    if (messages.isEmpty) {
                      return const Center(child: Text('Повідомлень ще немає.'));
                    }

                    return ListView.builder(
                      controller: _scrollController,
                      reverse: true,
                      itemCount: messages.length,
                      itemBuilder: (context, index) {
                        final msg = messages[messages.length - 1 - index];
                        final isMe = msg.senderId == userId;

                        return GestureDetector(
                          onLongPress: () => _showDeleteMenu(context, msg.id),
                          child: Align(
                            alignment: isMe ? Alignment.centerLeft : Alignment.centerRight,
                            child: Container(
                              margin: const EdgeInsets.symmetric(vertical: 4, horizontal: 8),
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: isMe ? Colors.blue[100] : Colors.green[100],
                                borderRadius: BorderRadius.only(
                                  topLeft: const Radius.circular(12),
                                  topRight: const Radius.circular(12),
                                  bottomLeft: Radius.circular(isMe ? 0 : 12),
                                  bottomRight: Radius.circular(isMe ? 12 : 0),
                                ),
                              ),
                              child: Text(msg.text),
                            ),
                          ),
                        );
                      },
                    );
                  },
                ),
              ),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
                child: Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _controller,
                        decoration: const InputDecoration(
                          hintText: 'Введіть повідомлення...',
                          border: OutlineInputBorder(),
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    ElevatedButton(
                      onPressed: () => _sendMessage(userId),
                      child: const Text('Відправити'),
                    ),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}