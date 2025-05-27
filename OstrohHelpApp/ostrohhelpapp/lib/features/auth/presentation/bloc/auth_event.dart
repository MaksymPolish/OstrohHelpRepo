import 'package:equatable/equatable.dart';

abstract class AuthEvent extends Equatable {
  const AuthEvent();

  @override
  List<Object?> get props => [];
}

class CheckAuthStatus extends AuthEvent {}

class SignInWithGoogleRequested extends AuthEvent {}

class SignOutRequested extends AuthEvent {}

class UpdateUserCourse extends AuthEvent {
  final String userId;
  final String course;

  const UpdateUserCourse(this.userId, this.course);

  @override
  List<Object?> get props => [userId, course];
} 