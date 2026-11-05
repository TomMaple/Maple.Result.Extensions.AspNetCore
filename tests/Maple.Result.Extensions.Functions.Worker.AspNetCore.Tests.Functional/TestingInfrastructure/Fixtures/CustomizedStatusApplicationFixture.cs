using Microsoft.AspNetCore.Http;
using System;

namespace Maple.Result.Extensions.Functions.Worker.AspNetCore.Tests.Functional.TestingInfrastructure.Fixtures;

public sealed class CustomizedStatusApplicationFixture : TestApplicationFixture
{
    private protected override Action<ProblemDetailsContext>? CustomizeProblemDetails =>
        context => context.ProblemDetails.Status = StatusCodes.Status400BadRequest;
}
