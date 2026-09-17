# WhyConfig.NET

See which .NET configuration source set a value, and which values it replaced.

## Install

Requires the .NET 10 SDK.

```bash
dotnet tool install -g WhyConfig.NET
```

## Use

From your app's project directory:

```bash
whyconfig explain Database:Host
```

Example output:

```text
Database:Host

appsettings.json
  db.production.com
  overridden

appsettings.Development.json
  localhost
  WINNER

Effective value: localhost
```

To inspect another project, add `--project path/to/YourApp`. Run `whyconfig --help` for more options.
