import 'package:equatable/equatable.dart';

class User extends Equatable {
  final String? id;
  final String? email;
  final String? displayName;
  final String? fullName;
  final String? photoUrl;
  final String? roleId;
  final String? roleName;
  final String? course;

  const User({
    this.id,
    this.email,
    this.displayName,
    this.fullName,
    this.photoUrl,
    this.roleId,
    this.roleName,
    this.course,
  });

  @override
  List<Object?> get props => [id, email, displayName, fullName, photoUrl, roleId, roleName, course];
} 