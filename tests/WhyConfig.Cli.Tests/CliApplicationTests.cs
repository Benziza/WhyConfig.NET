using System.Text.Json;
using WhyConfig.Cli;

namespace WhyConfig.Cli.Tests;

public class CliApplicationTests
{
    [Fact]
    public void ShowsJsonEnvironmentAndApplicationArgumentPrecedence()
    {
        using var project = new TempProject();
        project.Write("appsettings.json", """{"Api":{"Url":"https://production.example"}}""");
        project.Write("appsettings.Development.json", """{"Api":{"Url":"http://localhost:5000"}}""");
        var previous = Environment.GetEnvironmentVariable("Api__Url");
        Environment.SetEnvironmentVariable("Api__Url", "http://container:5000");

        try
        {
            var (code, output, error) = Run("explain", "Api:Url", "--project", project.Path,
                "--environment", "Development", "--app-arg", "Api:Url=http://argument:5000");

            Assert.Equal(0, code);
            Assert.Empty(error);
            Assert.Contains("appsettings.json", output);
            Assert.Contains("appsettings.Development.json", output);
            Assert.Contains("Environment Variables", output);
            Assert.Contains("Application Command Line", output);
            Assert.Contains("Effective value: http://argument:5000", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Api__Url", previous);
        }
    }

    [Fact]
    public void RedactsSensitiveValuesInJsonUnlessRequested()
    {
        using var project = new TempProject();
        project.Write("appsettings.json", """{"Api":{"Token":"super-secret-value"}}""");

        var (code, output, _) = Run("explain", "Api:Token", "--project", project.Path,
            "--environment", "Production", "--json");

        Assert.Equal(0, code);
        Assert.DoesNotContain("super-secret-value", output);
        using var document = JsonDocument.Parse(output);
        Assert.Equal("<redacted>", document.RootElement.GetProperty("effectiveValue").GetString());

        var (_, revealed, _) = Run("explain", "Api:Token", "--project", project.Path,
            "--environment", "Production", "--show-secrets");
        Assert.Contains("super-secret-value", revealed);
    }

    [Fact]
    public void RedactsConnectionStringsSection()
    {
        using var project = new TempProject();
        project.Write("appsettings.json", """{"ConnectionStrings":{"Default":"Server=db;Password=sensitive"}}""");

        var (code, output, _) = Run("explain", "ConnectionStrings:Default", "--project", project.Path,
            "--environment", "Production");

        Assert.Equal(0, code);
        Assert.Contains("<redacted>", output);
        Assert.DoesNotContain("sensitive", output);
    }

    [Fact]
    public void ReportsMissingKeyWithExitCodeOne()
    {
        using var project = new TempProject();

        var (code, output, _) = Run("explain", "Missing:Value", "--project", project.Path,
            "--environment", "Production");

        Assert.Equal(1, code);
        Assert.Contains("No provider supplied this key.", output);
    }

    [Fact]
    public void AcceptsProjectWithUserSecretsIdAndNoSecretsFile()
    {
        using var project = new TempProject();
        project.Write("Example.csproj", """<Project><PropertyGroup><UserSecretsId>whyconfig-test-nonexistent</UserSecretsId></PropertyGroup></Project>""");
        project.Write("appsettings.json", """{"Api":{"Url":"https://example.com"}}""");

        var (code, output, error) = Run("explain", "Api:Url", "--project",
            System.IO.Path.Combine(project.Path, "Example.csproj"), "--environment", "Development");

        Assert.Equal(0, code);
        Assert.Empty(error);
        Assert.Contains("https://example.com", output);
    }

    [Fact]
    public void RejectsUnknownOption()
    {
        var (code, _, error) = Run("explain", "Api:Url", "--bogus");

        Assert.Equal(2, code);
        Assert.Contains("Unknown option", error);
    }

    private static (int Code, string Output, string Error) Run(params string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var code = CliApplication.Run(args, output, error);
        return (code, output.ToString(), error.ToString());
    }

    private sealed class TempProject : IDisposable
    {
        public TempProject()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WhyConfig.NET.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Write(string name, string contents) => File.WriteAllText(System.IO.Path.Combine(Path, name), contents);

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
