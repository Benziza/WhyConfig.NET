# Releasing WhyConfig.NET

The GitHub repository can stay private while the tool package is public on NuGet.org.

1. In the [Benziza NuGet.org account](https://www.nuget.org/profiles/Benziza), create a [Trusted Publishing policy](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) with package owner `Benziza`, repository owner `Benziza`, repository `WhyConfig.NET`, workflow file `publish-nuget.yml`, no environment, and package glob `WhyConfig.NET`. Allow publishing new packages and new versions.
2. Run the **Publish NuGet tool** workflow from `main` in the Actions tab. It uses GitHub OIDC to obtain a short-lived publishing credential; no API key secret is needed.
3. After NuGet.org lists the package, verify installation with `dotnet tool install --global WhyConfig.NET` and `whyconfig --help`.

For another release, change `Version` in `src/WhyConfig.Cli/WhyConfig.Cli.csproj`, merge it, then run the workflow again. NuGet.org does not allow replacing a published package version.
