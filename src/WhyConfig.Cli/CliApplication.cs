using System.Text.Json;
using WhyConfig.Core;

namespace WhyConfig.Cli;

public static class CliApplication
{
    private const string Usage = """
        Usage: whyconfig explain <key> [options]

        Options:
          --project <directory|csproj>  Project to inspect (default: current directory)
          --environment <name>          Environment (default: DOTNET_ENVIRONMENT, ASPNETCORE_ENVIRONMENT, then Production)
          --secrets-id <id>             User Secrets ID (Development only)
          --app-arg <Key=Value>         Application command-line argument; repeatable
          --show-secrets                Show values for keys that look sensitive
          --json                        Emit JSON
          --help                        Show this help
        """;

    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            output.WriteLine(Usage);
            return args.Length == 0 ? 2 : 0;
        }

        try
        {
            var options = CliOptions.Parse(args);
            var offline = OfflineConfiguration.Build(options);
            var providers = offline.Root.Providers.ToArray();
            var names = providers.Select((provider, index) =>
                (provider, name: offline.ProviderNames[index]))
                .ToDictionary(item => item.provider, item => item.name);
            var explanation = ConfigExplainer.Explain(offline.Root, options.Key, provider => names[provider]);
            WriteExplanation(explanation, options, output);
            return explanation.IsPresent ? 0 : 1;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidDataException
            or System.Xml.XmlException or FormatException)
        {
            error.WriteLine($"whyconfig: {exception.Message}");
            error.WriteLine(Usage);
            return 2;
        }
    }

    private static void WriteExplanation(ConfigurationExplanation explanation, CliOptions options, TextWriter output)
    {
        var hide = !options.ShowSecrets && IsSensitiveKey(explanation.Key);
        string Display(string? value) => value is null ? "(null)" : hide ? "<redacted>" : value;
        string? JsonValue(string? value) => value is null ? null : hide ? "<redacted>" : value;

        if (options.Json)
        {
            output.WriteLine(JsonSerializer.Serialize(new
            {
                key = explanation.Key,
                found = explanation.IsPresent,
                effectiveValue = explanation.IsPresent ? JsonValue(explanation.EffectiveValue) : null,
                winner = explanation.Winner?.Provider,
                sources = explanation.Sources.Select(source => new
                {
                    provider = source.Provider,
                    value = JsonValue(source.Value),
                    status = source.IsWinner ? "winner" : "overridden"
                })
            }, new JsonSerializerOptions { WriteIndented = true }));
            return;
        }

        output.WriteLine(explanation.Key);
        output.WriteLine();
        if (!explanation.IsPresent)
        {
            output.WriteLine("No provider supplied this key.");
            return;
        }

        foreach (var source in explanation.Sources)
        {
            output.WriteLine(source.Provider);
            output.WriteLine($"  {Display(source.Value)}");
            output.WriteLine($"  {(source.IsWinner ? "WINNER" : "overridden")}");
            output.WriteLine();
        }

        output.WriteLine($"Effective value: {Display(explanation.EffectiveValue)}");
    }

    private static bool IsSensitiveKey(string key)
    {
        var leaf = key.Split(':').Last().Replace("_", "", StringComparison.Ordinal).ToLowerInvariant();
        return leaf is "key" or "credential" or "connectionstring"
            || leaf.Contains("password", StringComparison.Ordinal)
            || leaf.Contains("secret", StringComparison.Ordinal)
            || leaf.Contains("token", StringComparison.Ordinal)
            || leaf.Contains("apikey", StringComparison.Ordinal)
            || leaf.Contains("privatekey", StringComparison.Ordinal);
    }
}
