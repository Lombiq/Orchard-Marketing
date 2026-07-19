namespace Lombiq.Marketing.Pirsch.Constants;

public static class PirschProxyConstants
{
    public const string ProxyPathPrefix = "/secret-sauce";
    public const string ProxyScriptPath = ProxyPathPrefix + "/sauce.js";
    public const string ProxyPageViewPath = ProxyPathPrefix + "/pv";
    public const string ProxyEventPath = ProxyPathPrefix + "/e";
    public const string ProxySessionPath = ProxyPathPrefix + "/s";

    public const string PirschBaseUrl = "https://api.pirsch.io";
    public const string PirschScriptPath = "/pa.js";
    public const string PirschPageViewPath = "/api/v1/hit";
    public const string PirschEventPath = "/api/v1/event";
    public const string PirschSessionPath = "/api/v1/session";
}
