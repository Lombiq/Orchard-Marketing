using Lombiq.HelpfulLibraries.AspNetCore.Security;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Lombiq.HelpfulLibraries.AspNetCore.Security.ContentSecurityPolicyDirectives;

namespace Lombiq.Marketing.Pirsch.Services;

public sealed class PirschSecurityPolicyProvider : IContentSecurityPolicyProvider
{
    public ValueTask UpdateAsync(IDictionary<string, string> securityPolicies, HttpContext context)
    {
        CspHelper.MergeValues(securityPolicies, ScriptSrc, "api.pirsch.io");
        CspHelper.MergeValues(securityPolicies, ConnectSrc, "api.pirsch.io");

        return ValueTask.CompletedTask;
    }
}
