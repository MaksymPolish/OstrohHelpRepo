import 'package:get_it/get_it.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:firebase_core/firebase_core.dart'; // Add this import

import '../../features/auth/data/repositories/auth_repository_impl.dart';
import '../../features/auth/domain/repositories/auth_repository.dart';
import '../../features/auth/presentation/bloc/auth_bloc.dart';

final sl = GetIt.instance;

Future<void> init() async {
  // Blocs
  if (!sl.isRegistered<AuthBloc>()) {
    sl.registerFactory(
      () => AuthBloc(
        authRepository: sl(),
      ),
    );
  }

  // Repositories
  if (!sl.isRegistered<AuthRepository>()) {
    sl.registerLazySingleton<AuthRepository>(
      () => AuthRepositoryImpl(
        firebaseAuth: sl(), // Will be resolved when needed
      ),
    );
  }

  // Register FirebaseAuth as a factory so it's not accessed early
  if (!sl.isRegistered<FirebaseAuth>()) {
    sl.registerFactory<FirebaseAuth>(() {
      // This ensures Firebase is initialized before using FirebaseAuth
      assert(Firebase.apps.isNotEmpty, 'Firebase must be initialized before accessing FirebaseAuth');
      return FirebaseAuth.instance;
    });
  }
}