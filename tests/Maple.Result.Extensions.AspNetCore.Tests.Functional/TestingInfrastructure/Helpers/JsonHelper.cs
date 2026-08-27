using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Maple.Result.Extensions.AspNetCore.Tests.Functional.TestingInfrastructure.Helpers;

internal static class JsonHelper
{
    private const string TraceIdPropertyName = "traceId";

    /// <summary>
    ///     Reads the response body and returns it as a normalized JSON text, with the non-deterministic
    ///     <c>traceId</c> extension removed, so that it can be compared with an expected JSON text.
    /// </summary>
    internal static async Task<string> ReadNormalizedJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
            return json;

        var node = JsonNode.Parse(json)
                   ?? throw new InvalidOperationException("The response body is not a valid JSON document.");

        if (node is JsonObject jsonObject)
            jsonObject.Remove(TraceIdPropertyName);

        return node.ToJsonString();
    }

    /// <summary>
    ///     Returns the expected JSON text normalized the same way as the response body, so that the formatting
    ///     of the expected JSON literal does not affect the comparison.
    /// </summary>
    internal static string Normalize(string json)
    {
        var node = JsonNode.Parse(json)
                   ?? throw new InvalidOperationException("The expected JSON text is not a valid JSON document.");

        return node.ToJsonString();
    }
}
