# WhyConfig.NET

See which .NET configuration source set a value, and which values it replaced.

![Animated demo of whyconfig explaining Database:Host](assets/whyconfig-demo.gif)

## Install

Requires the .NET 10 SDK.

```bash
dotnet tool install -g WhyConfig.NET
```

## Use

```bash
whyconfig explain Database:Host --project path/to/YourApp --environment Development
```

The CLI checks the project's `appsettings` files, Development User Secrets, and current environment variables. Run `whyconfig --help` for options.

For the exact configuration of a running app, use `WhyConfig.Core` with its `IConfigurationRoot`:

```csharp
using WhyConfig.Core;

var builder = WebApplication.CreateBuilder(args);
var result = ConfigExplainer.Explain(builder.Configuration, "Database:Host");
Console.WriteLine($"{result.Winner?.Provider}: {result.EffectiveValue}");
```
