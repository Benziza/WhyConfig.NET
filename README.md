# WhyConfig.NET

See which .NET configuration source set a value, and which values it replaced.

![Animated demo of whyconfig explaining Database:Host](assets/whyconfig-demo.gif)

## Try it

Requires the .NET 10 SDK. From this repository, run:

```bash
dotnet run --project src/WhyConfig.Cli -- explain Database:Host --project path/to/YourApp --environment Development
```

The CLI checks the project's `appsettings` files, Development User Secrets, and current environment variables. Run `dotnet run --project src/WhyConfig.Cli -- --help` for options.

For the exact configuration of a running app, use `WhyConfig.Core` with its `IConfigurationRoot`:

```csharp
using WhyConfig.Core;

var builder = WebApplication.CreateBuilder(args);
var result = ConfigExplainer.Explain(builder.Configuration, "Database:Host");
Console.WriteLine($"{result.Winner?.Provider}: {result.EffectiveValue}");
```
