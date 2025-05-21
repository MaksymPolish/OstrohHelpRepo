using Domain.Users;

namespace Api.Dtos;

public record MessageDto(
    string Id,
    string ConsultationId,
    string SenderId,
    string ReceiverId,
    string Text,
    bool IsRead,
    DateTime SentAt,
    string FullNameSender,
    string FullNameReceiver);
