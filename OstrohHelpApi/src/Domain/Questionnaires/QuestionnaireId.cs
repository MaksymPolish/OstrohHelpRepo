namespace Domain.Questionnaires;

public record QuestionnaireId(Guid Value)
{
    public static QuestionnaireId New() => new(Guid.NewGuid());
    public static QuestionnaireId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}