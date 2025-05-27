import '../../domain/entities/user.dart';

class UserModel extends User {
  const UserModel({
    String? id,
    String? email,
    String? displayName,
    String? photoUrl,
    String? roleId,
    String? roleName,
    String? course,
  }) : super(
          id: id,
          email: email,
          displayName: displayName,
          photoUrl: photoUrl,
          roleId: roleId,
          roleName: roleName,
          course: course,
        );

  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      id: json['id'] as String?,
      email: json['email'] as String?,
      displayName: json['displayName'] as String?,
      photoUrl: json['photoUrl'] as String?,
      roleId: json['roleId'] as String?,
      roleName: json['roleName'] as String?,
      course: json['course'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'email': email,
      'displayName': displayName,
      'photoUrl': photoUrl,
      'roleId': roleId,
      'roleName': roleName,
      'course': course,
    };
  }
} 