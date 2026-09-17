# WhyConfig.NET

**Why did my .NET app get this configuration value?** WhyConfig.NET shows every provider that supplied a key, in precedence order, and marks the provider that won.

```text
Database:Host

appsettings.json
  db.production.com
  overridden

appsettings.Development.json
  localhost
  overridden

Environment Variables
  db.docker
  WINNER

Effective value: db.docker
```

The project has two entry points:

- **WhyConfig.Core** inspects an application's actual `IConfigurationRoot`. Use it when you need the exact provider chain, including custom providers.
- **whyconfig** is a CLI for investigating a project outside the running app. It rebuilds the common .NET application configuration layers from the files and environment available to the CLI.

## Quick start

Requires the .NET 10 SDK. From this repository:

```bash
dotnet run --project src/WhyConfig.Cli -- explain Database:Host --project path/to/YourApp --environment Development
```

The project path may be a directory or a `.csproj` file. If omitted, it defaults to the current directory. For a shell command named `whyconfig`, build and install the local tool package:

```bash
dotnet pack src/WhyConfig.Cli -c Release -o artifacts
dotnet tool install WhyConfig.NET --tool-path .tools --add-source artifacts
```

Then run `.tools/whyconfig explain Database:Host --project path/to/YourApp` on macOS/Linux, or `.tools\whyconfig.exe explain Database:Host --project path\to\YourApp` on Windows.

## CLI options

```text
whyconfig explain <key> [options]

--project <directory|csproj>   Project to inspect
--environment <name>           Environment to use
--secrets-id <id>              Explicit User Secrets ID
--app-arg <Key=Value>          Application argument; repeatable
--show-secrets                 Show values for keys that look sensitive
--json                         Emit machine-readable JSON
```

For example, if the application was launched with an argument that overrides `Api:Url`:

```bash
whyconfig explain Api:Url --project ./MyApp --environment Development --app-arg Api:Url=http://localhost:5000
```

The CLI reads `appsettings.json`, then `appsettings.{Environment}.json`, then User Secrets in `Development` when it can find a `UserSecretsId`, then its own process environment variables, then any `--app-arg` values. `--secrets-id` supplies the ID directly if it is not declared in the selected `.csproj`. If no environment is supplied, the CLI uses `DOTNET_ENVIRONMENT`, then `ASPNETCORE_ENVIRONMENT`, then `Production`. Supply `--environment` when the application's environment is different or uncertain.

Exit code `0` means the key was found, `1` means no provider supplied it, and `2` means the command or input was invalid.

## Inspect the running application's configuration

Reference `src/WhyConfig.Core/WhyConfig.Core.csproj` from your application, then call the library with the actual root:

```csharp
using WhyConfig.Core;

var builder = WebApplication.CreateBuilder(args);
var explanation = ConfigExplainer.Explain(builder.Configuration, "Database:Host");

Console.WriteLine($"Winner: {explanation.Winner?.Provider}");
Console.WriteLine($"Effective value: {explanation.EffectiveValue}");
foreach (var source in explanation.Sources)
{
    Console.WriteLine($"{source.Provider}: {source.Value} ({(source.IsWinner ? "winner" : "overridden")})");
}
```

`IConfigurationRoot.Providers` contains the registered providers in order; later providers take precedence when they contain the same key. The library calls each provider's `TryGet` and reads the effective value from the root. This follows the [Microsoft configuration documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0).

## Accuracy and sensitive values

The CLI reconstructs a **common default** configuration setup. It cannot discover custom providers, application-specific registration order, launch arguments that were not supplied through `--app-arg`, or environment variables inside a different process or container. For an exact answer about a running app, call `WhyConfig.Core` with that app's `IConfigurationRoot`.

The CLI masks values for keys with names such as `Password`, `Token`, `Secret`, `ApiKey`, and `ConnectionString` unless `--show-secrets` is set. This is a naming heuristic; other keys may still contain sensitive data. The library returns raw values to its caller.

## Development

```bash
dotnet test WhyConfig.slnx -c Release
dotnet pack src/WhyConfig.Cli -c Release -o artifacts
```
