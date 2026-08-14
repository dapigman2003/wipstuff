#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

for required in \
  src/StS2Launcher.Core/StS2Launcher.Core.csproj \
  src/StS2Launcher.Core/SteamConnectionProbe.cs \
  src/StS2Launcher.Core/SteamNetworkTraceListener.cs \
  src/StS2Launcher.Core/SteamHttpClientFactory.cs \
  src/StS2Launcher.Core/CmNetworkProbe.cs \
  src/StS2Launcher.Core/SocketsHandlerIsolationProbe.cs \
  src/StS2Launcher.Core/SteamKitEndpointReplayProbe.cs \
  src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj \
  src/StS2Launcher.Step05.iOS/RootViewController.cs \
  src/StS2Launcher.Step05.iOS/Info.plist \
  tools/StS2Launcher.SteamKitIosPatcher/Program.cs \
  scripts/build-step05.sh \
  scripts/verify-step05-ipa.sh; do
  if [[ ! -f "$required" ]]; then
    echo "ERROR: missing required Step 05.14 file: $required" >&2
    exit 2
  fi
done

if [[ -e src/StS2Launcher.Core/ProtobufAotCompatibility.cs ]]; then
  echo "ERROR: Step 05.14 must remove the no-op ProtobufAotCompatibility diagnostic proven noisy by Step 05.13." >&2
  exit 2
fi

python3 - <<'PY'
from pathlib import Path
import plistlib
import xml.etree.ElementTree as ET

ios_project = Path("src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj")
ios_root = ET.parse(ios_project).getroot()
props = {e.tag: (e.text or "").strip() for e in ios_root.iter() if e.text}
expected = {
    "TargetFramework": "net9.0-ios",
    "RuntimeIdentifier": "ios-arm64",
    "SupportedOSPlatformVersion": "18.0",
    "ApplicationId": "com.community.sts2launcher",
    "ApplicationVersion": "20",
    "ApplicationDisplayVersion": "0.0.20",
    "TrimMode": "full",
}
for key, value in expected.items():
    if props.get(key) != value:
        raise SystemExit(f"ERROR: Step 05.14 iOS property {key} expected {value!r}, got {props.get(key)!r}")

if list(ios_root.iter("NativeReference")):
    raise SystemExit("ERROR: Step 05.14 must not contain NativeReference.")

target = None
for node in ios_root.iter("Target"):
    if node.attrib.get("Name") == "Step052RemoveMacOnlyDiskArbitrationFramework":
        target = node
        break
if target is None:
    raise SystemExit("ERROR: retained DiskArbitration filter target is missing.")
if target.attrib.get("AfterTargets") != "_LoadLinkerOutput":
    raise SystemExit("ERROR: framework filter must run after _LoadLinkerOutput.")
if target.attrib.get("BeforeTargets") != "_ComputeLinkNativeExecutableInputs":
    raise SystemExit("ERROR: framework filter must run before _ComputeLinkNativeExecutableInputs.")
removes = [
    n.attrib.get("Remove") for n in target.iter("_LinkerFrameworks") if n.attrib.get("Remove")
]
if removes != ["DiskArbitration"]:
    raise SystemExit(f"ERROR: framework filter must remove only DiskArbitration; got {removes}")

core_root = ET.parse("src/StS2Launcher.Core/StS2Launcher.Core.csproj").getroot()
packages = {
    e.attrib.get("Include"): e.attrib.get("Version")
    for e in core_root.iter("PackageReference")
}
if packages != {"SteamKit2": "3.4.0"}:
    raise SystemExit(f"ERROR: Step 05.14 expects exactly SteamKit2 3.4.0 in Core; got {packages}")

patcher_root = ET.parse("tools/StS2Launcher.SteamKitIosPatcher/StS2Launcher.SteamKitIosPatcher.csproj").getroot()
patcher_packages = {
    e.attrib.get("Include"): e.attrib.get("Version")
    for e in patcher_root.iter("PackageReference")
}
if patcher_packages != {"Mono.Cecil": "0.11.6"}:
    raise SystemExit(f"ERROR: build-only patcher dependency changed: {patcher_packages}")

