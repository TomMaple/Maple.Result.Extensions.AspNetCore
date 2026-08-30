using System.Collections.Generic;
using System.Linq;

namespace Maple.Result.Extensions.AspNetCore.Mappers;

internal static class ErrorMapper
{
    internal static Dictionary<string, object?>? MapExtensions(Error error)
    {
        Dictionary<string, object?>? extensions = null;

        if (error.ErrorDetails is { Count: > 0 })
        {
            extensions ??= [];
            extensions["errors"] = error.ErrorDetails.Select(ErrorDetailsMapper.Map).ToArray();
        }

        var errorDetailTemplated = TemplatedMessageMapper.Map(error.DetailTemplated);
        if (errorDetailTemplated is not null)
        {
            extensions ??= [];
            extensions["detailTemplated"] = errorDetailTemplated;
        }

        return extensions;
    }
}