class Message {
  final String id;
  final String consultationId;
  final String senderId;
  final String text;
  final DateTime sentAt;
  final bool isRead;

  Message({
    required this.id,
    required this.consultationId,
    required this.senderId,
    required this.text,
    required this.sentAt,
    required this.isRead,
  });

  factory Message.fromJson(Map<String, dynamic> json) {
    return Message(
      id: json['id'] is Map ? json['id']['value'] ?? '' : (json['id'] ?? ''),
      consultationId: json['consultationId'] ?? '',
      senderId: json['senderId'] ?? '',
      text: json['text'] ?? '',
      sentAt: DateTime.parse(json['sentAt']),
      isRead: json['isRead'] ?? false,
    );
  }
} 