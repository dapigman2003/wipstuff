using Foundation;
using UIKit;

namespace StS2Launcher.Step05.iOS;

[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    // Godot's Apple-embedded display server still queries UIApplicationDelegate.window.
    // The app is scene-based, but the Step 15 bridge points this property at the already-existing
    // scene window before Godot setup2 runs. Keep the exported window/setWindow: surface explicit.
    public override UIWindow? Window { get; set; }

    public override bool FinishedLaunching(
        UIApplication application,
        NSDictionary? launchOptions)
    {
        Console.WriteLine("Step 15: AppDelegate.FinishedLaunching");
        return true;
    }

    public override UISceneConfiguration GetConfiguration(
        UIApplication application,
        UISceneSession connectingSceneSession,
        UISceneConnectionOptions options)
    {
        Console.WriteLine(
            $"Step 15: AppDelegate.GetConfiguration role={connectingSceneSession.Role}");

        return new UISceneConfiguration(
            "Default Configuration",
            connectingSceneSession.Role)
        {
            SceneType = typeof(UIWindowScene),
            DelegateType = typeof(SceneDelegate)
        };
    }
}
