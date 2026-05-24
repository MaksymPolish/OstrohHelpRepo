import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
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

  final List<Map<String, dynamic>> _roles = [
    {
      "id": {"value": UserRole.studentId},
      "nameKey": 'admin.users.role.student'
    },
    {
      "id": {"value": UserRole.psychologistId},
      "nameKey": 'admin.users.role.psychologist'
    },
    {
      "id": {"value": UserRole.serviceManagerId},
      "nameKey": 'admin.users.role.manager'
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
          SnackBar(content: Text('admin.users.deleted'.tr())),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('admin.users.deleteError'.tr(args: [e.toString()]))),
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
          SnackBar(content: Text('admin.users.roleUpdated'.tr())),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('admin.users.roleUpdateError'.tr(args: [e.toString()]))),
        );
      }
    }
  }

  Future<void> _loadUsers() async {
    final users = await _apiService.getAllUsers();
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
                  title: Text('admin.users.deleteUser'.tr()),
                  onTap: () {
                    Navigator.pop(context);
                    showDialog(
                      context: context,
                      builder: (BuildContext context) {
                        return AlertDialog(
                          title: Text('admin.users.confirmTitle'.tr()),
                          content: Text('admin.users.confirmDelete'.tr()),
                          actions: [
                            TextButton(
                              onPressed: () => Navigator.pop(context),
                              child: Text('common.cancel'.tr()),
                            ),
                            TextButton(
                              onPressed: () {
                                Navigator.pop(context);
                                _deleteUser(user['id']);
                              },
                              child: Text('admin.users.delete'.tr(), style: const TextStyle(color: Colors.red)),
                            ),
                          ],
                        );
                      },
                    );
                  },
                ),
              if (!isSelf)
                ListTile(
                  leading: const Icon(Icons.admin_panel_settings),
                  title: Text('admin.users.changeRole'.tr()),
                  onTap: () {
                    Navigator.pop(context);
                    showDialog(
                      context: context,
                      builder: (BuildContext context) {
                        return AlertDialog(
                          title: Text('admin.users.chooseRole'.tr()),
                          content: SizedBox(
                            width: double.maxFinite,
                            child: ListView.builder(
                              shrinkWrap: true,
                              itemCount: _roles.length,
                              itemBuilder: (context, index) {
                                final role = _roles[index];
                                return ListTile(
                                  title: Text((role['nameKey'] as String).tr()),
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
                Padding(
                  padding: EdgeInsets.all(16.0),
                  child: Text(
                    'admin.users.selfActionBlocked'.tr(),
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
    return Scaffold(
      appBar: AppBar(title: Text('admin.users.title'.tr())),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'admin.users.searchHint'.tr(),
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
                ? Center(child: Text('admin.users.empty'.tr()))
                : ListView.builder(
                    itemCount: _filteredUsers.length,
                    itemBuilder: (context, index) {
                      final u = _filteredUsers[index];
                      return ListTile(
                        title: Text(u['fullName'] ?? u['email'] ?? 'common.unknown'.tr()),
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
