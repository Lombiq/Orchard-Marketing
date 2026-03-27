# Lombiq Marketing for Orchard Core

[![Lombiq.Marketing NuGet](https://img.shields.io/nuget/v/Lombiq.Marketing?label=Lombiq.Marketing)](https://www.nuget.org/packages/Lombiq.Marketing/)

## About

This module contains the shared pieces for the Lombiq Marketing modules.

## Documentation

This module contains the feature below.

### `Lombiq.Marketing`

This feature contains the common abstractions and services used by the other marketing modules.

Right now it mainly does two things:

- It provides a common way to register marketing hits for short URL redirects.
- It provides a common way to render client-side tracking markup from one or more provider modules.

Usually you won't use this module on its own. It is the base for other modules, like the URL shortener and analytics integrations.

To use the module, follow these steps:

- Add the module to your solution.
- Enable the `Lombiq.Marketing` feature if a dependent module doesn't enable it for you.
- Add one or more marketing provider modules on top of it.

To implement a marketing provider module like `Lombiq.Marketing.Pirsch`, follow these steps:

- Reference `Lombiq.Marketing` from your provider project.
- If you want to register hits for short URL redirects, implement `IShortUrlHitHandler`.
- If you want to inject client-side tracking markup, implement `IClientSideTrackingProvider`.
- Register your services from your module startup.
- Add your own settings, API client, proxy, or other provider-specific logic in your provider module.
- If your settings change the rendered client-side tracking markup, use `ClientSideTrackingSiteDisplayDriver<TSettings>` for the settings driver so the shared markup cache is invalidated automatically.
- If needed, add your own admin UI, maintenance tasks, and tests the same way as the Pirsch module does.
