using System.Text.Json.Serialization;

namespace Maple.Result.Extensions.AspNetCore.ViewModels;

public record ErrorDetail
{
    public ErrorDetail(string? propertyPointer, string detail, TemplatedMessage? detailTemplated = null)
    {
        PropertyPointer = propertyPointer;
        Detail = detail;
        DetailTemplated = detailTemplated;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("pointer")]
    public string? PropertyPointer { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("detail")]
    public string Detail { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("detailTemplated")]
    public TemplatedMessage? DetailTemplated { get; init; }
}