/// Модель вкладення повідомлення з превʼю URL
class MessageAttachment {
  final String id;
  final String fileName;
  final String fileUrl;
  final String fileType;
  final int fileSizeBytes;
  final DateTime createdAt;
  final String? thumbnailUrl;
  final String? mediumPreviewUrl;
  final String? videoPosterUrl;
  final String? pdfPagePreviewUrl;
  final bool isDeleted;

  MessageAttachment({
    required this.id,
    required this.fileName,
    required this.fileUrl,
    required this.fileType,
    required this.fileSizeBytes,
    required this.createdAt,
    this.thumbnailUrl,
    this.mediumPreviewUrl,
    this.videoPosterUrl,
    this.pdfPagePreviewUrl,
    this.isDeleted = false,
  });

  factory MessageAttachment.fromJson(Map<String, dynamic> json) {
    return MessageAttachment(
      id: json['id'] ?? '',
      fileName: json['fileName'] ?? '',
      fileUrl: json['fileUrl'] ?? '',
      fileType: json['fileType'] ?? '',
      fileSizeBytes: json['fileSizeBytes'] ?? 0,
      createdAt: DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now(),
      thumbnailUrl: json['thumbnailUrl'],
      mediumPreviewUrl: json['mediumPreviewUrl'],
      videoPosterUrl: json['videoPosterUrl'],
      pdfPagePreviewUrl: json['pdfPagePreviewUrl'],
      isDeleted: json['isDeleted'] ?? false,
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'fileName': fileName,
        'fileUrl': fileUrl,
        'fileType': fileType,
        'fileSizeBytes': fileSizeBytes,
        'createdAt': createdAt.toIso8601String(),
        'thumbnailUrl': thumbnailUrl,
        'mediumPreviewUrl': mediumPreviewUrl,
        'videoPosterUrl': videoPosterUrl,
        'pdfPagePreviewUrl': pdfPagePreviewUrl,
        'isDeleted': isDeleted,
      };

  MessageAttachment copyWith({
    String? id,
    String? fileName,
    String? fileUrl,
    String? fileType,
    int? fileSizeBytes,
    DateTime? createdAt,
    String? thumbnailUrl,
    String? mediumPreviewUrl,
    String? videoPosterUrl,
    String? pdfPagePreviewUrl,
    bool? isDeleted,
  }) {
    return MessageAttachment(
      id: id ?? this.id,
      fileName: fileName ?? this.fileName,
      fileUrl: fileUrl ?? this.fileUrl,
      fileType: fileType ?? this.fileType,
      fileSizeBytes: fileSizeBytes ?? this.fileSizeBytes,
      createdAt: createdAt ?? this.createdAt,
      thumbnailUrl: thumbnailUrl ?? this.thumbnailUrl,
      mediumPreviewUrl: mediumPreviewUrl ?? this.mediumPreviewUrl,
      videoPosterUrl: videoPosterUrl ?? this.videoPosterUrl,
      pdfPagePreviewUrl: pdfPagePreviewUrl ?? this.pdfPagePreviewUrl,
      isDeleted: isDeleted ?? this.isDeleted,
    );
  }
}

/// Модель повідомлення з підтримкою шифрування
class Message {
  final String id;
  final String consultationId;
  final String senderId;
  final String senderName;
  final String? senderPhotoUrl;
  final String receiverId;
  final String receiverName;
  final String? receiverPhotoUrl;

  /// Зашифровані дані (Base64) - замість text
  final String? encryptedContent;
  final String? iv;
  final String? authTag;

  /// Старе поле (для сумісності) - може залишатися null для новых messages
  @Deprecated('Use encryptedContent instead')
  final String? text;

  final DateTime sentAt;
  final bool isRead;
  final bool isDeleted;
  final List<MessageAttachment> attachments;

  Message({
    required this.id,
    required this.consultationId,
    required this.senderId,
    required this.senderName,
    this.senderPhotoUrl,
    required this.receiverId,
    required this.receiverName,
    this.receiverPhotoUrl,
    this.encryptedContent,
    this.iv,
    this.authTag,
    this.text,
    required this.sentAt,
    required this.isRead,
    this.isDeleted = false,
    this.attachments = const [],
  });

