namespace Maple.Result.Extensions.AspNetCore.Mappers;

internal static class TemplatedMessageMapper
{
    internal static TemplatedMessage? Map(Maple.Result.TemplatedMessage? source)
    {
        return source is null 
            ? null 
            : new TemplatedMessage(source.TemplateId, source.Params);
    }
}