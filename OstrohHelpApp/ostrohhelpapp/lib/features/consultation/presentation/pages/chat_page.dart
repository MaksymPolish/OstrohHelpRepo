import 'dart:async';
import 'package:file_selector/file_selector.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:image_picker/image_picker.dart';
import 'package:intl/intl.dart';
import 'package:ostrohhelpapp/features/message/data/services/message_api_service.dart';
import 'package:ostrohhelpapp/features/message/data/models/message.dart';
import 'package:ostrohhelpapp/features/consultation/data/services/chat_service.dart';
import 'package:ostrohhelpapp/features/auth/presentation/bloc/auth_bloc.dart';
import 'package:ostrohhelpapp/features/auth/presentation/bloc/auth_state.dart';
import 'package:ostrohhelpapp/features/consultation/data/services/consultation_api_service.dart';
import 'package:ostrohhelpapp/core/auth/token_storage.dart';
import 'package:signalr_netcore/signalr_client.dart';
import 'package:video_player/video_player.dart';

class ChatPage extends StatefulWidget {
  final String consultationId;
  const ChatPage({super.key, required this.consultationId});

  @override
  State<ChatPage> createState() => _ChatPageState();
}

class _ChatPageState extends State<ChatPage> {
  final MessageApiService _messageApiService = MessageApiService();
  final ChatService _chatService = ChatService();
  final TokenStorage _tokenStorage = TokenStorage();
  final ConsultationApiService _consultationApiService = ConsultationApiService();
  final TextEditingController _controller = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  final ImagePicker _imagePicker = ImagePicker();
  final List<Message> _messages = [];
  late Future<Map<String, dynamic>> _consultationFuture;
  final List<StreamSubscription> _subscriptions = [];
  bool _isUploading = false;
  bool _isConnected = false;
  bool _isTyping = false;
  bool _otherUserTyping = false;
  bool _otherUserOnline = false;
  Timer? _typingTimer;
  Timer? _typingIndicatorTimer;
  bool _didInitChat = false;
  late final String _hubBaseUrl;

  @override
  void initState() {
    super.initState();
    _consultationFuture = _consultationApiService.getConsultationById(widget.consultationId);
    _hubBaseUrl = _messageApiService.baseUrl.replaceFirst('/api', '');
  }

  Future<void> _initializeChat(String userId) async {
    try {
      final token = await _tokenStorage.getToken();
      if (token == null || token.isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Не вдалося отримати токен доступу.')),
        );
        return;
      }

      await _chatService.initialize(
        serverUrl: _hubBaseUrl,
        accessToken: token,
        currentUserId: userId,
      );

      if (mounted) {
        setState(() {
          _isConnected = _chatService.isConnected;
        });
      }

      _subscriptions.add(
        _chatService.messages.listen((message) {
          if (!mounted) return;
          if (_messages.any((m) => m.id == message.id)) return;
          setState(() {
            _messages.add(message);
            _messages.sort((a, b) => a.sentAt.compareTo(b.sentAt));
          });
          _scrollToBottom();
          if (message.senderId != userId && !message.isRead) {
            _chatService.markAsRead(
              messageId: message.id,
              consultationId: widget.consultationId,
            );
          }
        }),
      );

      _subscriptions.add(
        _chatService.messagesLoaded.listen((messages) {
          if (!mounted) return;
          setState(() {
            _messages
              ..clear()
              ..addAll(messages);
            _messages.sort((a, b) => a.sentAt.compareTo(b.sentAt));
          });
          for (final message in messages) {
            if (message.senderId != userId && !message.isRead) {
              _chatService.markAsRead(
                messageId: message.id,
                consultationId: widget.consultationId,
              );
            }
          }
          _scrollToBottom();
        }),
      );

      _subscriptions.add(
        _chatService.connectionState.listen((state) {
          if (!mounted) return;
          setState(() {
            _isConnected = state == HubConnectionState.Connected;
          });
        }),
      );

      _subscriptions.add(
        _chatService.userOnline.listen((event) {
          if (!mounted) return;
          setState(() {
            _otherUserOnline = event.isOnline;
          });
        }),
      );

      _subscriptions.add(
        _chatService.typingUsers.listen((typingUserId) {
          if (!mounted || typingUserId == userId) return;
          setState(() {
            _otherUserTyping = true;
          });
          _typingIndicatorTimer?.cancel();
          _typingIndicatorTimer = Timer(const Duration(seconds: 3), () {
            if (mounted) {
              setState(() {
                _otherUserTyping = false;
              });
            }
          });
        }),
      );

      _subscriptions.add(
        _chatService.messageRead.listen((messageId) {
          if (!mounted) return;
          final index = _messages.indexWhere((m) => m.id == messageId);
          if (index == -1) return;
          setState(() {
            _messages[index] = _messages[index].copyWith(isRead: true);
          });
        }),
      );

      _subscriptions.add(
        _chatService.messageDeleted.listen((messageId) {
          if (!mounted) return;
          setState(() {
            _messages.removeWhere((m) => m.id == messageId);
          });
        }),
      );

      _subscriptions.add(
        _chatService.errors.listen((error) {
          if (!mounted) return;
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Помилка: $error')),
          );
        }),
      );

