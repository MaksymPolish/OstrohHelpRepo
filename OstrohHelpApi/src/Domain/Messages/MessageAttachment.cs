using System;

namespace Domain.Messages;

public class MessageAttachment
{
    public Guid Id { get; set; }
    public MessageId MessageId { get; set; }
    public string FileUrl { get; set; }
    public string FileType { get; set; }
    public DateTime CreatedAt { get; set; }

    public Message Message { get; set; }
}