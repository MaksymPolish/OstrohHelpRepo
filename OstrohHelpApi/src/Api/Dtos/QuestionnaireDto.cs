namespace Api.Dtos;

public class QuestionnaireDto
{
    public string Id { get; set; } = null!;
    public string? UserId { get; set; }
    public string FullName { get; set; } = "Анонімно";
    public string Email { get; set; } = "Анонімно";
    public string StatusId { get; set; } = null!;
    public string StatusName { get; set; } = "Unknown";
    public string Description { get; set; } = null!;
    public bool IsAnonymous { get; set; }
    public DateTime SubmittedAt { get; set; }
}