patcher = Path("tools/StS2Launcher.SteamKitIosPatcher/Program.cs").read_text()
for required in (
    "System.DateTime System.Diagnostics.Process::get_StartTime()",
    '"get_UtcNow"',
    'ExpectedVersionPrefix = "3.4.0"',
    "matches.Length > 1",
    "Process.StartTime status: already absent",
    "ModuleAttributes.StrongNameSigned",
    "STEP05.14 STEAMKIT IOS PATCH: PASS",
):
    if required not in patcher:
        raise SystemExit(f"ERROR: SteamKit iOS patcher guard missing: {required}")

with Path("src/StS2Launcher.Step05.iOS/Info.plist").open("rb") as f:
    plist = plistlib.load(f)
if plist.get("CFBundleIdentifier") != "com.community.sts2launcher":
    raise SystemExit("ERROR: unexpected bundle ID.")
if plist.get("CFBundleShortVersionString") != "0.0.20":
    raise SystemExit("ERROR: Info.plist Step 05.14 display version regression.")
if str(plist.get("CFBundleVersion")) != "20":
    raise SystemExit("ERROR: Info.plist Step 05.14 build version regression.")

scene = Path("src/StS2Launcher.Step05.iOS/SceneDelegate.cs").read_text()
if "class SceneDelegate : UIWindowSceneDelegate" not in scene:
    raise SystemExit("ERROR: UIWindowSceneDelegate regression.")
if "public override UIWindow? Window" not in scene:
    raise SystemExit("ERROR: UIWindowSceneDelegate.Window regression.")

factory = Path("src/StS2Launcher.Core/SteamHttpClientFactory.cs").read_text()
for required in (
    "HttpClientPurpose.CMWebSocket",
    "new SocketsHttpHandler",
    "new HttpClient(handler, disposeHandler: true)",
    "new HttpClient()",
    'new ProductInfoHeaderValue("SteamKit", assemblyVersion)',
):
    if required not in factory:
        raise SystemExit(f"ERROR: Step 05.14 HTTP factory guard missing: {required}")

probe = Path("src/StS2Launcher.Core/SteamConnectionProbe.cs").read_text()
for required in (
    "SteamConfiguration.Create",
    "WithProtocolTypes(protocolTypes)",
    "ProtocolTypes.WebSocket",
    ".WithHttpClientFactory(Factory)",
    "SteamHttpClientFactory.Create(purpose)",
    "new SteamClient(configuration)",
    "new CallbackManager(steamClient)",
    "ConnectedCallback",
    "DisconnectedCallback",
    "steamClient.IsConnected",
    "steamClient.CurrentEndPoint",
    "FirstChanceException",
    "steamClient.Connect()",
    "steamClient.Disconnect()",
    "manager.RunWaitCallbacks",
    "steamClient.DebugNetworkListener = networkTrace",
    "Outgoing ClientHello observed",
    "DebugLog.AddListener(debugListener)",
    "DebugLog.Enabled = true",
    "DebugLog.RemoveListener(debugListener)",
    "DebugLog.Enabled = previousDebugLogEnabled",
    "SteamKit post-connect exception logged:",
    "Unhandled exception after connecting",
    "SteamKit DebugLog:",
    "First-chance supplemental exceptions:",
):
    if required not in probe:
        raise SystemExit(f"ERROR: Step 05.14 Steam probe missing: {required}")

for forbidden in (
    "ProtobufAotCompatibility",
    "RuntimeTypeModel.Default",
    "AutoCompile = false",
    "ReflectionEmit observed stage(s):",
    "Stage timeline:",
):
    if forbidden in probe:
        raise SystemExit(f"ERROR: Step 05.14 must not retain the superseded protobuf/stage diagnostic: {forbidden}")

network_trace = Path("src/StS2Launcher.Core/SteamNetworkTraceListener.cs").read_text()
for required in (
    "IDebugNetworkListener",
    "OnIncomingNetworkMessage",
    "OnOutgoingNetworkMessage",
    "EMsg.ClientHello",
    "OutgoingClientHelloObserved",
    "metadata-only",
):
    if required not in network_trace:
        raise SystemExit(f"ERROR: Step 05.14 debug network trace guard missing: {required}")

if "ProtocolTypes.Tcp" in probe:
    raise SystemExit("ERROR: Step 05.14 SteamKit probe must stay WebSocket-only.")

network_probe = Path("src/StS2Launcher.Core/CmNetworkProbe.cs").read_text()
for required in (
    "ISteamDirectory/GetCMListForConnect",
    "HttpClient",
    "Dns.GetHostAddressesAsync",
    "TcpClient",
    "ClientWebSocket",
    '"/cmsocket/"',
    "JsonDocument.Parse",
    "StS2Launcher-iOS-Step05.14/0.0.20",
):
    if required not in network_probe:
        raise SystemExit(f"ERROR: Step 05.14 CM network regression probe missing: {required}")

