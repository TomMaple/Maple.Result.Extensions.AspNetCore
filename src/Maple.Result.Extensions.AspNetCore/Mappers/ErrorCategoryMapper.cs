using System;
using Microsoft.AspNetCore.Http;

namespace Maple.Result.Extensions.AspNetCore.Mappers;

internal static class ErrorCategoryMapper
{
    internal static int GetStatusCode(ErrorCategory category)
    {
        return category switch
        {
            ErrorCategory.Validation => StatusCodes.Status400BadRequest,
            ErrorCategory.Unauthenticated => StatusCodes.Status401Unauthorized,
            ErrorCategory.Unauthorized => StatusCodes.Status403Forbidden,
            ErrorCategory.NotFound => StatusCodes.Status404NotFound,
            ErrorCategory.Timeout => StatusCodes.Status408RequestTimeout,
            ErrorCategory.Conflict => StatusCodes.Status409Conflict,
            ErrorCategory.Failure => StatusCodes.Status422UnprocessableEntity,
            ErrorCategory.Critical => StatusCodes.Status500InternalServerError,
            ErrorCategory.NotImplemented => StatusCodes.Status501NotImplemented,
            ErrorCategory.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => throw new NotImplementedException($"Unsupported ErrorCategory: {category}")
        };
    }
}