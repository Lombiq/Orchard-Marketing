using Lombiq.Marketing.Pirsch.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

public interface IPirschApiClient
{
    Task SendHitAsync(PirschHitRequest request, CancellationToken cancellationToken = default);
}
