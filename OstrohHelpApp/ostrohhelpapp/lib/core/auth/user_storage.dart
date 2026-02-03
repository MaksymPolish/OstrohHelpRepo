import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';
import '../../features/auth/domain/entities/user.dart';

class UserStorage {
  static const _userKey = 'cached_user';
  static const _lastLoginKey = 'last_login_at';
  static const _sessionDurationHours = 2;

  Future<void> saveUser(User user) async {
    final prefs = await SharedPreferences.getInstance();
    final data = {
      'id': user.id,
      'email': user.email,
      'displayName': user.displayName,
      'fullName': user.fullName,
      'photoUrl': user.photoUrl,
      'roleId': user.roleId,
      'roleName': user.roleName,
      'course': user.course,
    };
    await prefs.setString(_userKey, json.encode(data));
    await prefs.setInt(_lastLoginKey, DateTime.now().millisecondsSinceEpoch);
  }

  Future<User?> getUser() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_userKey);
    if (raw == null) return null;
    final data = json.decode(raw) as Map<String, dynamic>;
    return User(
      id: data['id'] as String?,
      email: data['email'] as String?,
      displayName: data['displayName'] as String?,
      fullName: data['fullName'] as String?,
      photoUrl: data['photoUrl'] as String?,
      roleId: data['roleId'] as String?,
      roleName: data['roleName'] as String?,
      course: data['course'] as String?,
    );
  }

  Future<bool> isSessionExpired() async {
    final prefs = await SharedPreferences.getInstance();
    final lastLogin = prefs.getInt(_lastLoginKey);
    if (lastLogin == null) return true;
    final last = DateTime.fromMillisecondsSinceEpoch(lastLogin);
    final expiresAt = last.add(const Duration(hours: _sessionDurationHours));
    return DateTime.now().isAfter(expiresAt);
  }

  Future<void> clearUser() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_userKey);
    await prefs.remove(_lastLoginKey);
  }
}
