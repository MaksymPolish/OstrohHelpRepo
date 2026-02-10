import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
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

  String _formatTime(DateTime value) {
    return DateFormat('HH:mm').format(value.toLocal());
  }

  void _showAttachmentHint(String label) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('$label у розробці')),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Чат із психологом'),
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
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 16),
                      itemCount: messages.length,
                      itemBuilder: (context, index) {
                        final msg = messages[messages.length - 1 - index];
                        final isMe = msg.senderId == userId;
                        final bubbleColor = isMe
                            ? colorScheme.primary.withOpacity(0.18)
                            : colorScheme.surface;
                        final textColor = colorScheme.onSurface;

                        return GestureDetector(
                          onLongPress: () => _showDeleteMenu(context, msg.id),
                          child: Align(
                            alignment: isMe ? Alignment.centerRight : Alignment.centerLeft,
                            child: ConstrainedBox(
                              constraints: BoxConstraints(
                                maxWidth: MediaQuery.of(context).size.width * 0.72,
                              ),
                              child: Container(
                                margin: const EdgeInsets.symmetric(vertical: 6),
                                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                                decoration: BoxDecoration(
                                  color: bubbleColor,
                                  borderRadius: BorderRadius.only(
                                    topLeft: const Radius.circular(16),
                                    topRight: const Radius.circular(16),
                                    bottomLeft: Radius.circular(isMe ? 16 : 4),
                                    bottomRight: Radius.circular(isMe ? 4 : 16),
                                  ),
                                ),
                                child: Column(
                                  crossAxisAlignment:
                                      isMe ? CrossAxisAlignment.end : CrossAxisAlignment.start,
                                  children: [
                                    Text(
                                      msg.text,
                                      style: theme.textTheme.bodyLarge?.copyWith(color: textColor),
                                    ),
                                    const SizedBox(height: 6),
                                    Text(
                                      _formatTime(msg.sentAt),
                                      style: theme.textTheme.bodySmall?.copyWith(
                                        color: textColor.withOpacity(0.6),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ),
                          ),
                        );
                      },
                    );
                  },
                ),
              ),
              SafeArea(
                top: false,
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                  decoration: BoxDecoration(
                    color: colorScheme.surface,
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withOpacity(0.05),
                        blurRadius: 12,
                        offset: const Offset(0, -2),
                      ),
                    ],
                  ),
                  child: Row(
                    children: [
                      IconButton(
                        onPressed: () => _showAttachmentHint('Зображення'),
                        icon: const Icon(Icons.image_outlined),
                        tooltip: 'Зображення',
                      ),
                      IconButton(
                        onPressed: () => _showAttachmentHint('Фото'),
                        icon: const Icon(Icons.photo_camera_outlined),
                        tooltip: 'Фото',
                      ),
                      Expanded(
                        child: TextField(
                          controller: _controller,
                          minLines: 1,
                          maxLines: 4,
                          decoration: InputDecoration(
                            hintText: 'Введіть повідомлення...',
                            filled: true,
                            fillColor: colorScheme.background,
                            contentPadding: const EdgeInsets.symmetric(
                              horizontal: 14,
                              vertical: 12,
                            ),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(16),
                              borderSide: BorderSide.none,
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: 8),
                      FloatingActionButton.small(
                        onPressed: () => _sendMessage(userId),
                        backgroundColor: colorScheme.primary,
                        foregroundColor: colorScheme.onPrimary,
                        child: const Icon(Icons.send),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}