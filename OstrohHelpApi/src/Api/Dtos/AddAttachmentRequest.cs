namespace Api.Dtos;

public class AddAttachmentRequest
{
    public Guid MessageId { get; set; }
    public string FileUrl { get; set; }
    public string FileType { get; set; }
}
