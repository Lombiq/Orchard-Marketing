using Lombiq.Marketing.Pirsch.ViewModels;
using System.Threading.Tasks;

namespace Lombiq.Marketing.Pirsch.Services;

public interface IPirschClientSideTrackingViewModelService
{
    Task<PirschClientSideTrackingViewModel?> GetViewModelAsync();

    Task InvalidateCachedViewModelAsync();
}
