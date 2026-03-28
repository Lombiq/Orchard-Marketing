using Lombiq.Marketing.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Marketing",
    Author = "Lombiq Technologies",
    Version = "0.0.1",
    Description = "",
    Website = "https://github.com/Lombiq/Orchard-Marketing"
)]

[assembly: Feature(
    Id = FeatureIds.Base,
    Name = "Lombiq Marketing",
    Category = "Marketing",
    Description = "",
    Dependencies =
    [
        Lombiq.Marketing.UrlShortener.Constants.FeatureIds.Base,
    ]
)]
