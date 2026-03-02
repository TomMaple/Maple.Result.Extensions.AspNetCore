using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Maple.Result.Extensions.AspNetCore.Mappers;

internal static class ErrorMapper
{
    internal static ProblemDetails Map(Error error, int statusCode)
    {
        var problemDetails = new ProblemDetails
        {
            Type = error.TypeUri,
            Title = error.Title,
            Status = statusCode,
            Detail = error.Detail,
            Instance = error.InstanceUri
        };

        var errorDetails = error.ErrorDetails
            .Select(ErrorDetailsMapper.Map)
            .ToArray();

        if (errorDetails is {Length: >0})
            problemDetails.Extensions["errors"] = errorDetails;

        var errorDetailTemplated = TemplatedMessageMapper.Map(error.DetailTemplated);
        if (errorDetailTemplated is not null)
            problemDetails.Extensions["detailTemplated"] = errorDetailTemplated;

        return problemDetails;
    }
}