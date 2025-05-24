import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../../features/auth/domain/entities/user.dart';
import '../../../../features/auth/domain/repositories/auth_repository.dart';

part 'user_event.dart';
part 'user_state.dart';

class UserBloc extends Bloc<UserEvent, UserState> {
  final AuthRepository authRepository;

  UserBloc({required this.authRepository}) : super(UserInitial()) {
    on<LoadUser>(_onLoadUser);
  }

  Future<void> _onLoadUser(
    LoadUser event,
    Emitter<UserState> emit,
  ) async {
    emit(UserLoading());
    final result = await authRepository.getCurrentUser();
    result.fold(
      (failure) => emit(UserError(failure.toString())),
      (user) => emit(user != null ? UserLoaded(user) : UserNotFound()),
    );
  }
} 