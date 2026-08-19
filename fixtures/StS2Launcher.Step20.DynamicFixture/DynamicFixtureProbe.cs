namespace StS2Launcher.Step20.DynamicFixture;

public static class DynamicFixtureProbe
{
    public static int Run()
    {
        var values = new[] { 1, 2, 3, 4 };
        var sum = 0;
        try
        {
            foreach (var value in values)
                sum += Identity(value);
            sum += Identity(32);
        }
        finally
        {
            sum += 0;
        }
        return sum;
    }

    private static T Identity<T>(T value) => value;
}
