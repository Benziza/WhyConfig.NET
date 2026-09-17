using Microsoft.Extensions.Configuration;

namespace WhyConfig.Core;

/// <summary>Inspects the providers of an application's actual configuration root.</summary>
public static class ConfigExplainer
{
    public static ConfigurationExplanation Explain(
        IConfigurationRoot configuration,
        string key,
        Func<IConfigurationProvider, string>? providerName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var found = new List<(string Name, string? Value)>();
        foreach (var provider in configuration.Providers)
        {
            if (!provider.TryGet(key, out var value))
            {
                continue;
            }

            var name = providerName?.Invoke(provider) ?? provider.ToString();
            found.Add((string.IsNullOrWhiteSpace(name) ? provider.GetType().Name : name, value));
        }

        var sources = found.Select((source, index) =>
            new ProviderValue(source.Name, source.Value, index == found.Count - 1)).ToArray();

        return new ConfigurationExplanation(key, configuration[key], sources);
    }
}
