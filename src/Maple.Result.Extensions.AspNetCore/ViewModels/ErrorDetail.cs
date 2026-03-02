using System.Text.Json.Serialization;

namespace Maple.Result.Extensions.AspNetCore.ViewModels;

public record ErrorDetail
{
    public ErrorDetail(string? PropertyPointer, string Detail, TemplatedMessage? DetailTemplated = null)
    {
        this.PropertyPointer = PropertyPointer;
        this.Detail = Detail;
        this.DetailTemplated = DetailTemplated;
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