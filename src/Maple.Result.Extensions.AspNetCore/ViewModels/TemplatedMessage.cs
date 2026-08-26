using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Maple.Result.Extensions.AspNetCore.ViewModels;

public record TemplatedMessage
{
    public TemplatedMessage(string TemplateId, IReadOnlyDictionary<string, object>? Params = null)
    {
        this.TemplateId = TemplateId;
        this.Params = Params;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("templateId")]
    public string TemplateId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("params")]
    public IReadOnlyDictionary<string, object>? Params { get; init; }

    internal static TemplatedMessage? Map(Maple.Result.TemplatedMessage? source)
    {
        return source is null
            ? null
            : new TemplatedMessage(source.TemplateId, source.Params);
    }
}
