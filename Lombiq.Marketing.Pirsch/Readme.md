# Lombiq Marketing - Pirsch analytics for Orchard Core

[![Lombiq.Marketing.Pirsch NuGet](https://img.shields.io/nuget/v/Lombiq.Marketing.Pirsch?label=Lombiq.Marketing.Pirsch)](https://www.nuget.org/packages/Lombiq.Marketing.Pirsch/)

## About

This module integrates [Pirsch](https://pirsch.io/ref/BQdZP2jgqM) into Orchard Core.

## Documentation

This module contains the feature below.

### `Lombiq.Marketing.Pirsch`

This feature adds a basic Pirsch integration for Orchard Core.

It supports these scenarios:

- Client-side tracking with a script snippet.
- [Proxy](https://docs.pirsch.io/advanced/proxy) for the Pirsch script.
- Pirsch script is sanitized and proxy is used.
- Automatic rendering of the tracking script into a selected zone. Otherwise, manually use the `ClientSideTracking` shape in your theme's head section.
- Server-side hit registration for [short URL redirects](../Lombiq.Marketing.UrlShortener/Readme.md).
- [Client Hints](https://docs.pirsch.io/get-started/client-hints) support for better tracking.
- Admin settings with appsettings fallback.
- Maintenance for clearing the admin-stored client secret.

Pirsch settings can come from appsettings or from the Orchard admin. If both are set, the admin settings take precedence.

To use the module, follow these steps:

1. Enable the `Lombiq.Marketing.Pirsch` feature from the admin.
2. Open `Configuration -> Settings -> Marketing -> Pirsch`.
3. Configure the client secret, client-side tracking snippet, and optional other settings.
4. If you want automatic rendering, set the zone where the script should be rendered.

Example appsettings or environment variable configuration:

```json5
{
  "OrchardCore": {
    "Lombiq_Marketing": {
      "Pirsch": {
        // If you want to clear the admin-stored Pirsch API client secret during tenant start.
        "ClearClientSecretsMaintenance": {
          "IsEnabled": true
        },
        // The client-side tracking snippet to inject into the theme. You can also set this from the admin. This
        // will be also sanitized before rendering. You can get this on your Pirsch dashboard under Settings → Integration.
        "ClientSideCodeSnippet": "<script defer src=\"https://api.pirsch.io/pa.js\" id=\"pianjs\" data-code=\"CuvrMvtROyq2u4D5gCFIwCk6qrYnMJlN\" data-dev=\"test\"></script>",
        // The data-dev attribute value to use in the client-side tracking snippet, if not set in the snippet 
        // itself. More info: https://docs.pirsch.io/get-started/frontend-integration#testing-pirsch-locally
        "DataDev": "dev-url.com",
        // The layout zone where the client-side tracking snippet should be rendered automatically.
        "AutoRenderZone": "Head",
        // The Pirsch API client secret. We do not recommend setting this from appsettings for security reasons,
        // but you can do it for testing or if you have a secure way to store secrets in your hosting environment.
        // You can get this on your Pirsch dashboard under Settings → Integration → Clients to configure access to only
        //a single site; use your account menu → Account → Clients to access all your sites.
        "ClientSecret": "your-client-secret"
      }
    }
  }
}
```
