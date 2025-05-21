using Domain.Conferences;
using Domain.Users;

namespace Domain.Messages;

public class Message
{
    public MessageId Id { get; set; }
    
    public Consultations Consultations { get; set; }
    public ConsultationsId ConsultationId { get; set; }
    public UserId SenderId { get; set; }
    public UserId ReceiverId { get; set; }
    public string Text { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public static Message Create(MessageId id, ConsultationsId consultationId, UserId senderId, UserId receiverId, string text, bool isRead, DateTime sentAt, DateTime? deletedAt) =>
        new(id, consultationId, senderId, receiverId, text, isRead, sentAt, deletedAt);
    
    Message(MessageId id, ConsultationsId consultationId, UserId senderId, UserId receiverId, string text, bool isRead, DateTime sentAt, DateTime? deletedAt)
    {
        Id = id;
        ConsultationId = consultationId;
        SenderId = senderId;
        ReceiverId = receiverId;
        Text = text;
        IsRead = isRead;
        SentAt = sentAt;
        DeletedAt = deletedAt;
    }
    
    public void MarkAsRead()
    {
        IsRead = true;
    }
}