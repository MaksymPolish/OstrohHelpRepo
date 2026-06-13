import 'package:get_it/get_it.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:google_sign_in/google_sign_in.dart';

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
}
