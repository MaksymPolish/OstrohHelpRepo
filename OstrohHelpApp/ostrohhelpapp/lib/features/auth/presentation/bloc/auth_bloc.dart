import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:firebase_auth/firebase_auth.dart' as firebase_auth;
import '../../domain/entities/user.dart' as app_user;
import '../../data/services/auth_api_service.dart';
import 'auth_event.dart';
import 'auth_state.dart';
import '../../../../core/di/injection_container.dart';

class AuthBloc extends Bloc<AuthEvent, AuthState> {
  final firebase_auth.FirebaseAuth _auth = firebase_auth.FirebaseAuth.instance;
  final GoogleSignIn _googleSignIn = GoogleSignIn();
  final AuthApiService _apiService = sl<AuthApiService>();

  AuthBloc() : super(AuthInitial()) {
    on<CheckAuthStatus>(_onCheckAuthStatus);
    on<SignInWithGoogleRequested>(_onSignInWithGoogleRequested);
    on<SignOutRequested>(_onSignOutRequested);
  }

  Future<void> _onCheckAuthStatus(
    CheckAuthStatus event,
    Emitter<AuthState> emit,
  ) async {
    final currentUser = _auth.currentUser;
    if (currentUser != null) {
      try {
        final userData = await _apiService.getUserById(currentUser.uid);
        emit(Authenticated(_mapApiUser(userData)));
      } catch (e) {
        emit(AuthError(e.toString()));
      }
    } else {
      emit(Unauthenticated());
    }
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

      // Додаю вивід idToken у консоль
      if (googleAuth.idToken != null) {
        print('==========================================');
        print('Google ID Token:');
        print(googleAuth.idToken);
        print('==========================================');
      }

      final firebase_auth.UserCredential userCredential = await _auth.signInWithCredential(credential);
      
      // Send ID token to your backend
      if (googleAuth.idToken != null) {
        final userData = await _apiService.googleLogin(googleAuth.idToken!);
        emit(Authenticated(_mapApiUser(userData)));
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
      ]);
      emit(Unauthenticated());
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }

  app_user.User _mapApiUser(Map<String, dynamic> userData) {
    return app_user.User(
      id: userData['id'],
      email: userData['email'],
      displayName: userData['displayName'],
      photoUrl: userData['photoUrl'],
      roleId: userData['roleId'],
      roleName: userData['roleName'],
    );
  }
} 