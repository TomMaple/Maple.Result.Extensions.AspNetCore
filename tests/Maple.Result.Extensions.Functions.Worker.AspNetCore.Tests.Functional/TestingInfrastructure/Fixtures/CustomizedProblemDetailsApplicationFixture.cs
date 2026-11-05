using Microsoft.AspNetCore.Http;
using System;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional.TestingInfrastructure.Fixtures;

public sealed class CustomizedProblemDetailsApplicationFixture : TestApplicationFixture
{
    internal const string ExtensionName = "customized";

    private protected override Action<ProblemDetailsContext>? CustomizeProblemDetails =>
        context => context.ProblemDetails.Extensions[ExtensionName] = true;
}
