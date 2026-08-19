namespace StS2Launcher.Step16.Fixture;

/// <summary>
/// Project-owned Step 16 metadata/IL fixture. This assembly is copied into the
/// application bundle as inert test data; the iOS launcher never loads or
/// executes it. Mono.Cecil reads/writes this file as raw managed metadata.
/// </summary>
public static class FixtureTarget
{
    public const string IdentityMarker = "STEP16_CECIL_FIXTURE_V1";

    public static int RewriteMe() => 7;

    public static string Identity() => IdentityMarker;
}
