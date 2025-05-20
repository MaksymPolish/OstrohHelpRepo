namespace Domain.Conferences.Statuses;

public record ConsultationStatusesId(Guid Value)
{
    public static ConsultationStatusesId New() => new(Guid.NewGuid());
    public static ConsultationStatusesId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}