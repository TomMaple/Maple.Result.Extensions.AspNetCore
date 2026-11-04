using Maple.Result;
using Maple.Result.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    [HttpGet]
    [Route("Unauthenticated")]
    public IActionResult GetUnauthenticated()
    {
        Result result = Error.Unauthenticated(
            ErrorUri.Tag("tag:sample.org,2026-02-27:testing.unauthenticated"),
            "Please sign in.",
            "The authentication token is missing or invalid.",
            null,
            "errors.authentication.missing",
            ("key1", "value1"), ("key2", 123));

        return result.ToActionResult(this);
    }

    [HttpGet]
    [Route("Unauthenticated2")]
    public IActionResult GetUnauthenticated2()
    {
        return Unauthorized(Error.Unauthenticated(
            ErrorUri.Tag("tag:sample.org,2026-02-27:testing.unauthenticated"),
            "Please sign in.",
            "The authentication token is missing or invalid."));
    }

    [HttpGet]
    [Route("Unauthenticated3")]
    public IActionResult GetUnauthenticated3()
    {
        return Unauthorized();
    }
}
