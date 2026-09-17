namespace WhyConfig.Cli;

public sealed record CliOptions(
    string Key,
    string ProjectPath,
    string? Environment,
    string? SecretsId,
    IReadOnlyList<string> AppArguments,
    bool ShowSecrets,
    bool Json)
{
    public static CliOptions Parse(string[] args)
    {
        if (args.Length < 2 || args[0] != "explain" || args[1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException("Expected: whyconfig explain <key> [options].");
        }

        string? project = null;
        string? environment = null;
        string? secretsId = null;
        var appArguments = new List<string>();
        var showSecrets = false;
        var json = false;

        for (var index = 2; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--project":
                    project = ReadValue(args, ref index);
                    break;
                case "--environment":
                    environment = ReadValue(args, ref index);
                    break;
                case "--secrets-id":
                    secretsId = ReadValue(args, ref index);
                    break;
                case "--app-arg":
                    var appArgument = ReadValue(args, ref index);
                    if (appArgument.IndexOf('=') <= 0)
                    {
                        throw new ArgumentException("--app-arg requires Key=Value.");
                    }
                    appArguments.Add(appArgument);
                    break;
                case "--show-secrets":
                    showSecrets = true;
                    break;
                case "--json":
                    json = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {args[index]}");
            }
        }

        return new CliOptions(args[1], project ?? Directory.GetCurrentDirectory(), environment,
            secretsId, appArguments, showSecrets, json);
    }

    private static string ReadValue(string[] args, ref int index)
    {
        if (++index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new ArgumentException($"Missing value for {args[index - 1]}.");
        }

        return args[index];
    }
}
