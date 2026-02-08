import 'dart:convert';
import 'package:http/http.dart' as http;
import '../auth/token_storage.dart';

class ApiClient {
  final String baseUrl;
  final http.Client _httpClient;
  final TokenStorage _tokenStorage;

  ApiClient({
    required this.baseUrl,
    http.Client? httpClient,
    TokenStorage? tokenStorage,
  }) : _httpClient = httpClient ?? http.Client(),
       _tokenStorage = tokenStorage ?? TokenStorage();

  Future<Map<String, String>> _getHeaders({Map<String, String>? additionalHeaders}) async {
    final token = await _tokenStorage.getToken();
    final headers = <String, String>{
      'Content-Type': 'application/json',
      ...?additionalHeaders,
    };
    if (token != null) {
      headers['Authorization'] = 'Bearer $token';
    }
    return headers;
  }

  Future<Map<String, dynamic>> post(
    String endpoint,
    Map<String, dynamic> body, {
    Map<String, String>? headers,
  }) async {
      final requestHeaders = await _getHeaders(additionalHeaders: headers);
    final response = await _httpClient.post(
      Uri.parse('$baseUrl$endpoint'),
      headers: requestHeaders,
      body: json.encode(body),
    );

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return json.decode(response.body) as Map<String, dynamic>;
    } else {
      throw ApiException(
        statusCode: response.statusCode,
        message: response.body,
      );
    }
  }

  Future<Map<String, dynamic>> get(
    String endpoint, {
    Map<String, String>? headers,
  }) async {
    final requestHeaders = await _getHeaders(additionalHeaders: headers);
    final response = await _httpClient.get(
      Uri.parse('$baseUrl$endpoint'),
      headers: requestHeaders,
    );

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return json.decode(response.body) as Map<String, dynamic>;
    } else {
      throw ApiException(
        statusCode: response.statusCode,
        message: response.body,
      );
    }
  }

  Future<void> delete(
    String endpoint, {
    Map<String, String>? headers,
    Object? body,
  }) async {
    final requestHeaders = await _getHeaders(additionalHeaders: headers);
    final response = await _httpClient.delete(
      Uri.parse('$baseUrl$endpoint'),
      headers: requestHeaders,
      body: body != null ? json.encode(body) : null,
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw ApiException(
        statusCode: response.statusCode,
        message: response.body,
      );
    }
  }

  Future<void> put(
    String endpoint,
    Map<String, dynamic> body, {
    Map<String, String>? headers,
  }) async {
    final requestHeaders = await _getHeaders(additionalHeaders: headers);
    final response = await _httpClient.put(
      Uri.parse('$baseUrl$endpoint'),
      headers: requestHeaders,
      body: json.encode(body),
    );

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw ApiException(
        statusCode: response.statusCode,
        message: response.body,
      );
    }
  }
}

class ApiException implements Exception {
  final int statusCode;
  final String message;

  ApiException({
    required this.statusCode,
    required this.message,
  });

  @override
  String toString() => 'ApiException: $statusCode - $message';
} 