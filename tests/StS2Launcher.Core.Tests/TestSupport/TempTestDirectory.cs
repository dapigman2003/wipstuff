namespace StS2Launcher.Core.Tests;

internal sealed class TempTestDirectory : IDisposable
{
    public TempTestDirectory(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Temporary-directory prefix must be non-empty.", nameof(prefix));

        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            prefix.Trim() + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch
        {
            // Test cleanup is best-effort and must not mask the test result.
        }
    }
}