handler_probe = Path("src/StS2Launcher.Core/SocketsHandlerIsolationProbe.cs").read_text()
for required in (
    "SteamHttpClientFactory.Create(HttpClientPurpose.CMWebSocket)",
    "ClientWebSocket",
    "socket.ConnectAsync(uri, client, cts.Token)",
    "client.GetAsync",
    "SocketsHttpHandler — HTTP",
    "custom-invoker",
    "FormatExceptionWithStack",
):
    if required not in handler_probe:
        raise SystemExit(f"ERROR: Step 05.14 handler-isolation probe missing: {required}")

endpoint_replay = Path("src/StS2Launcher.Core/SteamKitEndpointReplayProbe.cs").read_text()
for required in (
    "SteamHttpClientFactory.Create(HttpClientPurpose.CMWebSocket)",
    "new ClientWebSocket()",
    "socket.ConnectAsync(uri, client, cts.Token)",
    '"/cmsocket/"',
    "SteamKit CurrentEndPoint",
    "EXACT STEAMKIT ENDPOINT REPLAY",
):
    if required not in endpoint_replay:
        raise SystemExit(f"ERROR: Step 05.14 exact-endpoint replay probe missing: {required}")

combined = probe + "\n" + network_probe + "\n" + factory + "\n" + handler_probe + "\n" + endpoint_replay + "\n" + network_trace
for forbidden in (
    "BeginAuthSession",
    "AuthSessionDetails",
    "SteamUser.LogOn",
    "RefreshToken",
    "Password =",
    "TwoFactor",
    "SteamGuard",
):
    if forbidden in combined:
        raise SystemExit(f"ERROR: Step 05.14 contains authentication behavior: {forbidden}")

root_view = Path("src/StS2Launcher.Step05.iOS/RootViewController.cs").read_text()
for required in (
    "STEP 05.14 — STEAMKIT INTERNAL ERROR CAPTURE",
    "Version 0.0.20",
    "NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN",
    "Run Step 05.14 SteamKit DebugLog Test",
    'TimeSpan.FromSeconds(25)',
    'TimeSpan.FromSeconds(12)',
    "STEAM CONNECTION PASS — 3/3",
    "Disconnected.UserInitiated",
    "IsConnected ever",
    "CurrentEndPoint",
    "Outgoing ClientHello",
    "Debug network trace",
    "STEAMKIT ASSEMBLY: PASS",
    "STEAMKIT INTERNAL FAIL • SEE DEBUGLOG",
    "inspect SteamKit DebugLog",
    "_endpointReplayProbe.RunAsync",
):
    if required not in root_view:
        raise SystemExit(f"ERROR: Step 05.14 UI marker missing: {required}")

core_text = "\n".join(p.read_text(errors="ignore") for p in Path("src/StS2Launcher.Core").glob("*.cs"))
for forbidden in ("UIKit", "Foundation", "Security.", "ObjCRuntime", "Mono.Cecil"):
    if forbidden in core_text:
        raise SystemExit(f"ERROR: Core contains forbidden platform/build dependency: {forbidden}")

build_script = Path("scripts/build-step05.sh").read_text()
for required in (
    'export NUGET_PACKAGES="$ROOT/.nuget/packages"',
    'rm -rf "$NUGET_PACKAGES/steamkit2/3.4.0"',
    'dotnet restore "$PROJECT"',
    'dotnet run --project "$PATCHER"',
    '--no-restore',
    'STEP05.14 STEAMKIT IOS PATCH: PASS',
    'StS2-Launcher-Step-05.14.ipa',
):
    if required not in build_script:
        raise SystemExit(f"ERROR: Step 05.14 build isolation guard missing: {required}")

print("Step 05.14 SteamKit-DebugLog/Core/UI boundary validation passed.")
PY

if grep -RniE 'Godot|Mono\.Cecil|Harmony|NativeGodotHost' \
  src --include='*.cs' --include='*.csproj'; then
  echo "ERROR: Step 05.14 app source contains a forbidden later-stage subsystem." >&2
  exit 3
fi

for script in scripts/*.sh; do
  bash -n "$script"
done

echo "Step 05.14 repository validation passed."
