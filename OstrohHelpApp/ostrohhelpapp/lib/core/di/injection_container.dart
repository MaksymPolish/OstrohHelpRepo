import 'package:get_it/get_it.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';

import '../../features/auth/data/services/auth_api_service.dart';
import '../../features/auth/presentation/bloc/auth_bloc.dart';

final sl = GetIt.instance;

Future<void> init() async {
  if (!sl.isRegistered<AuthApiService>()) {
    sl.registerLazySingleton(() => AuthApiService());
  }

  if (!sl.isRegistered<AuthBloc>()) {
    sl.registerFactoryParam<AuthBloc, BuildContext, void>(
      (context, _) => AuthBloc(context),
    );
  }

  if (!sl.isRegistered<FirebaseAuth>()) {
    sl.registerFactory<FirebaseAuth>(() {
      assert(Firebase.apps.isNotEmpty, 'Firebase must be initialized before accessing FirebaseAuth');
      return FirebaseAuth.instance;
    });
  }
}
