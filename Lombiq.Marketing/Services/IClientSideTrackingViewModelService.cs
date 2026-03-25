using Lombiq.Marketing.Models;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public interface IClientSideTrackingViewModelService
{
    Task<ClientSideTrackingViewModel?> GetViewModelAsync();

    Task InvalidateCachedViewModelAsync();
}
