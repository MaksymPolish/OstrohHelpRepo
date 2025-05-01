using Domain.Consultations;
using Domain.Users;

namespace Domain.Messages;

public class Message
{
    public MessageId Id { get; set; }
    public ConsultationsId ConsultationId { get; set; }
    public UserId SenderId { get; set; }
    public UserId ReceiverId { get; set; }
    public string Text { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}