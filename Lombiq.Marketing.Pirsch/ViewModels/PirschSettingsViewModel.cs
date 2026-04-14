namespace Lombiq.Marketing.Pirsch.ViewModels;

public class PirschSettingsViewModel
{
    public string? ClientSecret { get; set; }

    public bool HasClientSecret { get; set; }

    public bool ClearClientSecret { get; set; }

    public string? ClientSideCodeSnippet { get; set; }

    public string? DataDev { get; set; }

    public string? AutoRenderZone { get; set; }
}
