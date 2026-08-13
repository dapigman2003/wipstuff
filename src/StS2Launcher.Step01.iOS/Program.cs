using UIKit;

namespace StS2Launcher.Step01.iOS;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("StS2 Launcher Step 01.1: entering UIApplication.Main");
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
