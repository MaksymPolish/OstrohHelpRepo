import 'package:flutter/material.dart';

class ConsultationPage extends StatelessWidget {
  const ConsultationPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Consultations'),
      ),
      body: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: 5, // Example consultations
        itemBuilder: (context, index) {
          return Card(
            margin: const EdgeInsets.only(bottom: 16),
            child: ListTile(
              contentPadding: const EdgeInsets.all(16),
              title: Text(
                'Consultation ${index + 1}',
                style: const TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 18,
                ),
              ),
              subtitle: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const SizedBox(height: 8),
                  Text(
                    'Date: ${DateTime.now().add(Duration(days: index)).toString().split(' ')[0]}',
                    style: const TextStyle(color: Colors.grey),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Time: ${9 + index}:00 AM',
                    style: const TextStyle(color: Colors.grey),
                  ),
                ],
              ),
              trailing: ElevatedButton(
                onPressed: () {
                  // TODO: Implement booking functionality
                },
                child: const Text('Book Now'),
              ),
            ),
          );
        },
      ),
    );
  }
} 