namespace Domain.Consultations;

public record ConsultationsId(Guid Value)
{
    public static ConsultationsId New() => new(Guid.NewGuid());
    public static ConsultationsId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}