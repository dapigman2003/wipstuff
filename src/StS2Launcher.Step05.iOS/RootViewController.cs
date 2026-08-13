using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using SteamKit2;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed class RootViewController : UIViewController
{
    private readonly LauncherController _controller = new();
    private readonly KeychainProbe _keychainProbe =
        new(new KeychainCredentialStore());
    private readonly CmNetworkProbe _cmNetworkProbe = new();
    private readonly SocketsHandlerIsolationProbe _handlerIsolationProbe = new();
    private readonly SteamConnectionProbe _steamProbe = new();
    private readonly SteamKitEndpointReplayProbe _endpointReplayProbe = new();

    private UILabel? _steamAssemblyLabel;
    private UILabel? _steamResultLabel;
    private UILabel? _steamDetailLabel;
    private UIButton? _steamButton;
    private UILabel? _coreLabel;
    private UILabel? _keychainLabel;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.SystemBackground;

        var scroll = new UIScrollView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            AlwaysBounceVertical = true
        };

        var content = new UIStackView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Distribution = UIStackViewDistribution.Fill,
            Spacing = 13
        };

        View.AddSubview(scroll);
        scroll.AddSubview(content);

        NSLayoutConstraint.ActivateConstraints(
        [
            scroll.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
            scroll.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
            scroll.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            scroll.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

            content.TopAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TopAnchor, 18),
            content.BottomAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.BottomAnchor, -18),
            content.LeadingAnchor.ConstraintEqualTo(scroll.FrameLayoutGuide.LeadingAnchor, 24),
            content.TrailingAnchor.ConstraintEqualTo(scroll.FrameLayoutGuide.TrailingAnchor, -24)
        ]);

        content.AddArrangedSubview(Label(
            "StS2 Launcher",
            UIFont.BoldSystemFontOfSize(30),
            UIColor.Label));

        content.AddArrangedSubview(Label(
            "STEP 05.10 — CLIENTHELLO AOT DIAGNOSTICS",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.16",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN",
            UIFont.BoldSystemFontOfSize(13),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "SteamKit2 load + connection",
            UIFont.BoldSystemFontOfSize(22),
            UIColor.Label));

        _steamAssemblyLabel = Label(
            "STEAMKIT ASSEMBLY: checking…",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.Label);
        content.AddArrangedSubview(_steamAssemblyLabel);

        _steamResultLabel = Label(
            "STEAM CONNECTION: NOT RUN",
            UIFont.BoldSystemFontOfSize(17),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_steamResultLabel);

        _steamDetailLabel = Label(
            "Step 05.9 proved SteamKit's exact selected CM endpoint completes the same custom-invoker WebSocket upgrade outside SteamKit. Step 05.10 keeps that replay as a regression check and instruments SteamKit's next post-upgrade boundary: outgoing ClientHello serialization plus the exact Reflection.Emit caller context. It never authenticates.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_steamDetailLabel);

        _steamButton = SystemButton("Run Step 05.10 ClientHello Diagnostics", 17);
        _steamButton.TouchUpInside += async (_, _) => await RunSteamProbeAsync();
        content.AddArrangedSubview(_steamButton);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Regression checks",
            UIFont.BoldSystemFontOfSize(22),
            UIColor.Label));

        _coreLabel = Label(
            "CORE: checking…",
            UIFont.SystemFontOfSize(15),
            UIColor.Label);
        content.AddArrangedSubview(_coreLabel);

        _keychainLabel = Label(
            "KEYCHAIN: checking…",
            UIFont.SystemFontOfSize(15),
            UIColor.Label);
        content.AddArrangedSubview(_keychainLabel);

        var coreSelfTest = SystemButton("Run Core Self-Test", 15);
        coreSelfTest.TouchUpInside += (_, _) =>
        {
            var result = CoreSelfTest.Run();
            _coreLabel!.Text = result.Summary;
            _statusLabel!.Text = result.Passed
                ? "PASS: Core regression self-test."
                : "FAIL: Core regression self-test.";
        };
        content.AddArrangedSubview(coreSelfTest);

        var keychainRead = SystemButton("Check Step-04 Keychain Is Empty", 15);
        keychainRead.TouchUpInside += (_, _) => CheckKeychainRegression();
        content.AddArrangedSubview(keychainRead);

        var nextCore = SystemButton("Next Core State", 15);
        nextCore.TouchUpInside += (_, _) =>
        {
            var snapshot = _controller.NextDemoState();
            _coreLabel!.Text =
                $"CORE STATE {snapshot.StateNumber}/{snapshot.StateCount}: {snapshot.Title}";
        };
        content.AddArrangedSubview(nextCore);

        var resetCore = SystemButton("Reset Core State", 15);
        resetCore.TouchUpInside += (_, _) =>
        {
            var snapshot = _controller.Reset();
            _coreLabel!.Text =
                $"CORE STATE {snapshot.StateNumber}/{snapshot.StateCount}: {snapshot.Title}";
        };
        content.AddArrangedSubview(resetCore);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: starting Step 05.10 ClientHello diagnostics.",
            UIFont.SystemFontOfSize(14),
            UIColor.Label);
        content.AddArrangedSubview(_statusLabel);

        _lifecycleLabel = Label(
            "Lifecycle: Starting",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_lifecycleLabel);

        foreach (var button in content.ArrangedSubviews.OfType<UIButton>())
            button.HeightAnchor.ConstraintGreaterThanOrEqualTo(44).Active = true;

        RunStartupChecks();

        Console.WriteLine("Step 05.10: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";
    }

    private void RunStartupChecks()
    {
        try
        {
            var snapshot = _controller.Snapshot;
            _coreLabel!.Text =
                $"CORE LINK: PASS — {snapshot.State}";
        }
        catch (Exception ex)
        {
            _coreLabel!.Text =
                $"CORE LINK: FAIL — {ex.GetType().Name}";
        }

        try
        {
            _steamAssemblyLabel!.Text =
                $"STEAMKIT ASSEMBLY: PASS — {SteamConnectionProbe.AssemblyVersion}";
        }
        catch (Exception ex)
        {
            _steamAssemblyLabel!.Text =
                $"STEAMKIT ASSEMBLY: FAIL — {ex.GetType().Name}: {ex.Message}";
        }

        CheckKeychainRegression();

        _statusLabel!.Text =
            "PASS: UIKit startup completed. Steam network probe has not run yet.";
    }

    private async Task RunSteamProbeAsync()
    {
        if (_steamButton is null ||
            _steamResultLabel is null ||
            _steamDetailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        _steamButton.Enabled = false;
        _steamResultLabel.Text = "CM NETWORK: TESTING…";
        _steamResultLabel.TextColor = UIColor.Label;
        _steamDetailLabel.Text =
            "1/4 Re-confirming native iOS/.NET CM network boundary…";
        _statusLabel.Text =
            "NETWORK TEST RUNNING — leave the app in foreground.";

        try
        {
            var network = await _cmNetworkProbe.RunAsync(
                TimeSpan.FromSeconds(12),
                TimeSpan.FromSeconds(8));

            InvokeOnMainThread(() =>
            {
                _steamDetailLabel.Text =
                    FormatNetworkResult(network) +
                    "\n\n2/4 SocketsHttpHandler/custom-invoker regression running…";
            });

            var handlerIsolation = await _handlerIsolationProbe.RunAsync(
                network.WebSocketEndpoint,
                TimeSpan.FromSeconds(12));

            InvokeOnMainThread(() =>
            {
                _steamDetailLabel.Text =
                    FormatNetworkResult(network) +
                    "\n\n" +
                    FormatHandlerIsolationResult(handlerIsolation) +
                    "\n\n3/4 SteamKit WebSocket + ClientHello diagnostic running…";
            });

            var webSocket = await _steamProbe.RunAsync(TimeSpan.FromSeconds(25));

            InvokeOnMainThread(() =>
            {
                _steamDetailLabel.Text =
                    FormatNetworkResult(network) +
                    "\n\n" +
                    FormatHandlerIsolationResult(handlerIsolation) +
                    "\n\n" +
                    FormatTransportResult(webSocket) +
                    "\n\n4/4 Replaying SteamKit-selected CM as endpoint regression check…";
            });

            var endpointReplay = await _endpointReplayProbe.RunAsync(
                webSocket.LastCurrentEndPoint,
                TimeSpan.FromSeconds(12));

            InvokeOnMainThread(() =>
            {
                var nativeNetworkReady =
                    network.DirectoryHttpsPassed &&
                    network.DnsPassed &&
                    network.TcpPassed &&
                    network.WebSocketPassed;

                var handlerReady = handlerIsolation.HttpsPassed && handlerIsolation.WebSocketPassed;

                _steamResultLabel.Text = webSocket.Passed
                    ? "STEAM CONNECTION PASS — 3/3"
                    : !nativeNetworkReady
                        ? "CM NETWORK BOUNDARY FAIL"
                        : !handlerReady
                            ? "SOCKETS HANDLER REGRESSION"
                            : endpointReplay.Passed
                                ? webSocket.OutgoingClientHelloObserved
                                ? "CLIENTHELLO OUT • CALLBACK FAIL"
                                : "CLIENTHELLO NOT OBSERVED • STEAMKIT FAIL"
                                : "STEAMKIT ENDPOINT REPLAY FAIL";

                _steamResultLabel.TextColor = webSocket.Passed
                    ? UIColor.Label
                    : UIColor.SystemRed;

                _steamDetailLabel.Text =
                    FormatNetworkResult(network) +
                    "\n\n" +
                    FormatHandlerIsolationResult(handlerIsolation) +
                    "\n\n" +
                    FormatTransportResult(webSocket) +
                    "\n\n" +
                    FormatEndpointReplayResult(endpointReplay) +
                    $"\n\nSteamKit assembly: {SteamConnectionProbe.AssemblyVersion}";

                _statusLabel.Text = webSocket.Passed
                    ? "PASS: SteamKit CM WebSocket connected and disconnected."
                    : !nativeNetworkReady
                        ? "RESULT: native CM network regression; inspect the 4/4 probe above."
                        : !handlerReady
                            ? "RESULT: the previously proven custom-invoker handler path regressed in this run."
                            : endpointReplay.Passed
                                ? webSocket.OutgoingClientHelloObserved
                                    ? "RESULT: exact endpoint replay passes and outgoing ClientHello serialized; inspect the caller stack for the failure after ClientHello."
                                    : "RESULT: exact endpoint replay passes but outgoing ClientHello was not observed; inspect Reflection.Emit caller context for the ClientHello construction/serialization boundary."
                                : "RESULT: SteamKit's chosen CM does not reproduce the successful HTTP upgrade; investigate CM selection/candidate quality before patching SteamKit internals.";

                _steamButton.Enabled = true;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _steamResultLabel.Text = "STEP 05.10 CLIENTHELLO DIAGNOSTICS: EXCEPTION";
                _steamResultLabel.TextColor = UIColor.SystemRed;
                _steamDetailLabel.Text =
                    $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
                _statusLabel.Text =
                    "FAIL: unhandled exception in Step 05.10 diagnostics.";
                _steamButton.Enabled = true;
            });
        }
    }

    private static string FormatNetworkResult(CmNetworkProbeResult result)
    {
        return
            $"{result.Summary}\n" +
            $"Directory HTTPS: {(result.DirectoryHttpsPassed ? "PASS" : "FAIL")}" +
            $"{(result.DirectoryStatusCode.HasValue ? $" — HTTP {result.DirectoryStatusCode}" : string.Empty)}\n" +
            $"CM endpoints: {result.EndpointCount}\n" +
            $"DNS: {(result.DnsPassed ? "PASS" : "FAIL")} — {result.DnsDetail}\n" +
            $"Raw TCP: {(result.TcpPassed ? "PASS" : "FAIL")} — {result.TcpEndpoint ?? "none"}\n" +
            $"TCP detail: {result.TcpDetail}\n" +
            $"Raw WebSocket: {(result.WebSocketPassed ? "PASS" : "FAIL")} — {result.WebSocketEndpoint ?? "none"}\n" +
            $"WebSocket detail: {result.WebSocketDetail}\n" +
            $"Native elapsed: {result.Elapsed.TotalSeconds:F1}s";
    }

    private static string FormatHandlerIsolationResult(SocketsHandlerIsolationProbeResult result)
    {
        return
            $"{result.Summary}\n" +
            $"SocketsHttpHandler HTTPS: {(result.HttpsPassed ? "PASS" : "FAIL")} — {result.HttpsDetail}\n" +
            $"Custom-invoker WebSocket: {(result.WebSocketPassed ? "PASS" : "FAIL")} — {result.WebSocketDetail}\n" +
            $"Handler elapsed: {result.Elapsed.TotalSeconds:F1}s\n" +
            $"Handler exception/stack:\n{result.ExceptionDetail}";
    }


    private static string FormatEndpointReplayResult(SteamKitEndpointReplayProbeResult result)
    {
        return
            $"{result.Summary}\n" +
            $"SteamKit-selected endpoint: {result.SourceEndPoint ?? "none"}\n" +
            $"Replay URI: {result.WebSocketUri ?? "none"}\n" +
            $"Replay detail: {result.Detail}\n" +
            $"Replay elapsed: {result.Elapsed.TotalSeconds:F1}s\n" +
            $"Replay exception/stack:\n{result.ExceptionDetail}";
    }

    private static string FormatTransportResult(SteamConnectionProbeResult result)
    {
        return
            $"{result.Summary}\n" +
            $"Protocols: {result.Protocols}\n" +
            $"ConnectedCallback: {(result.ConnectedCallbackReceived ? "YES" : "NO")}\n" +
            $"DisconnectedCallback: {(result.DisconnectedCallbackReceived ? "YES" : "NO")}\n" +
            $"Disconnected.UserInitiated: " +
            $"{(result.DisconnectedUserInitiated.HasValue ? result.DisconnectedUserInitiated.Value.ToString() : "N/A")}\n" +
            $"IsConnected ever: {result.IsConnectedEver}\n" +
            $"CurrentEndPoint: {result.LastCurrentEndPoint ?? "never-set"}\n" +
            $"Outgoing ClientHello: {(result.OutgoingClientHelloObserved ? "YES" : "NO")}\n" +
            $"Debug network trace: {result.DebugNetworkTrace}\n" +
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s\n" +
            result.Detail;
    }

    private void CheckKeychainRegression()
    {
        try
        {
            var value = _keychainProbe.ReadPersistedValue();

            _keychainLabel!.Text = value is null
                ? "KEYCHAIN REGRESSION: PASS — Step-04 dummy value absent"
                : $"KEYCHAIN REGRESSION: NOTE — Step-04 dummy value present ({value})";
        }
        catch (Exception ex)
        {
            _keychainLabel!.Text =
                $"KEYCHAIN REGRESSION: FAIL — {ex.GetType().Name}: {ex.Message}";
        }
    }

    private static UILabel Label(string text, UIFont font, UIColor color)
    {
        return new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = text,
            TextColor = color,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = font
        };
    }

    private static UIButton SystemButton(string title, nfloat fontSize)
    {
        var button = UIButton.FromType(UIButtonType.System);
        button.TranslatesAutoresizingMaskIntoConstraints = false;
        button.SetTitle(title, UIControlState.Normal);
        button.TitleLabel!.Font = UIFont.BoldSystemFontOfSize(fontSize);
        return button;
    }

    private static UIView Separator()
    {
        var view = new UIView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BackgroundColor = UIColor.Separator
        };
        view.HeightAnchor.ConstraintEqualTo(1).Active = true;
        return view;
    }
}
