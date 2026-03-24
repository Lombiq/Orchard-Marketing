using System;
using System.Text.Json.Serialization;

namespace Lombiq.Marketing.Pirsch.Models;

public sealed class PirschHitRequest : PirschRequestData
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; set; }
}
