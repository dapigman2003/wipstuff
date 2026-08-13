using Foundation;
using UIKit;

namespace StS2Launcher.Step01.iOS;

public sealed class RootViewController : UIViewController
{
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.White;

        var title = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "StS2 Launcher",
            TextColor = UIColor.Black,
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.BoldSystemFontOfSize(30)
        };

        var pass = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "STEP 01 — UI BOOTSTRAP PASS",
            TextColor = UIColor.Black,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = UIFont.BoldSystemFontOfSize(20)
        };

        var version = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "Version 0.0.1",
            TextColor = UIColor.DarkGray,
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.SystemFontOfSize(16)
        };

        _statusLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "Status: UI rendered successfully.",
            TextColor = UIColor.Black,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = UIFont.SystemFontOfSize(16)
        };

        _lifecycleLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "Lifecycle: Starting",
            TextColor = UIColor.DarkGray,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = UIFont.SystemFontOfSize(14)
        };

        var writeLogButton = UIButton.FromType(UIButtonType.System);
        writeLogButton.TranslatesAutoresizingMaskIntoConstraints = false;
        writeLogButton.SetTitle("Write Test Log", UIControlState.Normal);
        writeLogButton.TitleLabel!.Font = UIFont.BoldSystemFontOfSize(18);
        writeLogButton.TouchUpInside += (_, _) => WriteTestLog();

        var stack = new UIStackView(
        [
            title,
            pass,
            version,
            _statusLabel,
            _lifecycleLabel,
            writeLogButton
        ])
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Distribution = UIStackViewDistribution.Fill,
            Spacing = 18
        };

        View.AddSubview(stack);

        NSLayoutConstraint.ActivateConstraints(
        [
            stack.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 28),
            stack.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -28),
            stack.CenterYAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.CenterYAnchor),

            writeLogButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(50)
        ]);

        Console.WriteLine("Step 01: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";
    }

    private void WriteTestLog()
    {
        try
        {
            var documents =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var path = Path.Combine(documents, "step01-device-test.log");

            var line =
                $"{DateTimeOffset.Now:O} STEP01 PASS " +
                $"iOS={UIDevice.CurrentDevice.SystemVersion} " +
                $"device={UIDevice.CurrentDevice.Model}{Environment.NewLine}";

            File.AppendAllText(path, line);

            if (_statusLabel is not null)
            {
                _statusLabel.Text =
                    $"PASS: test log written at {DateTime.Now:HH:mm:ss}";
            }

            Console.WriteLine($"Step 01: wrote device test log: {path}");
        }
        catch (Exception ex)
        {
            if (_statusLabel is not null)
            {
                _statusLabel.Text =
                    $"FAIL: {ex.GetType().Name}: {ex.Message}";
            }

            Console.Error.WriteLine($"Step 01 file-write failure: {ex}");
        }
    }
}
