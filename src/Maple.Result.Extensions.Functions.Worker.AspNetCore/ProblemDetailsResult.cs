using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore;

/// <summary>
///     An <see cref="ObjectResult" /> that passes its <see cref="ProblemDetails" /> through the registered
///     <see cref="ProblemDetailsFactory" /> when it is executed, as this is the first moment when
///     the <see cref="Microsoft.AspNetCore.Http.HttpContext" /> is available. This adds the <c>traceId</c> extension and
///     applies the <c>CustomizeProblemDetails</c> configuration.
/// </summary>
internal sealed class ProblemDetailsResult : ObjectResult
{
    internal ProblemDetailsResult(ProblemDetails problemDetails)
        : base(problemDetails)
    {
        StatusCode = problemDetails.Status;
    }

    /// <inheritdoc />
    public override Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // This instance is returned as an ObjectResult, so the caller could have replaced the value.
        if (Value is not ProblemDetails source)
            return base.ExecuteResultAsync(context);

        // A new result is executed, because the value has to be replaced before the ObjectResultExecutor reads it,
        // and this instance has to stay unchanged, as it can be executed many times.
        var problemDetails = CreateProblemDetails(context, source);
        var result = new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status,
            Formatters = Formatters
        };

        foreach (var contentType in ContentTypes)
            result.ContentTypes.Add(contentType);

        return result.ExecuteResultAsync(context);
    }

    #region helper methods

    private ProblemDetails CreateProblemDetails(ActionContext context, ProblemDetails source)
    {
        var httpContext = context.HttpContext;
        var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        var problemDetails = factory.CreateProblemDetails(
            httpContext, StatusCode ?? source.Status, source.Title, source.Type, source.Detail, source.Instance);

        foreach (var extension in source.Extensions)
            problemDetails.Extensions[extension.Key] = extension.Value;

        return problemDetails;
    }

    #endregion
}
