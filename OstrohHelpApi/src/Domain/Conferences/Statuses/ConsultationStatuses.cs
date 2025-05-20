namespace Domain.Conferences.Statuses;

public class ConsultationStatuses
{
    public ConsultationStatusesId Id { get; set; }
    public string Name { get; set; }

    public static ConsultationStatuses Create(ConsultationStatusesId id, string name) => new(id, name);

    ConsultationStatuses(ConsultationStatusesId id, string name)
    {
        Id = id;
        Name = name;
    }
}