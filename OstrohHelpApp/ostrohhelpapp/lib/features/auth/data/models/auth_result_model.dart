class AuthResultModel {
  final String id;
  final String email;
  final String fullName;
  final String roleId;
  final String? roleName;
  final String jwtToken;
  final String refreshToken;
  final DateTime expiresAt;

  const AuthResultModel({
    required this.id,
    required this.email,
    required this.fullName,
    required this.roleId,
    this.roleName,
    required this.jwtToken,
    required this.refreshToken,
    required this.expiresAt,
  });

  factory AuthResultModel.fromJson(Map<String, dynamic> json) {
    return AuthResultModel(
      id: json['id'] as String,
      email: json['email'] as String,
      fullName: json['fullName'] as String,
      roleId: json['roleId'] as String,
      roleName: json['roleName'] as String?,
      jwtToken: json['jwtToken'] as String,
      refreshToken: json['refreshToken'] as String,
      expiresAt: DateTime.parse(json['expiresAt'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'email': email,
      'fullName': fullName,
      'roleId': roleId,
      'roleName': roleName,
      'jwtToken': jwtToken,
      'refreshToken': refreshToken,
      'expiresAt': expiresAt.toIso8601String(),
    };
  }
} 