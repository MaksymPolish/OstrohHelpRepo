import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../../../../core/auth/token_storage.dart';

class AuthApiService {
  final String baseUrl = 'http://10.0.2.2:5000/api';
  final TokenStorage _tokenStorage = TokenStorage();

  Future<Map<String, String>> _getHeaders() async {
    final token = await _tokenStorage.getToken();
    final headers = <String, String>{
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };
    if (token != null) {
      headers['Authorization'] = 'Bearer $token';
    }
    return headers;
  }

  Future<Map<String, dynamic>> googleLogin(String idToken) async {
    try {
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
        final data = json.decode(response.body);
        
        // Save JWT and refresh tokens with expiration
        if (data['jwtToken'] != null) {
          await _tokenStorage.saveToken(
            data['jwtToken'],
            expiresAt: data['expiresAt'],
          );
        }
        if (data['refreshToken'] != null) {
          await _tokenStorage.saveRefreshToken(data['refreshToken']);
        }
        return data;
      }
      throw Exception('Failed to authenticate with Google: Status ${response.statusCode} - ${response.body}');
    } catch (e) {
      debugPrint('Google Login Error: $e');
      rethrow;
    }
  }

  Future<Map<String, dynamic>> getUserById(String id) async {
      final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/auth/$id'),
      headers: headers,
    );
    if (response.statusCode == 200) {
      return json.decode(response.body);
    }
    throw Exception('Failed to get user data: ${response.body}');
  }

  

  Future<List<Map<String, dynamic>>> getAllUsers() async {
      final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/auth/all'),
      headers: headers,
    );
    if (response.statusCode == 200) {
      final List<dynamic> list = json.decode(response.body);
      return list.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to get users: ${response.body}');
  }

  Future<void> deleteUser(String id) async {
      final headers = await _getHeaders();
    final response = await http.delete(
      Uri.parse('$baseUrl/auth/User-Delete'),
      headers: headers,
      body: json.encode(id),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      if (response.body.isEmpty) return;
      throw Exception('Failed to delete user: ${response.body}');
    }
  }

  Future<void> updateUserCourse(String userId, String course) async {
      final headers = await _getHeaders();
    final response = await http.put(
      Uri.parse('$baseUrl/auth/User-course'),
      headers: headers,
      body: json.encode({
        'userId': userId,
        'course': course,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to update user course: ${response.body}');
    }
  }

  Future<void> updateUserRole(String userId, String roleId) async {
      final headers = await _getHeaders();
    final response = await http.put(
      Uri.parse('$baseUrl/auth/User-Role-Update'),
      headers: headers,
      body: json.encode({
        'userId': userId,
        'roleId': roleId,
      }),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      if (response.body.isEmpty) return;
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