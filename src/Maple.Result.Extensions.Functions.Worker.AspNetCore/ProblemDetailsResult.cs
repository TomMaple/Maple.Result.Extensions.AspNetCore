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

        // The value has to be replaced here, not in the OnFormatting method, because the ObjectResultExecutor
        // reads the value before it calls that method.
        Value = CreateProblemDetails(context, (ProblemDetails)Value!);

        return base.ExecuteResultAsync(context);
    }

    #region helper methods

    private static ProblemDetails CreateProblemDetails(ActionContext context, ProblemDetails source)
    {
        var httpContext = context.HttpContext;
        var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        var problemDetails = factory.CreateProblemDetails(
            httpContext, source.Status, source.Title, source.Type, source.Detail, source.Instance);

        foreach (var extension in source.Extensions)
            problemDetails.Extensions[extension.Key] = extension.Value;

        return problemDetails;
    }

    #endregion
}
