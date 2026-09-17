# Releasing WhyConfig.NET

The GitHub repository can stay private while the tool package is public on NuGet.org.

1. Create a NuGet.org account and a [Push API key](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package#create-an-api-key). For the first release, allow new packages in the key's package scope.
2. Save the key as the `NUGET_API_KEY` secret in this repository's **Settings → Secrets and variables → Actions**. Do not commit the key.
3. Run the **Publish NuGet tool** workflow from `main` in the Actions tab.
4. After NuGet.org lists the package, verify installation with `dotnet tool install --global WhyConfig.NET` and `whyconfig --help`.

For another release, change `Version` in `src/WhyConfig.Cli/WhyConfig.Cli.csproj`, merge it, then run the workflow again. NuGet.org does not allow replacing a published package version.
