using System.Threading.Tasks;

namespace Lombiq.Marketing.UrlShortener.Events;

/// <summary>
/// Handles the redirect flow of a short URL before the response is sent.
/// </summary>
public interface IShortUrlRedirectEventHandler
{
    /// <summary>
    /// Executes before the short URL redirect is issued.
    /// </summary>
    /// <param name="context">The redirect context.</param>
    Task RedirectingAsync(ShortUrlRedirectContext context) => Task.CompletedTask;
}
