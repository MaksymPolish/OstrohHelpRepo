import 'package:flutter/material.dart';
import '../../../auth/data/services/auth_api_service.dart';
import '../../../../core/auth/role_checker.dart';

class AdminUsersPage extends StatefulWidget {
  final String currentUserId;
  const AdminUsersPage({super.key, required this.currentUserId});

  @override
  State<AdminUsersPage> createState() => _AdminUsersPageState();
}

class _AdminUsersPageState extends State<AdminUsersPage> {
  final _apiService = AuthApiService();
  final _searchController = TextEditingController();
  List<Map<String, dynamic>> _allUsers = [];
  List<Map<String, dynamic>> _filteredUsers = [];

  String? get currentUserId => widget.currentUserId;

  // 🔐 ПЕРЕВІРКА РОЛІ: Список доступних ролей для зміни (посилаються на UserRole константи)
  final List<Map<String, dynamic>> _roles = [
    {
      "id": {"value": UserRole.studentId},
      "name": "Студент"
    },
    {
      "id": {"value": UserRole.psychologistId},
      "name": "Психолог"
    },
    {
      "id": {"value": UserRole.serviceManagerId},
      "name": "Керівник психологічної служби"
    }
  ];

  void _filterUsers(String query) {
    setState(() {
      _filteredUsers = _allUsers.where((user) {
        final displayName = (user['fullName'] ?? '').toString().toLowerCase();
        final email = (user['email'] ?? '').toString().toLowerCase();
        final searchLower = query.toLowerCase();
        return displayName.contains(searchLower) || email.contains(searchLower);
      }).toList();
    });
  }

  Future<void> _deleteUser(String userId) async {
    try {
      await _apiService.deleteUser(userId);
      await _loadUsers(); // Reload the list after deletion
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Користувача видалено')),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Помилка при видаленні: $e')),
        );
      }
    }
  }

  Future<void> _updateUserRole(String userId, String roleId) async {
    try {
      await _apiService.updateUserRole(userId, roleId);
      await _loadUsers(); // Reload the list after role update
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Роль користувача оновлено')),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Помилка при оновленні ролі: $e')),
        );
      }
    }
  }

  Future<void> _loadUsers() async {
    final users = await _apiService.getAllUsers();
    print('USERS FROM API: $users');
    setState(() {
      _allUsers = users;
      _filteredUsers = users;
    });
  }

  void _showUserOptions(Map<String, dynamic> user) {
    final isSelf = currentUserId != null && user['id'] == currentUserId;
    showModalBottomSheet(
      context: context,
      builder: (BuildContext context) {
        return SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              if (!isSelf)
                ListTile(
                  leading: const Icon(Icons.delete, color: Colors.red),
                  title: const Text('Видалити користувача'),
                  onTap: () {
                    Navigator.pop(context);
                    showDialog(
                      context: context,
                      builder: (BuildContext context) {
                        return AlertDialog(
                          title: const Text('Підтвердження'),
                          content: const Text('Ви впевнені, що хочете видалити цього користувача?'),
                          actions: [
                            TextButton(
                              onPressed: () => Navigator.pop(context),
                              child: const Text('Скасувати'),
                            ),
                            TextButton(
                              onPressed: () {
                                Navigator.pop(context);
                                _deleteUser(user['id']);
                              },
                              child: const Text('Видалити', style: TextStyle(color: Colors.red)),
                            ),
                          ],
                        );
                      },
                    );
                  },
                ),
              // 🔐 ПЕРЕВІРКА РОЛІ: Дозволити змінювати роль тільки іншим користувачам, не собі
              if (!isSelf)
                ListTile(
                  leading: const Icon(Icons.admin_panel_settings),
                  title: const Text('Змінити роль'),
                  onTap: () {
                    Navigator.pop(context);
                    showDialog(
                      context: context,
                      builder: (BuildContext context) {
                        return AlertDialog(
                          title: const Text('Виберіть роль'),
                          content: SizedBox(
                            width: double.maxFinite,
                            child: ListView.builder(
                              shrinkWrap: true,
                              itemCount: _roles.length,
                              itemBuilder: (context, index) {
                                final role = _roles[index];
                                return ListTile(
                                  title: Text(role['name']),
                                  onTap: () {
                                    Navigator.pop(context);
                                    _updateUserRole(user['id'], role['id']['value']);
                                  },
                                );
                              },
                            ),
                          ),
                        );
                      },
                    );
                  },
                ),
              if (isSelf)
                const Padding(
                  padding: EdgeInsets.all(16.0),
                  child: Text(
                    'Ви не можете змінити роль або видалити самого себе.',
                    style: TextStyle(color: Colors.grey),
                  ),
                ),
            ],
          ),
        );
      },
    );
  }

  @override
  void initState() {
    super.initState();
    _loadUsers();
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    print('ALL USERS: $_allUsers');
    print('FILTERED USERS: $_filteredUsers');
    return Scaffold(
      appBar: AppBar(title: const Text('Всі користувачі')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Пошук по імені або email',
                prefixIcon: const Icon(Icons.search),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
                contentPadding: const EdgeInsets.symmetric(vertical: 0),
              ),
              onChanged: _filterUsers,
            ),
          ),
          Expanded(
            child: _filteredUsers.isEmpty
                ? const Center(child: Text('Користувачів не знайдено'))
                : ListView.builder(
                    itemCount: _filteredUsers.length,
                    itemBuilder: (context, index) {
                      final u = _filteredUsers[index];
                      return ListTile(
                        title: Text(u['fullName'] ?? u['email'] ?? 'No name'),
                        subtitle: Text(u['email'] ?? ''),
                        trailing: Text(u['roleName'] ?? ''),
                        onLongPress: () => _showUserOptions(u),
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }
}