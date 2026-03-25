using System.Threading.Tasks;

namespace Lombiq.Marketing.Services;

public interface IClientSideTrackingProvider
{
    Task<string?> GetClientSideTrackingMarkupAsync();
}
