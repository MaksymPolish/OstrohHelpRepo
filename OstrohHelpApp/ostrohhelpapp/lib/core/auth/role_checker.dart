
class UserRole {
  static const String studentId = '00000000-0000-0000-0000-000000000001';

  static const String psychologistId = '00000000-0000-0000-0000-000000000002';

  static const String serviceManagerId = '00000000-0000-0000-0000-000000000003';

  static const String adminId = serviceManagerId;

  static const Map<String, String> roleNames = {
    studentId: 'Student',
    psychologistId: 'Psychologist',
    serviceManagerId: 'HeadOfService',
  };
}

class RoleChecker {
  static bool isAdmin(String? roleId) {
    return roleId == UserRole.serviceManagerId;
  }

  static bool isPsychologist(String? roleId) {
    return roleId == UserRole.psychologistId;
  }

  static bool isStudent(String? roleId) {
    return roleId == UserRole.studentId;
  }

  static bool isAdminOrPsychologist(String? roleId) {
    return isAdmin(roleId) || isPsychologist(roleId);
  }

  static bool isAdminOrPsychologistByName(String? roleName) {
    if (roleName == null) return false;
    final normalized = roleName.trim().toLowerCase();
    return normalized.contains('psychologist') || normalized.contains('headofservice');
  }

  static String getRoleName(String? roleId) {
    if (roleId == null) return 'Невідома роль';
    return UserRole.roleNames[roleId] ?? 'Невідома роль';
  }

  static bool hasRole(String? userRoleId, String requiredRoleId) {
    return userRoleId == requiredRoleId;
  }

  static bool hasAnyRole(String? userRoleId, List<String> allowedRoleIds) {
    return userRoleId != null && allowedRoleIds.contains(userRoleId);
  }
}

