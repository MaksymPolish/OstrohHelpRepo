namespace Domain.Questionnaires.Statuses;

public class QuestionnaireStatuses
{
    public QuestionnaireStatusesId Id { get; set; }
    public string Name { get; set; }
    
    public static new QuestionnaireStatuses Create(QuestionnaireStatusesId id, string name) => new(id, name);
    public QuestionnaireStatuses(QuestionnaireStatusesId id, string name)
    {
        Id = id;
        Name = name;
    }
}