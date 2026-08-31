using System.Collections.Generic;
using Sut = Maple.Result.Extensions.AspNetCore.Mappers.ErrorDetailsMapper;

namespace Maple.Result.Extensions.AspNetCore.Tests.Unit.Mappers;

public class ErrorDetailsMapperTests
{
    #region consts

    private const string PropertyPointer = "#/property1";
    private const string Detail = "Property 1 failure detail.";
    private const string TemplateId = "errors.failure.property1";

    #endregion

    [Fact]
    public void Map_SourceWithoutTemplatedDetail_ReturnsPointerAndDetailOnly()
    {
        // Arrange
        var source = new ErrorDetail(PropertyPointer, Detail);

        // Act
        var result = Sut.Map(source);

        // Assert
        result.PropertyPointer.ShouldBe(PropertyPointer);
        result.Detail.ShouldBe(Detail);
        result.DetailTemplated.ShouldBeNull();
    }

    [Fact]
    public void Map_SourceWithTemplatedDetail_ReturnsMappedTemplatedDetail()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { ["pk1"] = "pv1" };
        var source = new ErrorDetail(PropertyPointer, Detail, new TemplatedMessage(TemplateId, parameters));

        // Act
        var result = Sut.Map(source);

        // Assert
        result.PropertyPointer.ShouldBe(PropertyPointer);
        result.Detail.ShouldBe(Detail);
        result.DetailTemplated.ShouldNotBeNull();
        result.DetailTemplated.TemplateId.ShouldBe(TemplateId);
        result.DetailTemplated.Params.ShouldNotBeNull();
        result.DetailTemplated.Params["pk1"].ShouldBe("pv1");
    }

    [Fact]
    public void Map_SourceWithTemplatedDetailWithoutParams_ReturnsTemplatedDetailWithoutParams()
    {
        // Arrange
        var source = new ErrorDetail(PropertyPointer, Detail,
            new TemplatedMessage(TemplateId, new Dictionary<string, object>()));

        // Act
        var result = Sut.Map(source);

        // Assert
        result.DetailTemplated.ShouldNotBeNull();
        result.DetailTemplated.TemplateId.ShouldBe(TemplateId);
        result.DetailTemplated.Params.ShouldBeNull();
    }

    [Fact]
    public void Map_SourceWithoutPropertyPointer_ReturnsNullPropertyPointer()
    {
        // Arrange
        var source = new ErrorDetail(null, Detail);

        // Act
        var result = Sut.Map(source);

        // Assert
        result.PropertyPointer.ShouldBeNull();
        result.Detail.ShouldBe(Detail);
    }
}
