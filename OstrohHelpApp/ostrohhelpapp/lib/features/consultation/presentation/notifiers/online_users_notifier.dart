import 'package:flutter/foundation.dart';

class OnlineUsersNotifier extends ChangeNotifier {
  OnlineUsersNotifier._();
  static final OnlineUsersNotifier instance = OnlineUsersNotifier._();

  final Set<String> _onlineUserIds = <String>{};

  Set<String> get onlineUserIds => Set<String>.unmodifiable(_onlineUserIds);

  bool isOnline(String userId) => _onlineUserIds.contains(userId);

  void setUserStatus({
    required String userId,
    required bool isOnline,
  }) {
    if (userId.isEmpty) return;

    final hasUser = _onlineUserIds.contains(userId);
    if (isOnline && !hasUser) {
      _onlineUserIds.add(userId);
      notifyListeners();
      return;
    }

    if (!isOnline && hasUser) {
      _onlineUserIds.remove(userId);
      notifyListeners();
    }
  }

  void clear() {
    if (_onlineUserIds.isEmpty) return;
    _onlineUserIds.clear();
    notifyListeners();
  }
}
