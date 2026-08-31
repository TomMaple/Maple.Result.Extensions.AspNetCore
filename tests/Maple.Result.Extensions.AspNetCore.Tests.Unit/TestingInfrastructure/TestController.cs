using Microsoft.AspNetCore.Mvc;

namespace Maple.Result.Extensions.AspNetCore.Tests.Unit.TestingInfrastructure;

/// <summary>
///     A minimal concrete <see cref="ControllerBase" />, as the extension methods under test take one
///     and <see cref="ControllerBase" /> itself is abstract.
/// </summary>
internal sealed class TestController : ControllerBase
{
}
