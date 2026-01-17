using AutoMapper;

namespace Api.Mapper;

public class GuidToStringConverter : IValueConverter<Guid, string>
{
    public string Convert(Guid sourceMember, ResolutionContext context)
        => sourceMember.ToString();
}

public class NullableGuidToStringConverter : IValueConverter<Guid?, string>
{
    public string Convert(Guid? sourceMember, ResolutionContext context)
        => sourceMember?.ToString() ?? string.Empty;
}
