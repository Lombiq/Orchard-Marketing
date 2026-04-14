using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

/// <summary>
/// Proxies browser-side Pirsch requests through the application.
/// </summary>
public interface IPirschProxyService
{
    /// <summary>
    /// Proxies and caches the Pirsch tracking script request.
    /// </summary>
    Task ProxyScriptAsync();

    /// <summary>
    /// Proxies a Pirsch page view request.
    /// </summary>
    Task ProxyPageViewAsync();

    /// <summary>
    /// Proxies a Pirsch event request.
    /// </summary>
    Task ProxyEventAsync();

    /// <summary>
    /// Proxies a Pirsch session keep-alive request.
    /// </summary>
    Task ProxySessionAsync();
}
