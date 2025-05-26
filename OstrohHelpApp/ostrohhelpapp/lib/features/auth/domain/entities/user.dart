import 'package:equatable/equatable.dart';

class User extends Equatable {
  final String? id;
  final String? email;
  final String? displayName;
  final String? photoUrl;
  final String? roleId;
  final String? roleName;

  const User({
    this.id,
    this.email,
    this.displayName,
    this.photoUrl,
    this.roleId,
    this.roleName,
  });

  @override
  List<Object?> get props => [id, email, displayName, photoUrl, roleId, roleName];
} 