# Lombiq Marketing for Orchard Core

[![Lombiq.Marketing NuGet](https://img.shields.io/nuget/v/Lombiq.Marketing?label=Lombiq.Marketing)](https://www.nuget.org/packages/Lombiq.Marketing/)
[![Lombiq.Marketing.Pirsch NuGet](https://img.shields.io/nuget/v/Lombiq.Marketing.Pirsch?label=Lombiq.Marketing.Pirsch)](https://www.nuget.org/packages/Lombiq.Marketing.Pirsch/)
[![Lombiq.Marketing.UrlShortener NuGet](https://img.shields.io/nuget/v/Lombiq.Marketing.UrlShortener?label=Lombiq.Marketing.UrlShortener)](https://www.nuget.org/packages/Lombiq.Marketing.UrlShortener/)

## About

A set of Orchard Core modules for marketing-related functionality, including shared marketing abstractions, short URL handling, and analytics provider integrations.

## Documentation

This folder currently contains the following modules.

### `Lombiq.Marketing`

The shared foundation for Orchard Marketing modules. It contains abstractions for registering marketing hits from short URL redirects and for composing client-side tracking markup from one or more provider modules.

See also: [Lombiq.Marketing/Readme.md](./Lombiq.Marketing/Readme.md)

### `Lombiq.Marketing.Pirsch`

A concrete analytics provider module for [Pirsch](https://pirsch.io/ref/testlink). It adds Pirsch site settings, client-side tracking, first-party proxying, server-side hit reporting, and maintenance for clearing admin-stored client secrets.

See also: [Lombiq.Marketing.Pirsch/Readme.md](./Lombiq.Marketing.Pirsch/Readme.md)

### `Lombiq.Marketing.UrlShortener`

A URL shortener module that adds a Short URL content type, redirect middleware, and redirect-related extensibility points that can be used by marketing modules.

See also: [Lombiq.Marketing.UrlShortener/Readme.md](./Lombiq.Marketing.UrlShortener/Readme.md)

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
