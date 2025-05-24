import 'package:flutter/material.dart';

class ConsultationPage extends StatelessWidget {
  const ConsultationPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Consultation'),
        centerTitle: true,
      ),
      body: const Center(
        child: Text('Consultation Page'),
      ),
    );
  }
} 