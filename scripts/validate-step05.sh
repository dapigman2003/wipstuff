#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

IOS_PROJECT="src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
CORE_PROJECT="src/StS2Launcher.Core/StS2Launcher.Core.csproj"
PATCHER_PROJECT="tools/StS2Launcher.SteamKitIosPatcher/StS2Launcher.SteamKitIosPatcher.csproj"
PATCHER_SOURCE="tools/StS2Launcher.SteamKitIosPatcher/Program.cs"
HTTP_FACTORY_SOURCE="src/StS2Launcher.Core/SteamHttpClientFactory.cs"

for required in "$IOS_PROJECT" "$CORE_PROJECT" "$PATCHER_PROJECT" "$PATCHER_SOURCE" "$HTTP_FACTORY_SOURCE"; do
  [[ -f "$required" ]] || {
    echo "ERROR: missing required Step 05.7 file: $required" >&2
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
    raise SystemExit("ERROR: Step 05.7 must not contain NativeReference.")

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
    "ApplicationVersion": "13",
    "ApplicationDisplayVersion": "0.0.13",
    "TrimMode": "full",
}
if values != expected:
    raise SystemExit(f"ERROR: Step 05.7 iOS build properties changed: {values}")

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
    raise SystemExit(f"ERROR: Step 05.7 expects exactly SteamKit2 3.3.1 in Core; got {packages}")

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
    "STEP05.7 STEAMKIT IOS PATCH: PASS",
):
    if required not in patcher:
        raise SystemExit(f"ERROR: SteamKit iOS patcher guard missing: {required}")

plist_path = Path("src/StS2Launcher.Step05.iOS/Info.plist")
with plist_path.open("rb") as f:
    plist = plistlib.load(f)
if plist.get("CFBundleIdentifier") != "com.community.sts2launcher":
    raise SystemExit("ERROR: unexpected bundle ID.")
if plist.get("CFBundleShortVersionString") != "0.0.13":
    raise SystemExit("ERROR: Info.plist Step 05.7 display version regression.")
if str(plist.get("CFBundleVersion")) != "13":
    raise SystemExit("ERROR: Info.plist Step 05.7 build version regression.")

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
        raise SystemExit(f"ERROR: Step 05.7 HTTP factory guard missing: {required}")

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
):
    if required not in probe:
        raise SystemExit(f"ERROR: Steam WebSocket compatibility probe missing: {required}")

if "ProtocolTypes.Tcp" in probe:
    raise SystemExit("ERROR: Step 05.7 SteamKit probe must stay WebSocket-only.")

network_probe = Path("src/StS2Launcher.Core/CmNetworkProbe.cs").read_text()
for required in (
    "ISteamDirectory/GetCMListForConnect",
    "HttpClient",
    "Dns.GetHostAddressesAsync",
    "TcpClient",
    "ClientWebSocket",
    '"/cmsocket/"',
    "JsonDocument.Parse",
    "StS2Launcher-iOS-Step05.7/0.0.13",
):
    if required not in network_probe:
        raise SystemExit(f"ERROR: Step 05.7 CM network regression probe missing: {required}")

# Authentication is intentionally forbidden in Step 05.x.
combined = probe + "\n" + network_probe + "\n" + factory
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
        raise SystemExit(f"ERROR: Step 05.7 contains authentication behavior: {forbidden}")

root_view = Path("src/StS2Launcher.Step05.iOS/RootViewController.cs").read_text()
for required in (
    "STEP 05.7 — IOS WEBSOCKET HANDLER FIX",
    "Version 0.0.13",
    "NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN",
    "Run SteamKit iOS WebSocket Fix Test",
    'TimeSpan.FromSeconds(25)',
    "STEAM CONNECTION PASS — 3/3",
    "Disconnected.UserInitiated",
    "IsConnected ever",
    "CurrentEndPoint",
    "STEAMKIT ASSEMBLY: PASS",
):
    if required not in root_view:
        raise SystemExit(f"ERROR: Step 05.7 UI marker missing: {required}")

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
    'STEP05.7 STEAMKIT IOS PATCH: PASS',
    'StS2-Launcher-Step-05.7.ipa',
):
    if required not in build_script:
        raise SystemExit(f"ERROR: Step 05.7 build isolation guard missing: {required}")

print("Step 05.7 WebSocket-handler/Core/UI boundary validation passed.")
PY

# Still forbidden from the actual application source: Godot, runtime Cecil,
# Harmony/native game host. The build-only Cecil tool lives only under tools/.
if grep -RniE 'Godot|Mono\.Cecil|Harmony|NativeGodotHost' \
  src --include='*.cs' --include='*.csproj'; then
  echo "ERROR: Step 05.7 app source contains a forbidden later-stage subsystem." >&2
  exit 3
fi

for script in scripts/*.sh; do
  bash -n "$script"
done

echo "Step 05.7 repository validation passed."
