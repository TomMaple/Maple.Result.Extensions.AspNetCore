using System.Text.Json.Serialization;

namespace Maple.Result.Extensions.AspNetCore.ViewModels;

/// <summary>
///     Represents an individual error occurence that contains the details of an error, including the property pointer,
///     detail message, and optional templated message.
/// </summary>
public sealed record ErrorDetail
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ErrorDetail" /> class with the specified property pointer,
    ///     detail message, and optional templated message.
    /// </summary>
    /// <param name="propertyPointer">The JSON Pointer which identifies the invalid property in the input data.</param>
    /// <param name="detail">The human-readable explanation specific to this individual error occurrence.</param>
    /// <param name="detailTemplated">The templated message with the human-readable explanation.</param>
    public ErrorDetail(string? propertyPointer, string detail, TemplatedMessage? detailTemplated = null)
    {
        PropertyPointer = propertyPointer;
        Detail = detail;
        DetailTemplated = detailTemplated;
    }

    /// <summary>
    ///     Gets the JSON Pointer which identifies the invalid property in the input data.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("pointer")]
    public string? PropertyPointer { get; init; }

    /// <summary>
    ///     Gets the human-readable explanation specific to this individual error occurrence.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("detail")]
    public string Detail { get; init; }

    /// <summary>
    ///     Gets the templated message with the human-readable explanation.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("detailTemplated")]
    public TemplatedMessage? DetailTemplated { get; init; }
}
