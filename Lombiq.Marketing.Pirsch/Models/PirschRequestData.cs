using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Lombiq.Marketing.Pirsch.Models;

public class PirschRequestData
{
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("user_agent")]
    public string? UserAgent { get; set; }

    [JsonPropertyName("accept_language")]
    public string? AcceptLanguage { get; set; }

    [JsonPropertyName("sec_ch_ua")]
    public string? SecChUa { get; set; }

    [JsonPropertyName("sec_ch_ua_mobile")]
    public string? SecChUaMobile { get; set; }

    [JsonPropertyName("sec_ch_ua_platform")]
    public string? SecChUaPlatform { get; set; }

    [JsonPropertyName("sec_ch_ua_platform_version")]
    public string? SecChUaPlatformVersion { get; set; }

    [JsonPropertyName("sec_ch_width")]
    public string? SecChWidth { get; set; }

    [JsonPropertyName("sec_ch_viewport_width")]
    public string? SecChViewportWidth { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("referrer")]
    public string? Referrer { get; set; }

    [JsonPropertyName("screen_width")]
    public int? ScreenWidth { get; set; }

    [JsonPropertyName("screen_height")]
    public int? ScreenHeight { get; set; }

    [JsonPropertyName("disable_bot_filter")]
    public bool? DisableBotFilter { get; set; }

    [JsonPropertyName("tags")]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Necessary for deserialization.")]
    public IDictionary<string, string>? Tags { get; set; } = new Dictionary<string, string>();
}
