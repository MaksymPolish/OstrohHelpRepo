namespace Domain.Questionnaires;

public record QuestionnaireStatusesId(Guid Value)
{
    public static QuestionnaireStatusesId New() => new(Guid.NewGuid());
    public static QuestionnaireStatusesId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}