namespace Domain.Conferences.Statuses;
public class ConsultationStatuses
{
    // Mapping between enum and Guid
    private static readonly Dictionary<ConsultationStatusEnum, Guid> StatusEnumToGuid = new()
    {
        { ConsultationStatusEnum.Assigned, new Guid("00000000-0000-0000-0000-000000000021") },
        { ConsultationStatusEnum.Rejected, new Guid("00000000-0000-0000-0000-000000000022") },
        { ConsultationStatusEnum.Completed, new Guid("00000000-0000-0000-0000-000000000023") },
        { ConsultationStatusEnum.Pending, new Guid("00000000-0000-0000-0000-000000000024") },
    };

    private static readonly Dictionary<Guid, ConsultationStatusEnum> GuidToStatusEnum = StatusEnumToGuid.ToDictionary(kv => kv.Value, kv => kv.Key);

    public static Guid GetGuidByEnum(ConsultationStatusEnum statusEnum)
    {
        return StatusEnumToGuid.TryGetValue(statusEnum, out var guid) ? guid : Guid.Empty;
    }

    public static ConsultationStatusEnum GetEnumByGuid(Guid guid)
    {
        return GuidToStatusEnum.TryGetValue(guid, out var statusEnum) ? statusEnum : ConsultationStatusEnum.Pending;
    }
    public ConsultationStatusesId Id { get; set; }
    public string Name { get; set; }
    public ConsultationStatusEnum Status { get; set; }

    public static ConsultationStatuses Create(ConsultationStatusesId id, string name)
    {
        return new ConsultationStatuses(id, name, MapNameToEnum(name));
    }

    public ConsultationStatuses(ConsultationStatusesId id, string name, ConsultationStatusEnum status)
    {
        Id = id;
        Name = name;
        Status = status;
    }

    private static ConsultationStatusEnum MapNameToEnum(string name)
    {
        return name switch
        {
            "Назначено" => ConsultationStatusEnum.Assigned,
            "Відхилено" => ConsultationStatusEnum.Rejected,
            "Завершено" => ConsultationStatusEnum.Completed,
            "Очікує підтвердження" => ConsultationStatusEnum.Pending,
            _ => ConsultationStatusEnum.Pending
        };
    }
}