import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:firebase_auth/firebase_auth.dart' as firebase_auth;
import '../../domain/entities/user.dart' as app_user;
import '../../data/services/auth_api_service.dart';
import 'auth_event.dart';
import 'auth_state.dart';
import '../../../../core/di/injection_container.dart';
import '../../../../core/auth/token_storage.dart';
import '../../../../core/auth/user_storage.dart';
import 'package:flutter/material.dart';

class AuthBloc extends Bloc<AuthEvent, AuthState> {
  final firebase_auth.FirebaseAuth _auth = firebase_auth.FirebaseAuth.instance;
  final GoogleSignIn _googleSignIn = GoogleSignIn(
    signInOption: SignInOption.standard,
  );
  final AuthApiService _apiService = sl<AuthApiService>();
    final TokenStorage _tokenStorage = TokenStorage();
    final UserStorage _userStorage = UserStorage();
  final BuildContext context;

  AuthBloc(this.context) : super(AuthInitial()) {
    on<CheckAuthStatus>(_onCheckAuthStatus);
    on<SignInWithGoogleRequested>(_onSignInWithGoogleRequested);
    on<SignOutRequested>(_onSignOutRequested);
    on<UpdateUserCourse>(_onUpdateUserCourse);
  }

  Future<void> _onCheckAuthStatus(
    CheckAuthStatus event,
    Emitter<AuthState> emit,
  ) async {
    final token = await _tokenStorage.getToken();
    final cachedUser = await _userStorage.getUser();
    final isTokenExpired = await _tokenStorage.isTokenExpired();
    final isSessionExpired = await _userStorage.isSessionExpired();

    // Перевіряємо, чи токен і сесія валідні
    if (token != null && cachedUser != null && !isTokenExpired && !isSessionExpired) {
      emit(Authenticated(cachedUser));
      return;
    }

    // Якщо щось застаріло - очищаємо все
    if (isTokenExpired || isSessionExpired) {
      await _tokenStorage.clearTokens();
      await _userStorage.clearUser();
    }

    emit(Unauthenticated());
  }

  Future<void> _onSignInWithGoogleRequested(
    SignInWithGoogleRequested event,
    Emitter<AuthState> emit,
  ) async {
    try {
      emit(AuthLoading());
      
      final GoogleSignInAccount? googleUser = await _googleSignIn.signIn();
      if (googleUser == null) {
        emit(Unauthenticated());
        return;
      }

      final GoogleSignInAuthentication googleAuth = await googleUser.authentication;
      final credential = firebase_auth.GoogleAuthProvider.credential(
        accessToken: googleAuth.accessToken,
        idToken: googleAuth.idToken,
      );

      // Try to sign in with Firebase, but don't fail if there's a Pigeon error
      try {
        await _auth.signInWithCredential(credential);
      } catch (firebaseError) {
        // Continue anyway - we don't really need Firebase, just the API
        debugPrint('Firebase Sign In warning (continuing with API): $firebaseError');
      }

      if (googleAuth.idToken != null) {
        final userData = await _apiService.googleLogin(googleAuth.idToken!);
        final user = _mapApiUser(userData);
        await _userStorage.saveUser(user);
        emit(Authenticated(user));
      } else {
        throw Exception('Failed to get ID token from Google');
      }
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }

  Future<void> _onSignOutRequested(
    SignOutRequested event,
    Emitter<AuthState> emit,
  ) async {
    try {
      emit(AuthLoading());
      await Future.wait([
        _auth.signOut(),
        _googleSignIn.signOut(),
        _tokenStorage.clearTokens(),
        _userStorage.clearUser(),
      ]);
      emit(Unauthenticated());
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }

  Future<void> _onUpdateUserCourse(
    UpdateUserCourse event,
    Emitter<AuthState> emit,
  ) async {
    try {
      await _apiService.updateUserCourse(event.userId, event.course);
      final userData = await _apiService.getUserById(event.userId);
      final updatedUser = _mapApiUser(userData);
      await _userStorage.saveUser(updatedUser);
      emit(Authenticated(updatedUser));
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }

  app_user.User _mapApiUser(Map<String, dynamic> userData) {
    return app_user.User(
      id: userData['id'],
      email: userData['email'],
      displayName: userData['fullName'] ?? userData['displayName'],
      fullName: userData['fullName'],
      photoUrl: userData['photoUrl'],
      roleId: userData['roleId'],
      roleName: userData['roleName'], // може бути null при login
      course: userData['course']?.toString(), // може бути null при login
    );
  }
} 