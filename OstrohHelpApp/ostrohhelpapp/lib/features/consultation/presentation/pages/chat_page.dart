import 'dart:io';

import 'package:file_selector/file_selector.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:image_picker/image_picker.dart';
import 'package:intl/intl.dart';
import 'package:ostrohhelpapp/features/message/data/services/message_api_service.dart';
import 'package:ostrohhelpapp/features/message/data/models/message.dart';
import 'package:ostrohhelpapp/features/auth/presentation/bloc/auth_bloc.dart';
import 'package:ostrohhelpapp/features/auth/presentation/bloc/auth_state.dart';
import 'package:ostrohhelpapp/features/consultation/data/services/consultation_api_service.dart';
import 'package:video_player/video_player.dart';

class ChatPage extends StatefulWidget {
  final String consultationId;
  const ChatPage({super.key, required this.consultationId});

  @override
  State<ChatPage> createState() => _ChatPageState();
}

class _ChatPageState extends State<ChatPage> {
  final MessageApiService _messageApiService = MessageApiService();
  final ConsultationApiService _consultationApiService = ConsultationApiService();
  final TextEditingController _controller = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  final ImagePicker _imagePicker = ImagePicker();
  late Future<List<Message>> _messagesFuture;
  late Future<Map<String, dynamic>> _consultationFuture;
  bool _isUploading = false;

  @override
  void initState() {
    super.initState();
    _messagesFuture = _loadMessages();
    _consultationFuture = _consultationApiService.getConsultationById(widget.consultationId);
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

  Future<String> _resolveReceiverId(String userId) async {
    final consultation = await _consultationFuture;
    final studentId = consultation['studentId']?.toString() ?? '';
    final psychologistId = consultation['psychologistId']?.toString() ?? '';
    if (userId == studentId) return psychologistId;
    return studentId;
  }

  Future<String?> _getLatestMessageIdForUser(String userId) async {
    final raw = await _messageApiService.getMessages(widget.consultationId);
    final messages = raw.map((json) => Message.fromJson(json)).toList();
    final mine = messages.where((msg) => msg.senderId == userId).toList();
    if (mine.isEmpty) return null;
    mine.sort((a, b) => a.sentAt.compareTo(b.sentAt));
    return mine.last.id;
  }

  Future<String?> _sendMessage(
    String userId, {
    String? text,
    List<String>? mediaPaths,
  }) async {
    final content = (text ?? _controller.text).trim();
    final attachments = mediaPaths ?? <String>[];
    if (content.isEmpty && attachments.isEmpty) return null;
    final textPayload = content.isEmpty && attachments.isNotEmpty ? 'Attachment' : content;

    try {
      final receiverId = await _resolveReceiverId(userId);
      final created = await _messageApiService.sendMessage({
        'consultationId': widget.consultationId,
        'senderId': userId,
        'receiverId': receiverId,
        'text': textPayload,
        'content': content,
        'mediaPaths': attachments,
      });
      final messageId = created['id']?.toString();

      // --- Очищення текстового поля ---
      if (text == null || text == _controller.text) {
        _controller.clear();
      }

      // --- Оновлення списку повідомлень ---
      setState(() {
        _messagesFuture = _loadMessages();
      });
      return messageId;
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Не вдалося відправити повідомлення: $e')),
      );
    }
    return null;
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
      final messageId = await _getLatestMessageIdForUser(userId);
      if (messageId == null || messageId.isEmpty) {
        throw Exception('Message id is missing for attachment');
      }
      await _messageApiService.addAttachment(
        messageId: messageId,
        fileUrl: url,
        fileType: fileType,
      );
      setState(() {
        _messagesFuture = _loadMessages();
      });
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
      return ClipRRect(
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
                        onPressed: _isUploading ? null : () => _sendMessage(userId),
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

  void _showVideoPreview(String url) {
    showDialog(
      context: context,
      builder: (context) => _VideoPreviewDialog(url: url),
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