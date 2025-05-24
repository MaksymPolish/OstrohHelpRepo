import 'package:flutter/material.dart';

class QuestionaryPage extends StatelessWidget {
  const QuestionaryPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Questionary'),
        centerTitle: true,
      ),
      body: const Center(
        child: Text('Questionary Page'),
      ),
    );
  }
} 