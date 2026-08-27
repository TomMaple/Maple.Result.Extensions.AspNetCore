using Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Fixtures;
using Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Helpers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.AspNetCore.Tests.Functional;

public class ResultExtensionsMinimalApiFunctionalTests : IClassFixture<TestApplicationFixture>
{
    #region consts

    private const string ExpectedValueJson =
        """
        {
          "id": 13,
          "name": "Test value"
        }
        """;

    private const string ExpectedFailureJson =
        """
        {
          "type": "tag:test.com,2026:failure",
          "title": "Failure title",
          "status": 422,
          "detail": "Failure detail.",
          "instance": "https://test.com/instances/failure"
        }
        """;

    private const string ExpectedFailureMappingJson =
        """
        {
          "id": 11,
          "name": "Failure title"
        }
        """;

    private const string ExpectedMappedSuccessJson =
        """
        {
          "id": 31,
          "name": "Mapped success"
        }
        """;

    private const string ExpectedMappedValueJson =
        """
        {
          "id": 26,
          "name": "Test value"
        }
        """;

    #endregion

    #region read-only fields

    private readonly HttpClient _sut;

    #endregion

    #region constructors

    public ResultExtensionsMinimalApiFunctionalTests(TestApplicationFixture fixture)
    {
        _sut = fixture.Client;
    }

    #endregion

    #region success

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResult_ReturnsNoContent()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/success");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NoContent);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithValue_ReturnsOkWithValue()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/success/value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.OK);
        json.ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithNullValue_ReturnsNoContent()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/success/null-value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NoContent);
        json.ShouldBeEmpty();
    }

    #endregion

    #region errors

    [Fact]
    public async Task ToMinimalApiResult_FailureError_ReturnsUnprocessableEntityProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/error");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToMinimalApiResult_NotFoundError_ReturnsNotFoundProblemDetails()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:not-found",
              "title": "Not found title",
              "status": 404,
              "detail": "Not found detail.",
              "instance": "https://test.com/instances/not-found"
            }
            """;

        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/error/not-found");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NotFound);
        json.ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region custom mappings passed to the method

    [Fact]
    public async Task ToMinimalApiResult_ErrorMatchingMappingPassedToTheMethod_ReturnsCustomMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/error/custom-mapping");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    [Fact]
    public async Task ToMinimalApiResult_ErrorNotMatchingMappingPassedToTheMethod_ReturnsDefaultProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/error/custom-mapping-not-matching");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    #endregion

    #region success status code

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithSuccessStatusCode_ReturnsGivenStatusCode()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/status-code/success");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Accepted);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithValueAndSuccessStatusCode_ReturnsGivenStatusCodeWithValue()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/status-code/success/value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Created);
        json.ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithNullValueAndNoSuccessNoResponseStatusCode_ReturnsSuccessStatusCode()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/status-code/success/null-value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.IMUsed);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithNullValueAndSuccessNoResponseStatusCode_ReturnsSuccessNoResponseStatusCode()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/status-code/success/no-response-status-code");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.ResetContent);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToMinimalApiResult_ErrorWithSuccessStatusCode_ReturnsDefaultProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/status-code/error");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    #endregion

    #region success mapping

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithSuccessMapping_ReturnsMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/success-mapping/success");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.Accepted);
        json.ShouldBe(JsonHelper.Normalize(ExpectedMappedSuccessJson));
    }

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithValueAndSuccessMapping_ReturnsMappedValue()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/success-mapping/success/value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.NonAuthoritativeInformation);
        json.ShouldBe(JsonHelper.Normalize(ExpectedMappedValueJson));
    }

    [Fact]
    public async Task ToMinimalApiResult_SuccessfulResultWithNullValueAndSuccessMapping_ReturnsMappedNullValueResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/success-mapping/success/null-value");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.ResetContent);
        json.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToMinimalApiResult_ErrorWithSuccessMapping_ReturnsDefaultProblemDetails()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/success-mapping/error");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public async Task ToMinimalApiResult_ErrorWithSuccessMappingAndMappingPassedToTheMethod_ReturnsCustomMappedResponse()
    {
        // Act
        var (statusCode, json) = await GetAsync(_sut, "minimal/success-mapping/error/custom-mapping");

        // Assert
        statusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        json.ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    #endregion

    #region helper methods

    private static async Task<(HttpStatusCode StatusCode, string Json)> GetAsync(HttpClient client, string route)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(route, cancellationToken);

        var json = await JsonHelper.ReadNormalizedJsonAsync(response, cancellationToken);

        return (response.StatusCode, json);
    }

    #endregion
}
