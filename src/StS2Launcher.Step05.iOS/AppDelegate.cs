using Foundation;
using UIKit;

namespace StS2Launcher.Step05.iOS;

[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    public override bool FinishedLaunching(
        UIApplication application,
        NSDictionary? launchOptions)
    {
        Console.WriteLine("Step 05.16: AppDelegate.FinishedLaunching");
        return true;
    }

    public override UISceneConfiguration GetConfiguration(
        UIApplication application,
        UISceneSession connectingSceneSession,
        UISceneConnectionOptions options)
    {
        Console.WriteLine(
            $"Step 05.16: AppDelegate.GetConfiguration role={connectingSceneSession.Role}");

        return new UISceneConfiguration(
            "Default Configuration",
            connectingSceneSession.Role)
        {
            SceneType = typeof(UIWindowScene),
            DelegateType = typeof(SceneDelegate)
        };
    }
}
