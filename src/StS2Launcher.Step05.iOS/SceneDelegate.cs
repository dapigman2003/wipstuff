using Foundation;
using UIKit;

namespace StS2Launcher.Step05.iOS;

[Register("SceneDelegate")]
public sealed class SceneDelegate : UIWindowSceneDelegate
{
    private RootViewController? _rootViewController;

    // UIWindowSceneDelegate supplies the Objective-C exported window/setWindow:
    // property UIKit expects for a UIWindowScene delegate.
    public override UIWindow? Window { get; set; }

    public override void WillConnect(
        UIScene scene,
        UISceneSession session,
        UISceneConnectionOptions connectionOptions)
    {
        Console.WriteLine("Step 15: SceneDelegate.WillConnect");

        if (scene is not UIWindowScene windowScene)
        {
            Console.Error.WriteLine(
                $"Step 15: expected UIWindowScene, received {scene.GetType().FullName}");
            return;
        }

        try
        {
            _rootViewController = new RootViewController();

            Window = new UIWindow(windowScene)
            {
                BackgroundColor = UIColor.White,
                RootViewController = _rootViewController
            };

            Window.MakeKeyAndVisible();

            Console.WriteLine("Step 15: UIWindow is key and visible");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step 13 startup exception: {ex}");

            var fallback = new UIViewController();
            fallback.View!.BackgroundColor = UIColor.White;

            var label = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = UIColor.Red,
                Lines = 0,
                TextAlignment = UITextAlignment.Center,
                Text = $"STEP 15 STARTUP ERROR\n\n{ex.GetType().Name}\n{ex.Message}"
            };

            fallback.View.AddSubview(label);
            NSLayoutConstraint.ActivateConstraints(
            [
                label.LeadingAnchor.ConstraintEqualTo(fallback.View.LeadingAnchor, 24),
                label.TrailingAnchor.ConstraintEqualTo(fallback.View.TrailingAnchor, -24),
                label.CenterYAnchor.ConstraintEqualTo(fallback.View.CenterYAnchor)
            ]);

            Window = new UIWindow(windowScene)
            {
                BackgroundColor = UIColor.White,
                RootViewController = fallback
            };

            Window.MakeKeyAndVisible();
        }
    }

    public override void DidBecomeActive(UIScene scene)
    {
        Console.WriteLine("Step 15: scene active");
        _rootViewController?.SetLifecycleState("Active");
    }

    public override void WillResignActive(UIScene scene)
    {
        Console.WriteLine("Step 15: scene will resign active");
        _rootViewController?.SetLifecycleState("Inactive");
    }

    public override void WillEnterForeground(UIScene scene)
    {
        Console.WriteLine("Step 15: scene entering foreground");
        _rootViewController?.SetLifecycleState("Entering foreground");
    }

    public override void DidEnterBackground(UIScene scene)
    {
        Console.WriteLine("Step 15: scene entered background");
    }
}