  Message copyWith({
    String? id,
    String? consultationId,
    String? senderId,
    String? senderName,
    String? senderPhotoUrl,
    String? receiverId,
    String? receiverName,
    String? receiverPhotoUrl,
    String? encryptedContent,
    String? iv,
    String? authTag,
    String? text,
    DateTime? sentAt,
    bool? isRead,
    bool? isDeleted,
    List<MessageAttachment>? attachments,
  }) {
    return Message(
      id: id ?? this.id,
      consultationId: consultationId ?? this.consultationId,
      senderId: senderId ?? this.senderId,
      senderName: senderName ?? this.senderName,
      senderPhotoUrl: senderPhotoUrl ?? this.senderPhotoUrl,
      receiverId: receiverId ?? this.receiverId,
      receiverName: receiverName ?? this.receiverName,
      receiverPhotoUrl: receiverPhotoUrl ?? this.receiverPhotoUrl,
      encryptedContent: encryptedContent ?? this.encryptedContent,
      iv: iv ?? this.iv,
      authTag: authTag ?? this.authTag,
      text: text ?? this.text,
      sentAt: sentAt ?? this.sentAt,
      isRead: isRead ?? this.isRead,
      isDeleted: isDeleted ?? this.isDeleted,
      attachments: attachments ?? this.attachments,
    );
  }

  factory Message.fromJson(Map<String, dynamic> json) {
    // Parse attachments
    final rawAttachments = json['attachments'];
    final attachments = rawAttachments is List
        ? rawAttachments
            .map((item) => MessageAttachment.fromJson(item as Map<String, dynamic>))
            .toList()
        : <MessageAttachment>[];

    // Fallback to mediaPaths for legacy compatibility
    final rawMedia = json['mediaPaths'];
    if (rawMedia is List && attachments.isEmpty) {
      for (var mediaUrl in rawMedia) {
        attachments.add(
          MessageAttachment(
            id: '',
            fileName: mediaUrl.toString().split('/').last,
            fileUrl: mediaUrl.toString(),
            fileType: _getFileType(mediaUrl.toString()),
            fileSizeBytes: 0,
            createdAt: DateTime.now(),
          ),
        );
      }
    }

    final rawSentAt = json['sentAt'] ?? json['createdAt'] ?? DateTime.now().toIso8601String();

    return Message(
      id: json['id'] is Map ? json['id']['value'] ?? '' : (json['id'] ?? ''),
      consultationId: json['consultationId'] ?? '',
      senderId: json['senderId'] ?? '',
      senderName: json['senderName'] ?? 'Unknown',
      senderPhotoUrl: json['senderPhotoUrl'],
      receiverId: json['receiverId'] ?? '',
      receiverName: json['receiverName'] ?? 'Unknown',
      receiverPhotoUrl: json['receiverPhotoUrl'],
      // Нові поля для шифрування
      encryptedContent: json['encryptedContent'],
      iv: json['iv'],
      authTag: json['authTag'],
      // Старе поле для сумісності
      text: json['text'],
      sentAt: DateTime.tryParse(rawSentAt) ?? DateTime.now(),
      isRead: json['isRead'] ?? false,
      isDeleted: json['isDeleted'] ?? false,
      attachments: attachments,
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'consultationId': consultationId,
        'senderId': senderId,
        'senderName': senderName,
        'senderPhotoUrl': senderPhotoUrl,
        'receiverId': receiverId,
        'receiverName': receiverName,
        'receiverPhotoUrl': receiverPhotoUrl,
        'encryptedContent': encryptedContent,
        'iv': iv,
        'authTag': authTag,
        'text': text,
        'sentAt': sentAt.toIso8601String(),
        'isRead': isRead,
        'isDeleted': isDeleted,
        'attachments': attachments.map((a) => a.toJson()).toList(),
      };

  /// Отримує тип файлу з URL
  static String _getFileType(String fileUrl) {
    try {
      final uri = Uri.parse(fileUrl);
      final path = uri.path;
      final extension = path.split('.').last.toLowerCase();
      return extension;
    } catch (e) {
      return 'unknown';
    }
  }

  /// Отримує мედіа шляхи для відображення
  /// (підтримує як новий attachments так і старий mediaPaths формат)
  List<String> getMediaPaths() {
    if (attachments.isNotEmpty) {
      return attachments.map((a) => a.fileUrl).toList();
    }
    return [];
  }
} 