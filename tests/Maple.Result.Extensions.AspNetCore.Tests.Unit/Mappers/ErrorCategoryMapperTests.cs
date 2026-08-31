using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using Sut = Maple.Result.Extensions.AspNetCore.Mappers.ErrorCategoryMapper;

namespace Maple.Result.Extensions.AspNetCore.Tests.Unit.Mappers;

public class ErrorCategoryMapperTests
{
    [Theory]
    [InlineData(ErrorCategory.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorCategory.Unauthenticated, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorCategory.Unauthorized, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorCategory.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCategory.Timeout, StatusCodes.Status408RequestTimeout)]
    [InlineData(ErrorCategory.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorCategory.Failure, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(ErrorCategory.Critical, StatusCodes.Status500InternalServerError)]
    [InlineData(ErrorCategory.NotImplemented, StatusCodes.Status501NotImplemented)]
    [InlineData(ErrorCategory.Unavailable, StatusCodes.Status503ServiceUnavailable)]
    public void GetStatusCode_KnownCategory_ReturnsMatchingStatusCode(ErrorCategory category, int expectedStatusCode)
    {
        // Act
        var result = Sut.GetStatusCode(category);

        // Assert
        result.ShouldBe(expectedStatusCode);
    }

    /// <summary>
    ///     Guards against a new <see cref="ErrorCategory" /> member being added by the Maple.Result package
    ///     without a matching arm being added to the mapper, which would otherwise surface only at run time,
    ///     as an unhandled exception thrown from the error handling path itself.
    /// </summary>
    [Fact]
    public void GetStatusCode_EveryDeclaredCategory_IsMapped()
    {
        // Arrange
        var categories = Enum.GetValues<ErrorCategory>();

        // Act
        var unmappedCategories = categories
            .Where(category => Record.Exception(() => Sut.GetStatusCode(category)) is not null)
            .ToArray();

        // Assert
        unmappedCategories.ShouldBeEmpty(
            $"Every declared {nameof(ErrorCategory)} value must be mapped to a status code, "
            + $"but the mapper throws for: {string.Join(", ", unmappedCategories)}.");
    }

    [Fact]
    public void GetStatusCode_UndeclaredCategory_ThrowsNotImplementedException()
    {
        // Arrange
        const ErrorCategory UndeclaredCategory = (ErrorCategory)int.MaxValue;

        // Act & Assert
        Should.Throw<NotImplementedException>(() => Sut.GetStatusCode(UndeclaredCategory));
    }
}
