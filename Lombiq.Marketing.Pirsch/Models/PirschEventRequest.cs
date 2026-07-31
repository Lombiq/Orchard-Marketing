using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Lombiq.Marketing.Pirsch.Models;

public sealed class PirschEventRequest : PirschRequestData
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("event_name")]
    public string? EventName { get; set; }

    [JsonPropertyName("event_duration")]
    public int? EventDuration { get; set; }

    [JsonPropertyName("event_meta")]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Necessary for deserialization.")]
    public IDictionary<string, string>? EventMeta { get; set; } = new Dictionary<string, string>();

    [JsonPropertyName("non_interactive")]
    public bool? NonInteractive { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
