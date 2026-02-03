import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../core/auth/token_storage.dart';

class ConsultationApiService {
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

  Future<List<Map<String, dynamic>>> getAllConsultations() async {
    final headers = await _getHeaders();
    final response = await http.get(
      Uri.parse('$baseUrl/Consultations/all'),
      headers: headers,
    );
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      return data.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to load consultations');
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
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      print('Consultation for user $userId: $data');
      return data.cast<Map<String, dynamic>>();
    }
    throw Exception('Failed to load user questionnaires');
  }

  Future<Map<String, dynamic>> acceptConsultation(Map<String, dynamic> consultation) async {
      final headers = await _getHeaders();
    final response = await http.post(
      Uri.parse('$baseUrl/Consultations/Accept-Questionnaire'),
      headers: headers,
      body: json.encode(consultation),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      return json.decode(response.body);
    }
    throw Exception('Failed to accept consultation');
  }

  Future<void> updateConsultation(String id, Map<String, dynamic> consultation) async {
      final headers = await _getHeaders();
    final response = await http.put(
      Uri.parse('$baseUrl/Consultations/Update-Consultation'),
      headers: headers,
      body: json.encode(consultation),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to update questionnaire');
    }
  }

  Future<void> deleteConsultation(String id) async {
      final headers = await _getHeaders();
    final response = await http.delete(
      Uri.parse('$baseUrl/Consultations/Delete-Consultation'),
      headers: headers,
      body: json.encode({'id': id}),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to delete consultation');
    }
  }

  
} 