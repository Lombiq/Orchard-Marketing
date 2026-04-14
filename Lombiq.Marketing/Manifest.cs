using Lombiq.Marketing.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Marketing",
    Author = "Lombiq Technologies",
    Version = "0.0.1",
    Description = "Contains the common abstractions and services used by the other marketing modules",
    Website = "https://github.com/Lombiq/Orchard-Marketing"
)]

[assembly: Feature(
    Id = FeatureIds.Base,
    Name = "Lombiq Marketing",
    Category = "Marketing",
    Description = "Contains the common abstractions and services used by the other marketing modules",
    Dependencies =
    [
        Lombiq.Marketing.UrlShortener.Constants.FeatureIds.Base,
    ]
)]
