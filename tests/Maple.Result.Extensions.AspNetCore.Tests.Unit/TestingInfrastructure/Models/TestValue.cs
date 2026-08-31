using System.Text.Json.Serialization;

namespace Maple.Result.Extensions.AspNetCore.Tests.Unit.TestingInfrastructure.Models;

public sealed record TestValue
{
    public TestValue(int id, string name)
    {
        Id = id;
        Name = name;
    }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }
}
