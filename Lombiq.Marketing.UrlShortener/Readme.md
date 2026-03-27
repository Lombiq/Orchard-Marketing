# Lombiq Marketing - URL Shortener for Orchard Core

[![Lombiq.Marketing.UrlShortener NuGet](https://img.shields.io/nuget/v/Lombiq.Marketing.UrlShortener?label=Lombiq.Marketing.UrlShortener)](https://www.nuget.org/packages/Lombiq.Marketing.UrlShortener/)

## About

This module adds short URL support to Orchard Core.

## Documentation

This module contains the feature below.

### `Lombiq.Marketing.UrlShortener`

This feature adds a `ShortUrl` content type that you can use to create short URL redirects.

Each short URL points to a destination URL. You can also store UTM values with the short URL entry.

The redirect itself goes to the configured destination URL as-is. The UTM values are not added to the browser redirect URL. The UTM values are sent to the registered `IShortUrlRedirectEventHandler` implementations as part of the redirect context, so you can use them for hit registration in your other modules.

The module also contains extension points that other modules can use when a short URL redirect happens. Implement a `IShortUrlRedirectEventHandler` to get notified about the redirect and access the short URL entry and the context of the request.

To use the module, follow these steps:

- Enable the `Lombiq.Marketing.UrlShortener` feature from the admin.
- Open `Tools -> Short URLs`.
- Create a new short URL content item.
- Set the short URL, destination URL, and optional UTM values.
