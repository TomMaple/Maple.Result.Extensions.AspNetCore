using System.Collections.Generic;
using Sut = Maple.Result.Extensions.AspNetCore.Mappers.TemplatedMessageMapper;

namespace Maple.Result.Extensions.AspNetCore.Tests.Unit.Mappers;

public class TemplatedMessageMapperTests
{
    #region consts

    private const string TemplateId = "errors.failure.detail";

    #endregion

    [Fact]
    public void Map_NullSource_ReturnsNull()
    {
        // Act
        var result = Sut.Map(null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Map_SourceWithoutParams_ReturnsTemplateIdWithoutParams()
    {
        // Arrange
        var source = new TemplatedMessage(TemplateId);

        // Act
        var result = Sut.Map(source);

        // Assert
        result.ShouldNotBeNull();
        result.TemplateId.ShouldBe(TemplateId);
        result.Params.ShouldBeNull();
    }

    [Fact]
    public void Map_SourceWithParams_ReturnsTemplateIdWithParams()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 123 };
        var source = new TemplatedMessage(TemplateId, parameters);

        // Act
        var result = Sut.Map(source);

        // Assert
        result.ShouldNotBeNull();
        result.TemplateId.ShouldBe(TemplateId);
        result.Params.ShouldNotBeNull();
        result.Params.Count.ShouldBe(2);
        result.Params["key1"].ShouldBe("value1");
        result.Params["key2"].ShouldBe(123);
    }

    [Fact]
    public void Map_SourceWithEmptyParams_ReturnsTemplateIdWithoutParams()
    {
        // Arrange
        var source = new TemplatedMessage(TemplateId, new Dictionary<string, object>());

        // Act
        var result = Sut.Map(source);

        // Assert
        result.ShouldNotBeNull();
        result.TemplateId.ShouldBe(TemplateId);
        result.Params.ShouldBeNull();
    }
}
