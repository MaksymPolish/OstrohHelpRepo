import 'package:flutter/material.dart';

class ChatPage extends StatelessWidget {
  final String consultationId;

  const ChatPage({
    super.key,
    required this.consultationId,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text('Чат'),
      ),
      body: const Center(
        child: Text('Чат буде доступний після початку консультації'),
      ),
    );
  }
} 