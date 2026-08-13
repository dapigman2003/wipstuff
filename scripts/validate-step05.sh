#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

IOS_PROJECT="src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
CORE_PROJECT="src/StS2Launcher.Core/StS2Launcher.Core.csproj"
PATCHER_PROJECT="tools/StS2Launcher.SteamKitIosPatcher/StS2Launcher.SteamKitIosPatcher.csproj"
PATCHER_SOURCE="tools/StS2Launcher.SteamKitIosPatcher/Program.cs"
HTTP_FACTORY_SOURCE="src/StS2Launcher.Core/SteamHttpClientFactory.cs"
HANDLER_PROBE_SOURCE="src/StS2Launcher.Core/SocketsHandlerIsolationProbe.cs"
ENDPOINT_REPLAY_SOURCE="src/StS2Launcher.Core/SteamKitEndpointReplayProbe.cs"
NETWORK_TRACE_SOURCE="src/StS2Launcher.Core/SteamNetworkTraceListener.cs"
PROTOBUF_AOT_SOURCE="src/StS2Launcher.Core/ProtobufAotCompatibility.cs"

for required in "$IOS_PROJECT" "$CORE_PROJECT" "$PATCHER_PROJECT" "$PATCHER_SOURCE" "$HTTP_FACTORY_SOURCE" "$HANDLER_PROBE_SOURCE" "$ENDPOINT_REPLAY_SOURCE" "$NETWORK_TRACE_SOURCE" "$PROTOBUF_AOT_SOURCE"; do
  [[ -f "$required" ]] || {
    echo "ERROR: missing required Step 05.11 file: $required" >&2
    exit 2
  }
done

python3 - <<'PY'
from pathlib import Path
import plistlib
import xml.etree.ElementTree as ET

ios_path = "src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
ios_root = ET.parse(ios_path).getroot()

refs = [e.attrib.get("Include", "") for e in ios_root.iter("ProjectReference")]
if refs != ["../StS2Launcher.Core/StS2Launcher.Core.csproj"]:
    raise SystemExit(f"ERROR: iOS project expected only Core ProjectReference; got {refs}")

if any(True for _ in ios_root.iter("PackageReference")):
    raise SystemExit("ERROR: SteamKit/build-tool packages must not be referenced directly by iOS UI.")
if any(True for _ in ios_root.iter("NativeReference")):
    raise SystemExit("ERROR: Step 05.11 must not contain NativeReference.")

values = {}
for name in (
    "TargetFramework", "RuntimeIdentifier", "ApplicationVersion",
    "ApplicationDisplayVersion", "TrimMode",
):
    node = next(iter(ios_root.iter(name)), None)
    values[name] = (node.text or "").strip() if node is not None else None

expected = {
    "TargetFramework": "net9.0-ios",
    "RuntimeIdentifier": "ios-arm64",
    "ApplicationVersion": "17",
    "ApplicationDisplayVersion": "0.0.17",
    "TrimMode": "full",
}
if values != expected:
    raise SystemExit(f"ERROR: Step 05.11 iOS build properties changed: {values}")

# Retain the exact native-framework boundary fix proven by Step 05.2+.
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
    node.attrib.get("Remove")
    for node in target.iter("_LinkerFrameworks")
    if node.attrib.get("Remove")
]
if removes != ["DiskArbitration"]:
    raise SystemExit(f"ERROR: framework filter must remove only DiskArbitration; got {removes}")

core_root = ET.parse("src/StS2Launcher.Core/StS2Launcher.Core.csproj").getroot()
packages = {
    e.attrib.get("Include"): e.attrib.get("Version")
    for e in core_root.iter("PackageReference")
}
if packages != {"SteamKit2": "3.3.1"}:
    raise SystemExit(f"ERROR: Step 05.11 expects exactly SteamKit2 3.3.1 in Core; got {packages}")

patcher_root = ET.parse(
    "tools/StS2Launcher.SteamKitIosPatcher/StS2Launcher.SteamKitIosPatcher.csproj"
).getroot()
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
    "replacements != 1",
    "ModuleAttributes.StrongNameSigned",
    "STEP05.11 STEAMKIT IOS PATCH: PASS",
):
    if required not in patcher:
        raise SystemExit(f"ERROR: SteamKit iOS patcher guard missing: {required}")

plist_path = Path("src/StS2Launcher.Step05.iOS/Info.plist")
with plist_path.open("rb") as f:
    plist = plistlib.load(f)
if plist.get("CFBundleIdentifier") != "com.community.sts2launcher":
    raise SystemExit("ERROR: unexpected bundle ID.")
if plist.get("CFBundleShortVersionString") != "0.0.17":
    raise SystemExit("ERROR: Info.plist Step 05.11 display version regression.")
