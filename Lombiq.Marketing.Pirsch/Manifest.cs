using Lombiq.Marketing.Pirsch.Constants;
using OrchardCore.Modules.Manifest;
using MarketingFeatureIds = Lombiq.Marketing.Constants.FeatureIds;

[assembly: Module(
    Name = "Lombiq Marketing - Pirsch",
    Author = "Lombiq Technologies",
    Version = "0.0.1",
    Description = "Integrate the basics of Pirsch into Orchard Core.",
    Website = "https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions"
)]

[assembly: Feature(
    Id = FeatureIds.Base,
    Name = "Lombiq Marketing - Pirsch",
    Category = "Marketing",
    Description = "Integrate the basics of Pirsch into Orchard Core.",
    Dependencies =
    [
        MarketingFeatureIds.Base,
    ]
)]
