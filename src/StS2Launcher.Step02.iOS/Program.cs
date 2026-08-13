using UIKit;

namespace StS2Launcher.Step02.iOS;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("StS2 Launcher Step 02: entering UIApplication.Main");
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
