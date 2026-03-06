using System.Threading.Tasks;

namespace Lombiq.Marketing.UrlShortener.Events;

public interface IShortUrlRedirectEventHandler
{
    Task RedirectingAsync(ShortUrlRedirectContext context) => Task.CompletedTask;
}
