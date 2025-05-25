import 'dart:convert';
import 'package:http/http.dart' as http;

class AuthApiService {
  final String baseUrl = 'http://10.0.2.2:5132/api';

  Future<Map<String, dynamic>> googleLogin(String idToken) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/google-login'),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: json.encode({
        'idToken': idToken.toString(),
      }),
    );

    if (response.statusCode == 200) {
      return json.decode(response.body);
    }
    throw Exception('Failed to authenticate with Google: ${response.body}');
  }

  Future<Map<String, dynamic>> getUserById(String id) async {
    final response = await http.get(
      Uri.parse('$baseUrl/auth/$id'),
      headers: {
        'Accept': 'application/json',
      },
    );
    if (response.statusCode == 200) {
      return json.decode(response.body);
    }
    throw Exception('Failed to get user data: ${response.body}');
  }

  

  Future<List<Map<String, dynamic>>> getAllUsers() async {
    final response = await http.get(
      Uri.parse('$baseUrl/auth/all'),
      headers: {
        'Accept': 'application/json',
      },
    );
    if (response.statusCode == 200) {
      final Map<String, dynamic> data = json.decode(response.body);
      final List<dynamic> list = data['items'];
      return list.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to get users: ${response.body}');
  }

  Future<void> deleteUser(String id) async {
    final response = await http.delete(
      Uri.parse('$baseUrl/auth/User-Delete'),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: json.encode(id),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to delete user: ${response.body}');
    }
  }

  Future<void> updateUserCourse(String userId, String course) async {
    final response = await http.put(
      Uri.parse('$baseUrl/auth/User-course'),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: json.encode({
        'userId': userId,
        'course': course,
      }),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to update user course: ${response.body}');
    }
  }

  Future<void> updateUserRole(String userId, String roleId) async {
    final response = await http.put(
      Uri.parse('$baseUrl/auth/User-Role-Update'),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: json.encode({
        'userId': userId,
        'roleId': roleId,
      }),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to update user role: ${response.body}');
    }
  }

  Future<Map<String, dynamic>> getUserByEmail(String email) async {
    final response = await http.get(
      Uri.parse('$baseUrl/auth/get-by-email?email=$email'),
      headers: {
        'Accept': 'application/json',
      },
    );
    if (response.statusCode == 200) {
      return json.decode(response.body);
    }
    throw Exception('Failed to get user by email: ${response.body}');
  }
} 