using Microsoft.Extensions.Configuration;
using WhyConfig.Core;

namespace WhyConfig.Core.Tests;

public class ConfigExplainerTests
{
    [Fact]
    public void ExplainsOverridesInProviderOrder()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:Host"] = "db.production.com" })
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:Host"] = "localhost" })
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:Host"] = "db.docker" })
            .Build();

        var explanation = ConfigExplainer.Explain(configuration, "Database:Host",
            provider => $"Layer {configuration.Providers.ToList().IndexOf(provider) + 1}");

        Assert.Equal("db.docker", explanation.EffectiveValue);
        Assert.Equal(new[] { "db.production.com", "localhost", "db.docker" },
            explanation.Sources.Select(source => source.Value));
        Assert.Equal(new[] { false, false, true }, explanation.Sources.Select(source => source.IsWinner));
        Assert.Equal("Layer 3", explanation.Winner?.Provider);
    }

    [Fact]
    public void MissingKeyHasNoWinner()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        var explanation = ConfigExplainer.Explain(configuration, "Missing");

        Assert.False(explanation.IsPresent);
        Assert.Null(explanation.Winner);
        Assert.Null(explanation.EffectiveValue);
    }

    [Fact]
    public void NullInLastProviderStillWins()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Value"] = "earlier" })
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Value"] = null })
            .Build();

        var explanation = ConfigExplainer.Explain(configuration, "Value");

        Assert.True(explanation.IsPresent);
        Assert.Null(explanation.EffectiveValue);
        Assert.Null(explanation.Winner?.Value);
        Assert.True(explanation.Winner?.IsWinner);
    }

    [Fact]
    public void RejectsEmptyKey()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentException>(() => ConfigExplainer.Explain(configuration, " "));
    }
}
