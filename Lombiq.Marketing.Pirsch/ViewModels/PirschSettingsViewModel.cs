namespace Lombiq.Marketing.Pirsch.ViewModels;

public class PirschSettingsViewModel
{
    public string ClientSecret { get; set; } = string.Empty;

    public bool HasClientSecret { get; set; }

    public bool ClearClientSecret { get; set; }

    public string ClientSideCodeSnippet { get; set; } = string.Empty;
}
