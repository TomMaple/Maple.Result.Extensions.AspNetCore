namespace Maple.Result.Extensions.AspNetCore.Mappers;

internal static class TemplatedMessageMapper
{
    internal static ViewModels.TemplatedMessage? Map(TemplatedMessage? source)
    {
        var @params = source?.Params is { Count: > 0 }
            ? source.Params
            : null;

        return source is null
            ? null
            : new ViewModels.TemplatedMessage(source.TemplateId, @params);
    }
}
