using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace WhyConfig.Cli;

public sealed class OfflineConfiguration
{
    private OfflineConfiguration(IConfigurationRoot root, IReadOnlyList<string> providerNames)
    {
        Root = root;
        ProviderNames = providerNames;
    }

    public IConfigurationRoot Root { get; }

    public IReadOnlyList<string> ProviderNames { get; }

    public static OfflineConfiguration Build(CliOptions options)
    {
        var projectPath = Path.GetFullPath(options.ProjectPath);
        string directory;
        string? projectFile;

        if (File.Exists(projectPath) && Path.GetExtension(projectPath).Equals(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            projectFile = projectPath;
            directory = Path.GetDirectoryName(projectPath)!;
        }
        else if (Directory.Exists(projectPath))
        {
            directory = projectPath;
            var projects = Directory.GetFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly);
            projectFile = projects.Length == 1 ? projects[0] : null;
        }
        else
        {
            throw new ArgumentException($"Project path does not exist: {projectPath}");
        }

        var environment = options.Environment
            ?? System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Production";

        if (string.IsNullOrWhiteSpace(environment) || environment.Contains('/') || environment.Contains('\\'))
        {
            throw new ArgumentException("Environment must be a nonempty name without path separators.");
        }

        var builder = new ConfigurationBuilder().SetBasePath(directory);
        var names = new List<string>();

        builder.AddJsonFile("appsettings.json", optional: true);
        names.Add("appsettings.json");

        builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
        names.Add($"appsettings.{environment}.json");

        if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            var secretsId = options.SecretsId ?? ReadSecretsId(projectFile);
            if (!string.IsNullOrWhiteSpace(secretsId))
            {
                builder.AddUserSecrets(secretsId);
                names.Add("User Secrets");
            }
        }

        builder.AddEnvironmentVariables();
        names.Add("Environment Variables");

        if (options.AppArguments.Count > 0)
        {
            builder.AddCommandLine(options.AppArguments.Select(argument => $"--{argument}").ToArray());
            names.Add("Application Command Line");
        }

        var root = builder.Build();
        return new OfflineConfiguration(root, names);
    }

    private static string? ReadSecretsId(string? projectFile)
    {
        if (projectFile is null)
        {
            return null;
        }

        var project = XDocument.Load(projectFile);
        return project.Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "UserSecretsId")
            ?.Value.Trim();
    }
}
