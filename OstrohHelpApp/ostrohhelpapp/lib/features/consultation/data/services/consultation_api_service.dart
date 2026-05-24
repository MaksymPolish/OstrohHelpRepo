import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../core/config/app_config.dart';
import '../../../../core/auth/token_storage.dart';

class ConsultationApiService {
  String get baseUrl => AppConfig.apiBaseUrl;
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

  Future<List<Map<String, dynamic>>> getAllConsultations() async {
    final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/Consultations/all'),
      headers: headers,
    );
    if (response.statusCode == 204) {
      return [];
    }
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      return data.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to load consultations: ${response.statusCode} - ${response.body}');
  }

  Future<Map<String, dynamic>> getConsultationById(String id) async {
    final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/Consultations/Get-Consultation-ById/$id'),
      headers: headers,
    );
    if (response.statusCode == 200) {
      return json.decode(response.body);
    }
    throw Exception('Failed to load Consultation');
  }

  Future<List<Map<String, dynamic>>> getConsultationsByUserId(String userId) async {
    final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/Consultations/Get-All-Consultations-By-UserId/$userId'),
      headers: headers,
    );
    if (response.statusCode == 204) {
      return [];
    }
    if (response.statusCode == 404) {
      return [];
    }
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      return data.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to load user consultations: ${response.statusCode} - ${response.body}');
  }

  Future<void> acceptConsultation(Map<String, dynamic> consultation) async {
    final headers = await _getHeaders();
    final response = await http.post(
      Uri.parse('$baseUrl/Consultations/Accept-Questionnaire'),
      headers: headers,
      body: json.encode(consultation),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to accept consultation: ${response.statusCode} - ${response.body}');
    }
  }

  Future<void> updateConsultation(String id, Map<String, dynamic> consultation) async {
    final headers = await _getHeaders();
    final response = await http.put(
      Uri.parse('$baseUrl/Consultations/Update-Consultation'),
      headers: headers,
      body: json.encode(consultation),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to update consultation: ${response.statusCode} - ${response.body}');
    }
  }

  Future<void> deleteConsultation(String id) async {
    final headers = await _getHeaders();
    final response = await http.delete(
      Uri.parse('$baseUrl/Consultations/Delete-Consultation'),
      headers: headers,
      body: json.encode({'consultationId': id}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Failed to delete consultation: ${response.statusCode} - ${response.body}');
    }
  }

  
} 
