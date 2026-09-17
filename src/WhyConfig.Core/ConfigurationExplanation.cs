namespace WhyConfig.Core;

/// <summary>The values supplied for one key, in provider registration order.</summary>
public sealed record ConfigurationExplanation(
    string Key,
    string? EffectiveValue,
    IReadOnlyList<ProviderValue> Sources)
{
    public bool IsPresent => Sources.Count > 0;

    public ProviderValue? Winner => Sources.Count == 0 ? null : Sources[^1];
}

/// <summary>A provider that supplied the requested key.</summary>
public sealed record ProviderValue(string Provider, string? Value, bool IsWinner);
