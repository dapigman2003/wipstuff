using StS2Launcher.Step20.DependencyFixture;

namespace StS2Launcher.Step20.RootFixture;

public static class RootFixtureProbe
{
    public static int Run() => DependencyProbe.Add(40, 2);
}
