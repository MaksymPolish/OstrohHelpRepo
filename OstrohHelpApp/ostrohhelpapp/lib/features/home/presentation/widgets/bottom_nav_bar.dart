import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import '../../../consultation/presentation/pages/consultation_list_page.dart';
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
    final colorScheme = Theme.of(context).colorScheme;

    return BottomNavigationBar(
      currentIndex: currentIndex,
      type: BottomNavigationBarType.fixed,
      selectedItemColor: colorScheme.primary,
      unselectedItemColor: colorScheme.onSurface.withOpacity(0.6),
      items: [
        BottomNavigationBarItem(
          icon: Icon(Icons.home),
          label: 'nav.home'.tr(),
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.assignment),
          label: 'nav.questionnaires'.tr(),
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.people),
          label: 'nav.consultations'.tr(),
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.person),
          label: 'nav.profile'.tr(),
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
            page = const ConsultationListPage();
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
