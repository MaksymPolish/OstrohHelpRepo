using System;

namespace Api.Dtos;

public class MessageAttachmentDto
{
    public Guid Id { get; set; }
    public string FileUrl { get; set; }
    public string FileType { get; set; }
    public DateTime CreatedAt { get; set; }
}