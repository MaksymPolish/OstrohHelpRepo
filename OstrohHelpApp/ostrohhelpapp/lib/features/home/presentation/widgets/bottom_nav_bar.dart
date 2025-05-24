import 'package:flutter/material.dart';
import '../../../consultation/presentation/pages/consultation_page.dart';
import '../../../profile/presentation/pages/profile_page.dart';
import '../../../questionnaire/presentation/pages/questionnaires_list_page.dart';
import '../pages/home_page.dart';

class CustomBottomNavBar extends StatelessWidget {
  final int currentIndex;

  const CustomBottomNavBar({
    super.key,
    required this.currentIndex,
  });

  @override
  Widget build(BuildContext context) {
    return BottomNavigationBar(
      currentIndex: currentIndex,
      type: BottomNavigationBarType.fixed,
      selectedItemColor: Theme.of(context).primaryColor,
      unselectedItemColor: Colors.grey,
      items: const [
        BottomNavigationBarItem(
          icon: Icon(Icons.home),
          label: 'Home',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.assignment),
          label: 'Questionnaire',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.people),
          label: 'Consultation',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.person),
          label: 'Profile',
        ),
      ],
      onTap: (index) {
        Widget page;
        switch (index) {
          case 0:
            page = const HomePage();
            break;
          case 1:
            page = const QuestionnairesListPage();
            break;
          case 2:
            page = const ConsultationPage();
            break;
          case 3:
            page = const ProfilePage();
            break;
          default:
            page = const HomePage();
        }
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (context) => page),
        );
      },
    );
  }
} 