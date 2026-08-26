using System.Collections.Generic;
using System.Linq;

namespace Maple.Result.Extensions.AspNetCore.Mappers;

internal static class ErrorMapper
{
    internal static Dictionary<string, object?>? MapExtensions(Error error)
    {
        Dictionary<string, object?>? extensions = null;

        var errorDetails = error.ErrorDetails
            .Select(ErrorDetailsMapper.Map)
            .ToArray();

        if (errorDetails is { Length: > 0 })
        {
            extensions ??= new Dictionary<string, object?>();
            extensions["errors"] = errorDetails;
        }

        var errorDetailTemplated = TemplatedMessageMapper.Map(error.DetailTemplated);
        if (errorDetailTemplated is not null)
        {
            extensions ??= new Dictionary<string, object?>();
            extensions["detailTemplated"] = errorDetailTemplated;
        }

        return extensions;
    }
}