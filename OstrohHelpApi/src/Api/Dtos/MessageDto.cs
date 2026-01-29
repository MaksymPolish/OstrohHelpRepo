using Domain.Messages;
using Domain.Users;

namespace Api.Dtos;
public class MessageDto
{
    public MessageId Id { get; set; }
    public string ConsultationId { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string Text { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public string FullNameSender { get; set; }
    public string FullNameReceiver { get; set; }
    public List<MessageAttachmentDto> Attachments { get; set; }
}