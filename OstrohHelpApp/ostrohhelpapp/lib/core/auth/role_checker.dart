/// Утиліти для перевірки ролей користувача
/// 
/// Всі ролі та перевірки зосереджені в одному місці для зручності
/// управління та редагування roleId

class UserRole {
  /// ID ролі Студента
  static const String studentId = '00000000-0000-0000-0000-000000000001';

  /// ID ролі Психолога
  static const String psychologistId = '00000000-0000-0000-0000-000000000002';

  /// ID ролі Керівника психологічної служби
  static const String serviceManagerId = '00000000-0000-0000-0000-000000000003';

  /// Альтернативна назва для сумісності
  static const String adminId = serviceManagerId;

  /// Назви ролей для відображення
  static const Map<String, String> roleNames = {
    studentId: 'Student',
    psychologistId: 'Psychologist',
    serviceManagerId: 'HeadOfService',
  };
}

/// Функції для перевірки ролей
class RoleChecker {
  /// Перевірити чи користувач є адміністратором (керівником служби)
  static bool isAdmin(String? roleId) {
    return roleId == UserRole.serviceManagerId;
  }

  /// Перевірити чи користувач є психологом
  static bool isPsychologist(String? roleId) {
    return roleId == UserRole.psychologistId;
  }

  /// Перевірити чи користувач є студентом
  static bool isStudent(String? roleId) {
    return roleId == UserRole.studentId;
  }

  /// Перевірити чи користувач є адміністратором (керівником служби) ІЛИ психологом
  /// (Ці ролі мають повним доступ до адміністративних функцій)
  static bool isAdminOrPsychologist(String? roleId) {
    return isAdmin(roleId) || isPsychologist(roleId);
  }

  /// Перевірити роль за назвою (fallback, якщо roleId не заповнений)
  static bool isAdminOrPsychologistByName(String? roleName) {
    if (roleName == null) return false;
    final normalized = roleName.trim().toLowerCase();
    return normalized.contains('psychologist') || normalized.contains('headofservice');
  }

  /// Отримати назву ролі за ID
  static String getRoleName(String? roleId) {
    if (roleId == null) return 'Невідома роль';
    return UserRole.roleNames[roleId] ?? 'Невідома роль';
  }

  /// Перевірити чи користувач має певну роль
  static bool hasRole(String? userRoleId, String requiredRoleId) {
    return userRoleId == requiredRoleId;
  }

  /// Перевірити чи користувач має одну з кількох ролей
  static bool hasAnyRole(String? userRoleId, List<String> allowedRoleIds) {
    return userRoleId != null && allowedRoleIds.contains(userRoleId);
  }
}
