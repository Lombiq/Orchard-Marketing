using Lombiq.Marketing.Pirsch.Constants;
using OrchardCore.Modules.Manifest;
using MarketingFeatureIds = Lombiq.Marketing.Constants.FeatureIds;

[assembly: Module(
    Name = "Lombiq Marketing - Pirsch analytics",
    Author = "Lombiq Technologies",
    Version = "0.0.1",
    Description = "Integrate the basics of Pirsch analytics into Orchard Core.",
    Website = "https://github.com/Lombiq/Orchard-Marketing"
)]

[assembly: Feature(
    Id = FeatureIds.Base,
    Name = "Lombiq Marketing - Pirsch analytics",
    Category = "Marketing",
    Description = "Integrate the basics of Pirsch analytics into Orchard Core.",
    Dependencies =
    [
        MarketingFeatureIds.Base,
    ]
)]
