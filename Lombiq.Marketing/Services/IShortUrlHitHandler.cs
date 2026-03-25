using Lombiq.Marketing.Models;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public interface IShortUrlHitHandler
{
    Task HandleHitAsync(ShortUrlHitContext context);
}
