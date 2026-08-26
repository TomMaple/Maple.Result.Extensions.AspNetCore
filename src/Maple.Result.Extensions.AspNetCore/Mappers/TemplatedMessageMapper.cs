namespace Maple.Result.Extensions.AspNetCore.Mappers;

internal static class TemplatedMessageMapper
{
    internal static ViewModels.TemplatedMessage? Map(TemplatedMessage? source)
    {
        return source is null 
            ? null 
            : new ViewModels.TemplatedMessage(source.TemplateId, source.Params);
    }
}
