class Message {
  final String id;
  final String consultationId;
  final String senderId;
  final String text;
  final DateTime sentAt;
  final bool isRead;
  final List<String> mediaPaths;

  Message({
    required this.id,
    required this.consultationId,
    required this.senderId,
    required this.text,
    required this.sentAt,
    required this.isRead,
    required this.mediaPaths,
  });

  factory Message.fromJson(Map<String, dynamic> json) {
    // Parse attachments array to extract fileUrl values
    final rawAttachments = json['attachments'];
    final attachmentUrls = rawAttachments is List
        ? rawAttachments
            .map((item) => item is Map<String, dynamic> ? item['fileUrl']?.toString() ?? '' : '')
            .where((url) => url.isNotEmpty)
            .toList()
        : <String>[];
    
    // Fallback to mediaPaths if attachments is empty
    final rawMedia = json['mediaPaths'];
    final mediaUrls = rawMedia is List
        ? rawMedia.map((item) => item.toString()).toList()
        : <String>[];
    
    final mediaPaths = attachmentUrls.isNotEmpty ? attachmentUrls : mediaUrls;
    final rawText = json['text'] ?? json['content'] ?? '';
    final rawSentAt = json['sentAt'] ?? json['createdAt'] ?? DateTime.now().toIso8601String();

    return Message(
      id: json['id'] is Map ? json['id']['value'] ?? '' : (json['id'] ?? ''),
      consultationId: json['consultationId'] ?? '',
      senderId: json['senderId'] ?? '',
      text: rawText,
      sentAt: DateTime.tryParse(rawSentAt) ?? DateTime.now(),
      isRead: json['isRead'] ?? false,
      mediaPaths: mediaPaths,
    );
  }
} 