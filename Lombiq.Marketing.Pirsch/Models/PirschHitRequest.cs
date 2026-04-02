using System;
using System.Text.Json.Serialization;

namespace Lombiq.Marketing.Pirsch.Models;

public sealed class PirschHitRequest : PirschRequestData
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("time")]
    public DateTime? Time { get; set; }
}
