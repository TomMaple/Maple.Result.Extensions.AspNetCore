using Maple.Result.Extensions.AspNetCore.Configuration;
using Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Application.Models;
using Microsoft.AspNetCore.Http;
using System;

namespace Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Fixtures;

public sealed class CustomMappingApplicationFixture : TestApplicationFixture
{
    internal const string ErrorTitleHeaderName = "x-error-title";

    private const string ValidationTypeUri = "tag:test.com,2026:validation";

    private protected override Action<ResultMappingOptions>? ConfigureResultMapping => Configure;

    #region helper methods

    private static void Configure(ResultMappingOptions options)
    {
        // Matches the errors of the ErrorCategory.Conflict category.
        options.Mappings?.Add((error, controller) => error.Category == ErrorCategory.Conflict
            ? controller.StatusCode(StatusCodes.Status410Gone, new TestValue(99, error.Title))
            : null);

        // Matches a single error type, regardless of its category.
        options.Mappings?.Add((error, controller) => error.TypeUri == ValidationTypeUri
            ? controller.StatusCode(StatusCodes.Status429TooManyRequests, new TestValue(42, error.TypeUri))
            : null);

        // The first of the two mappings matching the ErrorCategory.Timeout category wins.
        options.Mappings?.Add((error, controller) => error.Category == ErrorCategory.Timeout
            ? controller.StatusCode(StatusCodes.Status426UpgradeRequired, new TestValue(1, "First matching mapping"))
            : null);
        options.Mappings?.Add((error, controller) => error.Category == ErrorCategory.Timeout
            ? controller.StatusCode(StatusCodes.Status424FailedDependency, new TestValue(2, "Second matching mapping"))
            : null);

        // Uses the controller to add a response header on top of returning the value.
        options.Mappings?.Add((error, controller) =>
        {
            if (error.Category != ErrorCategory.NotImplemented)
                return null;

            controller.Response.Headers.Append(ErrorTitleHeaderName, error.Title);

            return controller.Ok(new TestValue(7, error.Title));
        });
    }

    #endregion
}