      await _chatService.joinConsultation(widget.consultationId);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Не вдалося підключитися: $e')),
      );
    }
  }

  Future<void> _uploadAndSendFile({
    required String userId,
    required String filePath,
    String? caption,
  }) async {
    if (_isUploading) return;
    setState(() {
      _isUploading = true;
    });

    try {
      final upload = await _messageApiService.uploadToCloud(
        userId: userId,
        filePath: filePath,
      );
      final url = upload['url']?.toString();
      final fileType = upload['fileType']?.toString();
      if (url == null || url.isEmpty || fileType == null || fileType.isEmpty) {
        throw Exception('Upload returned empty url');
      }
      final rawCaption = (caption ?? _controller.text).trim();
      final textPayload = rawCaption.isEmpty ? 'Attachment' : rawCaption;
      await _chatService.sendMessage(
        consultationId: widget.consultationId,
        text: textPayload,
        attachments: [
          ChatAttachment(
            fileUrl: url,
            fileType: fileType,
          ),
        ],
      );
      if (rawCaption.isNotEmpty || caption == null) {
        _controller.clear();
      }
      _typingTimer?.cancel();
      if (_isTyping) {
        setState(() {
          _isTyping = false;
        });
      }
      await _chatService.stopTyping(widget.consultationId);
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Не вдалося завантажити файл: $e')),
      );
    } finally {
      if (mounted) {
        setState(() {
          _isUploading = false;
        });
      }
    }
  }

  Future<void> _pickImageFromGallery(String userId) async {
    final file = await _imagePicker.pickImage(source: ImageSource.gallery, imageQuality: 85);
    if (file == null) return;
    await _uploadAndSendFile(userId: userId, filePath: file.path);
  }

  Future<void> _pickImageFromCamera(String userId) async {
    final file = await _imagePicker.pickImage(source: ImageSource.camera, imageQuality: 85);
    if (file == null) return;
    await _uploadAndSendFile(userId: userId, filePath: file.path);
  }

  Future<void> _pickFile(String userId) async {
    const mediaGroup = XTypeGroup(
      label: 'Media',
      extensions: [
        'jpg',
        'jpeg',
        'png',
        'webp',
        'gif',
        'mp4',
        'mov',
        'webm',
      ],
    );
    const documentGroup = XTypeGroup(
      label: 'Documents',
      extensions: [
        'pdf',
        'doc',
        'docx',
        'ppt',
        'pptx',
        'xls',
        'xlsx',
        'txt',
      ],
    );
    final file = await openFile(acceptedTypeGroups: [mediaGroup, documentGroup]);
    if (file == null) return;
    await _uploadAndSendFile(userId: userId, filePath: file.path);
  }

  Future<void> _deleteMessage(String messageId) async {
    try {
      await _chatService.deleteMessage(
        messageId: messageId,
        consultationId: widget.consultationId,
      );
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

  void _handleTextChanged(String text) {
    if (!_isConnected) return;

    if (text.isNotEmpty && !_isTyping) {
      setState(() {
        _isTyping = true;
      });
      _chatService.typing(widget.consultationId);
    }

    _typingTimer?.cancel();
    _typingTimer = Timer(const Duration(seconds: 2), () {
      if (mounted) {
        setState(() {
          _isTyping = false;
        });
        _chatService.stopTyping(widget.consultationId);
      }
    });
  }

  Future<void> _sendMessage() async {
    if (!_isConnected) return;

    final content = _controller.text.trim();
    if (content.isEmpty) return;

    try {
      await _chatService.sendMessage(
        consultationId: widget.consultationId,
        text: content,
      );
      _controller.clear();
      _typingTimer?.cancel();
      if (_isTyping) {
        setState(() {
          _isTyping = false;
        });
      }
      await _chatService.stopTyping(widget.consultationId);
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Не вдалося відправити повідомлення: $e')),
      );
    }
  }

  void _scrollToBottom() {
    if (!_scrollController.hasClients) return;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted || !_scrollController.hasClients) return;
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 250),
        curve: Curves.easeOut,
      );
    });
  }

  String _formatTime(DateTime value) {
    return DateFormat('HH:mm').format(value.toLocal());
  }

  bool _isImageUrl(String url) {
    final lower = url.toLowerCase();
    return lower.endsWith('.png') ||
        lower.endsWith('.jpg') ||
        lower.endsWith('.jpeg') ||
        lower.endsWith('.gif') ||
        lower.endsWith('.webp');
  }

  bool _isVideoUrl(String url) {
    final lower = url.toLowerCase();
    return lower.endsWith('.mp4') || lower.endsWith('.mov') || lower.endsWith('.webm');
  }

  String _fileNameFromUrl(String url) {
    final parts = url.split('/');
    return parts.isNotEmpty ? parts.last : 'file';
  }

  Widget _buildAttachment(BuildContext context, String url) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    if (_isImageUrl(url)) {
      return InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () => _showImagePreview(url),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(12),
          child: Image.network(
            url,
            fit: BoxFit.cover,
            height: 160,
            width: double.infinity,
            errorBuilder: (context, _, __) => Container(
              height: 160,
              color: colorScheme.surface,
              alignment: Alignment.center,
              child: const Icon(Icons.broken_image_outlined),
            ),
          ),
        ),
      );
    }

    if (_isVideoUrl(url)) {
      return InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () => _showVideoPreview(url),
        child: Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: colorScheme.surface,
            borderRadius: BorderRadius.circular(12),
          ),
          child: Row(
            children: [
              Icon(Icons.play_circle_fill, color: colorScheme.primary),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  _fileNameFromUrl(url),
                  style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurface),
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
        ),
      );
    }

    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: colorScheme.surface,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Icon(Icons.insert_drive_file_outlined, color: colorScheme.primary),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              _fileNameFromUrl(url),
              style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurface),
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAppBarTitle(ThemeData theme, ColorScheme colorScheme) {
    return FutureBuilder<Map<String, dynamic>>(
      future: _consultationFuture,
      builder: (context, snapshot) {
        final data = snapshot.data;
        final name = (data?['psychologistName'] as String?)?.trim();
        final statusName = (data?['statusName'] as String?)?.trim();
        final photoUrl = data?['psychologistPhotoUrl'] as String?;
        final initials = (name != null && name.isNotEmpty)
            ? name
                .split(' ')
                .map((part) => part.isNotEmpty ? part[0] : '')
                .take(2)
                .join()
            : 'P';

        return Row(
          children: [
            CircleAvatar(
              radius: 18,
              backgroundColor: colorScheme.primary.withOpacity(0.2),
              backgroundImage: photoUrl != null ? NetworkImage(photoUrl) : null,
              child: photoUrl == null
                  ? Text(
                      initials.toUpperCase(),
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: colorScheme.primary,
                        fontWeight: FontWeight.w700,
                      ),
                    )
                  : null,
            ),
            const SizedBox(width: 10),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    name ?? 'Психолог',
                    style: theme.textTheme.bodyLarge?.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                    overflow: TextOverflow.ellipsis,
                  ),
                  if (statusName != null && statusName.isNotEmpty)
                    Text(
                      statusName,
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: colorScheme.onSurface.withOpacity(0.6),
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  Text(
                    _otherUserOnline ? 'Online' : 'Offline',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: _otherUserOnline
                          ? Colors.green
                          : colorScheme.onSurface.withOpacity(0.5),
                    ),
                  ),
                ],
              ),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Scaffold(
      appBar: AppBar(
        title: _buildAppBarTitle(theme, colorScheme),
      ),
      body: BlocBuilder<AuthBloc, AuthState>(
        builder: (context, state) {
          if (state is! Authenticated) {
            return const Center(child: Text('Будь ласка, увійдіть в систему'));
          }

          final userId = state.user.id ?? '';
          if (!_didInitChat) {
            _didInitChat = true;
            WidgetsBinding.instance.addPostFrameCallback((_) {
              _initializeChat(userId);
            });
          }
          return Column(
            children: [
              Expanded(
                child: _messages.isEmpty
                    ? Center(
                        child: Text(
                          _isConnected ? 'Повідомлень ще немає.' : 'Підключення до чату...',
                        ),
                      )
                    : ListView.builder(
                        controller: _scrollController,
                        reverse: false,
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 16),
                        itemCount: _messages.length,
                        itemBuilder: (context, index) {
                          final msg = _messages[index];
                          final isMe = msg.senderId == userId;
                          final bubbleColor = isMe
                              ? colorScheme.primary.withOpacity(0.18)
                              : colorScheme.surface;
                          final textColor = colorScheme.onSurface;
                          final attachments = msg.mediaPaths;

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
                                      if (msg.text.trim().isNotEmpty)
                                        Text(
                                          msg.text,
                                          style: theme.textTheme.bodyLarge?.copyWith(color: textColor),
                                        ),
                                      if (attachments.isNotEmpty) ...[
                                        if (msg.text.trim().isNotEmpty) const SizedBox(height: 10),
                                        ...attachments.map((url) => Padding(
                                              padding: const EdgeInsets.only(bottom: 8),
                                              child: _buildAttachment(context, url),
                                            )),
                                      ],
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
                      ),
              ),
              if (_otherUserTyping)
                Padding(
                  padding: const EdgeInsets.only(left: 16, right: 16, bottom: 6),
                  child: Row(
                    children: [
                      Text(
                        'Набирає повідомлення...',
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: colorScheme.onSurface.withOpacity(0.6),
                          fontStyle: FontStyle.italic,
                        ),
                      ),
                    ],
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
                        onPressed: _isUploading ? null : () => _pickImageFromGallery(userId),
                        icon: const Icon(Icons.image_outlined),
                        tooltip: 'Зображення',
                      ),
                      IconButton(
                        onPressed: _isUploading ? null : () => _pickImageFromCamera(userId),
                        icon: const Icon(Icons.photo_camera_outlined),
                        tooltip: 'Фото',
                      ),
                      IconButton(
                        onPressed: _isUploading ? null : () => _pickFile(userId),
                        icon: const Icon(Icons.attach_file),
                        tooltip: 'Файл',
                      ),
                      Expanded(
                        child: TextField(
                          controller: _controller,
                          minLines: 1,
                          maxLines: 4,
                          enabled: _isConnected && !_isUploading,
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
                          onChanged: _handleTextChanged,
                        ),
                      ),
                      const SizedBox(width: 8),
                      FloatingActionButton.small(
                        onPressed: _isUploading ? null : _sendMessage,
                        backgroundColor: colorScheme.primary,
                        foregroundColor: colorScheme.onPrimary,
                        child: _isUploading
                            ? const SizedBox(
                                width: 18,
                                height: 18,
                                child: CircularProgressIndicator(strokeWidth: 2),
                              )
                            : const Icon(Icons.send),
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

  @override
  void dispose() {
    _typingTimer?.cancel();
    _typingIndicatorTimer?.cancel();
    for (final subscription in _subscriptions) {
      subscription.cancel();
    }
    _chatService.leaveConsultation(widget.consultationId);
    _chatService.dispose();
    _controller.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  void _showVideoPreview(String url) {
    showDialog(
      context: context,
      builder: (context) => _VideoPreviewDialog(url: url),
    );
  }

  void _showImagePreview(String url) {
    showDialog(
      context: context,
      builder: (context) => _ImagePreviewDialog(url: url),
    );
  }
}

class _VideoPreviewDialog extends StatefulWidget {
  const _VideoPreviewDialog({required this.url});

  final String url;

  @override
  State<_VideoPreviewDialog> createState() => _VideoPreviewDialogState();
}

class _VideoPreviewDialogState extends State<_VideoPreviewDialog> {
  late final VideoPlayerController _controller;
  bool _initialized = false;

  @override
  void initState() {
    super.initState();
    _controller = VideoPlayerController.networkUrl(Uri.parse(widget.url))
      ..initialize().then((_) {
        if (mounted) {
          setState(() {
            _initialized = true;
          });
        }
      });
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Dialog(
      backgroundColor: colorScheme.surface,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (_initialized)
              AspectRatio(
                aspectRatio: _controller.value.aspectRatio,
                child: VideoPlayer(_controller),
              )
            else
              const SizedBox(
                height: 200,
                child: Center(child: CircularProgressIndicator()),
              ),
            const SizedBox(height: 12),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                IconButton(
                  onPressed: () {
                    setState(() {
                      if (_controller.value.isPlaying) {
                        _controller.pause();
                      } else {
                        _controller.play();
                      }
                    });
                  },
                  icon: Icon(
                    _controller.value.isPlaying ? Icons.pause_circle : Icons.play_circle,
                    color: colorScheme.primary,
                    size: 36,
                  ),
                ),
                IconButton(
                  onPressed: () {
                    Navigator.pop(context);
                  },
                  icon: Icon(Icons.close, color: colorScheme.onSurface.withOpacity(0.7)),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _ImagePreviewDialog extends StatelessWidget {
  const _ImagePreviewDialog({required this.url});

  final String url;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Dialog(
      backgroundColor: Colors.black87,
      insetPadding: EdgeInsets.zero,
      child: Stack(
        children: [
          Center(
            child: InteractiveViewer(
              minScale: 0.5,
              maxScale: 4.0,
              child: Image.network(
                url,
                fit: BoxFit.contain,
                errorBuilder: (context, _, __) => Container(
                  padding: const EdgeInsets.all(32),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        Icons.broken_image_outlined,
                        color: colorScheme.onSurface.withOpacity(0.5),
                        size: 64,
                      ),
                      const SizedBox(height: 16),
                      Text(
                        'Не вдалося завантажити зображення',
                        style: theme.textTheme.bodyMedium?.copyWith(
                          color: colorScheme.onSurface.withOpacity(0.7),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
          Positioned(
            top: 16,
            right: 16,
            child: IconButton(
              onPressed: () => Navigator.pop(context),
              icon: const Icon(Icons.close, color: Colors.white),
              style: IconButton.styleFrom(
                backgroundColor: Colors.black54,
              ),
            ),
          ),
        ],
      ),
    );
  }
}