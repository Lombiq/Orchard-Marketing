using System.Threading.Tasks;
using Lombiq.Marketing.ViewModels;

namespace Lombiq.Marketing.Services;

public interface IPirschClientSideTrackingViewModelService
{
    Task<PirschClientSideTrackingViewModel> GetViewModelAsync();

    Task InvalidateCachedViewModelAsync();
}