if str(plist.get("CFBundleVersion")) != "17":
    raise SystemExit("ERROR: Info.plist Step 05.11 build version regression.")

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
        raise SystemExit(f"ERROR: Step 05.11 HTTP factory guard missing: {required}")

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
    "NotSupportedException",
    "HttpRequestException",
    "steamClient.Connect()",
    "steamClient.Disconnect()",
    "manager.RunWaitCallbacks",
    "HTTP factory calls:",
    "CM WebSocket handler:",
    "steamClient.DebugNetworkListener = networkTrace",
    "Environment.StackTrace",
    "TargetSite",
    "CurrentManagedThreadId",
    "IsConnected at throw",
    "Outgoing ClientHello observed",
):
    if required not in probe:
        raise SystemExit(f"ERROR: Steam WebSocket compatibility probe missing: {required}")

aot_helper = Path("src/StS2Launcher.Core/ProtobufAotCompatibility.cs").read_text()
for required in (
    "RuntimeTypeModel.Default",
    "AutoCompile",
    "model.AutoCompile = false",
    "protobuf-net",
):
    if required not in aot_helper:
        raise SystemExit(f"ERROR: Step 05.11 protobuf AOT compatibility guard missing: {required}")

if "ProtobufAotCompatibility.Configure()" not in probe:
    raise SystemExit("ERROR: Step 05.11 Steam probe must configure protobuf no-emit mode before connecting.")
if "Protobuf AOT mode:" not in probe:
    raise SystemExit("ERROR: Step 05.11 Steam probe must report protobuf AOT mode.")

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
        raise SystemExit(f"ERROR: Step 05.11 debug network trace guard missing: {required}")

if "ProtocolTypes.Tcp" in probe:
    raise SystemExit("ERROR: Step 05.11 SteamKit probe must stay WebSocket-only.")

network_probe = Path("src/StS2Launcher.Core/CmNetworkProbe.cs").read_text()
for required in (
    "ISteamDirectory/GetCMListForConnect",
    "HttpClient",
    "Dns.GetHostAddressesAsync",
    "TcpClient",
    "ClientWebSocket",
    '"/cmsocket/"',
    "JsonDocument.Parse",
    "StS2Launcher-iOS-Step05.11/0.0.17",
):
    if required not in network_probe:
        raise SystemExit(f"ERROR: Step 05.11 CM network regression probe missing: {required}")

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
        raise SystemExit(f"ERROR: Step 05.11 handler-isolation probe missing: {required}")

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
        raise SystemExit(f"ERROR: Step 05.11 exact-endpoint replay probe missing: {required}")

# Authentication is intentionally forbidden in Step 05.x.
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
        raise SystemExit(f"ERROR: Step 05.11 contains authentication behavior: {forbidden}")

root_view = Path("src/StS2Launcher.Step05.iOS/RootViewController.cs").read_text()
for required in (
    "STEP 05.11 — PROTOBUF NO-EMIT TEST",
    "Version 0.0.17",
    "NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN",
    "Run Step 05.11 Protobuf No-Emit Test",
    'TimeSpan.FromSeconds(25)',
    'TimeSpan.FromSeconds(12)',
    "STEAM CONNECTION PASS — 3/3",
    "Disconnected.UserInitiated",
    "IsConnected ever",
    "CurrentEndPoint",
    "Outgoing ClientHello",
    "Debug network trace",
    "STEAMKIT ASSEMBLY: PASS",
    "CLIENTHELLO NOT OBSERVED • STEAMKIT FAIL",
    "_endpointReplayProbe.RunAsync",
):
    if required not in root_view:
        raise SystemExit(f"ERROR: Step 05.11 UI marker missing: {required}")

# Core remains UI/platform-binding neutral and contains no build-time Cecil dependency.
core_text = "\n".join(
    p.read_text(errors="ignore")
    for p in Path("src/StS2Launcher.Core").glob("*.cs")
)
for forbidden in ("UIKit", "Foundation", "Security.", "ObjCRuntime", "Mono.Cecil"):
    if forbidden in core_text:
        raise SystemExit(f"ERROR: Core contains forbidden platform/build dependency: {forbidden}")

build_script = Path("scripts/build-step05.sh").read_text()
for required in (
    'export NUGET_PACKAGES="$ROOT/.nuget/packages"',
    'rm -rf "$NUGET_PACKAGES/steamkit2/3.3.1"',
    'dotnet restore "$PROJECT"',
    'dotnet run --project "$PATCHER"',
    '--no-restore',
    'STEP05.11 STEAMKIT IOS PATCH: PASS',
    'StS2-Launcher-Step-05.11.ipa',
):
    if required not in build_script:
        raise SystemExit(f"ERROR: Step 05.11 build isolation guard missing: {required}")

print("Step 05.11 protobuf-no-emit/Core/UI boundary validation passed.")
PY

# Still forbidden from the actual application source: Godot, runtime Cecil,
# Harmony/native game host. The build-only Cecil tool lives only under tools/.
if grep -RniE 'Godot|Mono\.Cecil|Harmony|NativeGodotHost' \
  src --include='*.cs' --include='*.csproj'; then
  echo "ERROR: Step 05.11 app source contains a forbidden later-stage subsystem." >&2
  exit 3
fi

for script in scripts/*.sh; do
  bash -n "$script"
done

echo "Step 05.11 repository validation passed."
