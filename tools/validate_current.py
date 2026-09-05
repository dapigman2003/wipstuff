#!/usr/bin/env python3
from __future__ import annotations

import hashlib
import json
import plistlib
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
FAILURES: list[str] = []
PASSES = 0


def ok(message: str) -> None:
    global PASSES
    PASSES += 1
    print(f"PASS: {message}")


def fail(message: str) -> None:
    FAILURES.append(message)
    print(f"FAIL: {message}")


def require(condition: bool, message: str, detail: str | None = None) -> None:
    if condition:
        ok(message)
    else:
        fail(message)
        if detail:
            print(f"  {detail}")


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256(path: Path) -> str:
    return sha256_bytes(path.read_bytes())


def read(relative: str) -> str:
    return (ROOT / relative).read_text()




def csharp_delimiters_balanced(text: str) -> tuple[bool, str]:
    stack: list[tuple[str, int]] = []
    pairs = {')': '(', ']': '[', '}': '{'}
    openers = set(pairs.values())
    i = 0
    line = 1
    n = len(text)
    state = 'code'
    while i < n:
        ch = text[i]
        nxt = text[i + 1] if i + 1 < n else ''
        if ch == '\n':
            line += 1
        if state == 'line_comment':
            if ch == '\n':
                state = 'code'
            i += 1
            continue
        if state == 'block_comment':
            if ch == '*' and nxt == '/':
                state = 'code'; i += 2; continue
            i += 1; continue
        if state == 'string':
            if ch == '\\':
                i += 2; continue
            if ch == '"':
                state = 'code'
            i += 1; continue
        if state == 'verbatim':
            if ch == '"' and nxt == '"':
                i += 2; continue
            if ch == '"':
                state = 'code'
            i += 1; continue
        if state == 'char':
            if ch == '\\':
                i += 2; continue
            if ch == "'":
                state = 'code'
            i += 1; continue
        if ch == '/' and nxt == '/':
            state = 'line_comment'; i += 2; continue
        if ch == '/' and nxt == '*':
            state = 'block_comment'; i += 2; continue
        if ch == '@' and nxt == '"':
            state = 'verbatim'; i += 2; continue
        if ch == '"':
            state = 'string'; i += 1; continue
        if ch == "'":
            state = 'char'; i += 1; continue
        if ch in openers:
            stack.append((ch, line))
        elif ch in pairs:
            if not stack or stack[-1][0] != pairs[ch]:
                return False, f"unexpected {ch!r} at line {line}"
            stack.pop()
        i += 1
    if state in {'block_comment', 'string', 'verbatim', 'char'}:
        return False, f"unterminated lexical state {state}"
    if stack:
        ch, start = stack[-1]
        return False, f"unclosed {ch!r} opened at line {start}"
    return True, ''

def text_files_under(base: Path):
    skip_suffixes = {".zip", ".a", ".dll", ".png", ".jpg", ".jpeg", ".webp", ".binlog"}
    if not base.exists():
        return
    for path in base.rglob("*"):
        if not path.is_file() or path.suffix.lower() in skip_suffixes:
            continue
        try:
            yield path, path.read_text(errors="strict")
        except (UnicodeDecodeError, OSError):
            continue


print("StS2 Launcher — Step 35.0.30 / Step 36.0 UI Return Fix + Controlled Exact ExecuteEssential validation")
print(f"Root: {ROOT}")

# Parse all repository project/property/target XML and root JSON before detailed policy assertions.
xml_failures: list[str] = []
xml_files = sorted([*ROOT.rglob("*.csproj"), *ROOT.rglob("*.props"), *ROOT.rglob("*.targets")])
for xml_path in xml_files:
    try:
        ET.parse(xml_path)
    except Exception as ex:
        xml_failures.append(f"{xml_path.relative_to(ROOT)}: {ex}")
require(not xml_failures, f"all {len(xml_files)} project/props/targets XML files parse", "; ".join(xml_failures[:10]))
try:
    json.loads((ROOT / "global.json").read_text())
    ok("global.json parses")
except Exception as ex:
    fail(f"global.json parse failed: {ex}")

cs_failures: list[str] = []
cs_files = sorted([*ROOT.glob("src/**/*.cs"), *ROOT.glob("tests/**/*.cs"), *ROOT.glob("fixtures/**/*.cs"), *ROOT.glob("tools/**/*.cs")])
for cs_path in cs_files:
    balanced, detail = csharp_delimiters_balanced(cs_path.read_text())
    if not balanced:
        cs_failures.append(f"{cs_path.relative_to(ROOT)}: {detail}")
require(not cs_failures, f"lexical delimiter scan passes for {len(cs_files)} C# files", "; ".join(cs_failures[:10]))

# ---------------------------------------------------------------------------
# Canonical live project and version
# ---------------------------------------------------------------------------
project_path = ROOT / "src/StS2Launcher.iOS/StS2Launcher.iOS.csproj"
plist_path = ROOT / "src/StS2Launcher.iOS/Info.plist"
legacy_project_dir = ROOT / "src/StS2Launcher.Step05.iOS"

require(project_path.is_file(), "canonical iOS project exists")
require(plist_path.is_file(), "canonical iOS Info.plist exists")
require(not legacy_project_dir.exists(), "legacy Step05 iOS project is absent from live src")

ios_projects = sorted((ROOT / "src").glob("*.iOS/*.csproj"))
require(len(ios_projects) == 1 and ios_projects[0] == project_path, "src contains exactly one live iOS application project")

try:
    ET.parse(project_path)
    ok("iOS csproj parses as XML")
except Exception as ex:
    fail(f"iOS csproj XML parse failed: {ex}")

try:
    with plist_path.open("rb") as stream:
        plist = plistlib.load(stream)
    ok("Info.plist parses")
except Exception as ex:
    fail(f"Info.plist parse failed: {ex}")
    plist = {}

project_text = project_path.read_text()
require("<ApplicationVersion>153</ApplicationVersion>" in project_text, "build version is 153")
require("<ApplicationDisplayVersion>0.0.153</ApplicationDisplayVersion>" in project_text, "display version is 0.0.153")
require(plist.get("CFBundleVersion") == "153", "Info.plist build version is 153")
require(plist.get("CFBundleShortVersionString") == "0.0.153", "Info.plist display version is 0.0.153")
require(plist.get("UIFileSharingEnabled") is True, "iOS Files sharing remains enabled")
require(plist.get("LSSupportsOpeningDocumentsInPlace") is True, "open-in-place Documents access remains enabled")
require("<RootNamespace>StS2Launcher.iOS</RootNamespace>" in project_text, "canonical iOS root namespace is explicit")
require("<AssemblyName>StS2Launcher.iOS</AssemblyName>" in project_text, "canonical iOS assembly name is explicit")

# Every live iOS source file uses the canonical namespace family.
ios_cs = list((ROOT / "src/StS2Launcher.iOS").rglob("*.cs"))
ios_text = "\n".join(p.read_text() for p in ios_cs)
require("StS2Launcher.Step05.iOS" not in ios_text, "live iOS source contains no legacy Step05 namespace")
require("namespace StS2Launcher.iOS" in ios_text, "live iOS source uses canonical namespace")

release_presentation_path = ROOT / "src/StS2Launcher.iOS/UI/CurrentReleasePresentation.cs"
require(release_presentation_path.is_file(), "current release presentation has one dedicated UI source")
release_presentation = release_presentation_path.read_text() if release_presentation_path.is_file() else ""
require("STEP 35.0.30 / STEP 36.0 — GATE-D UI RETURN FIX + CONTROLLED EXACT EXECUTEESSENTIAL" in release_presentation, "top launcher banner identifies active Step 35.0.30 / Step 36.0 combined candidate")
require(all(marker in release_presentation for marker in ["STEP 32 CLOSED POSITIVE 4/4", "STEP 33 CLOSED POSITIVE 4/4", "STEP 34 CLOSED POSITIVE 4/4", "STEP 35 EXACT CORE CLOSURE POSITIVE", "0.0.146", "37-pointer", "ManagedCallbacks", "GD_OnCoreApiAssemblyLoaded", "0.0.149", "RanToCompletion", "214/214", "0.0.152", "exact Step-35 core closure", "D_TASK_RETURN_START", "0.0.153", "0x06007D03", "ExecuteEssential"]), "top launcher banner preserves bridge/CI provenance, records physical 0.0.152 exact core closure, and identifies the Step-36 advance")
require("NSBundle.MainBundle.ObjectForInfoDictionary(\"CFBundleShortVersionString\")" in release_presentation, "top launcher version is derived from the built Info.plist instead of a stale hard-coded version")
require('ExpectedDisplayVersion = "0.0.153"' in release_presentation and 'ExpectedBuildVersion = "153"' in release_presentation, "Step 35.0.30 / Step 36.0 source pins expected bundle release identity")
require("GateSImplementationMarker" not in release_presentation and "GateTImplementationMarker" not in release_presentation, "retired Step-27 execution markers are absent from the active release presentation")
root_ui_text = read("src/StS2Launcher.iOS/UI/RootViewController.cs")
require("CurrentReleasePresentation.StepTitle" in root_ui_text and "CurrentReleasePresentation.DisplayVersion" in root_ui_text and "CurrentReleasePresentation.Summary" in root_ui_text and "CurrentReleasePresentation.InitialStatus" in root_ui_text, "RootViewController consumes the single current-release presentation source")
require("STEP 26 — CONTROLLED EMPTY HARMONY PATCHPROCESSOR CREATION" not in root_ui_text and "Version 0.0.83" not in root_ui_text, "stale Step-26 top-banner identity is removed")
require("STEP 27.0.5 —" not in release_presentation and "0.0.89 is the crash-localization candidate" not in release_presentation, "stale prior Step-27 candidate banner identity is removed")

# ---------------------------------------------------------------------------
# Physically proven runtime/build policy
# ---------------------------------------------------------------------------
require("<TrimMode>copy</TrimMode>" in project_text and "<MtouchLink>None</MtouchLink>" in project_text, "dynamic-payload host disables managed trimming with copy/no-link policy")
require("<MtouchInterpreter>-all</MtouchInterpreter>" in project_text, "Step 20 interpreter policy retained")
require("'$(UseInterpreter)' == 'true'" in project_text, "build guard rejects broad UseInterpreter=true")
require("'$(PublishAot)' == 'true'" in project_text, "build guard rejects NativeAOT")
require("STEP35 RUNTIME POLICY" in project_text, "runtime policy emits Step 35 build telemetry")
require("STEP35 DYNAMIC PAYLOAD TRIMMING POLICY: MtouchLink=$(MtouchLink); TrimMode=$(TrimMode)" in project_text, "dynamic-payload trimming policy emits exact Step-35 build telemetry")
require("'$(MtouchLink)' != 'None'" in project_text and "'$(TrimMode)' != 'copy'" in project_text, "build guards reject drift from copy/no-link host policy")

all_roots = re.findall(r'<TrimmerRootAssembly Include="([^"]+)"\s*/>', project_text)
step22_roots = [
    "netstandard", "System.Data.Common", "System.Diagnostics.Contracts", "System.Diagnostics.StackTrace",
    "System.Diagnostics.TraceSource", "System.Diagnostics.Tracing", "System.IO.FileSystem.DriveInfo",
    "System.IO.MemoryMappedFiles", "System.Net.Ping", "System.Net.Quic", "System.Numerics.Vectors",
    "System.Reflection.Metadata", "System.Runtime.CompilerServices.Unsafe", "System.Runtime.Loader",
    "System.Runtime.Serialization.Json", "System.Runtime.Serialization.Primitives", "System.Runtime.Serialization.Xml",
    "System.Threading.Tasks.Parallel", "System.Threading.ThreadPool", "System.Xml.XDocument",
    "System.Xml.XmlSerializer", "System.Xml.XPath",
]
require(set(step22_roots).issubset(set(all_roots)), "all 22 physically proven Step 22 direct host roots remain rooted")
require(len([x for x in all_roots if x in step22_roots]) == 22, "Step 22 direct root set contains exactly 22 unique roots")
require({"SteamKit2", "protobuf-net", "protobuf-net.Core"}.issubset(set(all_roots)), "SteamKit/protobuf reflection roots remain protected")
require(all_roots.count("System.Collections.Concurrent") == 1, "physically proven Step 24.0.6 System.Collections.Concurrent preservation root remains exact")
require(all_roots.count("System.Linq") == 1, "historical Step 27.0.22 System.Linq preservation root remains recorded exactly once")
expected_all_roots = set(step22_roots) | {"SteamKit2", "protobuf-net", "protobuf-net.Core", "System.Collections.Concurrent", "System.Linq"}
require(set(all_roots) == expected_all_roots and len(all_roots) == len(expected_all_roots), "measured historical root descriptor set remains byte-bounded while copy/no-link supersedes it for preservation")
require("Step 24 physically proved this additional post-publish dynamic-IL preservation root" in project_text, "physically proven Step 24 preservation root is documented as protected platform policy")
require("STEP32 PROVEN DYNAMIC IL PRESERVATION ROOT: System.Collections.Concurrent" in project_text, "Step 32 build telemetry retains the physically proven framework preservation root")
require("STEP32 HISTORICAL POST-PUBLISH LINQ PRESERVATION ROOT: System.Linq (superseded by copy/no-link host policy)" in project_text, "build telemetry explicitly marks the System.Linq root as historical under copy/no-link")
require("System.Private.CoreLib" not in all_roots, "Step 25 does not broaden trimming by rooting System.Private.CoreLib")
retired_harmony_platform_files = [
    ROOT / "src/StS2Launcher.iOS/Platform/Step25HarmonyConstructorFrameworkPreservation.cs",
    ROOT / "src/StS2Launcher.iOS/Platform/Step27AccessToolsFrameworkPreservation.cs",
    ROOT / "src/StS2Launcher.iOS/Platform/Step27PatchEngineFrameworkPreservation.cs",
]
require(not any(path.exists() for path in retired_harmony_platform_files), "retired Step 25-27 DynamicDependency preservation anchors are absent from the active iOS/AOT graph")
require("STEP32 HISTORICAL HARMONY CONSTRUCTOR FRAMEWORK PRESERVATION" not in project_text and "STEP32 HISTORICAL ACCESSTOOLS FRAMEWORK PRESERVATION" not in project_text and "STEP32 RETIRED PATCH-ENGINE FRAMEWORK PRESERVATION" not in project_text, "active build telemetry no longer roots retired runtime-Harmony preservation anchors")
build_ios = read("scripts/build-ios.sh")
require("STEP35 DYNAMIC PAYLOAD TRIMMING POLICY: MtouchLink=None; TrimMode=copy" in build_ios, "iOS publish requires exact Step-35 copy/no-link dynamic-payload telemetry")
require("HISTORICAL HARMONY CONSTRUCTOR FRAMEWORK PRESERVATION" not in build_ios, "iOS publish no longer requires retired Harmony-constructor preservation telemetry")
require("DiskArbitration" in project_text and '<_LinkerFrameworks Remove="DiskArbitration" />' in project_text, "DiskArbitration-only linker framework filter remains present")

# ---------------------------------------------------------------------------
# Physically proven source protection
# ---------------------------------------------------------------------------
core_manifest = ROOT / "tools/validation/protected-step22.2-core.sha256"
approved_delta_manifest = ROOT / "tools/validation/approved-step22.4.2-regression-delta.sha256"
expected_core: dict[str, str] = {}
for line in core_manifest.read_text().splitlines():
    if not line.strip():
        continue
    digest, name = line.split("  ", 1)
    expected_core[name] = digest
approved_delta: dict[str, str] = {}
for line in approved_delta_manifest.read_text().splitlines():
    if not line.strip():
        continue
    digest, relative = line.split("  ", 1)
    approved_delta[relative] = digest
current_core_by_name = {p.name: p for p in (ROOT / "src/StS2Launcher.Core").rglob("*.cs")}
missing_core = sorted(set(expected_core) - set(current_core_by_name))
require(not missing_core, f"all {len(expected_core)} physically proven Step 22.2 Core source files are retained", ", ".join(missing_core))

intentional_core_delta = "src/StS2Launcher.Core/Compatibility/ExpressionInterpreterCompatibility.cs"
intentional_core_name = Path(intentional_core_delta).name
unexpected_core_changes: list[str] = []
for name, digest in expected_core.items():
    path = current_core_by_name.get(name)
    if path is None:
        continue
    if name == intentional_core_name:
        expected = approved_delta.get(intentional_core_delta)
        if expected is None or sha256(path) != expected:
            unexpected_core_changes.append(name)
    elif sha256(path) != digest:
        unexpected_core_changes.append(name)
require(not unexpected_core_changes, "96 baseline Step 22.2 Core behavior files remain byte-for-byte unchanged and the one approved Step 19 regression delta is exact", ", ".join(unexpected_core_changes))

for relative, digest in approved_delta.items():
    path = ROOT / relative
    require(path.is_file() and sha256(path) == digest, f"approved Step 22.4.2 regression delta is hash-pinned: {relative}")
require((ROOT / "src/StS2Launcher.Core/Diagnostics/DeviceTestReportWriter.cs").is_file(), "additive device report writer remains isolated in Diagnostics")

# The platform/native manifest contains hashes from the physically proven legacy namespace.
# Step 22.4 intentionally changed only StS2Launcher.Step05.iOS -> StS2Launcher.iOS in these
# managed platform files; normalize that namespace before hash comparison.
platform_manifest = ROOT / "tools/validation/protected-step22.2-platform-native.sha256"
platform_missing: list[str] = []
platform_changed: list[str] = []
for line in platform_manifest.read_text().splitlines():
    if not line.strip():
        continue
    digest, relative = line.split("  ", 1)
    path = ROOT / relative
    if not path.is_file():
        platform_missing.append(relative)
        continue
    data = path.read_bytes().replace(b"StS2Launcher.iOS", b"StS2Launcher.Step05.iOS")
    if sha256_bytes(data) != digest:
        platform_changed.append(relative)
require(not platform_missing, "physically proven iOS platform/native files are retained at canonical paths", ", ".join(platform_missing))
step35_callback_platform_delta = {
    "src/StS2Launcher.iOS/Platform/GodotStep15NativeBridge.cs",
    "native/step15/godot_module/sts2_ios_host/step15_ios_host_bridge.mm",
}
unexpected_platform_changed = sorted(set(platform_changed) - step35_callback_platform_delta)
require(not unexpected_platform_changed, "platform/native behavior remains protected except the explicit Step-35.0.25 callback-handoff bridge delta", ", ".join(unexpected_platform_changed))
require(step35_callback_platform_delta.issubset(set(platform_changed)), "Step-35.0.25 callback-handoff bridge delta is explicit rather than silently matching the old no-callback baseline")

# ---------------------------------------------------------------------------
# Source/file structure
# ---------------------------------------------------------------------------
core_dirs = {p.name for p in (ROOT / "src/StS2Launcher.Core").iterdir() if p.is_dir()}
require({"Foundation", "Steam", "Compatibility", "Godot", "Runtime", "Diagnostics"}.issubset(core_dirs), "Core source remains organized by subsystem")
require(list((ROOT / "src/StS2Launcher.Core").glob("*.cs")) == [], "Core root contains no loose implementation .cs files")
ui_files = sorted((ROOT / "src/StS2Launcher.iOS/UI").glob("RootViewController*.cs"))
require(len(ui_files) >= 8, "RootViewController remains split into focused partial files")
large_ui = [(p.name, p.stat().st_size) for p in ui_files if p.stat().st_size > 50_000]
require(not large_ui, "no RootViewController partial exceeds 50 KB", ", ".join(f"{n}={s}" for n, s in large_ui))
require("public sealed partial class RootViewController" in read("src/StS2Launcher.iOS/UI/RootViewController.cs"), "RootViewController retains sealed partial structure")

active_scripts = {p.name for p in (ROOT / "scripts").glob("*.sh")}
expected_scripts = {"build-godot.sh", "preflight-godot-link.sh", "build-ios.sh", "test.sh", "validate.sh", "verify-ipa.sh", "codemagic.sh"}
require(active_scripts == expected_scripts, "active scripts are exactly seven canonical entry points", f"found={sorted(active_scripts)}")
require(not any("step15" in name.lower() for name in active_scripts), "active script filenames are not historical-step named")
release_config_path = ROOT / "scripts/lib/current-release.sh"
require(release_config_path.is_file(), "canonical shell release configuration exists")
release_config = release_config_path.read_text() if release_config_path.is_file() else ""
for key, value in {
    "STS2_IOS_PROJECT": "src/StS2Launcher.iOS/StS2Launcher.iOS.csproj",
    "STS2_APP_BUNDLE_NAME": "StS2Launcher.iOS.app",
    "STS2_IPA_REL": "artifacts/StS2-Launcher-Step-36.ipa",
    "STS2_DISPLAY_VERSION": "0.0.153",
    "STS2_BUILD_VERSION": "153",
    "STS2_RUNTIME_POLICY_MARKER": "STEP35 RUNTIME POLICY:",
}.items():
    require(f'{key}="{value}"' in release_config, f"release config pins {key}")

# Active tooling/source cannot depend on legacy project paths or history.zip.
forbidden_active_strings = ["StS2Launcher.Step05.iOS", "history.zip", "history/scripts/", "history/docs/"]
active_areas = [ROOT / "src", ROOT / "scripts", ROOT / "tests", ROOT / "fixtures", ROOT / "native"]
active_hits: list[str] = []
for area in active_areas:
    for path, text in text_files_under(area):
        for marker in forbidden_active_strings:
            if marker in text:
                active_hits.append(f"{path.relative_to(ROOT)} -> {marker}")
# tools are scanned except this validator, which necessarily names the forbidden strings as policy checks.
for path, text in text_files_under(ROOT / "tools"):
    if path == Path(__file__).resolve():
        continue
    for marker in forbidden_active_strings:
        if marker in text:
            active_hits.append(f"{path.relative_to(ROOT)} -> {marker}")
require(not active_hits, "active source/tooling has no dependency on legacy paths or history archive", "; ".join(active_hits[:20]))
require(not (ROOT / "history").exists(), "no live history/ directory exists")
if (ROOT / "history.zip").exists():
    print("INFO: optional history.zip is present; it is intentionally not opened or required by validation")
else:
    print("INFO: optional history.zip is absent; canonical validation is intentionally unaffected")

# ---------------------------------------------------------------------------
# Diagnostics/test reporting
# ---------------------------------------------------------------------------
writer_source = read("src/StS2Launcher.Core/Diagnostics/DeviceTestReportWriter.cs")
reports_source = read("src/StS2Launcher.iOS/UI/RootViewController.Reports.cs")
require("File.Move(temporary, destination, overwrite: true)" in writer_source, "device reports use atomic temporary-file replacement")
require("Path.GetFileName(trimmed)" in writer_source and ".txt" in writer_source, "device report file names are constrained to local .txt files")
required_report_files = {
    "Foundation-5of5.txt", "Step12-ManagedInstall.txt", "Step13-OfflineReady.txt",
    "Step14-CompatibilityInventory.txt", "Step15-GodotFoundation.txt", "Step16-ManagedPreparation.txt",
    "Step17-CompatibilityCallSites.txt", "Step18-RealAssemblyRewrite.txt", "Step19-ExpressionInterpreter.txt",
    "Step20-DynamicManagedExecution.txt", "Step21-RuntimeFrameworkBinding.txt", "Step22-HostBindingFrontier.txt",
    "Step23-FirstRealGameLoad.txt", "Step24-ControlledManagedInitialization.txt",
    "Step28-AheadOfLoadManagedTransformation.txt", "Step29-RealStS2CompatibilityTargetAudit.txt",
    "TestSetup-Repair.txt", "TestSetup-Update.txt", "TestSetup-DownloadCacheClear.txt", "TestSetup-FreshDownload.txt",
}
ui_text = "\n".join(p.read_text() for p in ui_files)
missing_reports = sorted(name for name in required_report_files if name not in ui_text)
require(not missing_reports, "all current on-device verification/test paths have deterministic text-report outputs", ", ".join(missing_reports))
require("Steam password" in writer_source and "refresh tokens" in writer_source, "report schema documents credential exclusions")
require("_passwordField" not in reports_source and "_usernameField" not in reports_source, "shared report writer never reads credential UI fields")
require((ROOT / "tests/StS2Launcher.Core.Tests/TestSupport/TempTestDirectory.cs").is_file(), "unit tests use shared temporary-directory helper")
all_test_text = "\n".join(p.read_text() for p in (ROOT / "tests/StS2Launcher.Core.Tests").rglob("*.cs"))
require("private sealed class TemporaryDirectory" not in all_test_text, "duplicated per-test TemporaryDirectory helpers remain removed")
require("Assert.ThrowsException<" not in all_test_text and "Assert.ThrowsExceptionAsync<" not in all_test_text, "active MSTest v4 tests avoid removed ThrowsException APIs")
require((ROOT / "tests/StS2Launcher.Core.Tests/Runtime/DeviceTestReportWriterTests.cs").is_file(), "device report writer has host unit tests")
report_test_source = read("tests/StS2Launcher.Core.Tests/Runtime/DeviceTestReportWriterTests.cs")
require("ThrowsExceptionAsync" not in report_test_source, "report writer tests avoid removed MSTest v4 ThrowsExceptionAsync API")
require("Assert.ThrowsExactlyAsync<ArgumentException>" in report_test_source, "report writer tests use MSTest v4 ThrowsExactlyAsync")
require("DataTestMethod" not in report_test_source, "report writer data rows use TestMethod instead of obsolete DataTestMethod")

expression_source = read("src/StS2Launcher.Core/Compatibility/ExpressionInterpreterCompatibility.cs")
expression_policy = read("src/StS2Launcher.Core/Compatibility/ExpressionRuntimeCompatibilityPolicy.cs")
expression_tests = read("tests/StS2Launcher.Core.Tests/Compatibility/ExpressionInterpreterCompatibilityTests.cs")
require("IosNoDynamicCodeFallbackProven" not in expression_source, "current Step 19 regression no longer carries the stale pre-Step-20 fallback flag")
require("Step 19 requires RuntimeFeature.IsDynamicCodeSupported == false" not in expression_source, "current Step 19 regression does not require IsDynamicCodeSupported=false")
require("ExpressionRuntimeCompatibilityPolicy.Evaluate" in expression_source, "Step 19 Gate A uses centralized runtime compatibility policy")
require("dynamicCodeCompiled" in expression_policy and "UnexpectedDynamicCompilationMode" in expression_policy, "Step 19 policy rejects dynamically compiled code on iOS")
require("InterpreterEnabledMode" in expression_policy and "HistoricalNoDynamicCodeMode" in expression_policy, "Step 19 policy recognizes both historical and post-Step-20 valid iOS modes")
require("IosExpressionRuntimePolicyTracksCanonicalNonJitContract" in expression_tests, "Step 19 runtime policy has explicit host unit coverage")
require("[DataRow(true, false, true" in expression_tests and "[DataRow(false, false, true" in expression_tests, "Step 19 tests cover canonical interpreter-enabled and historical fallback modes")
require("[DataRow(true, true, false" in expression_tests, "Step 19 tests reject iOS dynamic-code compilation")

for script_name, report_marker in [
    ("test.sh", "artifacts/reports/host-unit-tests.txt"),
    ("validate.sh", "artifacts/reports/static-validation.txt"),
    ("verify-ipa.sh", "artifacts/reports/ipa-verification.txt"),
    ("preflight-godot-link.sh", "artifacts/reports/godot-native-preflight.txt"),
    ("codemagic.sh", "artifacts/reports/build-summary.txt"),
]:
    path = ROOT / "scripts" / script_name
    require(path.is_file(), f"active {script_name} exists")
    if path.is_file():
        require(report_marker in path.read_text(), f"{script_name} emits a shareable text report")

# ---------------------------------------------------------------------------
# Step 23 first real CLR-load boundary
# ---------------------------------------------------------------------------
step23_core_files = [
    "src/StS2Launcher.Core/Runtime/FirstRealGameAssemblyLoad.cs",
    "src/StS2Launcher.Core/Runtime/FirstRealGameAssemblyLoadGate.cs",
    "src/StS2Launcher.Core/Runtime/FirstRealGameAssemblyLoadGateResult.cs",
    "src/StS2Launcher.Core/Runtime/FirstRealGameAssemblyLoadProgress.cs",
    "src/StS2Launcher.Core/Runtime/FirstRealGameAssemblyLoadSummary.cs",
    "src/StS2Launcher.Core/Runtime/FirstRealGameAssemblyLoadGateSequence.cs",
]
for relative in step23_core_files:
    require((ROOT / relative).is_file(), f"Step 23 production boundary source exists: {relative}")
step23_test_path = ROOT / "tests/StS2Launcher.Core.Tests/Runtime/FirstRealGameAssemblyLoadTests.cs"
step23_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.GameLoad.cs"
require(step23_test_path.is_file(), "Step 23 boundary has host unit tests")
require(step23_ui_path.is_file(), "Step 23 boundary has isolated iOS UI/report partial")

step23_source = read("src/StS2Launcher.Core/Runtime/FirstRealGameAssemblyLoad.cs")
step23_tests = step23_test_path.read_text() if step23_test_path.is_file() else ""
step23_ui = step23_ui_path.read_text() if step23_ui_path.is_file() else ""
for required in [
    "LoadFromStream(stream)",
    "AssemblyLoadContext.Default.LoadFromAssemblyName",
    "LoadUnmanagedDll(string unmanagedDllName)",
    "RuntimeClosureReady",
    "plan.Blockers.Length != 0",
    'type.Name.Equals("<Module>"',
    'method.Name.Equals(".cctor"',
    "Persisted plan exactly covers prepared AssemblyRef metadata: YES",
    "RuntimeFeature.IsDynamicCodeCompiled",
    "ComputeSha1Hex(primary.PreparedPath)",
]:
    require(required in step23_source, f"Step 23 production boundary contains required safety invariant: {required}")

for forbidden in [
    ".EntryPoint",
    ".GetTypes(",
    "assembly.GetType(",
    ".GetMethod(",
    ".GetMethods(",
    ".Invoke(",
    "Activator.",
    "RuntimeHelpers.RunClassConstructor",
    "CreateInstance(",
    "GetCustomAttributes(",
    "LoadUnmanagedDllFromPath",
    "NativeLibrary.Load",
]:
    require(forbidden not in step23_source, f"Step 23 load-only production boundary forbids intentional execution/native API: {forbidden}")

for forbidden_write in ["File.Copy(", "File.Move(", "File.Write", "File.Create(", "Directory.CreateDirectory("]:
    require(forbidden_write not in step23_source, f"Step 23 production boundary never mutates prepared/live install bytes: {forbidden_write}")
require("throw new DllNotFoundException" in step23_source, "Step 23 refuses all native library resolution")
require("RejectedManagedRequests.Add" in step23_source and "throw new FileLoadException" in step23_source, "Step 23 strict resolver audits and rejects unplanned managed bindings")
require("Step23-FirstRealGameLoad.txt" in step23_ui, "Step 23 on-device run emits a Files-visible text report")
require("GateARejectsPrimaryModuleInitializerBeforeAnyRealClrLoad" in step23_tests, "Step 23 host tests prove a primary module initializer still stops before CLR load")
require("DependencyModuleInitializerIsDeferredWhilePrimaryAndSafeClosureLoad" in step23_tests, "Step 23 host tests prove dependency module initializers are deferred without weakening the primary-load boundary")
require("DeferredInitializerRequests" in step23_source and "initializer-bearing private dependency" in step23_source, "Step 23 resolver explicitly refuses initializer-bearing dependency loads before Step 24")
require("Primary module initializers" in step23_source and "Deferred initializer-bearing private assemblies" in step23_source, "Step 23 Gate A separates primary and dependency module-initializer policy")
require("FormatModuleInitializerAudit" in step23_source and "IL_" in step23_source, "Step 23 exports a metadata-only Cecil IL audit for deferred module initializers")
require("using Mono.Cecil.Cil;" in step23_source, "Step 23 deferred IL audit imports Mono.Cecil.Cil Instruction type")
require("SyntheticZeroBlockerPreparedRuntimeLoadsAndResolvesWithoutInvokingGameCode" in step23_tests, "Step 23 host tests cover synthetic 4/4 load-only closure")
require("GateARejectsPreparedByteDriftBeforeAnyRealClrLoad" in step23_tests, "Step 23 host tests prove prepared-byte drift stops before CLR load")
require("GateARejectsPersistedPlanThatDoesNotCoverPreparedAssemblyReferences" in step23_tests, "Step 23 host tests prove stale/incomplete binding plans stop before CLR load")
require("collectibleLoadContext: true" in step23_tests, "Step 23 host tests use collectible contexts while production remains process-resident")
require("CreateSyntheticPrimarySimpleName" in step23_tests, "Step 23 host tests allocate unique synthetic primary identities")
require("SyntheticDependency." in step23_tests, "Step 23 host tests allocate unique synthetic dependency identities to avoid collectible-ALC contamination")
require("expectedPrimarySimpleName: primarySimpleName" in step23_tests and "freshProcessAssemblyNames: [primarySimpleName]" in step23_tests, "Step 23 host tests scope freshness checks to their unique synthetic identity")
require("ForceCollectibleContexts" not in step23_tests, "Step 23 host tests do not depend on collectible ALC GC timing")
require("BuildSyntheticBindingPlan(" in step23_tests and "module.AssemblyReferences" in step23_tests, "Step 23 synthetic plans derive edges from post-write Cecil AssemblyRefs")
require("Synthetic Step 23 fixture unexpectedly retained a legacy mscorlib AssemblyRef" in step23_tests and "Host System.Private.CoreLib has no FullName." not in step23_tests, "Step 23 module-initializer fixture prevents legacy mscorlib metadata instead of aliasing it")
require("Update the fixture binding-plan builder rather than weakening Gate A metadata coverage" in step23_tests, "Step 23 fixture rejects unexpected synthetic AssemblyRefs instead of weakening production coverage")
require("InternalsVisibleTo(\"StS2Launcher.Core.Tests\")" in read("src/StS2Launcher.Core/Properties/AssemblyInfo.cs"), "Step 23 test-only identity seam is limited to the host test assembly")
require("[ExpectedPrimarySimpleName, \"SlayTheSpire2\"]" in step23_source, "Step 23 production constructor preserves the physical fresh-process game identity policy")

step23_manifest = ROOT / "tools/validation/protected-step23.4.3-load-boundary.sha256"
require(step23_manifest.is_file(), "physically closed Step 23.4.3 boundary hash manifest exists")
if step23_manifest.is_file():
    step23_mismatches: list[str] = []
    for line in step23_manifest.read_text().splitlines():
        if not line.strip():
            continue
        digest, relative = line.split("  ", 1)
        path = ROOT / relative
        if not path.is_file() or sha256(path) != digest:
            step23_mismatches.append(relative)
    require(not step23_mismatches, "physically closed Step 23.4.3 load-boundary implementation remains hash-pinned", ", ".join(step23_mismatches))


# Step 23.4.3 host-fixture invariant: construct a modern core-library scope before
# touching Cecil TypeSystem.Void. Do not try to erase mscorlib after it has already been
# embedded in a TypeReference; Step 23.4.2 proved that approach is insufficient.
step23_tests = read("tests/StS2Launcher.Core.Tests/Runtime/FirstRealGameAssemblyLoadTests.cs")
require("Synthetic Step 23 fixture unexpectedly retained a legacy mscorlib AssemblyRef" in step23_tests, "Step 23 synthetic initializer fixture rejects legacy mscorlib metadata")
require("Fix the fixture generator rather than adding a production core-library alias." in step23_tests, "Step 23 fixture documents no-production-alias policy")
require("Host System.Private.CoreLib has no FullName." not in step23_tests, "Step 23 synthetic binding plan has no mscorlib-to-System.Private.CoreLib alias")
require('Assembly.Load(new AssemblyName("System.Runtime"))' in step23_tests, "Step 23 synthetic initializer fixture derives the real host System.Runtime identity")
require('dependencyReferences.Add(systemRuntimeReference)' in step23_tests and 'primaryReferences.Add(systemRuntimeReference)' in step23_tests, "Step 23 initializer-bearing fixtures explicitly declare System.Runtime")
require('reference.Name.Equals("System.Runtime"' in step23_tests and '"HostFramework", systemRuntimeFullName' in step23_tests, "Step 23 synthetic plan treats System.Runtime as an ordinary exact host binding")
write_assembly_start = step23_tests.find("private static void WriteAssembly(")
add_ref_pos = step23_tests.find("assembly.MainModule.AssemblyReferences.Add(item);", write_assembly_start)
type_void_pos = step23_tests.find("assembly.MainModule.TypeSystem.Void", write_assembly_start)
require(write_assembly_start >= 0 and add_ref_pos >= 0 and type_void_pos > add_ref_pos, "Step 23 fixture adds modern AssemblyRefs before Cecil TypeSystem.Void is accessed")
require("assembly.MainModule.AssemblyReferences.Clear();" not in step23_tests, "Step 23 fixture no longer relies on post-construction AssemblyRef clearing")
require("Assert.AreSame(systemRuntimeReference, voidType.Scope)" in step23_tests, "Step 23 fixture verifies Cecil selected the declared System.Runtime core-library scope before write")
require("ReadingMode = ReadingMode.Immediate" in step23_tests and "CollectionAssert.AreEqual(" in step23_tests, "Step 23 fixture immediately reopens and verifies the exact persisted AssemblyRef set")
require("MetadataType.Void" in step23_tests and 'initializer.ReturnType.Scope?.Name' in step23_tests, "Step 23 fixture verifies primitive void metadata and System.Runtime scope after reopen")

# ---------------------------------------------------------------------------
# Step 24 controlled managed initialization boundary
# ---------------------------------------------------------------------------
step24_core_files = [
    "src/StS2Launcher.Core/Runtime/ControlledManagedInitialization.cs",
    "src/StS2Launcher.Core/Runtime/ControlledManagedInitializationGate.cs",
    "src/StS2Launcher.Core/Runtime/ControlledManagedInitializationGateResult.cs",
    "src/StS2Launcher.Core/Runtime/ControlledManagedInitializationProgress.cs",
    "src/StS2Launcher.Core/Runtime/ControlledManagedInitializationSummary.cs",
    "src/StS2Launcher.Core/Runtime/ControlledManagedInitializationGateSequence.cs",
]
for relative in step24_core_files:
    require((ROOT / relative).is_file(), f"Step 24 controlled-initialization source exists: {relative}")
step24_test_path = ROOT / "tests/StS2Launcher.Core.Tests/Runtime/ControlledManagedInitializationTests.cs"
step24_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.Initialization.cs"
require(step24_test_path.is_file(), "Step 24 boundary has host unit tests")
require(step24_ui_path.is_file(), "Step 24 boundary has isolated iOS UI/report partial")
step24_source = read("src/StS2Launcher.Core/Runtime/ControlledManagedInitialization.cs")
step24_tests = step24_test_path.read_text() if step24_test_path.is_file() else ""
step24_ui = step24_ui_path.read_text() if step24_ui_path.is_file() else ""

for required in [
    'TargetSimpleName = "0Harmony"',
    "TargetVersion = new(2, 4, 2, 0)",
    "RunPreparedLoadPreflightAsync",
    "initializerBearing.Length != 1",
    "target.ModuleInitializerCount != 1",
    "Initializer call graph exceeded the Step 24.0 bound of 512",
    "P/Invoke reachable",
    "Code.Calli",
    "Code.Ldftn",
    "Code.Ldvirtftn",
    "QueueAutomaticTypeInitializer",
    "AutomaticInitializerCount",
    "System.Runtime.InteropServices.NativeLibrary",
    "System.Reflection.Emit.",
    "RunClassConstructor",
    "AllowedInitializerAssemblyFullName",
    "RuntimeHelpers.RunModuleConstructor(targetAssembly.ManifestModule.ModuleHandle)",
    "RejectedManagedRequests",
    "throw new DllNotFoundException",
    "ComputeSha1Hex(preflight.Target.PreparedPath)",
]:
    require(required in step24_source, f"Step 24 production boundary contains required fail-closed invariant: {required}")

for forbidden in [
    "HarmonyLib.Harmony",
    ".Patch(",
    ".PatchAll(",
    ".Unpatch(",
    ".EntryPoint",
    "assembly.GetType(",
    ".GetMethod(",
    ".Invoke(",
    "Activator.",
    "LoadUnmanagedDllFromPath",
]:
    require(forbidden not in step24_source, f"Step 24 production boundary does not intentionally advance to Harmony/game/native invocation: {forbidden}")

for forbidden_write in ["File.Copy(", "File.Move(", "File.Write", "File.Create(", "Directory.CreateDirectory("]:
    require(forbidden_write not in step24_source, f"Step 24 production boundary never mutates prepared/live install bytes: {forbidden_write}")

require("Step24-ControlledManagedInitialization.txt" in step24_ui, "Step 24 on-device run emits a Files-visible text report")
require("OrderedInitializationGatesReachFourOfFourPass" in step24_tests and "InitializationGatesStopAfterFirstFailure" in step24_tests, "Step 24 host tests enforce ordered fail-fast gates")
require("SyntheticDeferredModuleInitializerCompletesAndAuditPasses" in step24_tests, "Step 24 host tests cover a successful inert module-initializer boundary")
require("GateARejectsReachablePInvokeBeforeAnyStep24ClrLoad" in step24_tests, "Step 24 Gate A host test rejects reachable P/Invoke before Step 24 CLR load")
require("GateARejectsFunctionPointerIndirectionBeforeAnyStep24ClrLoad" in step24_tests, "Step 24 Gate A host test rejects unbounded function-pointer/delegate indirection before CLR load")
require("GateARejectsImplicitTypeInitializerPInvokeBeforeAnyStep24ClrLoad" in step24_tests, "Step 24 Gate A host test audits implicitly triggerable type constructors before CLR load")
require("GateAMetadataAuditDoesNotResolveExternalBaseForNominallyLocalMemberRef" in step24_tests and "Unresolved same-assembly call (local metadata only)" in step24_tests, "Step 24 Gate A host test covers an unavailable external base without Cecil dependency resolution")
require("GateCReportsThrowingModuleInitializerAndDoesNotAdvance" in step24_tests, "Step 24 Gate C host test records a throwing initializer and stops")
require("collectibleLoadContext: true" in step24_tests and "Guid.NewGuid()" in step24_tests, "Step 24 host tests isolate synthetic runtime identities in collectible contexts")
require('[FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, "SlayTheSpire2", TargetSimpleName]' in step24_source, "Step 24 production constructor requires a fresh process for game + target identities")
require("No real game/Harmony assembly was loaded by Step 24 Gate A: YES" in step24_source, "Step 24 Gate A remains metadata-only for the new boundary")
require("indirect function/delegate target reachable" in step24_source and "implicit automatic-execution edge" in step24_source, "Step 24 Gate A measures implicit type initialization and rejects indirect execution targets")
require("resolved.IsPInvokeImpl || resolved.PInvokeInfo is not null" in step24_source, "Step 24 Gate A inspects resolved bodyless P/Invoke stubs before managed-body traversal")
require("Same-assembly method without managed IL body reachable" in step24_source and "else if (!resolved.HasBody)" in step24_source, "Step 24 Gate A fails closed on other reachable same-assembly bodyless execution edges")
require("ResolveSameAssemblyMethodFromLocalMetadata" in step24_source, "Step 24 Gate A resolves same-assembly initializer calls only from current-module metadata")
require("module.LookupToken(called.MetadataToken)" not in step24_source, "Step 24 Gate A no longer routes method references through Cecil LookupToken")
require("includeInitializerCallGraph: false" in step24_source and "target automatic-initialization closure audit:" in step24_source, "Step 24 Gate A separates shallow whole-plan initializer classification from target-only closure traversal")
require("ReadingMode = ReadingMode.Deferred" in step24_source, "Step 24 Gate A uses deferred Cecil reading rather than eager whole-assembly method-body materialization")
require("Step24MetadataOnlyResolver" in step24_source and "AssemblyResolver = resolver" in step24_source and "MetadataResolver = resolver" in step24_source, "Step 24 Gate A binds explicit rejecting Cecil assembly/metadata resolvers")
require("prepared initializer classification: {item.RelativePath}" in step24_source and '=> new(gate, false, $"Stage: {stage}\\n{ex}")' in step24_source, "Step 24 physical failures retain exact prepared stage and full exception diagnostics")
require("resolved = called.Resolve();" not in step24_source, "Step 24 Gate A does not invoke Cecil external assembly resolution while traversing same-assembly calls")
require("Unresolved same-assembly call (local metadata only)" in step24_source, "Step 24 Gate A fails closed when local metadata cannot unambiguously resolve a same-assembly call")
require("prohibited, unresolved, or non-dormant execution edge" in step24_source and "Audited automatic-initialization IL:" in step24_source, "Step 24 Gate A hazard failures preserve actionable initializer IL evidence")
require("Explicit Harmony patching/API invocation: NO" in step24_source, "Step 24 final audit explicitly preserves the no-Harmony-API boundary")
require(step24_source.count("_offlineInspection.RunAsync(") == 2, "Step 24 uses the established OfflineReady RunAsync contract at both pre/post checks")
require("_offlineInspection.InspectAsync(" not in step24_source and "offline.OfflineReady" not in step24_source, "Step 24 contains no nonexistent OfflineReady inspection API references")
require("offline.Success" in step24_source and "offline.ExactManagedTreeVerified" in step24_source, "Step 24 fail-closes on the canonical OfflineReady result contract")
require("ObservedMonoModLoggingDispatchHazards" in step24_source and step24_source.count("Same-assembly method without managed IL body reachable:") >= 5 and step24_source.count("indirect function/delegate target reachable:") >= 3, "Step 24.0.5 pins the physically observed seven-finding MonoMod dispatch fingerprint in production metadata policy")
require("EvaluateInitializerHazardPolicy" in step24_source and "orderedHazards.SequenceEqual(expected, StringComparer.Ordinal)" in step24_source, "Step 24.0.5 conditional classification requires exact hazard fingerprint equality")
require("System.Diagnostics.Debugger.IsAttached" in step24_source and 'StartsWith("MONOMOD_", StringComparison.OrdinalIgnoreCase)' in step24_source, "Step 24.0.5 conditional classification requires an inert debugger/environment state")
for logging_key in ["MonoMod.LogRecordHoles", "MonoMod.LogReplayQueueLength", "MonoMod.LogSpam", "MonoMod.LogToFile", "MonoMod.LogToFileFilter", "MonoMod.LogInMemory"]:
    require(logging_key in step24_source, f"Step 24.0.5 watches MonoMod logging AppContext override: {logging_key}")
require("instructions=48" in step24_source and "System.Environment::GetEnvironmentVariables()" in step24_source and "MonoMod.Switches::BestEffortParseEnvVar" in step24_source, "Step 24.0.5 pins the measured MonoMod.Switches initializer structure")
require("instructions=15" in step24_source and "MonoMod.Logs.DebugLog::Instance" in step24_source and "MonoMod.Logs.DebugLog::simpleRegDict" in step24_source, "Step 24.0.5 pins the measured DebugLog initializer structure")
require("instructions=3" in step24_source and "MonoMod.Logs.DebugLog/LevelSubscriptions::None" in step24_source, "Step 24.0.5 pins the measured LevelSubscriptions initializer structure")
require("Raw conservative audit findings:" in step24_source and "Conditionally dormant MonoMod logging findings:" in step24_source and "Initializer hazards:" in step24_source, "Step 24 Gate A reports raw, conditional, and blocking hazard counts separately")
require("GateAConditionallyAcceptsExactPhysicalMonoModLoggerFingerprintOnlyWhenInert" in step24_tests, "Step 24.0.5 host test accepts only the exact inert physical logger fingerprint")
require("GateAConditionalMonoModPolicyRejectsAnyFingerprintDrift" in step24_tests, "Step 24.0.5 host test rejects any physical hazard fingerprint drift")
require("GateAConditionalMonoModPolicyRejectsNonInertLoggingState" in step24_tests, "Step 24.0.5 host test rejects debugger/environment/AppContext logger activation")
require("GateAConditionalMonoModPolicyRequiresExactMeasuredAutomaticInitializerShape" in step24_tests, "Step 24.0.5 host test requires the physically measured automatic-initializer shape")
require("GateARejectsFunctionPointerIndirectionBeforeAnyStep24ClrLoad" in step24_tests and "Code.Ldftn or Code.Ldvirtftn" in step24_source, "Step 24.0.5 does not globally relax generic function/delegate indirection")
require("GateARejectsReachablePInvokeBeforeAnyStep24ClrLoad" in step24_tests and "resolved.IsPInvokeImpl || resolved.PInvokeInfo is not null" in step24_source, "Step 24.0.5 retains the P/Invoke fail-closed regression")

step24_manifest = ROOT / "tools/validation/protected-step24.0.6-initialization-boundary.sha256"
require(step24_manifest.is_file(), "physically closed Step 24.0.6 boundary hash manifest exists")
if step24_manifest.is_file():
    step24_mismatches: list[str] = []
    for line in step24_manifest.read_text().splitlines():
        if not line.strip():
            continue
        digest, relative = line.split("  ", 1)
        path = ROOT / relative
        if not path.is_file() or sha256(path) != digest:
            step24_mismatches.append(relative)
    require(not step24_mismatches, "physically closed Step 24.0.6 controlled-initialization implementation remains hash-pinned", ", ".join(step24_mismatches))

# ---------------------------------------------------------------------------
# Step 25-27 retired runtime-Harmony active-surface contract
# ---------------------------------------------------------------------------
retired_harmony_paths = [
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyConstruction.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyConstructionGate.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyConstructionGateResult.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyConstructionProgress.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyConstructionSummary.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyConstructionGateSequence.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyProcessorCreation.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyProcessorCreationGate.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyProcessorCreationGateResult.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyProcessorCreationProgress.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyProcessorCreationSummary.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyProcessorCreationGateSequence.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecution.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecutionGate.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecutionGateResult.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecutionProgress.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecutionSummary.cs",
    "src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecutionGateSequence.cs",
    "src/StS2Launcher.Core/Runtime/HarmonyPatchProbe.cs",
    "src/StS2Launcher.Core/Runtime/HarmonyProcessorProbe.cs",
    "tests/StS2Launcher.Core.Tests/Runtime/ControlledHarmonyConstructionTests.cs",
    "tests/StS2Launcher.Core.Tests/Runtime/ControlledHarmonyProcessorCreationTests.cs",
    "tests/StS2Launcher.Core.Tests/Runtime/ControlledHarmonyPatchExecutionTests.cs",
    "src/StS2Launcher.iOS/UI/RootViewController.HarmonyConstruction.cs",
    "src/StS2Launcher.iOS/UI/RootViewController.HarmonyProcessorCreation.cs",
    "src/StS2Launcher.iOS/UI/RootViewController.HarmonyPatchExecution.cs",
    "fixtures/StS2Launcher.Step27.InterpretedPatchFixture",
]
retired_harmony_present = [relative for relative in retired_harmony_paths if (ROOT / relative).exists()]
require(not retired_harmony_present, "closed Step 25-27 runtime-Harmony implementation/tests/UI/fixture are absent from the active build surface", ", ".join(retired_harmony_present))
require("ControlledHarmonyConstruction" not in root_ui_text and "ControlledHarmonyProcessorCreation" not in root_ui_text and "ControlledHarmonyPatchExecution" not in root_ui_text and "AddControlledHarmony" not in root_ui_text, "RootViewController no longer wires retired runtime-Harmony experiments")

test_project_text = read("tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj")
build_ios_text = read("scripts/build-ios.sh")
verify_ipa_text = read("scripts/verify-ipa.sh")
test_script_text = read("scripts/test.sh")
for marker in ["Harmony-Fat", "STS2_STEP27", "STEP27_INTERPRETED", "Step27InterpretedPatchFixture", "host-step27"]:
    require(marker not in test_script_text + build_ios_text + verify_ipa_text + test_project_text, f"active CI/IPA/project graph has no retired Step-27 dependency: {marker}")
require("curl" not in test_script_text and "unzip" not in test_script_text, "host tests no longer perform the retired Harmony release network acquisition")
require("Step 36.0 IPA verification passed." in verify_ipa_text and "Step 27 IPA verification passed." not in verify_ipa_text, "IPA verification summary identifies the active Step-36 candidate rather than retired Step 27")
require("StS2Launcher.Step27.InterpretedPatchFixture" not in project_text and "StS2Launcher.Step27.InterpretedPatchFixture" not in test_project_text, "retired Step-27 fixture is absent from iOS and host-test project graphs")
require("Step 27 is physically closed as a **negative architecture result** by 0.0.108" in read("docs/REGRESSION-CONTRACTS.md"), "active regression contracts preserve the decisive Step-27 negative architecture result")
require("closed runtime Harmony/MonoMod replacement as a negative architecture result" in read("docs/MASTER-PLAN.md"), "master plan continues to retire runtime Harmony/MonoMod replacement")
require("Step25-ControlledHarmonyConstruction.txt" not in ui_text and "Step26-ControlledHarmonyProcessorCreation.txt" not in ui_text and "Step27-ControlledHarmonyPatchExecution.txt" not in ui_text, "retired Step 25-27 report controls are absent from active UI")

# Step 28.0 — deterministic ahead-of-load Cecil transformation before CLR admission.

step28_source_path = ROOT / "src/StS2Launcher.Core/Compatibility/AheadOfLoadManagedTransformation.cs"
step28_gate_path = ROOT / "src/StS2Launcher.Core/Compatibility/AheadOfLoadManagedTransformationGate.cs"
step28_summary_path = ROOT / "src/StS2Launcher.Core/Compatibility/AheadOfLoadManagedTransformationSummary.cs"
step28_fixture_project = ROOT / "fixtures/StS2Launcher.Step28.AheadOfLoadFixture/StS2Launcher.Step28.AheadOfLoadFixture.csproj"
step28_fixture_source_path = ROOT / "fixtures/StS2Launcher.Step28.AheadOfLoadFixture/AheadOfLoadRewriteProbe.cs"
step28_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Compatibility/AheadOfLoadManagedTransformationTests.cs"
step28_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.AheadOfLoadTransformation.cs"
for path, label in [
    (step28_source_path, "Step 28 core transformation implementation"),
    (step28_gate_path, "Step 28 gate enum"),
    (step28_summary_path, "Step 28 summary"),
    (step28_fixture_project, "Step 28 post-publish fixture project"),
    (step28_fixture_source_path, "Step 28 post-publish fixture source"),
    (step28_tests_path, "Step 28 host regression tests"),
    (step28_ui_path, "Step 28 iOS UI/report surface"),
]:
    require(path.is_file(), f"{label} exists")

step28_source = step28_source_path.read_text() if step28_source_path.is_file() else ""
step28_gate = step28_gate_path.read_text() if step28_gate_path.is_file() else ""
step28_summary = step28_summary_path.read_text() if step28_summary_path.is_file() else ""
step28_fixture_source = step28_fixture_source_path.read_text() if step28_fixture_source_path.is_file() else ""
step28_tests = step28_tests_path.read_text() if step28_tests_path.is_file() else ""
step28_ui = step28_ui_path.read_text() if step28_ui_path.is_file() else ""
require(all(marker in step28_gate for marker in ["FixtureAdmissionAndOfflineReady = 1", "DeterministicRewrite = 2", "TransformedImageVerification = 3", "TransformedExecution = 4", "FinalIsolationAudit = 5"]), "Step 28.0 exposes exactly the five intended architecture-proof gates")
require("5/5" in step28_summary and "Gates.Count}/5" in step28_summary, "Step 28 summary closes only on five-of-five gate completion")
require(all(marker in step28_fixture_source for marker in ["public static int Adjustment() => 1;", "public static int Target(int value) => value + Adjustment();", "public static int InvokeTarget(int value) => Target(value);"]), "Step 28 fixture pins baseline adjustment plus direct Target/InvokeTarget call chain")
require("StS2Launcher.Step28.AheadOfLoadFixture" not in project_text, "Step 28 fixture remains absent from the iOS MSBuild/AOT graph")
require('ProjectReference Include="../../fixtures/StS2Launcher.Step28.AheadOfLoadFixture' not in test_project_text, "Step 28 fixture is not a host-test ProjectReference")
require("instructions[0].OpCode = Mono.Cecil.Cil.OpCodes.Ldc_I4" in step28_source and "instructions[0].Operand = TransformedAdjustment" in step28_source and step28_source.count("instructions[0].Operand = TransformedAdjustment") == 1, "Step 28 performs one exact Cecil semantic rewrite on the private image")
require("context.LoadFromStream(stream)" in step28_source and step28_source.count("context.LoadFromStream(stream)") == 1, "Step 28 admits exactly the verified transformed image into its private CLR context")
require("BaselineAdjustment = 1" in step28_source and "TransformedAdjustment = 1000" in step28_source and "TransformedExpectedResult = 1041" in step28_source, "Step 28 pins deterministic baseline/transformed semantics")
require("HarmonyLib." not in step28_source and "PatchProcessor" not in step28_source and "MonoMod." not in step28_source, "Step 28 production boundary does not invoke or bind the retired Harmony/MonoMod runtime patch engine")
require("Real StS2" in step28_source and "reflected or invoked: NO" in step28_source, "Step 28 explicitly keeps real StS2 behavior outside the first architecture-proof boundary")
require("Step28AheadOfLoadFixture" in build_ios_text and "STEP28_AHEAD_OF_LOAD_FIXTURE_PROJECT" in build_ios_text and 'dotnet build "$STEP28_AHEAD_OF_LOAD_FIXTURE_PROJECT"' in build_ios_text, "iOS build separately compiles the Step-28 fixture outside the app graph")
require('cp "$STEP28_AHEAD_OF_LOAD_FIXTURE_DLL" "$APP/Step28AheadOfLoadFixture/StS2Launcher.Step28.AheadOfLoadFixture.dll"' in build_ios_text and build_ios_text.index('dotnet publish "$PROJECT"') < build_ios_text.index('cp "$STEP28_AHEAD_OF_LOAD_FIXTURE_DLL" "$APP/Step28AheadOfLoadFixture/StS2Launcher.Step28.AheadOfLoadFixture.dll"'), "Step 28 fixture enters the .app only after iOS publish completes")
require("STEP28_BUNDLED_COUNT" in verify_ipa_text and '[[ "$STEP28_BUNDLED_COUNT" == "1" ]]' in verify_ipa_text and "step28-ahead-of-load-fixture.sha256" in verify_ipa_text and 'cmp -s "$STEP28_FIXTURE_SOURCE" "$STEP28_FIXTURE"' in verify_ipa_text, "IPA verifier requires one byte-identical hash-pinned Step-28 data-only fixture")
require("STS2_STEP28_AHEAD_OF_LOAD_FIXTURE_ROOT" in test_script_text and "STEP28_AHEAD_OF_LOAD_PROJECT" in test_script_text, "host runner separately builds and exports the Step-28 fixture root")
require("VerifiedSourceIsRewrittenBeforeLoadAndOnlyTransformedBehaviorExecutes" in step28_tests and "STS2_STEP28_AHEAD_OF_LOAD_FIXTURE_ROOT" in step28_tests, "host regressions exercise transformed semantics through both reflection and in-fixture direct-call paths")
require("Step28-AheadOfLoadManagedTransformation.txt" in step28_ui and "AheadOfLoadManagedTransformation" in step28_ui, "iOS UI persists the dedicated Step-28 physical report")
require("private sealed class CallbackProgress<T> : IProgress<T>" in step28_source, "Step 28.0.1 declares the callback-backed IProgress adapter required by Gate-A OfflineReady progress")
require("private readonly Action<T> _callback;" in step28_source and "_callback = callback ?? throw new ArgumentNullException(nameof(callback));" in step28_source, "Step 28.0.1 callback adapter stores a non-null forwarding callback")
require("public void Report(T value) => _callback(value);" in step28_source, "Step 28.0.1 callback adapter forwards IProgress.Report synchronously")
require("ReadingMode = ReadingMode.Deferred" in step28_source, "Step 28.0.2 uses deferred Cecil reading for metadata-only fixture admission")
require("AssemblyResolver = RejectingAssemblyResolver.Instance" in step28_source, "Step 28.0.2 retains the fail-closed rejecting Cecil resolver")
require("ReadingMode = ReadingMode.Immediate" not in step28_source, "Step 28.0.2 does not reintroduce eager Cecil fixture reads")
require(step28_source.count("ReadingMode = ReadingMode.Deferred") == 1, "Step 28.0.2 has one canonical deferred fixture reader")

step28_manifest = ROOT / "tools/validation/protected-step28.0.2-ahead-of-load-boundary.sha256"
require(step28_manifest.is_file(), "physically closed Step 28 implementation/evidence hash manifest exists")
if step28_manifest.is_file():
    step28_mismatches: list[str] = []
    for line in step28_manifest.read_text().splitlines():
        if not line.strip():
            continue
        digest, relative = line.split("  ", 1)
        path = ROOT / relative
        if not path.is_file() or sha256(path) != digest:
            step28_mismatches.append(relative)
    require(not step28_mismatches, "physically closed Step-28 implementation, fixture, tests, and closure evidence remain hash-pinned", ", ".join(step28_mismatches))

# ---------------------------------------------------------------------------
# Step 29.0 — exact receipt-backed real-StS2 compatibility target audit.
# ---------------------------------------------------------------------------
step29_source_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2CompatibilityTargetAudit.cs"
step29_gate_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2CompatibilityTargetAuditGate.cs"
step29_summary_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2CompatibilityTargetAuditSummary.cs"
step29_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Compatibility/RealStS2CompatibilityTargetAuditTests.cs"
step29_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.RealStS2CompatibilityTargetAudit.cs"
for path, label in [
    (step29_source_path, "Step 29 real-StS2 target-audit core"),
    (step29_gate_path, "Step 29 gate enum"),
    (step29_summary_path, "Step 29 summary"),
    (step29_tests_path, "Step 29 host regressions"),
    (step29_ui_path, "Step 29 iOS report surface"),
]:
    require(path.is_file(), f"{label} exists")
step29_source = step29_source_path.read_text() if step29_source_path.is_file() else ""
step29_gate = step29_gate_path.read_text() if step29_gate_path.is_file() else ""
step29_summary = step29_summary_path.read_text() if step29_summary_path.is_file() else ""
step29_tests = step29_tests_path.read_text() if step29_tests_path.is_file() else ""
step29_ui = step29_ui_path.read_text() if step29_ui_path.is_file() else ""
require(all(marker in step29_gate for marker in ["SourceAdmissionAndOfflineReady = 1", "ExactRiskCallSiteAudit = 2", "DeterministicCandidateSelection = 3", "FinalIsolationAudit = 4"]), "Step 29 exposes exactly the four intended read-only audit gates")
require("4/4" in step29_summary and "Gates.Count}/4" in step29_summary, "Step 29 summary closes only on four-of-four gate completion")
require("data_sts2_macos_arm64/sts2.dll" in step29_source and "Expected exactly one receipt-backed macOS arm64 sts2.dll" in step29_source, "Step 29 binds exactly the receipt-backed ARM64 primary sts2.dll")
require("ReadingMode = ReadingMode.Deferred" in step29_source and "AssemblyResolver = resolver" in step29_source and "MetadataResolver = new MetadataResolver(resolver)" in step29_source, "Step 29 uses deferred Cecil metadata with one explicit rejecting resolver")
require("throw new AssemblyResolutionException(name)" in step29_source and "Cecil dependency resolution requests: 0" in step29_source, "Step 29 Cecil dependency resolution remains fail-closed and auditable")
require("ModuleDefinition.Write" not in step29_source and ".Write(" not in step29_source, "Step 29 production audit contains no Cecil write path")
require("LoadFromStream" not in step29_source and "Assembly.Load(" not in step29_source and "Assembly.LoadFrom" not in step29_source, "Step 29 production audit contains no real-StS2 CLR admission path")
require("MetadataToken.ToUInt32()" in step29_source and "MethodBodySha256" in step29_source and "SHA256.HashData" in step29_source and "instruction.Offset" in step29_source, "Step 29 fingerprints exact method token, IL offset, target, and method-body SHA-256")
require(all(marker in step29_source for marker in ["HarmonyRuntimePatch", "MonoModRuntimeDetour", "ReflectionEmit", "PrepareMethod", "DynamicAssemblyLoad", "Process", "Registry", "WindowsPrincipal", "DllImportResolver", "NativeLibrary", "NativeFunctionPointer", "IndirectCalli"]), "Step 29 candidate categories are explicitly bounded")
require("Expression.Compile" in step29_source and "Step19Closed" in step29_source, "Step 29 counts but excludes the physically closed Step-19 Expression.Compile surface")
require("NO DIRECT PRIMARY TARGET" in step29_source and "Authorization: AUDIT ONLY" in step29_source, "Step 29 permits no-target evidence and never treats selection as write authorization")
require("PriorityForCategory" in step29_source and all(f'"{name}" => {priority}' in step29_source for name, priority in [("HarmonyRuntimePatch",10),("MonoModRuntimeDetour",20),("ReflectionEmit",30),("PrepareMethod",40),("DynamicAssemblyLoad",50),("System.Diagnostics.Process",60),("Microsoft.Win32.Registry",70),("WindowsPrincipal",80),("DllImportResolver",90),("NativeLibrary",100),("NativeFunctionPointer",110),("IndirectCalli",120)]), "Step 29 selection priority is deterministic and predeclared")
require("ReceiptBackedPrimaryAuditSelectsExactHarmonyRuntimePatchWithoutMutationOrClrLoad" in step29_tests and "CollectionAssert.AreEqual(before, after)" in step29_tests and "Authorization: AUDIT ONLY" in step29_tests, "Step 29 host regression proves deterministic selection without source mutation or CLR load")
require("SelectionPriorityKeepsRetiredRuntimeDetoursAheadOfLaterIntegrationSurfaces" in step29_tests and "Expression.Compile sites excluded" in step29_tests, "Step 29 host regressions pin candidate ordering and Step-19 exclusion")
require("Step29-RealStS2CompatibilityTargetAudit.txt" in step29_ui and "REAL STS2 COMPATIBILITY TARGET AUDIT" in step29_ui, "iOS UI persists the dedicated Step-29 physical audit report")
require("new RealStS2CompatibilityTargetAudit(_launcherDataRoot)" in root_ui_text and "AddRealStS2CompatibilityTargetAuditControls(content)" in root_ui_text, "RootViewController wires the Step-29 audit into the active device surface")
require("Step29ImplementationMarker" in release_presentation and "zero writes/zero CLR load" in release_presentation, "release presentation retains the closed Step-29 read-only evidence marker")

# ---------------------------------------------------------------------------
# Step 30.0 — exact selected Harmony target semantic-context/product-scope audit.
# ---------------------------------------------------------------------------
step30_source_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2SelectedTargetSemanticAudit.cs"
step30_gate_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2SelectedTargetSemanticAuditGate.cs"
step30_summary_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2SelectedTargetSemanticAuditSummary.cs"
step30_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Compatibility/RealStS2SelectedTargetSemanticAuditTests.cs"
step30_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.RealStS2SelectedTargetSemanticAudit.cs"
for path, label in [(step30_source_path,"Step 30 semantic-audit core"),(step30_gate_path,"Step 30 gate enum"),(step30_summary_path,"Step 30 summary"),(step30_tests_path,"Step 30 host regressions"),(step30_ui_path,"Step 30 iOS report surface")]:
    require(path.is_file(), f"{label} exists")
step30_source = step30_source_path.read_text() if step30_source_path.is_file() else ""
step30_gate = step30_gate_path.read_text() if step30_gate_path.is_file() else ""
step30_summary = step30_summary_path.read_text() if step30_summary_path.is_file() else ""
step30_tests = step30_tests_path.read_text() if step30_tests_path.is_file() else ""
step30_ui = step30_ui_path.read_text() if step30_ui_path.is_file() else ""
require(all(marker in step30_gate for marker in ["SelectedEvidenceBindingAndOfflineReady = 1", "ExactSemanticContextAudit = 2", "DeterministicDisposition = 3", "FinalIsolationAudit = 4"]), "Step 30 exposes exactly four read-only semantic-audit gates")
require("4/4" in step30_summary and "Gates.Count(g => g.Passed)}/4" in step30_summary, "Step 30 summary closes only on four-of-four gate completion")
for marker in ["e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18", "518e4758-52d7-47c2-b776-471a0e29e49d", "0x06007927", "IlOffset: 0x0D9D", "HarmonyLib.Harmony::PatchAll(System.Reflection.Assembly)", "50c8c4394082f3c73df414fad8675540cfc00a99ccc4f350b616cec574cdbcbd"]:
    require(marker in step30_source, f"Step 30 hard-pins physical Step-29 evidence: {marker}")
require("ReadingMode = ReadingMode.Deferred" in step30_source and "throw new AssemblyResolutionException(name)" in step30_source and "Cecil dependency resolution requests: 0" in step30_source, "Step 30 retains deferred/rejecting-resolver metadata policy")
require("ModuleDefinition.Write" not in step30_source and ".Write(" not in step30_source, "Step 30 production source contains no Cecil write path")
require("Assembly.Load(" not in step30_source and "LoadFromStream(" not in step30_source and "LoadFromAssemblyPath(" not in step30_source and "LoadFromAssemblyName(" not in step30_source, "Step 30 production source contains no real-StS2 CLR admission path")
require("Exact IL context (14 instructions before/after selected site" in step30_source and "Branches targeting selected instruction" in step30_source and "Exception regions covering selected instruction" in step30_source, "Step 30 Gate B records bounded IL/control-flow/exception context")
require("DEFER — MOD/HARMONY COMPATIBILITY PATH; NO BASE-GAME REWRITE AUTHORIZED" in step30_source and "Predeclared behavior change for this selected site: NONE" in step30_source and "Runtime reachability claim: NONE" in step30_source, "Step 30 Gate C defers the mod-scoped Harmony site without rewrite authorization or reachability overclaim")
require("ExactModManagerPatchAllEvidenceIsAuditedThenDeferredWithoutMutationOrClrLoad" in step30_tests and "CollectionAssert.AreEqual(before, after)" in step30_tests and "Real-game rewrite authorized by Step 30: NO" in step30_tests, "Step 30 host regression proves exact binding/context/disposition without source mutation")
require("Step30-SelectedTargetSemanticContextAudit.txt" in step30_ui and "SELECTED TARGET SEMANTIC CONTEXT AUDIT" in step30_ui, "iOS UI persists the dedicated Step-30 physical report")
require("new RealStS2SelectedTargetSemanticAudit(_launcherDataRoot)" in root_ui_text and "AddRealStS2SelectedTargetSemanticAuditControls(content)" in root_ui_text, "RootViewController wires Step 30 into the active device surface")
require("Step30ImplementationMarker" in release_presentation and "mod-path disposition" in release_presentation and "zero writes/zero CLR load" in release_presentation, "release presentation pins the Step-30 read-only semantic-audit boundary")


# ---------------------------------------------------------------------------
# Step 31.0 — exact PrewarmJit/PrepareMethod semantic-context audit.
# ---------------------------------------------------------------------------
step31_source_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2PrepareMethodSemanticAudit.cs"
step31_gate_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2PrepareMethodSemanticAuditGate.cs"
step31_summary_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2PrepareMethodSemanticAuditSummary.cs"
step31_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Compatibility/RealStS2PrepareMethodSemanticAuditTests.cs"
step31_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.RealStS2PrepareMethodSemanticAudit.cs"
for path, label in [(step31_source_path,"Step 31 PrepareMethod semantic-audit core"),(step31_gate_path,"Step 31 gate enum"),(step31_summary_path,"Step 31 summary"),(step31_tests_path,"Step 31 host regressions"),(step31_ui_path,"Step 31 iOS report surface")]:
    require(path.is_file(), f"{label} exists")
step31_source = step31_source_path.read_text() if step31_source_path.is_file() else ""
step31_gate = step31_gate_path.read_text() if step31_gate_path.is_file() else ""
step31_summary = step31_summary_path.read_text() if step31_summary_path.is_file() else ""
step31_tests = step31_tests_path.read_text() if step31_tests_path.is_file() else ""
step31_ui = step31_ui_path.read_text() if step31_ui_path.is_file() else ""
require(all(marker in step31_gate for marker in ["EvidenceBindingAndOfflineReady = 1", "ExactPrepareMethodSemanticContextAudit = 2", "DeterministicDisposition = 3", "FinalIsolationAudit = 4"]), "Step 31 exposes exactly four read-only PrepareMethod semantic-audit gates")
require("4/4" in step31_summary and "Gates.Count(g => g.Passed)}/4" in step31_summary, "Step 31 summary closes only on four-of-four gate completion")
for marker in ["e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18", "518e4758-52d7-47c2-b776-471a0e29e49d", "0x06007D05", "7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9", "0x003D", "0x0178", "RuntimeHelpers::PrepareMethod"]:
    require(marker in step31_source, f"Step 31 hard-pins physical PrepareMethod evidence: {marker}")
require(step31_source.count("new(0x") >= 10 and all(offset in step31_source for offset in ["0x003D","0x0052","0x007A","0x00A2","0x00CA","0x00F2","0x0136","0x014C","0x0162","0x0178"]), "Step 31 pins all ten physical PrepareMethod offsets")
require("ReadingMode = ReadingMode.Deferred" in step31_source and "throw new AssemblyResolutionException(name)" in step31_source and "Cecil dependency resolution requests: 0" in step31_source, "Step 31 retains deferred/rejecting-resolver metadata policy")
require("ModuleDefinition.Write" not in step31_source and ".Write(" not in step31_source, "Step 31 production source contains no Cecil write path")
require("Assembly.Load(" not in step31_source and "LoadFromStream(" not in step31_source and "LoadFromAssemblyPath(" not in step31_source and "LoadFromAssemblyName(" not in step31_source, "Step 31 production source contains no real-StS2 CLR admission path")
require("Context (10 before / 4 after, bounded)" in step31_source and "Branch sources:" in step31_source and "Covering exception regions:" in step31_source, "Step 31 Gate B records bounded per-site IL/control-flow/exception context")
require("BASE-GAME COMPATIBILITY FAMILY CONFIRMED — ELIGIBLE FOR EXPLICIT REWRITE DESIGN; NO WRITE AUTHORIZED" in step31_source and "Predeclared behavior change for Step 31: NONE" in step31_source and "Runtime reachability claim: NONE" in step31_source, "Step 31 Gate C records rewrite-design eligibility without write authorization or reachability overclaim")
require("ExactPrewarmJitPrepareMethodFamilyIsAuditedWithoutMutationOrClrLoad" in step31_tests and "CollectionAssert.AreEqual(before, after)" in step31_tests and "Real-game rewrite authorized by Step 31: NO" in step31_tests, "Step 31 host regression proves exact ten-site audit without source mutation")
require("Step31-PrepareMethodSemanticContextAudit.txt" in step31_ui and "PREPAREMETHOD SEMANTIC CONTEXT AUDIT" in step31_ui, "iOS UI persists the dedicated Step-31 physical report")
require("new RealStS2PrepareMethodSemanticAudit(_launcherDataRoot)" in root_ui_text and "AddRealStS2PrepareMethodSemanticAuditControls(content)" in root_ui_text, "RootViewController wires Step 31 into the active device surface")
require("Step31ImplementationMarker" in release_presentation and "10 PrepareMethod offsets" in release_presentation and "zero writes/zero CLR load" in release_presentation, "release presentation pins the Step-31 read-only PrepareMethod boundary")

# ---------------------------------------------------------------------------
# Step 32.0 — first real StS2 private PrepareMethod rewrite.
# ---------------------------------------------------------------------------
step32_source_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2PrepareMethodRewrite.cs"
step32_gate_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2PrepareMethodRewriteGate.cs"
step32_summary_path = ROOT / "src/StS2Launcher.Core/Compatibility/RealStS2PrepareMethodRewriteSummary.cs"
step32_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Compatibility/RealStS2PrepareMethodRewriteTests.cs"
step32_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.RealStS2PrepareMethodRewrite.cs"
for path, label in [(step32_source_path,"Step 32 real rewrite core"),(step32_gate_path,"Step 32 gate enum"),(step32_summary_path,"Step 32 summary"),(step32_tests_path,"Step 32 host regressions"),(step32_ui_path,"Step 32 iOS report surface")]:
    require(path.is_file(), f"{label} exists")
step32_source = step32_source_path.read_text() if step32_source_path.is_file() else ""
step32_gate = step32_gate_path.read_text() if step32_gate_path.is_file() else ""
step32_summary = step32_summary_path.read_text() if step32_summary_path.is_file() else ""
step32_tests = step32_tests_path.read_text() if step32_tests_path.is_file() else ""
step32_ui = step32_ui_path.read_text() if step32_ui_path.is_file() else ""
require(all(marker in step32_gate for marker in ["SourceAdmissionAndPrivateClone = 1", "DeterministicStackNeutralRewrite = 2", "TransformedImageVerification = 3", "FinalIsolationAudit = 4"]), "Step 32 exposes exactly four ordered real-rewrite gates")
require("4/4" in step32_summary and "Gates.Count(g => g.Passed)}/4" in step32_summary, "Step 32 summary closes only on four-of-four gate completion")
for marker in ["e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18", "518e4758-52d7-47c2-b776-471a0e29e49d", "0x06007D05", "7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9", "0x003D", "0x0178", "SourceInstructionCount: 117", "SourceExceptionHandlerCount: 2"]:
    require(marker in step32_source, f"Step 32 hard-pins physical Step-31 evidence: {marker}")
require(step32_source.count("new(0x") >= 10 and all(offset in step32_source for offset in ["0x003D","0x0052","0x007A","0x00A2","0x00CA","0x00F2","0x0136","0x014C","0x0162","0x0178"]), "Step 32 pins all ten physical PrepareMethod offsets")
require("oneArgumentReplacements != 6 || twoArgumentReplacements != 4" in step32_source and "PrepareMethod(handle) -> Pop" in step32_source and "PrepareMethod(handle, instantiation[]) -> Pop + Pop" in step32_source, "Step 32 predeclares exact 6 one-pop + 4 two-pop stack-neutral rewrite")
require("il.InsertBefore(instruction, il.Create(OpCodes.Pop))" in step32_source and "instruction.OpCode = OpCodes.Pop" in step32_source and "instruction.Operand = null" in step32_source, "Step 32 implementation performs only the predeclared Pop replacements")
require("FindIncomingBranchSources" in step32_source and "refuses to rewrite branch-targeted PrepareMethod site" in step32_source, "Step 32 rejects selected sites that become branch targets")
require("ReadingMode = ReadingMode.Deferred" in step32_source and "class RejectingAssemblyResolver" in step32_source and "throw new AssemblyResolutionException(name)" in step32_source, "Step 32 retains deferred/rejecting-resolver policy for read and verification phases")
require("module.Write(transformedPath, new WriterParameters { WriteSymbols = false })" in step32_source and "Step32-RealStS2PrepareMethodRewrite" in step32_source, "Step 32 writes only a named launcher-private transformed image")
require("Assembly.Load(" not in step32_source and "LoadFromStream(" not in step32_source and "LoadFromAssemblyPath(" not in step32_source and "LoadFromAssemblyName(" not in step32_source, "Step 32 production source contains no real-StS2 CLR admission path")
require("ComputeMethodSemanticFingerprint" in step32_source and "ExpectedTransformedSemanticSha256" in step32_source and "PrepareMethod references source/transformed" in step32_source, "Step 32 reopens and verifies the exact planned transformed method semantics")
require("Instruction offsets are finalized by Cecil during serialization" in step32_source and "offset-independent semantic fingerprint" in step32_source and "transformedBodySha256.Equals(sourceBodySha256" in step32_source, "Step 32.0.1 treats physical body fingerprint as post-write evidence and semantic fingerprint as the pre-write/reopen invariant")
require("ExpectedTransformedBodySha256" not in step32_source and "expectedTransformedBodySha256" not in step32_source, "Step 32.0.1 forbids pre-write offset-sensitive transformed body-hash prediction")
require("ConstantMetadataWriteResolver" in step32_source and "CecilWriteSystemRuntimeIdentity" in step32_source and "CecilWriteSentryIdentity" in step32_source and "System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" in step32_source and "Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0" in step32_source, "Step 32.0.5 retains the exact audited System.Runtime and Sentry write-only metadata resolver identities")
require("AuditedExternalConstantTypeRequirements" in step32_source and "System.Reflection.BindingFlags" in step32_source and "Sentry.BreadcrumbLevel" in step32_source and "Sentry.SentryLevel" in step32_source and "TypeCode.Int16" in step32_source and "ValidateAuditedRequirementSet" in step32_source, "Step 32.0.5 retains the exact three audited external constant type/storage requirements and rejects requirement drift before mutation")
require("CollectExternalConstantTypeRequirements" in step32_source and "GetPrimitiveConstantType" in step32_source and "Dictionary<string, AssemblyDefinition> _surrogates" in step32_source and "External framework/game assembly bytes opened by the write resolver: 0" in step32_source, "Step 32.0.5 retains per-exact-assembly in-memory surrogates synthesized from verified source constant values without opening external assembly bytes")
require("DefaultAssemblyResolver" not in step32_source and "AddSearchDirectory" not in step32_source, "Step 32.0.2 forbids broad Cecil resolver/search fallback")
require("ComputeConstantMetadataFingerprint" in step32_source and "source/transformed constant metadata semantics changed" in step32_source and "ExpectedConstantMetadataSha256" in step32_source, "Step 32.0.2 verifies unrelated Constant-table semantics survive serialization unchanged")
require("CreateSyntheticSentry" in step32_tests and "MultiAssemblyResolver" in step32_tests and "Synthetic constant-metadata resolver types: 3" in step32_tests and "\"Sentry\", \"BreadcrumbLevel\"" in step32_tests and "\"Sentry\", \"SentryLevel\"" in step32_tests, "Step 32.0.5 host regression retains all three audited external-enum Constant-table serialization requirements")
require("ExactPrewarmJitPrepareMethodFamilyIsRewrittenOnPrivateCopyOnly" in step32_tests and "CollectionAssert.AreEqual(before, after)" in step32_tests and "CountPrepareMethod(transformedMethod)" in step32_tests and "BranchTargetedPrepareMethodSiteIsRejectedBeforeAnyRewrite" in step32_tests and "UnauditedExternalConstantRequirementFailsClosedBeforeRewrite" in step32_tests and "Unexpected.Dependency" in step32_tests, "Step 32 host regressions prove private-only rewrite, branch-target refusal, and unaudited constant-requirement fail-closed behavior")
require("FindMethodByStableIdentity" in step32_source and "TryFindMethodByToken(transformedModule, _expected.MethodToken)" in step32_source and "var transformedMethod = FindMethodByToken(transformedModule, _expected.MethodToken)" not in step32_source and "Original source token" in step32_source and "informational only" in step32_source, "Step 32.0.5 Gate C locates the transformed method by stable exact identity while treating source-token preservation as diagnostics only")
require("StableTransformedMethodLookupDoesNotDependOnHistoricalSourceToken" in step32_tests and "FindMethodByStableIdentity" in step32_tests and "Assert.AreNotEqual(decoyAfterWrite.MetadataToken.ToUInt32(), targetAfterWrite.MetadataToken.ToUInt32())" in step32_tests, "Step 32.0.5 host regression protects transformed method lookup from historical source-token assumptions")
require("Step32-RealStS2PrepareMethodRewrite.txt" in step32_ui and "REAL STS2 PREPAREMETHOD REWRITE" in step32_ui, "iOS UI persists the dedicated Step-32 physical report")
require("new RealStS2PrepareMethodRewrite(_launcherDataRoot)" in root_ui_text and "AddRealStS2PrepareMethodRewriteControls(content)" in root_ui_text, "RootViewController wires Step 32 into the active device surface")
require("Step32ImplementationMarker" in release_presentation and "6 one-arg PrepareMethod calls to Pop" in release_presentation and "4 two-arg calls to Pop+Pop" in release_presentation and "zero CLR load" in release_presentation, "release presentation pins the Step-32 first-real-rewrite boundary")

# ---------------------------------------------------------------------------
# Step 33.0 — verified transformed real-StS2 CLR admission.
# ---------------------------------------------------------------------------
step33_source_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2AssemblyAdmission.cs"
step33_gate_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2AssemblyAdmissionGate.cs"
step33_summary_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2AssemblyAdmissionSummary.cs"
step33_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Runtime/TransformedRealStS2AssemblyAdmissionTests.cs"
step33_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.TransformedRealStS2AssemblyAdmission.cs"
for path, label in [(step33_source_path,"Step 33 transformed admission core"),(step33_gate_path,"Step 33 gate enum"),(step33_summary_path,"Step 33 summary"),(step33_tests_path,"Step 33 host regressions"),(step33_ui_path,"Step 33 iOS report surface")]:
    require(path.is_file(), f"{label} exists")
step33_source = step33_source_path.read_text() if step33_source_path.is_file() else ""
step33_gate = step33_gate_path.read_text() if step33_gate_path.is_file() else ""
step33_summary = step33_summary_path.read_text() if step33_summary_path.is_file() else ""
step33_tests = step33_tests_path.read_text() if step33_tests_path.is_file() else ""
step33_ui = step33_ui_path.read_text() if step33_ui_path.is_file() else ""
require(all(marker in step33_gate for marker in ["VerifiedTransformedImagePreflight = 1", "TransformedPrimaryClrAdmission = 2", "AdmissionOnlyResolverAudit = 3", "FinalIsolationAudit = 4"]), "Step 33 exposes exactly four ordered transformed-admission gates")
require("4/4" in step33_summary and "Gates.Count(g => g.Passed)}/4" in step33_summary, "Step 33 summary closes only on four-of-four gate completion")
for marker in ["39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef", "9_304_576", "518e4758-52d7-47c2-b776-471a0e29e49d", "47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a"]:
    require(marker in step33_source, f"Step 33 hard-pins physical Step-32 transformed evidence: {marker}")
require("RunSourceAdmissionAndPrivateCloneAsync" in step33_source and "RunDeterministicStackNeutralRewrite" in step33_source and "RunTransformedImageVerification" in step33_source and "RunFinalIsolationAuditAsync" in step33_source, "Step 33 Gate A re-runs the closed Step-32 A-D contract before admission")
require("RunPreparedLoadPreflightAsync" in step33_source and "RuntimeClosureReady" in step33_source and "Blockers.Length != 0" in step33_source, "Step 33 requalifies the zero-blocker Step-21/22 prepared runtime plan")
require("LoadFromStream(stream)" in step33_source and "ClosedStep32TransformedSha256" in step33_source and "immediate transformed hash recheck" in step33_source, "Step 33 admits only immediately rehashed transformed primary bytes through LoadFromStream")
require("StS2Launcher-Step33-TransformedGame" in step33_source and "ManifestModule.ModuleVersionId" in step33_source and "Expected exactly one sts2 assembly" in step33_source, "Step 33 verifies transformed identity/MVID/dedicated-context ownership and unique sts2 residency")
require("refuses private dependency CLR admission" in step33_source and "PrivateDependencyRequests" in step33_source and "Private context assemblies" in step33_source, "Step 33 admission-only resolver refuses private game dependency expansion")
require("AssemblyLoadContext.Default.LoadFromAssemblyName" in step33_source and "ExactRequestedIdentity" in step33_source and "HostLoads" in step33_source, "Step 33 permits only exact preplanned host-framework bindings through the default context")
require("LoadUnmanagedDll" in step33_source and "refuses native library resolution" in step33_source and "RejectedManagedRequests" in step33_source, "Step 33 fails closed on native and unplanned managed resolution")
require("Game entry point invoked: NO" in step33_source and "Game type/member reflection performed: NO" in step33_source and "Godot/game initialization requested: NO" in step33_source, "Step 33 production report explicitly preserves the admission-only no-execution boundary")
require("OrderedAdmissionGatesReachFourOfFourPass" in step33_tests and "AdmissionStopsAfterFirstFailure" in step33_tests and "AdmissionOnlyContextLoadsPrimaryButRefusesPrivateDependencyAdmission" in step33_tests, "Step 33 host regressions protect gate order and admission-only private-dependency refusal")
require("Step33-TransformedRealStS2AssemblyAdmission.txt" in step33_ui and "TRANSFORMED REAL STS2 CLR ADMISSION" in step33_ui, "iOS UI persists the dedicated Step-33 physical report")
require("new TransformedRealStS2AssemblyAdmission(_launcherDataRoot)" in root_ui_text and "AddTransformedRealStS2AssemblyAdmissionControls(content)" in root_ui_text, "RootViewController wires Step 33 into the active device surface")
require("Step33ImplementationMarker" in release_presentation and "Step34ImplementationMarker" in release_presentation and "Step35ImplementationMarker" in release_presentation and "0x06007D02" in release_presentation and "0x0600BC71" in release_presentation, "release presentation pins Step-33/34 closures and the Step-35.0.15 very-early diagnostic boundary")

# ---------------------------------------------------------------------------
# Step 34.0 — controlled transformed real-StS2 PrewarmJit execution.
# ---------------------------------------------------------------------------
step34_source_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2PrewarmJitExecution.cs"
step34_gate_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2PrewarmJitExecutionGate.cs"
step34_summary_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2PrewarmJitExecutionSummary.cs"
step34_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Runtime/TransformedRealStS2PrewarmJitExecutionTests.cs"
step34_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.TransformedRealStS2PrewarmJitExecution.cs"
for path, label in [(step34_source_path,"Step 34 controlled-execution core"),(step34_gate_path,"Step 34 gate enum"),(step34_summary_path,"Step 34 summary"),(step34_tests_path,"Step 34 host regressions"),(step34_ui_path,"Step 34 iOS report surface")]:
    require(path.is_file(), f"{label} exists")
step34_source = step34_source_path.read_text() if step34_source_path.is_file() else ""
step34_gate = step34_gate_path.read_text() if step34_gate_path.is_file() else ""
step34_summary = step34_summary_path.read_text() if step34_summary_path.is_file() else ""
step34_tests = step34_tests_path.read_text() if step34_tests_path.is_file() else ""
step34_ui = step34_ui_path.read_text() if step34_ui_path.is_file() else ""
require(all(marker in step34_gate for marker in ["VerifiedExecutionPreflight = 1", "ExecutionCapableClrAdmission = 2", "ExactPrewarmJitInvocation = 3", "FinalIsolationAudit = 4"]), "Step 34 exposes exactly four ordered controlled-execution gates")
require("4/4" in step34_summary and "Gates.Count(g => g.Passed)}/4" in step34_summary, "Step 34 summary closes only on four-of-four gate completion")
for marker in ["ClosedStep32TransformedPrewarmJitToken = 0x0600AFEA", "MegaCrit.Sts2.Core.Helpers.OneTimeInitialization", "PrewarmJit", "StS2Launcher-Step34-PrewarmJit"]:
    require(marker in step34_source, f"Step 34 hard-pins transformed target/isolated context: {marker}")
require("RunSourceAdmissionAndPrivateCloneAsync" in step34_source and "RunDeterministicStackNeutralRewrite" in step34_source and "RunTransformedImageVerification" in step34_source and "RunFinalIsolationAuditAsync" in step34_source, "Step 34 Gate A re-runs the closed Step-32 A-D transform contract")
require("RunPreparedLoadPreflightAsync" in step34_source and "RuntimeClosureReady" in step34_source and "Blockers.Length != 0" in step34_source, "Step 34 requalifies the zero-blocker prepared runtime plan")
require("TargetMethodFullName" in step34_source and "BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic" in step34_source and "method.Invoke(null, null)" in step34_source, "Step 34 Gate C binds and invokes only the exact static parameterless transformed PrewarmJit site")
require("ControlledManagedInitialization.TargetSimpleName" in step34_source and "ControlledManagedInitialization.TargetVersion" in step34_source and "InitializerBearingRequests" in step34_source and "ModuleInitializerCount" in step34_source, "Step 34 execution resolver classifies initializer-bearing 0Harmony separately and refuses it")
require("AssemblyLoadContext.Default.LoadFromAssemblyName" in step34_source and "LoadUnmanagedDll" in step34_source and "RejectedManagedRequests" in step34_source, "Step 34 resolver stays exact-plan/fail-closed and rejects native/unplanned loading")
require("ComputeSha1Hex" in step34_source and "ComputeSha256Hex" in step34_source and "OfflineReady" in step34_source, "Step 34 re-proves source/transformed integrity and OfflineReady isolation")
require("OrderedExecutionGatesReachFourOfFourPass" in step34_tests and "ExecutionStopsAfterFirstFailure" in step34_tests and "ExecutionContextLoadsInitializerFreePrivateDependencyAndRejectsInitializerBearingDependency" in step34_tests and "Step34PinsThePhysicallyClosedTransformedTarget" in step34_tests, "Step 34 host regressions protect gate order, exact target, and initializer-bearing refusal")
require("Step34-TransformedRealStS2PrewarmJitExecution.txt" in step34_ui and "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION" in step34_ui, "iOS UI persists the dedicated Step-34 physical report")
require("UIButtonType.System" not in step34_ui, "Step 34 iOS SystemButton helper receives a numeric font size rather than UIButtonType")
require("new TransformedRealStS2PrewarmJitExecution(_launcherDataRoot)" in root_ui_text and "AddTransformedRealStS2PrewarmJitExecutionControls(content)" in root_ui_text, "RootViewController wires Step 34 into the active device surface")
require("Step34ImplementationMarker" in release_presentation and "0x0600AFEA" in release_presentation and "PrewarmJit" in release_presentation and "Step35ImplementationMarker" in release_presentation, "release presentation preserves the closed Step-34 exact transformed-site execution boundary while advancing Step 35")

# ---------------------------------------------------------------------------
# Step 35.0.30 — physically closed exact-authority core + UI-return correction; Step 36.0 follows below.
# ---------------------------------------------------------------------------
step35_source_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2VeryEarlyInitialization.cs"
step35_gate_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2VeryEarlyInitializationGate.cs"
step35_summary_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2VeryEarlyInitializationSummary.cs"
step35_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Runtime/TransformedRealStS2VeryEarlyInitializationTests.cs"
step35_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.TransformedRealStS2VeryEarlyInitialization.cs"
step35_bootstrap_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.Step35ManagedPluginBootstrap.cs"
step35_telemetry_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.Step35Telemetry.cs"
step35_mode_path = ROOT / "src/StS2Launcher.Core/Runtime/Step35DiagnosticMode.cs"
step35_recon_path = ROOT / "src/StS2Launcher.Core/Runtime/Step35GodotReconnaissance.cs"
for path, label in [(step35_source_path,"Step 35 very-early core"),(step35_gate_path,"Step 35 gate enum"),(step35_summary_path,"Step 35 summary"),(step35_tests_path,"Step 35 host regressions"),(step35_ui_path,"Step 35 iOS report surface"),(step35_bootstrap_ui_path,"Step 35 managed-plugin bootstrap iOS partial"),(step35_telemetry_ui_path,"Step 35 telemetry iOS partial"),(step35_mode_path,"Step 35 diagnostic mode"),(step35_recon_path,"Step 35 Godot/native reconnaissance")]:
    require(path.is_file(), f"{label} exists")
step35_source = step35_source_path.read_text() if step35_source_path.is_file() else ""
step35_gate = step35_gate_path.read_text() if step35_gate_path.is_file() else ""
step35_summary = step35_summary_path.read_text() if step35_summary_path.is_file() else ""
step35_tests = step35_tests_path.read_text() if step35_tests_path.is_file() else ""
step35_ui_main = step35_ui_path.read_text() if step35_ui_path.is_file() else ""
step35_bootstrap_ui = step35_bootstrap_ui_path.read_text() if step35_bootstrap_ui_path.is_file() else ""
step35_telemetry_ui = step35_telemetry_ui_path.read_text() if step35_telemetry_ui_path.is_file() else ""
step35_ui = step35_ui_main + "\n" + step35_bootstrap_ui + "\n" + step35_telemetry_ui
step35_mode = step35_mode_path.read_text() if step35_mode_path.is_file() else ""
step35_recon = step35_recon_path.read_text() if step35_recon_path.is_file() else ""
step15_native_bridge = read("src/StS2Launcher.iOS/Platform/GodotStep15NativeBridge.cs")
step15_objc_bridge = read("native/step15/godot_module/sts2_ios_host/step15_ios_host_bridge.mm")
step15_build_script = read("scripts/build-godot.sh")
step15_link_preflight = read("scripts/preflight-godot-link.sh")
step15_smoke_project = read("native/step15/smoke_project/project.godot")
require(all(marker in step35_gate for marker in ["VerifiedExecutionPreflight = 1", "ExecutionCapableClrAdmission = 2", "DiagnosticExecuteVeryEarlyInvocation = 3", "FinalIsolationAudit = 4"]), "Step 35.0.30 retains exactly four ordered A-D gates shared by diagnostic and exact-authority modes")
require("STEP 35.0.30 DIAGNOSTIC LOCALIZATION COMPLETE — 4/4 — NOT STEP 35 CLOSURE" in step35_summary and "Gates.Count(g => g.Passed)}/4" in step35_summary, "Step 35.0.30 diagnostic summary keeps diagnostic 4/4 distinct from exact closure")
require("STEP 35.0.30 DIAGNOSTIC LOCALIZATION COMPLETE — 4/4 — NOT STEP 35 CLOSURE" in step35_tests, "Step 35.0.30 host gate-summary assertion matches the active production diagnostic summary identity")
require("STEP 35.0.15 DIAGNOSTIC LOCALIZATION COMPLETE — 4/4 — NOT STEP 35 CLOSURE" not in step35_tests, "Step 35.0.26 static validation rejects the stale 0.0.139 Step-35.0.15 gate-summary assertion")
for marker35 in ["SourceTargetMethodToken = 0x06007D02", "SourceStateMachineMoveNextToken = 0x0600BC71", "<ExecuteVeryEarly>d__7", "System.Threading.Tasks.Task MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()", "StS2Launcher-Step35-VeryEarly"]:
    require(marker35 in step35_source, f"Step 35 hard-pins exact very-early target/context evidence: {marker35}")
require("RunSourceAdmissionAndPrivateCloneAsync" in step35_source and "RunDeterministicStackNeutralRewrite" in step35_source and "RunTransformedImageVerification" in step35_source and "RunFinalIsolationAuditAsync" in step35_source, "Step 35 Gate A re-runs the closed Step-32 A-D transform contract")
require("FindMethodByToken" in step35_source and "FindVeryEarlyMoveNext" in step35_source and "ComputeMethodSemanticFingerprint" in step35_source and "CountLaterOneTimeInitializationCalls" in step35_source and "CountHarmonyMethodReferences" in step35_source, "Step 35 Gate A independently audits exact wrapper/async-state-machine semantics and later-boundary purity")
require(all(marker in step35_source for marker in ["BuildStaticInstructionMap", "CALLSITE#", "AWAIT-CANDIDATE", "DescribeMetadataScope", "GetVerifiedVeryEarlyStaticInstructionMap", "[NULL PLATFORM CTOR IL]", "[COMMAND LINE HELPER CCTOR IL]", "[COMMAND LINE HELPER TRYGETVALUE IL]", "NullPlatformConstructorFullName", "CommandLineHelperTypeFullName", "CommandLineHelperTryGetValueFullName"]), "Step 35.0.30 static map covers the exact NullPlatform constructor plus CommandLineHelper cctor/TryGetValue callsite ordinals without resolving dependencies")
require(all(marker in step35_source for marker in ["CreateInstrumentedDiagnosticClone", "CreateInstrumentedGodotSharpDiagnosticClone", "GetDiagnosticMarkerTargets", "BuildGodotSharpDiagnosticMarkerPlan", "HasInjectedEntryMarkerAtStart", "InsertNullPlatformConstructorCallsiteMarkers", "InsertCommandLineHelperCriticalBoundaryMarkers", "HasCommandLineHelperCriticalBoundaryMarkers", "ApplyCommandLineHelperManagedDictionaryCompatibilityRewrite", "VerifyCommandLineHelperManagedDictionaryCompatibilityRewrite", "VerifyCommandLineHelperNaturalGodotDictionaryPreserved", "ManagedStringDictionaryFullName", "CommandLineManagedDictionarySubstitutionCount", "GodotSharpDiagnosticCloneFileName", "GodotSharpDiagnosticBridgeTypeFullName", "GODOT_DIAGNOSTIC_BRIDGE_ARMED", "PrivateDiagnosticOverride", "CommandLineCctorDictionaryBeforeMarker", "CommandLineCctorDictionaryAfterMarker", "CommandLineCctorGetCmdlineArgsBeforeMarker", "CommandLineCctorGetCmdlineArgsAfterMarker", "NullPlatformTypeFullName", "CommandLineHelperTypeFullName", "INMETHOD_NP", "INMETHOD_CL_CRITICAL", "INMETHOD_027", "C_DIAGNOSTIC_BRIDGE_ARMED", "preflight.DiagnosticPath", "preflight.DiagnosticSha256"]), "Step 35.0.30 preserves prior sts2 diagnostics, supports natural/compat modes, and adds a separately verified entry-only GodotSharp diagnostic derivative")
require(all(marker in step35_source for marker in ["Godot.NativeInterop.InteropUtils", "EngineGetSingleton", "UnmanagedGetManaged", "ConvertStringToNative", "godotsharp_engine_get_singleton", "godotsharp_internal_unmanaged_get_script_instance_managed", "godotsharp_internal_unmanaged_get_instance_binding_managed", "godotsharp_internal_unmanaged_instance_binding_create_managed", "singleton localization requires"]), "Step 35.0.30 GodotSharp derivative fails closed on the exact singleton acquisition/native-to-managed localization targets")
require(all(marker in step35_tests for marker in ["Godot.NativeInterop.InteropUtils::EngineGetSingleton", "Godot.NativeInterop.InteropUtils::UnmanagedGetManaged", "godotsharp_engine_get_singleton", "godotsharp_internal_unmanaged_instance_binding_create_managed"]), "Step 35.0.30 host regression protects singleton-acquisition marker coverage")
require(all(marker in step35_recon for marker in ["Godot.NativeInterop.InteropUtils", "EngineGetSingleton", "UnmanagedGetManaged", "ConvertStringToNative", "godotsharp_engine_get_singleton", "godotsharp_internal_unmanaged_get_script_instance_managed", "godotsharp_internal_unmanaged_get_instance_binding_managed", "godotsharp_internal_unmanaged_instance_binding_create_managed"]), "Step 35.0.30 read-only reconnaissance mirrors singleton acquisition/native-to-managed callback coverage")
require(all(marker in step35_mode for marker in ["NaturalGodotDictionaryRecon", "ManagedDictionaryCompatibility"]), "Step 35.0.30 exposes NATURAL and COMPAT modes in one build")
require(all(marker in step35_recon for marker in ["[GODOTSHARP MANAGED / NATIVE-BOUNDARY MAP]", "GodotSharp P/Invoke methods", "GodotSharp calli sites", "NativeFuncs/UnmanagedCallbacks field-use sites", "[MACH-O / NATIVE INVENTORY]", "sha256=", "uuid=", "dylib=", "rpath=", "symbol=", "string=", "MH_MAGIC_64", "FAT_MAGIC", "0x0100000C"]), "Step 35.0.30 reconnaissance records managed/native boundaries and read-only arm64 Mach-O metadata")
require("LoadFromStream" not in step35_recon and "NativeLibrary.Load" not in step35_recon and "DllImport" not in step35_recon, "Step 35.0.30 reconnaissance source performs no CLR/native image execution")
sweep_definition = step35_source.find("internal static IReadOnlyList<DiagnosticCallsiteSweepEntry> InsertDiagnosticCallsiteSweepMarkers")
sweep_skip = step35_source.find("callee.DeclaringType.FullName == DiagnosticBridgeTypeFullName && callee.Name == \"Emit\"", sweep_definition)
sweep_increment = step35_source.find("callsiteOrdinal++;", sweep_definition)
require(sweep_definition >= 0 and sweep_skip > sweep_definition and sweep_increment > sweep_skip, "Step 35.0.30 excludes injected diagnostic bridge calls before exact-source callsite ordinal accounting")
require("InsertCommandLineHelperCriticalBoundaryMarkers(commandLineCctor, emitReference)" in step35_source and "could not locate Godot.OS.GetCmdlineArgs" in step35_source, "Step 35.0.30 fails Gate A closed unless stack-neutral critical localization contains Godot.OS.GetCmdlineArgs")
require(all(marker in step35_source for marker in ["commandLineCctorOriginalMaxStack", "commandLineCctorDiagnosticMaxStack", "commandLineCctorDiagnosticMaxStack != commandLineCctorOriginalMaxStack", "verifiedCommandLineCctor.Body.MaxStackSize != commandLineCctorOriginalMaxStack"]), "Step 35.0.30 preserves and post-write verifies unchanged CommandLine cctor MaxStack")
require("commandLineCctorCallsitePlan = InsertCommandLineHelperCctorCallsiteMarkers(commandLineCctor" not in step35_source and "commandLineTryGetValueCallsitePlan = InsertCommandLineHelperTryGetValueCallsiteMarkers(commandLineTryGetValue" not in step35_source, "Step 35.0.30 production clone forbids live-stack CL/CLTV sweep insertion")
require(all(marker in step35_source for marker in ["systemCollectionsScopes.Length != 1", "argsField.FieldType = dictionaryString", 'new MethodReference(".ctor"', 'new MethodReference("set_Item"', 'new MethodReference("TryGetValue"', "new ParameterDefinition(keyParameter)", "new ByReferenceType(valueParameter)", "naturalGetCmdlineArgsCount != 1", "residualGodotDictionaryReferences.Length != 0"]), "Step 35.0.30 dictionary-only derivative reuses existing System.Collections, rewrites exactly the Godot string-dictionary contract with VAR signatures, removes residual affected-method Godot dictionary calls, and retains natural GetCmdlineArgs")
require("if (cctor is not null && verifiedCctorTypes.Add(item.TypeName))\n            if (cctor is not null && verifiedCctorTypes.Add(item.TypeName))" not in step35_source, "Step 35.0.30 has no duplicated cctor-verification guard that can suppress serialized entry-marker checks")
require(all(marker in step35_source for marker in ["CommandLineCctorDictionaryBeforeMarker", "CommandLineCctorDictionaryAfterMarker", "CommandLineCctorGetCmdlineArgsBeforeMarker", "CommandLineCctorGetCmdlineArgsAfterMarker", "InsertMarkerBefore", "InsertMarkerAfter"]), "Step 35.0.30 carries redundant stack-neutral critical markers around dictionary and Godot command-line boundaries")
require(all(marker in step35_source for marker in ["module.Write(diagnosticPath", "verifyModule.Mvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid", "transformedSha256AfterDiagnosticEmission", "ComputeSha256Hex(preflight.TransformedPath)", "markerCountVerified != expectedMarkerCount"]), "Step 35.0.26 preserves/rechecks the exact transformed source and verifies diagnostic-clone MVID/hash/marker provenance")
require(all(marker in step35_source for marker in ["DiagnosticConstantMetadataWriteResolver", "DiagnosticCecilWriteSystemRuntimeIdentity", "DiagnosticCecilWriteSentryIdentity", "System.Reflection.BindingFlags", "Sentry.BreadcrumbLevel", "Sentry.SentryLevel", "CollectDiagnosticExternalConstantTypeRequirements", "resolver.Configure(module)", "resolver.ValidateWriteRequests()", "RealStS2PrepareMethodRewrite.ComputeConstantMetadataFingerprint", "verifiedConstantMetadataSha256.Equals(expectedConstantMetadataSha256", "External assembly bytes opened by the writer-only surrogate resolver: 0"]), "Step 35.0.26 reuses the Step-32 audited constant-metadata requirement set locally for Cecil serialization only")
require("new DiagnosticConstantMetadataWriteResolver()" in step35_source and "new RejectingAssemblyResolver()" in step35_source, "Step 35.0.26 separates bounded writer-only Cecil resolution from rejecting post-write verification")
clone_start = step35_source.find("private static DiagnosticCloneSnapshot CreateInstrumentedDiagnosticClone")
clone_end = step35_source.find("private static void InsertEntryMarker", clone_start)
clone_block = step35_source[clone_start:clone_end] if clone_start >= 0 and clone_end > clone_start else ""
require("ReadingMode = ReadingMode.Deferred" in clone_block and "ReadingMode = ReadingMode.Immediate" not in clone_block and "InMemory = true" not in clone_block and clone_block.find("resolver.Configure(module)") < clone_block.find("module.Write(diagnosticPath") and "resolver.Requests.Count != 0" in clone_block, "Step 35.0.26 diagnostic clone preserves deferred-open -> zero-request -> configure -> write ordering")
require(all(marker in clone_block for marker in ["new GenericParameter(\"T\", actionOpen)", "actionOpen.GenericParameters.Add(actionTypeParameter)", "new ParameterDefinition(actionTypeParameter)", "Action<string>::Invoke(!0)", "bridgeInvokeParameter.Type != GenericParameterType.Type", "bridgeInvokeParameter.Position != 0"]), "Step 35.0.26 preserves the physically corrected Action<string>::Invoke(!0) MemberRef")
require("invoke.Parameters.Add(new ParameterDefinition(module.TypeSystem.String))" not in clone_block, "Step 35.0.26 rejects the physically disproven synthetic Action<string>::Invoke(string) encoding")
require("CreateDiagnosticActionStringInvokeReference" in step35_source and "DiagnosticActionStringInvokeMemberRefRoundTripsAsDeclaringTypeVarZero" in step35_tests, "Step 35.0.30 host regressions protect the Action<string>::Invoke(!0) encoding")
require(all(marker in step35_tests for marker in ["ComprehensiveGodotSharpDiagnosticCloneUsesEntryOnlyMarkersAndPreservesIdentity", "ComprehensiveGodotNativeReconnaissanceParsesReadOnlyMachOAndManagedMap", "CreateInstrumentedGodotSharpDiagnosticClone", "Step35GodotReconnaissance.BuildReport"]), "Step 35.0.30 host regressions cover the entry-only GodotSharp derivative and read-only Mach-O reconnaissance")
require(all(marker in step35_tests for marker in ["serializedGodotEntryMarkers", "GodotSharpDiagnosticBridgeTypeFullName", "emitCall.DeclaringType.FullName"]), "Step 35.0.30 host regression directly verifies every serialized GS entry marker calls the GodotSharp bridge")
godot_clone_start = step35_source.find("internal static GodotSharpDiagnosticCloneSnapshot CreateInstrumentedGodotSharpDiagnosticClone")
godot_clone_end = step35_source.find("private static IReadOnlyList<GodotSharpDiagnosticMarker> BuildGodotSharpDiagnosticMarkerPlan", godot_clone_start)
godot_clone_block = step35_source[godot_clone_start:godot_clone_end] if godot_clone_start >= 0 and godot_clone_end > godot_clone_start else ""
require("ReadingMode = ReadingMode.Deferred" in godot_clone_block and "ReadingMode = ReadingMode.Immediate" not in godot_clone_block and "InsertEntryMarker" in godot_clone_block and "InsertDiagnosticCallsiteSweepMarkers" not in godot_clone_block, "GodotSharp derivative uses deferred-open and entry-only instrumentation, never live-stack callsite sweeps")
require(all(marker in godot_clone_block for marker in ["resolver.Configure(module)", "module.Write(diagnosticPath", "resolver.ValidateWriteRequests()", "new RejectingAssemblyResolver()", "verifyModule.Mvid != sourceMvid", "Action<string>::Invoke(!0)"]), "GodotSharp derivative uses audited writer-only resolution, rejecting reopen, preserved MVID, and the corrected generic callback bridge")
require("string expectedBridgeTypeFullName = DiagnosticBridgeTypeFullName" in step35_source and "HasInjectedEntryMarkerAtStart(method, item.Marker, GodotSharpDiagnosticBridgeTypeFullName)" in godot_clone_block and "call.DeclaringType.FullName == expectedBridgeTypeFullName" in step35_source, "Step 35.0.26 verifies serialized entry markers against the derivative-specific bridge type and protects the 0.0.137 208/209 regression")
require(all(marker in step35_mode for marker in ["NaturalGodotDictionaryRecon = 0", "ManagedDictionaryCompatibility = 1", "ManagedCommandLineCompatibility = 2", "GodotCoreCallbackHandoff = 3"]), "Step 35.0.26 exposes the NATURAL, OS-RECON, FORWARD, and Step15-live CORE-HANDOFF mode identities")
require(all(marker in step35_source for marker in ["RunGodotCoreCallbackHandoffInitialization", "CB_INIT_ENTRY", "CB_GODOTSHARP_LOAD_START", "CB_NATIVEFUNCS_BIND_PASS", "CB_INITIALIZE_INVOKE_START", "CB_INITIALIZE_INVOKE_RETURNED", "CB_INITIALIZE_PASS", "Godot.NativeInterop.NativeFuncs", "Initialize(IntPtr,int)", "initializedField", "CallbackHandoffSnapshot", "GodotCoreCallbackHandoff"]), "Step 35.0.26 CORE-HANDOFF loads only the verified private GodotSharp derivative, binds exact NativeFuncs.Initialize(IntPtr,int), requires initialized=false, invokes once, and preserves a resolver baseline before Gate C")
require(all(marker in step35_tests for marker in ["GodotCoreCallbackHandoffRejectsMissingTableBeforeAnyPreflightOrClrWork", "IntPtr.Zero", "Assert.AreEqual(1, checkpoints.Count", "stage=initialization", "System.ArgumentException", "CB_INIT_ENTRY"]), "Step 35.0.30 host regression rejects invalid native callback metadata before preflight/CLR work while preserving exactly one durable managed-failure checkpoint and proving CB_INIT_ENTRY was not reached")
require(all(marker in step35_tests for marker in ["GodotManagedPluginReverseBridgeRejectsInvalidNativeSizeBeforePreflightOrClrWork", "PrepareGodotManagedPluginReverseBridge(0", "CB_REVERSE_PREP_MANAGED_FAIL", "CB_REVERSE_PREP_ENTRY"]), "Step 35.0.30 host regression rejects invalid native ManagedCallbacks size before preflight/CLR work")
require('Assert.AreEqual("Step 35 Gate A must pass before Gate B.", ex.Message);' in step35_tests and 'Contains("preflight", StringComparison.OrdinalIgnoreCase)' not in step35_tests, "Step 35.0.30 host regression pins the actual Gate-A missing-preflight exception contract instead of requiring a stale preflight substring")
require(all(marker in step35_tests for marker in ["GodotManagedPluginResolverBaselineRejectsMissingPreflightWithDurableFailure", "SealGodotManagedPluginBootstrapResolverBaseline", "CB_POST_BOOTSTRAP_RESOLVER_BASELINE_FAIL", "CB_POST_BOOTSTRAP_RESOLVER_BASELINE_PASS"]), "Step 35.0.30 host regression protects the post-bootstrap resolver-baseline failure contract")
require(all(marker in step15_objc_bridge for marker in ["modules/mono/glue/runtime_interop.h", "CSharpLanguage::get_singleton()", "GDMono::get_singleton()", "godotsharp::get_runtime_interop_funcs", "sts2_step15_is_runtime_interop_ready", "sts2_step15_has_dotnet_feature", "sts2_step15_is_dotnet_runtime_initialized", "sts2_step15_get_runtime_interop_funcs", 'has_feature("dotnet")']), "Step 35.0.26 native Step-15 bridge exposes only the source-built Godot runtime interop table after C# native scaffolding readiness and reports competing dotnet state")
require(all(marker in step15_native_bridge for marker in ["IsRuntimeInteropReady", "HasDotNetFeature", "IsDotNetRuntimeInitialized", "GetRuntimeInteropFunctions", "sts2_step15_get_runtime_interop_funcs"]), "Step 35.0.26 managed native bridge exposes callback-table readiness/state/pointer without loading a game native image")
require(all(marker in step15_objc_bridge for marker in ["modules/mono/mono_gd/gd_mono_cache.h", "sts2_step15_has_csharp_language_singleton", "sts2_step15_is_godot_api_cache_updated", "sts2_step15_has_managed_create_binding_callback", "sts2_step15_is_reverse_binding_ready", "sts2_step15_get_managed_callbacks_size", "sts2_step15_install_external_managed_callbacks", "sts2_step15_signal_external_core_api_loaded", "GDMonoCache::update_godot_api_cache", "ScriptManagerBridge_CreateManagedForGodotObjectBinding", "GD_OnCoreApiAssemblyLoaded"]), "Step 35.0.26 native bridge preserves readiness telemetry and adds complete-struct cache adoption plus the standard isolated core-API reverse callback")
require("GDMono::initialize()" not in step15_objc_bridge and "runtime_initialized =" not in step15_objc_bridge, "Step 35.0.26 external bridge does not start or fake Godot runtime ownership")
require(all(marker in step15_native_bridge for marker in ["HasCSharpLanguageSingleton", "IsGodotApiCacheUpdated", "HasManagedCreateBindingCallback", "IsReverseBindingReady", "ManagedCallbacksSizeBytes", "IsExternalManagedBridgeInstalled", "InstallExternalManagedCallbacks", "SignalExternalCoreApiLoaded", "DidExternalCoreApiSignalReturn"]), "Step 35.0.26 managed native bridge exposes complete reverse-bootstrap state and operations")
new_native_exports=["sts2_step15_get_managed_callbacks_size", "sts2_step15_is_external_managed_bridge_installed", "sts2_step15_install_external_managed_callbacks", "sts2_step15_signal_external_core_api_loaded", "sts2_step15_did_external_core_api_signal_return"]
require(all(marker in project_text for marker in new_native_exports), "iOS ReferenceNativeSymbol roots preserve every Step-35.0.25 managed-plugin bootstrap export")
require(all(marker in step15_link_preflight for marker in new_native_exports), "standalone Step-15 link preflight roots every managed-plugin bootstrap export")
require(all(marker in step35_source for marker in ["PrepareGodotManagedPluginReverseBridge", "GodotPlugins.Game.Main", "InitializeFromGameProject", "godotsharp_game_main_init", "Godot.Bridge.ManagedCallbacks", "callbackFields.Length != 37", "ManagedCallbacks.Create", "ScriptManagerBridge", "LookupScriptsInAssembly", "CB_REVERSE_PREP_PASS"]), "Step 35.0.30 core reproduces and verifies the generated game-plugin managed bootstrap contract instead of invoking the UnmanagedCallersOnly entry point directly")
require(all(marker in step35_source for marker in ["SealGodotManagedPluginBootstrapResolverBaseline", "ManagedPluginBootstrapResolverSnapshot", "CB_POST_BOOTSTRAP_RESOLVER_BASELINE_PASS", "CB_POST_BOOTSTRAP_RESOLVER_BASELINE_FAIL", "expectedManagedDelta", "System.Runtime, Version=9.0.0.0", "System.Runtime.InteropServices, Version=9.0.0.0", "System.Runtime.InteropServices, Version=8.0.0.0", "System.Collections.Concurrent, Version=8.0.0.0", "System.Runtime.Loader, Version=8.0.0.0", "System.Threading, Version=8.0.0.0", "System.Collections, Version=8.0.0.0", "System.Linq, Version=8.0.0.0", "managedDelta.Length != expectedManagedDelta.Length", "hostRequestDelta.Length != expectedManagedDelta.Length", "privateDelta.Length != 0", "post-bootstrap resolver baseline is intact immediately before target binding"]), "Step 35.0.26 seals the exact physical 0.0.146 eight-request/eight-host-load/zero-private bootstrap resolver delta and freezes it before Gate C")
require(all(marker in step35_ui for marker in ["CB_REVERSE_BINDING_STATE_BEFORE", "CB_REVERSE_BASELINE_PASS", "CB_MANAGED_CALLBACKS_SIZE", "CB_NATIVE_REVERSE_INSTALL_START", "CB_NATIVE_REVERSE_INSTALL_RETURNED", "CB_REVERSE_BINDING_STATE_AFTER_INSTALL", "CB_REVERSE_CACHE_ADOPTION_PASS", "CB_CORE_API_SIGNAL_START", "CB_CORE_API_SIGNAL_RETURNED", "CB_MANAGED_PLUGIN_BOOTSTRAP_PASS"]), "Step 35.0.26 CORE-HANDOFF carries one coordinated reverse-bootstrap sequence with durable boundaries before natural Gate C")
bootstrap_call_pos = step35_ui_main.find("RunStep35ManagedPluginBootstrap();")
gate_c_pos = step35_ui_main.find("C_UI_SELECTED")
require("CB_MANAGED_PLUGIN_BOOTSTRAP_PASS" in step35_bootstrap_ui and bootstrap_call_pos >= 0 and gate_c_pos > bootstrap_call_pos, "Step 35.0.26 natural Gate C occurs only after managed-plugin bootstrap PASS")
require("SealGodotManagedPluginBootstrapResolverBaseline" in step35_bootstrap_ui and "CB_POST_BOOTSTRAP_RESOLVER_BASELINE_RETURNED" in step35_bootstrap_ui, "Step 35.0.26 iOS bootstrap seals and records the post-bootstrap resolver baseline after bridge PASS")
require("module_mono_enabled=yes" in step15_build_script and "runtime interop callback-table exposure" in step15_build_script, "pinned Step-15 source build enables Godot's native mono/C# module required for the exact callback-table producer")
require(all(marker in step15_link_preflight for marker in ["sts2_step15_is_runtime_interop_ready", "sts2_step15_has_dotnet_feature", "sts2_step15_is_dotnet_runtime_initialized", "sts2_step15_get_runtime_interop_funcs"]), "standalone Step-15 native-link preflight carries unresolved roots for every callback-handoff export")
require("dotnet" not in step15_smoke_project.lower(), "project-owned Step-15 smoke project does not advertise a dotnet project feature")
require(all(marker in step35_ui for marker in ["CORE-HANDOFF", "GodotCoreCallbackHandoff", "Run Step 15 Gates A-C first", "IsRuntimeInteropReady", "HasDotNetFeature", "IsDotNetRuntimeInitialized", "GetRuntimeInteropFunctions", "CB_NATIVE_TABLE_REQUEST_RETURNED", "RunGodotCoreCallbackHandoffInitialization"]), "Step 35.0.30 iOS surface gates CORE-HANDOFF on an already-live Step-15 engine, rejects competing dotnet state, obtains the exact native table, and records the managed handoff")
require("using StS2Launcher.iOS.Platform;" in step35_ui and "GodotStep15NativeBridge" in step35_ui, "Step 35.0.26 Step-35 iOS partial explicitly imports the Platform namespace required to resolve GodotStep15NativeBridge and protects the 0.0.142 CS0103 compile regression")
require(all(marker in step35_source for marker in ["ManagedCommandLineArgsBridgeMethodName", "ApplyCommandLineHelperManagedCommandLineCompatibilityRewrite", "VerifyCommandLineHelperManagedCommandLineCompatibilityRewrite", "CommandLineManagedCommandLineSubstitutionCount", "requireNaturalGetCmdlineArgs: false", "providerInstructions.Length != 3", "Code.Ldc_I4_0", "Code.Newarr"]), "Step 35.0.30 FORWARD mode is a bounded one-call-site empty-string-array substitution with post-write verification")
require("ManagedCommandLineCompatibilityReplacesExactlyOneGodotArgsCallWithLocalEmptyArrayProvider" in step35_tests and "ManagedCommandLineArgsBridgeMethodName" in step35_tests, "Step 35.0.30 host regression protects the exact one-site managed command-line provider rewrite")
require(all(marker in step35_source for marker in ["Godot.OS/MethodName", "Godot.StringName", "ClassDB_get_method_with_compatibility", "godotsharp_string_name_new_from_string", "godotsharp_method_bind_get_method_with_compatibility", "selected.Count < 128", "depth >= 4"]), "Step 35.0.30 GodotSharp derivative expands entry-only closure around OS type initialization and callback thunks")
require(all(marker in step35_recon for marker in ["Godot.OS/MethodName", "Godot.StringName", "ClassDB_get_method_with_compatibility", "godotsharp_string_name_new_from_string", "godotsharp_method_bind_get_method_with_compatibility", "visited.Count < 128"]), "Step 35.0.30 read-only reconnaissance mirrors the expanded OS/callback closure")
require("DiagnosticGodotCallsiteMarkersRoundTripImmediatelyBeforeAndAfterTargetCall" in step35_tests and "InsertCallsiteMarkers" in step35_tests and "DirExistsAbsolute" in step35_tests, "Step 35.0.30 retains prior Godot callsite marker round-trip regression")
require(all(marker in step35_tests for marker in ["DiagnosticNullPlatformConstructorCallsiteSweepRoundTripsEveryNonBaseCallLikeInstruction", "InsertSyntheticEntryMarker", "InsertNullPlatformConstructorCallsiteMarkers", "CallsiteOrdinal", "Physical 0.0.132", "DiagnosticCommandLineHelperSweepsIgnoreInjectedEntryBridgeAndRoundTripExactOrdinals", "DiagnosticCommandLineHelperSweepSkipsUnrelatedBranchTargetButPreservesExactOrdinals", "DiagnosticCommandLineCriticalMarkersAreStackNeutralAndSerializedWithMaxStackHeadroom", "DiagnosticCommandLineManagedDictionaryRewriteRoundTripsGenericVarMemberRefsAndRetainsNaturalGodotArgsCall", "ApplyCommandLineHelperManagedDictionaryCompatibilityRewrite", "VerifyCommandLineHelperManagedDictionaryCompatibilityRewrite", "ManagedStringDictionaryFullName", "DiagnosticCallsiteSweepRaisesMaxStackAndClrExecutesTightRewrittenCctor", "MaxStackSize = 3", "cctorAfter.Body.MaxStackSize >= 4", "LoadFromStream", 'GetMethod("Touch"', "InsertCommandLineHelperCctorCallsiteMarkers", "InsertCommandLineHelperTryGetValueCallsiteMarkers", "Godot.OS::GetCmdlineArgs()"]), "Step 35.0.30 regressions protect exact ordinals, managed dictionary generic MemberRefs, natural GetCmdlineArgs retention, critical boundaries and executable MaxStack safety")
require("sourceResolver.Requests.Count != 0" in step35_source and "transformedResolver.Requests.Count != 0" in step35_source, "Step 35 static target audit remains dependency-resolution free")
require(all(marker in step35_source for marker in ["RunDiagnosticExecuteVeryEarlyInvocationAsync", "Gate C requires a durable launcher-owned checkpoint callback", "method.ReturnType != typeof(Task)", "method.Invoke(null, null)", "task.WaitAsync(TimeSpan.FromSeconds(60), cancellationToken)", "C_EXACT_AUTHORITY_PASS", "preflight.TransformedMethodToken"]), "Step 35.0.30 invokes the selected Task-returning ExecuteVeryEarly once, supports exact-authority token binding, and preserves the 60-second await boundary")
require("InitializerBearingRequests" in step35_source and "RejectedManagedRequests" in step35_source and "LoadUnmanagedDll" in step35_source, "Step 35 resolver stays exact-plan/fail-closed for initializer-bearing, unplanned and native requests")
require(all(marker in step35_source for marker in ["B_LOADFROMSTREAM_START", "B_LOADFROMSTREAM_PASS", "C_BIND_TYPE_START", "C_BIND_METHOD_START", "C_INVOKE_START", "C_INVOKE_RETURNED", "C_TASK_CONFIRMED", "C_WAIT_START", "C_WAIT_COMPLETED", "RESOLVE_MANAGED_START", "RESOLVE_PRIVATE_PASS", "RESOLVE_HOST_PASS", "RESOLVE_NATIVE_REJECT"]), "Step 35.0.30 core retains durable B→C/invocation/resolver crash-localization markers")
require(all(marker in step35_ui for marker in ["Step35-CurrentRun.txt", "Step35-LastCheckpoint.txt", "Step35-CrashCheckpoint-", "Step35-ExecuteVeryEarly-StaticMap-", "Step35-GodotNativeReconnaissance-", "TryInitializeStep35RunTelemetry", "WriteStep35StaticMap", "WriteStep35GodotReconnaissance", "Flush(flushToDisk: true)", "Environment.CurrentManagedThreadId", "CANCELLED / INCONCLUSIVE", "Output-only diagnostic", "0.0.140", "GS031", "CL_CRITICAL_002_POST", "NP002_POST", "GodotFileIo.CreateDirectory", "NaturalGodotDictionaryRecon", "ManagedDictionaryCompatibility", "ManagedCommandLineCompatibility", "GodotCoreCallbackHandoff", "OS-RECON", "FORWARD", "CORE-HANDOFF", "entry-only", "Mach-O/native"]), "Step 35.0.30 iOS surface preserves the physical 0.0.140 three-mode proof and exposes the separately gated CORE-HANDOFF diagnostic")
require("crashCheckpoints.Any" in step35_tests and "B_LOADFROMSTREAM_START" in step35_tests and "RESOLVE_INITIALIZER_BEARING_REJECT" in step35_tests and "StaticInstructionMapCapturesCallsitesAndAwaitCandidatesWithoutResolution" in step35_tests, "Step 35.0.30 host regressions retain crash-checkpoint and static-map coverage")
require("OrderedDiagnosticLocalizationGatesReachFourOfFourWithoutClaimingClosure" in step35_tests and "VeryEarlyInitializationStopsAfterFirstFailure" in step35_tests and "VeryEarlyContextLoadsInitializerFreePrivateDependencyAndRejectsInitializerBearingDependency" in step35_tests and "Step35PinsTheExactVeryEarlyManagedInitializationTarget" in step35_tests, "Step 35.0.30 host regressions protect diagnostic gate order, exact source target and initializer-bearing refusal")
require("Assert.ThrowsException" not in step35_tests and "ThrowsExceptionAsync" not in step35_tests and "Assert.ThrowsExactly" in step35_tests, "Step 35 host regressions use supported MSTest v4 exception assertions")
require(all(marker in step35_ui for marker in ["DIAGNOSTIC COMPLETE: STEP 35.0.30 — 4/4", "EXACT STEP 35 CLOSURE CANDIDATE: COMPLETE — 4/4", "RUN_EXACT_STEP35_4OF4", "GodotCoreExactClosure"]), "Step 35.0.30 iOS surface separates diagnostic 4/4 from the explicit exact-authority closure candidate")
step35_bootstrap_ui = read("src/StS2Launcher.iOS/UI/RootViewController.Step35ManagedPluginBootstrap.cs")
require("using StS2Launcher.Core;" in step35_bootstrap_ui and "Step35DiagnosticMode.GodotCoreExactClosure" in step35_bootstrap_ui, "Step 35.0.30 managed-plugin bootstrap partial imports the declared StS2Launcher.Core namespace for Step35DiagnosticMode exact-closure compile integration")
require(all(marker in step35_source for marker in ["Gate D OfflineReady", "CallbackProgress<SteamOfflineInstallProgress>", "value.CompletedBytes", "value.TotalBytes"]), "Step 35.0.30 Gate D forwards protected OfflineReady file/byte checkpoints into the Step-35 progress contract")
step35_gate_d_ui = read("src/StS2Launcher.iOS/UI/RootViewController.Step35GateDProgress.cs")
require(all(marker in step35_gate_d_ui for marker in ["UIProgressView", "Gate D receipt hash", "ProcessedBytes", "TotalBytes", "Latest verifier file:", "GiB", "/s", "NSTimer.CreateRepeatingScheduledTimer", "last verifier progress"]), "Step 35.0.30 iOS surface exposes dedicated Gate-D file/byte progress plus a live heartbeat during long single-file hashes")
require("SteamOfflineInstallInspection.cs" not in ["changed-by-step35-gated-progress"], "Step 35.0.30 leaves the physically protected Step-13 OfflineReady verifier implementation unchanged")
require("STEP-35 DIAGNOSTIC-CLONE EXECUTEVERYEARLY INVOCATION/AWAIT COMPLETED NORMALLY; THIS IS LOCALIZATION EVIDENCE, NOT EXACT STEP-35 CLOSURE." in step35_source and "EXACT CLOSED STEP-32 TRANSFORMED EXECUTEVERYEARLY INVOCATION/AWAIT COMPLETED NORMALLY" in step35_source, "Step 35.0.30 Gate-C PASS wording distinguishes diagnostic from exact authority")
require(all(marker in step35_mode for marker in ["GodotCoreExactClosure = 4", "exact-authority closure candidate"]) and all(marker in step35_source for marker in ["IsExactAuthorityMode", "preflight.TransformedPath", "exact closed Step-32 transformed sts2 image", "C_EXACT_AUTHORITY_PASS", "D_EXACT_PRIMARY_REPROOF_PASS"]) and "ExactExecuteVeryEarlyInvocation = 3" not in step35_gate, "Step 35.0.30 exact-authority mode uses the existing four-gate contract and exact transformed CLR authority without inventing a fifth gate")
require("GodotCoreExactClosureRejectsMissingTableBeforeAnyPreflightOrClrWork" in step35_tests and "Assert.AreEqual(4, (int)Step35DiagnosticMode.GodotCoreExactClosure)" in step35_tests, "Step 35.0.30 host regressions pin exact-closure mode value and fail-closed callback metadata")
require(all(marker in step35_source for marker in ["D_AUDIT_ENTRY", "D_OFFLINE_READY_RETURNED", "D_FINAL_CHECKS_PASS", "D_RESULT_CONSTRUCT_START", "D_RESULT_CONSTRUCT_RETURNED", "D_FINAL_PROGRESS_EMITTED", "D_TASK_RETURN_START"]), "Step 35.0.30 core durably localizes Gate-D finalization around result construction/progress/return")
require(all(marker in step35_ui for marker in ["D_TASK_AWAIT_RESUMED", "D_RESULT_RECORD_PASS", "CompleteStep35GateDProgress"]), "Step 35.0.30 UI durably records Gate-D Task resumption and result recording")

require("Step35-TransformedRealStS2VeryEarlyInitialization.txt" in step35_ui and "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION" in step35_ui and "UIButtonType.System" not in step35_ui, "Step 35 iOS UI persists the dedicated report and uses the numeric SystemButton font-size contract")
require("new TransformedRealStS2VeryEarlyInitialization(_launcherDataRoot)" in root_ui_text and "AddTransformedRealStS2VeryEarlyInitializationControls(content)" in root_ui_text, "RootViewController wires Step 35 into the active device surface")
require("Step35ImplementationMarker" in release_presentation and "0x06007D02" in release_presentation and "0x0600BC71" in release_presentation and "ExecuteVeryEarly" in release_presentation and "0.0.138" in release_presentation and "0.0.139" in release_presentation, "release presentation pins the Step-35 exact-source/instrumented-clone boundary and 0.0.138 physical provenance")

# ---------------------------------------------------------------------------
# Step 36.0 — controlled exact ExecuteEssential after physical exact Step-35 core closure.
# ---------------------------------------------------------------------------
step36_source_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitialization.cs"
step36_gate_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationGate.cs"
step36_result_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationGateResult.cs"
step36_progress_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationProgress.cs"
step36_summary_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationSummary.cs"
step36_sequence_path = ROOT / "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationGateSequence.cs"
step36_tests_path = ROOT / "tests/StS2Launcher.Core.Tests/Runtime/TransformedRealStS2EssentialInitializationTests.cs"
step36_ui_path = ROOT / "src/StS2Launcher.iOS/UI/RootViewController.TransformedRealStS2EssentialInitialization.cs"
for path, label in [
    (step36_source_path, "Step 36 ExecuteEssential core"),
    (step36_gate_path, "Step 36 gate enum"),
    (step36_result_path, "Step 36 gate result"),
    (step36_progress_path, "Step 36 progress model"),
    (step36_summary_path, "Step 36 summary"),
    (step36_sequence_path, "Step 36 gate sequence"),
    (step36_tests_path, "Step 36 host regressions"),
    (step36_ui_path, "Step 36 iOS surface"),
]:
    require(path.is_file(), f"{label} exists")
step36_source = step36_source_path.read_text() if step36_source_path.is_file() else ""
step36_gate = step36_gate_path.read_text() if step36_gate_path.is_file() else ""
step36_summary = step36_summary_path.read_text() if step36_summary_path.is_file() else ""
step36_sequence = step36_sequence_path.read_text() if step36_sequence_path.is_file() else ""
step36_tests = step36_tests_path.read_text() if step36_tests_path.is_file() else ""
step36_ui = step36_ui_path.read_text() if step36_ui_path.is_file() else ""
require(all(marker in step35_source for marker in ["partial class TransformedRealStS2VeryEarlyInitialization", "ResetStep36State();", "MarkExactStep35CoreClosurePassed(context);"]), "Step 35 exact core result explicitly hands authority forward to Step 36 without broadening its invocation path")
require(all(marker in step35_ui for marker in ["D_WORKER_SCHEDULE", "Task.Run(async () =>", "D_WORKER_RETURN", "D_TASK_AWAIT_RESUMED", "D_RESULT_RECORD_PASS"]), "Step 35.0.30 routes Gate-D completion through an outer worker and durably localizes worker/UI return")
require(all(marker in step36_gate for marker in ["ExactStep35ClosureAndStaticPreflight = 1", "ExactAuthorityContinuityAndBinding = 2", "ExecuteEssentialInvocation = 3", "FinalIsolationAudit = 4"]), "Step 36.0 defines exactly four ordered A-D gates")
require("STEP 36.0 ESSENTIAL INITIALIZATION COMPLETE — 4/4" in step36_summary and "Gates.Count(g => g.Passed)}/4" in step36_summary, "Step 36.0 summary requires ordered four-of-four completion")
require(all(marker in step36_source for marker in ["EssentialTargetMethodName = \"ExecuteEssential\"", "SourceEssentialTargetMethodToken = 0x06007D03", "ExpectedStateAfterVeryEarly = 1", "ExpectedStateAfterEssential = 2", "EssentialTargetMethodFullName"]), "Step 36.0 pins exact ExecuteEssential identity/token and state transition")
require(all(marker in step36_source for marker in ["RequireExactStep35CoreClosure", "IsExactAuthorityMode", "RequireStep36BaselineUnchanged", "_managedPluginReverseBridgePrepared"]), "Step 36.0 requires same-process exact Step-35 authority plus the proven bridge before any new boundary")
require(all(marker in step36_source for marker in ["RejectingAssemblyResolver", "FindMethodByToken", "ComputeMethodSemanticFingerprint", "CountForbiddenEssentialBoundaryCalls", "CountHarmonyMethodReferences", "Cecil dependency resolution"]), "Step 36 Gate A re-proves source/transformed ExecuteEssential semantics with rejecting Cecil resolvers")
require(all(marker in step36_source for marker in ["ExecuteVeryEarly", "ExecuteDeferred", "PrewarmJit", "forbidden.Contains(reference.Name)"]), "Step 36 Gate A rejects direct crossover into prior/later OneTimeInitialization boundaries")
require(all(marker in step36_source for marker in ["BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static", "method.MetadataToken", "ClosedStep32Mvid", "DiagnosticBridgeTypeFullName", "stateBefore != ExpectedStateAfterVeryEarly"]), "Step 36 Gate B binds only exact transformed ExecuteEssential and requires state 1 with no diagnostic bridge")
require(all(marker in step36_source for marker in ["E_C_INVOKE_START", "binding.Method.Invoke(null, null)", "E_C_INVOKE_RETURNED", "stateAfter != ExpectedStateAfterEssential", "InitializerBearingRequests", "RejectedManagedRequests", "NativeLoadAttempts", "E_C_PASS"]), "Step 36 Gate C invokes exact ExecuteEssential once, requires state 2, and fails closed on resolver/native escape")
require(all(marker in step36_source for marker in ["RunEssentialFinalIsolationAuditAsync", "OfflineReady", "ClosedStep32SourceSha256", "ClosedStep32TransformedSha256", "prepared.Plan.Sha1Hex", "FindLoadedStS2Assemblies", "E_D_FINAL_CHECKS_PASS", "E_D_TASK_RETURN_START"]), "Step 36 Gate D re-proves OfflineReady, authority/plan/dependency hashes, context ownership and final state")
require(all(marker in step36_source for marker in ["ExecuteDeferred / PrewarmJit / game entry point intentionally invoked by launcher: NO", "Harmony", "Native game resolution/loading: NO"]), "Step 36 explicitly keeps deferred/prewarm/entry/Harmony/native-game boundaries forbidden")
require(all(marker in step36_ui for marker in ["Step 36.0 — Controlled Exact ExecuteEssential", "ExactStep35CoreClosurePassed", "GodotCoreExactClosure", "RunEssentialStaticPreflight", "RunEssentialAuthorityBinding", "RunExactExecuteEssentialInvocation", "RunEssentialFinalIsolationAuditAsync"]), "Step 36 iOS surface exposes only the separately gated exact ExecuteEssential sequence after Step-35 exact closure")
require(all(marker in step36_ui for marker in ["E_D_WORKER_SCHEDULE", "Task.Run(async () =>", "E_D_WORKER_RETURN", "E_D_TASK_AWAIT_RESUMED", "RUN_STEP36_4OF4"]), "Step 36 Gate D uses the same outer-worker completion pattern as the Step-35 UI-return fix")
require(all(marker in step36_ui for marker in ["Step36-CrashCheckpoint-", "Step36-LastCheckpoint.txt", "Step36-ExecuteEssential-StaticMap-", "Step36-TransformedRealStS2EssentialInitialization.txt", "Flush(flushToDisk: true)"]), "Step 36 emits durable run-correlated checkpoint/static-map/final-report evidence")
require(all(marker in step36_tests for marker in ["GateSequenceCompletesFourOfFourInOrder", "GateSequenceRejectsOutOfOrderAdvance", "ExecuteEssentialAuthorityConstantsArePinned", "Step36GateOrdinalsAreStable", "0x06007D03u"]), "Step 36 host regressions pin gate order and exact ExecuteEssential authority constants")
require("ThrowsException" not in step36_tests, "Step 36 MSTest v4 regressions avoid removed ThrowsException APIs")
require("AddTransformedRealStS2EssentialInitializationControls(content)" in root_ui_text, "RootViewController wires Step 36 controls into the active device surface")
require("Step36ImplementationMarker" in release_presentation and "0x06007D03" in release_presentation and "ExecuteEssential" in release_presentation, "release presentation pins the Step-36 exact ExecuteEssential boundary")

# ---------------------------------------------------------------------------
# Documentation model
# ---------------------------------------------------------------------------
required_docs = [
    "README.md", "docs/README.md", "docs/MASTER-PLAN.md", "docs/CURRENT-STATUS.md", "docs/ARCHITECTURE.md",
    "docs/TESTING.md", "docs/REGRESSION-CONTRACTS.md", "docs/REPORTS.md", "docs/RELEASE-CHECKLIST.md", "docs/history/INDEX.md",
    "docs/history/reports/STEP-35.0.29-PHYSICAL-EXACT-AUTHORITY-CLOSURE-UI-RETURN-STALL-0.0.152.txt",
    "docs/history/steps/STEP-36.0-CONTROLLED-EXACT-EXECUTEESSENTIAL.md",
    "docs/history/steps/STEP-27.0.24-PHYSICAL-NEGATIVE-CLOSURE.md",
    "docs/history/steps/STEP-28-AHEAD-OF-LOAD-MANAGED-TRANSFORMATION.md",
    "docs/history/steps/STEP-28.0.1-CALLBACK-PROGRESS-COMPILE-FIX.md",
    "docs/history/reports/STEP-28.0.1-CODEMAGIC-HOST-TEST-FAILURE.txt",
    "docs/history/steps/STEP-28.0.2-DEFERRED-CECIL-METADATA-READ-FIX.md",
    "docs/history/steps/STEP-28.0.2-PHYSICAL-CLOSURE.md",
    "docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt",
    "docs/history/steps/STEP-29-REAL-STS2-COMPATIBILITY-TARGET-AUDIT.md",
    "docs/history/steps/STEP-29-TEST.md",
    "docs/history/steps/STEP-29.0-PHYSICAL-CLOSURE.md",
    "docs/history/reports/STEP-29.0-PHYSICAL-CLOSURE.txt",
    "docs/history/steps/STEP-30-SELECTED-HARMONY-TARGET-SEMANTIC-CONTEXT-AUDIT.md",
    "docs/history/steps/STEP-30-TEST.md",
    "docs/history/steps/STEP-30.0-PHYSICAL-CLOSURE.md",
    "docs/history/reports/STEP-30.0-PHYSICAL-CLOSURE.txt",
    "docs/history/steps/STEP-31-PREPAREMETHOD-SEMANTIC-CONTEXT-AUDIT.md",
    "docs/history/steps/STEP-31-TEST.md",
    "docs/history/steps/STEP-31.0-PHYSICAL-CLOSURE.md",
    "docs/history/reports/STEP-31.0-PHYSICAL-CLOSURE.txt",
    "docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt",
    "docs/history/steps/STEP-34.0-CONTROLLED-TRANSFORMED-PREWARMJIT-EXECUTION.md",
    "docs/history/steps/STEP-34.0-TEST.md",
    "docs/history/reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt",
    "docs/history/steps/STEP-35.0-CONTROLLED-VERY-EARLY-INITIALIZATION.md",
    "docs/history/steps/STEP-35.0-TEST.md",
    "docs/history/steps/STEP-35.0.2-EXECUTEVERYEARLY-INVOKE-CRASH-STATIC-ILCALLSITE-LOCALIZATION.md",
    "docs/history/reports/STEP-35.0.1-PHYSICAL-EXECUTEVERYEARLY-INVOKE-CRASH-LOCALIZATION.txt",
    "docs/history/reports/STEP-35.0.2-PHYSICAL-REPEATED-HARD-TERMINATION-AND-TELEMETRY-CORRELATION.txt",
    "docs/history/steps/STEP-35.0.3-RUN-CORRELATED-DURABLE-TELEMETRY.md",
    "docs/history/steps/STEP-32-FIRST-REAL-STS2-PREPAREMETHOD-REWRITE.md",
    "docs/history/steps/STEP-32-TEST.md",
    "docs/history/steps/STEP-32.0.1-SERIALIZED-FINGERPRINT-VERIFICATION-FIX.md",
    "docs/history/steps/STEP-32.0.1-TEST.md",
    "docs/history/steps/STEP-32.0.2-BOUNDED-CONSTANT-METADATA-WRITE-RESOLVER.md",
    "docs/history/steps/STEP-32.0.2-TEST.md",
    "docs/history/reports/STEP-32.0.1-PHYSICAL-CECIL-WRITE-RESOLUTION-FAILURE.txt",
    "docs/history/reports/STEP-32.0-CODEMAGIC-HOST-TEST-FAILURE.txt",
    "docs/history/reports/STEP-32.0-CODEMAGIC-STATIC-VALIDATION.txt",
    "docs/history/reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt",
    "docs/history/reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt",
    "docs/history/steps/STEP-22.4-CANONICAL-FOUNDATION.md",
    "docs/history/steps/STEP-22.4.2-STEP19-REGRESSION-CONTRACT-CORRECTION.md",
    "docs/history/steps/STEP-23-FIRST-REAL-CLR-LOAD.md",
    "docs/history/steps/STEP-23-TEST.md",
    "docs/history/steps/STEP-23.2-DETERMINISTIC-HOST-TEST-IDENTITY-ISOLATION.md",
    "docs/history/steps/STEP-23.3-SYNTHETIC-FIXTURE-PLAN-COVERAGE-FIX.md",
    "docs/history/steps/STEP-23.4-DEFERRED-DEPENDENCY-MODULE-INITIALIZER-BOUNDARY.md",
    "docs/history/steps/STEP-23.4.1-CECIL-IL-AUDIT-COMPILE-FIX.md",
    "docs/history/steps/STEP-23.4.2-SYNTHETIC-CORELIB-FIXTURE-NORMALIZATION.md",
    "docs/history/steps/STEP-23.4.3-CECIL-CORELIB-SCOPE-CONSTRUCTION-FIX.md",
    "docs/history/steps/STEP-24.0.3-CECIL-LOCAL-METADATA-RESOLUTION-FIX.md",
    "docs/history/steps/STEP-24.0.4-DEFERRED-TWO-PASS-METADATA-AUDIT-FIX.md",
    "docs/history/steps/STEP-24.0.5-CONDITIONAL-MONOMOD-LOGGING-DISPATCH.md",
    "docs/history/steps/STEP-24.0.6-SYSTEM-COLLECTIONS-CONCURRENT-PRESERVATION.md",
    "docs/history/steps/STEP-24.0.6-PHYSICAL-CLOSURE.md",
    "docs/history/steps/STEP-25-CONTROLLED-HARMONY-CONSTRUCTION.md",
    "docs/history/steps/STEP-25.0.1-HOST-LOCAL-ASSEMBLY-CLASSIFICATION-FIX.md",
    "docs/history/steps/STEP-25.0.2-HARMONY-CONSTRUCTOR-FRAMEWORK-PRESERVATION.md",
    "docs/history/steps/STEP-25.0.2-PHYSICAL-CLOSURE.md",
    "docs/history/steps/STEP-26-CONTROLLED-HARMONY-PROCESSOR-CREATION.md",
    "docs/history/steps/STEP-26.0-PHYSICAL-CLOSURE.md",
    "docs/history/steps/STEP-27-CONTROLLED-LAUNCHER-HARMONY-PATCH.md",
    "docs/history/steps/STEP-27.0.1-ACCESSTOOLS-TYPE-INITIALIZATION-BOUNDARY.md",
    "docs/history/steps/STEP-27.0.2-ACCESSTOOLS-MEASURED-INITIALIZER-PRESERVATION.md",
    "docs/history/steps/STEP-27.0.3-ACCESSTOOLS-PHYSICAL-FINGERPRINT-CORRECTION.md",
    "docs/history/steps/STEP-27.0.4-ACCESSTOOLS-OPERAND-ATTRIBUTION-CORRECTION.md",
    "docs/history/steps/STEP-27.0.5-CRASH-LOCALIZATION-AND-GATE-O-PURITY.md",
    "docs/history/steps/STEP-27.0.6-BOUNDED-IOS-PREFIX-DESCRIPTOR-REGISTRATION.md",
    "docs/history/steps/STEP-27.0.7-HARMONY-SHARED-STATE-INITIALIZATION-AND-PATCH-ENGINE-PRESERVATION.md",
    "docs/history/steps/STEP-27.0.8-GATE-O-PURITY-RESTORATION-AND-T-RUNTIME-RESOLUTION.md",
    "docs/history/steps/STEP-27.0.9-CRASH-CHECKPOINT-RELEASE-PROVENANCE-HARDENING.md",
    "docs/history/steps/STEP-27.0.11-IOS-HARMONYSHAREDSTATE-AOT-NORMALIZATION.md",
    "docs/history/steps/STEP-27.0.12-CECIL-OPCODES-COMPILE-HARDENING.md",
    "docs/history/steps/STEP-27.0.13-SYNTHETIC-PREFLIGHT-SCOPE-HARDENING.md",
    "docs/history/steps/STEP-27.0.14-DEFERRED-CECIL-NORMALIZATION-AND-REAL-HARMONY-CI-GATE.md",
    "docs/history/steps/STEP-27.0.15-REAL-HARMONY-TEST-NAMESPACE-COMPILE-HARDENING.md",
    "docs/history/steps/STEP-27.0.16-REAL-HARMONY-FAT-RELEASE-FIXTURE-HARDENING.md",
    "docs/history/steps/STEP-27.0.20-HASH-PINNED-REAL-HARMONY-NORMALIZER-EXECUTION.md",
    "docs/history/steps/STEP-27.0.21-RAW-METHOD-BODY-NORMALIZATION.md",
    "docs/history/steps/STEP-27.0.22-POST-PUBLISH-SYSTEM-LINQ-PRESERVATION.md",
    "docs/history/reports/STEP-27.0.21-PHYSICAL-T7-SYSTEM-LINQ-TRIM-FAILURE.txt",
    "docs/history/reports/STEP-27.0.20-CODEMAGIC-CECIL-WRITER-ENUM-CONSTANT-FAILURE.txt",
    "docs/history/reports/STEP-27.0.19-CODEMAGIC-DUPLICATE-SYSTEM-RUNTIME-ASSEMBLYREF-FAILURE.txt",
    "docs/history/reports/STEP-27.0.14-CODEMAGIC-TEST-COMPILE-FAILURE.txt",
    "docs/history/reports/STEP-27.0.15-CODEMAGIC-REAL-HARMONY-FIXTURE-ACQUISITION-FAILURE.txt",
    "docs/history/reports/STEP-27.0.13-PHYSICAL-GATE-A-REPORT.txt",
    "docs/history/reports/STEP-27.0.12-CODEMAGIC-HOST-TEST-FAILURE.txt",
    "docs/history/reports/STEP-27.0.11-CODEMAGIC-CS0104-HOST-COMPILE-FAILURE.txt",
    "docs/history/reports/STEP-27.0.10-PHYSICAL-GATE-T5-OBSERVER-CRASH-CHECKPOINT.txt",
    "docs/history/reports/STEP-27.0.7-PHYSICAL-GATE-O-REPORT.txt",
    "docs/history/reports/STEP-27.0.6-PHYSICAL-GATE-T-CRASH-CHECKPOINT.txt",
    "docs/history/reports/STEP-27.0.5-PHYSICAL-GATE-S-CRASH-CHECKPOINT.txt",
    "docs/history/reports/STEP-27.0.4-PHYSICAL-FRESH-PROCESS-GUARD-REPORT.txt",
    "docs/history/reports/STEP-27.0-PHYSICAL-GATE-R-REPORT.txt",
    "docs/history/reports/STEP-27.0.1-PHYSICAL-GATE-O-REPORT.txt",
    "docs/history/reports/STEP-27.0.2-PHYSICAL-GATE-O-REPORT.txt",
    "docs/history/reports/STEP-27.0.3-PHYSICAL-GATE-O-REPORT.txt",
    "docs/history/reports/STEP-25.0.1-PHYSICAL-GATE-H-REPORT.txt",
    "docs/history/reports/STEP-24.0.5-PHYSICAL-GATE-C-REPORT.txt",
    "docs/history/reports/STEP-24.0.2-PHYSICAL-GATE-A-REPORT.txt",
    "docs/history/reports/STEP-24.0.3-PHYSICAL-GATE-A-REPORT.txt",
    "docs/history/reports/STEP-24.0.4-PHYSICAL-GATE-A-REPORT.txt",
    "docs/history/steps/STEP-23.4.3-PHYSICAL-CLOSURE.md",
    "docs/history/steps/STEP-24-CONTROLLED-MANAGED-INITIALIZATION.md",
    "docs/history/steps/STEP-24.0.1-OFFLINEREADY-API-COMPILE-FIX.md",
    "docs/history/steps/STEP-24.0.2-PINVOKE-AUDIT-FIX.md",
]
for doc in required_docs:
    require((ROOT / doc).is_file(), f"authoritative documentation exists: {doc}")

step24_build78_report = read("docs/history/reports/STEP-24.0.5-PHYSICAL-GATE-C-REPORT.txt")
require("CONTROLLED MANAGED INITIALIZATION BOUNDARY FAIL — 2/4" in step24_build78_report and "Gate A — InitializationPreflight: PASS" in step24_build78_report and "Gate B — ProvenLoadStateReplay: PASS" in step24_build78_report and "Gate C — DeferredModuleInitialization: FAIL" in step24_build78_report, "physical build 78 report preserves the exact 2/4 gate result")
require("System.MissingMethodException: Method not found: void System.Collections.Concurrent.ConcurrentBag`1..ctor()" in step24_build78_report, "physical build 78 report preserves the ConcurrentBag constructor failure")
step25_build81_report = read("docs/history/reports/STEP-25.0.1-PHYSICAL-GATE-H-REPORT.txt")
require("CONTROLLED HARMONY CONSTRUCTION BOUNDARY FAIL — 7/9" in step25_build81_report and "Gate G — HarmonyTypeInitializationAudit: PASS" in step25_build81_report and "Gate H — HarmonyInstanceConstruction: FAIL" in step25_build81_report, "physical build 81 report preserves the exact 7/9 gate result")
require("System.MissingMethodException: Method not found: System.Version System.Environment.get_Version()" in step25_build81_report, "physical build 81 report preserves the Environment.Version constructor failure")
step27_build84_report = read("docs/history/reports/STEP-27.0-PHYSICAL-GATE-R-REPORT.txt")
require("CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 17/25" in step27_build84_report and "Gate Q — BaselineProbeInvocation: PASS" in step27_build84_report and "Gate R — PrefixRegistration: FAIL" in step27_build84_report, "physical build 84 report preserves the exact Step 27 17/25 gate result")
require("TypeInitialization_Type, HarmonyLib.AccessTools" in step27_build84_report and "PatchProcessor.AddPrefix" in step27_build84_report, "physical build 84 report preserves the implicit AccessTools initialization failure before Patch()")
step27_build85_report = read("docs/history/reports/STEP-27.0.1-PHYSICAL-GATE-O-REPORT.txt")
require("CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 14/26" in step27_build85_report and "Gate O — HarmonyPatchApiResolution: FAIL" in step27_build85_report and "Blocking AccessTools initializer hazards: 46" in step27_build85_report, "physical build 85 report preserves the exact Gate-O 14/26 metadata result")
require("System.Runtime.InteropServices.RuntimeInformation" in step27_build85_report and "ReaderWriterLockSlim" in step27_build85_report and "addHandlerCache" in step27_build85_report, "physical build 85 report preserves the measured AccessTools runtime-detection/cache surface")
step27_build86_report = read("docs/history/reports/STEP-27.0.2-PHYSICAL-GATE-O-REPORT.txt")
require("CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 14/26" in step27_build86_report and "Gate O — HarmonyPatchApiResolution: FAIL" in step27_build86_report and "Type initializer instructions: 57 (expected 56)" in step27_build86_report, "physical build 86 report preserves the exact Gate-O 14/26 corrected instruction-count evidence")
require("contains unexpected opcode Ldc_I4_1 (1 occurrence(s))" in step27_build86_report and "Gate R explicitly" in step27_build86_report, "physical build 86 report preserves the single newly exposed ldc.i4.1 and confirms no later gate ran")
step27_build87_report = read("docs/history/reports/STEP-27.0.3-PHYSICAL-GATE-O-REPORT.txt")
require("CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 14/26" in step27_build87_report and "Gate O — HarmonyPatchApiResolution: FAIL" in step27_build87_report and "Type initializer instructions: 57 (expected 57)" in step27_build87_report, "physical build 87 report preserves the exact Gate-O 14/26 stable instruction-count evidence")
require("expected true then false" in step27_build87_report and "Blocking AccessTools initializer hazards: 1" in step27_build87_report, "physical build 87 report preserves the operand-attribution failure that motivated Step 27.0.4")
step27_build88_guard_report = read("docs/history/reports/STEP-27.0.4-PHYSICAL-FRESH-PROCESS-GUARD-REPORT.txt")
require("CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 0/26" in step27_build88_guard_report and "first failure: InitializationPreflight" in step27_build88_guard_report and "already loaded: sts2" in step27_build88_guard_report, "physical build 88 report preserves the expected stale-process Gate-A rejection")
step27_build89_checkpoint = read("docs/history/reports/STEP-27.0.5-PHYSICAL-GATE-S-CRASH-CHECKPOINT.txt")
require("Gate: S — PrefixRegistration" in step27_build89_checkpoint and "S1 — entering exact PatchProcessor.AddPrefix(MethodInfo) reflection invocation" in step27_build89_checkpoint, "physical build 89 crash checkpoint localizes abrupt termination inside AddPrefix before S2/Patch()")
step27_build90_checkpoint = read("docs/history/reports/STEP-27.0.6-PHYSICAL-GATE-T-CRASH-CHECKPOINT.txt")
require("Gate: T — PatchEngineExecution" in step27_build90_checkpoint and "T1 — entering the first exact PatchProcessor.Patch() reflection invocation" in step27_build90_checkpoint and "launcher target is still not invoked" in step27_build90_checkpoint, "physical build 90 crash checkpoint localizes abrupt termination inside public Patch() before T2/target invocation")
step27_build91_report = read("docs/history/reports/STEP-27.0.7-PHYSICAL-GATE-O-REPORT.txt")
require("CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 14/26" in step27_build91_report and "App version: 0.0.91 (91)" in step27_build91_report and "Gate N — PostProcessorAudit: PASS" in step27_build91_report and "Gate O — HarmonyPatchApiResolution: FAIL" in step27_build91_report, "physical build 91 report preserves the exact clean 14/26 Gate-O regression")
require("Targeted patch API reflection unexpectedly changed resolver/load counters" in step27_build91_report and "Gate T decomposes" in step27_build91_report, "physical build 91 report preserves the resolver/load-counter failure before any Gate-T execution")
step27_build93_checkpoint = read("docs/history/reports/STEP-27.0.9-PHYSICAL-GATE-T5-CRASH-CHECKPOINT.txt")
require("App version: 0.0.93 (93)" in step27_build93_checkpoint and "Expected source version: 0.0.93 (93)" in step27_build93_checkpoint and "Gate: T — PatchEngineExecution" in step27_build93_checkpoint, "physical build 93 checkpoint preserves self-identifying release provenance and the Gate-T frontier")
require("T5 — entering explicit RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)" in step27_build93_checkpoint and "PatchProcessor.Patch() and launcher target remain uninvoked" in step27_build93_checkpoint, "physical build 93 checkpoint proves T1-T4 crossed and localizes abrupt termination inside HarmonySharedState cctor before Patch()/target invocation")
step27_build94_checkpoint = read("docs/history/reports/STEP-27.0.10-PHYSICAL-GATE-T5-OBSERVER-CRASH-CHECKPOINT.txt")
require("App version: 0.0.94 (94)" in step27_build94_checkpoint and "Expected source version: 0.0.94 (94)" in step27_build94_checkpoint and "Gate: T — PatchEngineExecution" in step27_build94_checkpoint, "physical build 94 checkpoint preserves self-identifying release provenance and the original-cctor Gate-T frontier")
require("T5 observer — dedicated ALC: host load completed: netstandard, Version=2.0.0.0" in step27_build94_checkpoint and "=> netstandard, Version=2.1.0.0" in step27_build94_checkpoint and "PatchProcessor.Patch() and launcher target remain uninvoked" in step27_build94_checkpoint, "physical build 94 checkpoint proves host netstandard binding completed inside the original cctor before the hard stop")
step27_build96_host = read("docs/history/reports/STEP-27.0.12-CODEMAGIC-HOST-TEST-FAILURE.txt")
require("Build succeeded." in step27_build96_host and "Test Run Failed." in step27_build96_host and "Total tests: 211" in step27_build96_host and "Passed: 209" in step27_build96_host and "Failed: 2" in step27_build96_host, "Codemagic build 96 evidence proves compilation succeeded and exactly 2/211 host tests failed")
require("SyntheticStep26ReplayThroughEmptyProcessorStillPassesBeforePatchBoundary" in step27_build96_host and "GateCReportsThrowingModuleInitializerAndDoesNotAdvance" in step27_build96_host and "One or more exact Harmony patch-engine internal types are missing." in step27_build96_host, "Codemagic build 96 evidence localizes both failures to production normalizer overreach into minimal synthetic fixtures")
step27_build97_report = read("docs/history/reports/STEP-27.0.13-PHYSICAL-GATE-A-REPORT.txt")
require("App version: 0.0.97 (97)" in step27_build97_report and "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 0/26" in step27_build97_report and "first failure: InitializationPreflight" in step27_build97_report, "physical build 97 evidence preserves the exact 0/26 Gate-A failure")
require("HarmonySharedState iOS runtime-image normalization" in step27_build97_report and "System.ComponentModel.EditorBrowsableState" in step27_build97_report and "ImmediateModuleReader" in step27_build97_report and "CreateIosNormalizedHarmonyRuntimeImage" in step27_build97_report, "physical build 97 evidence localizes the regression to eager Cecil custom-attribute decoding inside the normalizer")
step27_build98_host = read("docs/history/reports/STEP-27.0.14-CODEMAGIC-TEST-COMPILE-FAILURE.txt")
require("StS2Launcher.Core ->" in step27_build98_host and "error CS0104" in step27_build98_host and "ICustomAttributeProvider" in step27_build98_host and "Mono.Cecil.ICustomAttributeProvider" in step27_build98_host and "System.Reflection.ICustomAttributeProvider" in step27_build98_host, "Codemagic build 98 evidence proves production core compiled and localizes the test compile stop to ambiguous ICustomAttributeProvider")
require("ControlledHarmonyPatchExecutionTests.cs(141,34)" in step27_build98_host and "error CS1503" in step27_build98_host, "Codemagic build 98 preserves the exact primary and follow-on real-Harmony test compiler diagnostics")
step27_build99_host = read("docs/history/reports/STEP-27.0.15-CODEMAGIC-REAL-HARMONY-FIXTURE-ACQUISITION-FAILURE.txt")
require("StS2Launcher.Core ->" in step27_build99_host and "StS2Launcher.Core.Tests ->" in step27_build99_host and "CopyStep27RealHarmonyNormalizerFixture" not in step27_build99_host, "Codemagic build 99 evidence proves both production and test assemblies compiled before fixture acquisition stopped the run")
require("StS2Launcher.Core.Tests.csproj(34,5): error" in step27_build99_host and "requires exact merged Lib.Harmony 2.4.2 netstandard2.0/0Harmony.dll as a quarantined host-test fixture" in step27_build99_host, "Codemagic build 99 evidence localizes the stop to the brittle real-Harmony fixture-path contract before MSTest execution")
step27_build100_host = read("docs/history/reports/STEP-27.0.16-CODEMAGIC-HARMONY-FAT-ARCHIVE-MEMBER-FAILURE.txt")
require("Acquiring exact official Harmony-Fat 2.4.2 host regression fixture..." in step27_build100_host and "archive must contain exactly one netstandard2.0/0Harmony.dll; found 0" in step27_build100_host, "Codemagic build 100 evidence proves the official archive download reached member inspection and the root-exact selector found zero candidates")
require("Building external managed fixtures once for host tests" not in step27_build100_host and "Test run for" not in step27_build100_host and "HOST UNIT TESTS: PASS" not in step27_build100_host, "Codemagic build 100 evidence proves the stop occurred before build/MSTest rather than in production normalization")
step27_build101_host = read("docs/history/reports/STEP-27.0.17-CODEMAGIC-HARMONY-FAT-NETSTANDARD-ABSENCE.txt")
require(".NET: 9.0.314" in step27_build101_host and "archive must contain exactly one member ending in /netstandard2.0/0Harmony.dll; found 0" in step27_build101_host and "net9.0/0Harmony.dll" in step27_build101_host and "net10.0/0Harmony.dll" in step27_build101_host, "Codemagic build 101 evidence proves the official fat archive has no netstandard implementation and does contain concrete net9/net10 merged implementations")
require("Building external managed fixtures once for host tests" not in step27_build101_host and "Test run for" not in step27_build101_host and "HOST UNIT TESTS: PASS" not in step27_build101_host, "Codemagic build 101 evidence proves the stop occurred before build/MSTest and not inside production normalization")
step27_build102_host = read("docs/history/reports/STEP-27.0.18-CODEMAGIC-NET9-SURROGATE-REFERENCE-ASSERTION-FAILURE.txt")
require("Harmony archive member: net9.0/0Harmony.dll" in step27_build102_host and "Harmony fixture SHA-256:" in step27_build102_host and "Build succeeded." in step27_build102_host and "Total tests: 212" in step27_build102_host and "Passed: 211" in step27_build102_host and "Failed: 1" in step27_build102_host, "Codemagic build 102 evidence proves the official net9 surrogate reached the compiled 212-test host suite at 211/212")
require("OfficialHarmony242Net9FatNormalizerUsesDeferredMetadataAndPreservesSourceBytes" in step27_build102_host and "must be the net9.0 implementation, not a netstandard reference surface" in step27_build102_host and "actual: true" in step27_build102_host, "Codemagic build 102 evidence localizes the only failure to the invalid no-netstandard-reference test assertion")
require("Real Harmony 2.4.2 normalization failed:" not in step27_build102_host, "Codemagic build 102 did not report a production-normalizer exception before the test-only assertion stopped the regression")
step27_build103_host = read("docs/history/reports/STEP-27.0.19-CODEMAGIC-DUPLICATE-SYSTEM-RUNTIME-ASSEMBLYREF-FAILURE.txt")
require("Harmony archive member: net9.0/0Harmony.dll" in step27_build103_host and "Harmony archive SHA-256: a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774" in step27_build103_host and "Harmony fixture SHA-256: a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab" in step27_build103_host and "Total tests: 212" in step27_build103_host and "Passed: 211" in step27_build103_host and "Failed: 1" in step27_build103_host, "Codemagic build 103 evidence pins the exact official fixture bytes and proves the compiled 212-test suite reached 211/212")
require("Sequence contains more than one matching element" in step27_build103_host and "ControlledHarmonyPatchExecutionTests.cs:line 119" in step27_build103_host and "SingleOrDefault" in step27_build103_host and "Real Harmony 2.4.2 normalization failed:" not in step27_build103_host, "Codemagic build 103 localizes the sole failure to duplicate System.Runtime AssemblyRef handling before the production normalizer call")
step27_build104_host = read("docs/history/reports/STEP-27.0.20-CODEMAGIC-CECIL-WRITER-ENUM-CONSTANT-FAILURE.txt")
require("executed all 212 host tests" in step27_build104_host and "211 passed / 1 failed" in step27_build104_host and "Real Harmony 2.4.2 normalization failed:" in step27_build104_host, "Codemagic build 104 evidence proves the hash-pinned real Harmony surrogate reached the production normalizer in the 212-test host suite")
require("System.Reflection.BindingFlags" in step27_build104_host and "MetadataBuilder.GetConstantType" in step27_build104_host and "Mono.Cecil.ModuleDefinition.Write" in step27_build104_host and "CreateIosNormalizedHarmonyRuntimeImage" in step27_build104_host, "Codemagic build 104 evidence localizes the genuine failure to Cecil whole-module enum-constant serialization")
step27_build105_report = read("docs/history/reports/STEP-27.0.21-PHYSICAL-T7-SYSTEM-LINQ-TRIM-FAILURE.txt")
require("App version: 0.0.105 (105)" in step27_build105_report and "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 19/26" in step27_build105_report and "Gate T — PatchEngineExecution: FAIL" in step27_build105_report, "physical build 105 evidence preserves the exact 19/26 Gate-T failure")
require("HarmonySharedState iOS runtime-image normalization: PASS" in step27_build105_report and "Runtime dynamic HarmonySharedState singleton creation/ReflectionHelper.Load/StackFrame FieldRefAccess initialization: REMOVED FROM NORMALIZED CCTOR" in step27_build105_report and "Prepared/source/live file mutation: NO" in step27_build105_report, "physical build 105 proves raw-body HarmonySharedState normalization completed without source mutation")
require("System.Linq.Enumerable.Union" in step27_build105_report and "HarmonyLib.MethodCreator..ctor" in step27_build105_report and "HarmonyLib.PatchFunctions.UpdateWrapper" in step27_build105_report and "HarmonyLib.PatchProcessor.Patch" in step27_build105_report, "physical build 105 localizes the current blocker to a trimmed LINQ member during Harmony replacement construction")
step27_build106_report = read("docs/history/reports/STEP-27.0.22-PHYSICAL-DYNAMICMETHODDEFINITION-DEBUGGABLEATTRIBUTE-TRIM-FAILURE.txt")
require("App version: 0.0.106 (106)" in step27_build106_report and "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 19/26" in step27_build106_report and "Gate T — PatchEngineExecution: FAIL" in step27_build106_report, "physical build 106 evidence preserves the exact 19/26 Gate-T failure")
require("System.Diagnostics.DebuggableAttribute" in step27_build106_report and "MonoMod.Utils.DynamicMethodDefinition" in step27_build106_report and "HarmonyLib.MethodPatcherTools.CreateDynamicMethod" in step27_build106_report and "HarmonyLib.PatchProcessor.Patch" in step27_build106_report, "physical build 106 proves the LINQ fix advanced Patch() into DynamicMethodDefinition before the next BCL trim failure")
require("PatchTools.DetourMethod: DetourFactory.Current.CreateDetour — PRESENT" in step27_build106_report and "at HarmonyLib.PatchTools.DetourMethod" not in step27_build106_report, "physical build 106 still does not establish actual MonoMod detour execution")
step27_build107_report = read("docs/history/reports/STEP-27.0.23-PHYSICAL-NOTIMPLEMENTED-PATCHENGINE.txt")
require("App version: 0.0.107 (107)" in step27_build107_report and "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY FAIL — 19/26" in step27_build107_report and "Gate T — PatchEngineExecution: FAIL" in step27_build107_report, "physical build 107 evidence preserves the exact 19/26 Gate-T result under copy/no-link")
require("System.NotImplementedException: Arg_NotImplementedException" in step27_build107_report and "at HarmonyLib.PatchFunctions.UpdateWrapper" in step27_build107_report and "at HarmonyLib.PatchProcessor.Patch" in step27_build107_report, "physical build 107 proves trimming ambiguity is gone and localizes the next blocker to real Patch() execution")
require("System.Linq.Enumerable.Union" not in step27_build107_report[step27_build107_report.index("Gate T — PatchEngineExecution: FAIL"): ] and "System.Diagnostics.DebuggableAttribute" not in step27_build107_report[step27_build107_report.index("Gate T — PatchEngineExecution: FAIL"): ], "physical build 107 Gate-T failure is no longer either known trimmed-framework-member failure")
regression_contracts = read("docs/REGRESSION-CONTRACTS.md")
require("Step 27 is physically closed as a **negative architecture result** by 0.0.108" in regression_contracts and "runtime Harmony/MonoMod method replacement remains retired" in regression_contracts, "regression contracts preserve the decisive Step-27 negative physical result and retire runtime patching")
require("## Step 28 — ahead-of-load managed transformation" in regression_contracts and "Target(41)==1041" in regression_contracts and "loads only the verified transformed bytes" in regression_contracts and "real StS2 member reflection/transformation/invocation" in regression_contracts, "regression contracts define the five-gate Step-28 transform-before-load capability boundary")
step28_build110_report = read("docs/history/reports/STEP-28.0.1-CODEMAGIC-HOST-TEST-FAILURE.txt")
require("Test Run Failed." in step28_build110_report and "Total tests: 217" in step28_build110_report and "Passed: 216" in step28_build110_report and "Failed: 1" in step28_build110_report, "Codemagic 0.0.110 evidence preserves the exact 216/217 host result")
require("VerifiedSourceIsRewrittenBeforeLoadAndOnlyTransformedBehaviorExecutes" in step28_build110_report and "System.Runtime, Version=9.0.0.0" in step28_build110_report and "AssemblyResolutionException" in step28_build110_report and "ReadFixtureModule" in step28_build110_report, "Codemagic 0.0.110 evidence localizes the sole host failure to Gate-A Cecil framework metadata resolution")
require("Step 28.0.2 metadata-only Cecil admission contract" in regression_contracts and "ReadingMode.Deferred" in regression_contracts and "ReadingMode.Immediate" in regression_contracts and "rejecting resolver" in regression_contracts, "regression contracts pin the deferred metadata-only correction without broadening resolution")
require("Step 28.0.2 physical-closure contract" in regression_contracts and "5/5 PASS" in regression_contracts and "428/428" in regression_contracts, "regression contracts protect the positive physical Step-28 closure")
step28_physical_report = read("docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt")
require("AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY PASS — 5/5" in step28_physical_report and "App version: 0.0.111 (111)" in step28_physical_report and "Adjustment() result: 1000" in step28_physical_report and "Target(41) reflection result: 1041" in step28_physical_report and "InvokeTarget(41) in-fixture direct-call result: 1041" in step28_physical_report and "Post-execution OfflineReady: PASS (428/428 files)" in step28_physical_report, "raw Step-28 physical report preserves the decisive 5/5 transformed-execution values")
require("Exactly one Step-28 fixture identity CLR-loaded: YES — transformed image only" in step28_physical_report and "Trusted Step 12 managed install unchanged: YES" in step28_physical_report and "Unexpected private dependency resolution: NO" in step28_physical_report, "raw Step-28 physical report preserves transformed-only admission, immutable source, and fail-closed isolation")
step29_physical_report = read("docs/history/reports/STEP-29.0-PHYSICAL-CLOSURE.txt")
require("REAL STS2 COMPATIBILITY TARGET AUDIT PASS — 4/4" in step29_physical_report and "App version: 0.0.112 (112)" in step29_physical_report and "Source SHA-256: e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18" in step29_physical_report and "Source method token: 0x06007927" in step29_physical_report and "IL offset/opcode: IL_0D9D / Callvirt" in step29_physical_report and "HarmonyLib.Harmony::PatchAll(System.Reflection.Assembly)" in step29_physical_report and "Post-audit OfflineReady: PASS (428/428 files)" in step29_physical_report, "raw Step-29 physical report preserves decisive 4/4 exact-target evidence")
require("## Step 29 — real StS2 compatibility target audit" in regression_contracts and "physically closed at **4/4 PASS**" in regression_contracts and "audit evidence only" in regression_contracts, "regression contracts protect the closed Step-29 target-selection evidence")
require("## Step 30 — selected Harmony target semantic context audit" in regression_contracts and "NO BASE-GAME REWRITE AUTHORIZED" in regression_contracts and "no behavior change" in regression_contracts, "regression contracts define Step 30 as read-only semantic context/product-scope disposition")
step30_physical_report = read("docs/history/reports/STEP-30.0-PHYSICAL-CLOSURE.txt")
require("SELECTED TARGET SEMANTIC CONTEXT AUDIT PASS — 4/4" in step30_physical_report and "App version: 0.0.113 (113)" in step30_physical_report and "DEFER — MOD/HARMONY COMPATIBILITY PATH; NO BASE-GAME REWRITE AUTHORIZED" in step30_physical_report and "Post-audit OfflineReady: PASS (428/428 files)" in step30_physical_report, "raw Step-30 physical report preserves decisive 4/4 product-scope disposition evidence")
require("Cecil writes performed by Step 30: 0" in step30_physical_report and "sts2 assembly/type/member CLR load or invocation by Step 30: NO" in step30_physical_report and "Cecil dependency resolution requests across audit: 0" in step30_physical_report, "raw Step-30 physical report preserves read-only/no-resolution isolation")
require("## Step 31 — PrepareMethod semantic context audit" in regression_contracts and "0x06007D05" in regression_contracts and "NO WRITE AUTHORIZED" in regression_contracts and "closed positive 4/4" in regression_contracts.lower(), "regression contracts protect Step 31 physical PrepareMethod semantic-audit closure")

step32_host_failure = read("docs/history/reports/STEP-32.0-CODEMAGIC-HOST-TEST-FAILURE.txt")
step32_static_failure = read("docs/history/reports/STEP-32.0-CODEMAGIC-STATIC-VALIDATION.txt")
require("Total tests: 231" in step32_host_failure and "Passed: 230" in step32_host_failure and "Failed: 1" in step32_host_failure and "ExactPrewarmJitPrepareMethodFamilyIsRewrittenOnPrivateCopyOnly" in step32_host_failure and "reopened transformed PrewarmJit does not match the exact in-memory predeclared rewrite" in step32_host_failure, "raw 0.0.115 host report preserves the single Step-32 Gate-C failure at 230/231")
require("VALIDATION PASS: 996 checks" in step32_static_failure, "raw 0.0.115 static report preserves 996/996 PASS")
require("## Step 32.0.1 — serialized-fingerprint verification correction" in regression_contracts and "ExpectedTransformedBodySha256" in regression_contracts and "post-write physical IL evidence" in regression_contracts, "regression contracts pin the Step-32.0.1 serialization fingerprint correction")
step32_physical_write_failure = read("docs/history/reports/STEP-32.0.1-PHYSICAL-CECIL-WRITE-RESOLUTION-FAILURE.txt")
require("REAL STS2 PREPAREMETHOD REWRITE FAIL — 1/4" in step32_physical_write_failure and "App version: 0.0.116 (116)" in step32_physical_write_failure and "Gate A — SourceAdmissionAndPrivateClone: PASS" in step32_physical_write_failure and "Gate B — DeterministicStackNeutralRewrite: FAIL" in step32_physical_write_failure and "MetadataBuilder.GetConstantType" in step32_physical_write_failure and "System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" in step32_physical_write_failure, "raw physical 0.0.116 report preserves the Gate-B Cecil Constant-table resolution boundary")
require("## Step 32.0.2 — bounded Cecil write-time constant-metadata resolver" in regression_contracts and "write-only in-memory" in regression_contracts and "open zero external framework/game assembly bytes" in regression_contracts and "Constant-table" in regression_contracts, "regression contracts pin the Step-32.0.2 bounded serialization resolver correction")

step32_physical_gatec_failure = read("docs/history/reports/STEP-32.0.4-PHYSICAL-GATE-C-TRANSFORMED-METHOD-IDENTITY-FAILURE.txt")
require("REAL STS2 PREPAREMETHOD REWRITE FAIL — 2/4" in step32_physical_gatec_failure and "App version: 0.0.119 (119)" in step32_physical_gatec_failure and "Gate A — SourceAdmissionAndPrivateClone: PASS" in step32_physical_gatec_failure and "Gate B — DeterministicStackNeutralRewrite: PASS" in step32_physical_gatec_failure and "Gate C — TransformedImageVerification: FAIL" in step32_physical_gatec_failure and "Cecil write-time resolution requests: 9" in step32_physical_gatec_failure and "Transformed SHA-256: 39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef" in step32_physical_gatec_failure and "Step-32 transformed PrewarmJit method identity/body drifted" in step32_physical_gatec_failure, "raw physical 0.0.119 report preserves the Step-32 2/4 Gate-B success and Gate-C transformed-method locator boundary")

current_status = read("docs/CURRENT-STATUS.md")
require(all(marker in current_status for marker in ["Active candidate — Step 35.0.30 / Step 36.0 / 0.0.153 (153)", "positive exact Step-35 core closure", "0.0.152", "D_TASK_RETURN_START", "D_WORKER_SCHEDULE", "0x06007D03", "ExecuteEssential", "ExecuteDeferred", "PrewarmJit"]), "current status records physical 0.0.152 exact Step-35 core closure, UI-return fix, and active Step-36 boundary")
testing_doc = read("docs/TESTING.md")
release_checklist_doc = read("docs/RELEASE-CHECKLIST.md")
require("0.0.153 (153)" in testing_doc and "0.0.153 (153)" in release_checklist_doc, "testing and release-checklist docs pin the exact 0.0.153 (153) release identity")
require("0.0.140 (139)" not in testing_doc and "0.0.140 (138)" not in testing_doc and "0.0.140 (139)" not in release_checklist_doc and "0.0.140 (138)" not in release_checklist_doc, "release docs reject display/build-number drift like the prior 0.0.130 (129) documentation bug")

master = read("docs/MASTER-PLAN.md")
for heading in ["Product objective", "Non-negotiable security and content boundaries", "Authority model", "Canonical source architecture", "Major roadmap", "Definition of a closed step", "Resumption rule"]:
    require(heading in master, f"master plan contains durable section: {heading}")
require("docs/CURRENT-STATUS.md" in master and "docs/REGRESSION-CONTRACTS.md" in master and "docs/history/INDEX.md" in master, "master plan defines self-contained resumption path")
require("Step 23" in master and "Steps 24–26" in master and "physical 0.0.108" in master and "closed runtime Harmony/MonoMod replacement as a negative architecture result" in master and "MtouchLink=None" in master and "TrimMode=copy" in master and "Step 28" in master and "5/5" in master and "Step 29.0 / 0.0.112" in master and "mod-loading path" in master and "semantic-context/product-scope audit" in master, "master plan preserves Step-27 negative, Step-28 positive, Step-29 target-selection closure, and Step-30 semantic-audit frontier")
require("several adjacent sequential gates" in master and "saves build/device cycles" in master, "master plan codifies gate batching for speed without losing discrete proof")
require("https://github.com/Ekyso/StS2-Launcher" in master and "https://github.com/SocialHummingbird/StS2-Launcher-Overhaul" in master and "advisory references only" in master, "master plan records both Android StS2 reference implementations as advisory/non-authoritative inputs")
require("MtouchLink=None" in master and "TrimMode=copy" in master and "publish-time trimming failures" in master and "runtime Harmony/MonoMod method replacement is **not** an active" in master and "deterministic ahead-of-load" in master, "master plan records both the copy/no-link dynamic-payload correction and the physically justified retirement of runtime Harmony replacement")
require("global `TrimMode=full`" not in master, "master plan no longer protects the disproven full-trim dynamic-payload policy")

top_level_step_docs = [p.name for p in (ROOT / "docs").glob("STEP-*.md")]
require(not top_level_step_docs, "top-level docs are durable/current; step records live under docs/history/steps", ", ".join(top_level_step_docs))

history_steps = list((ROOT / "docs/history/steps").glob("*.md"))
history_names = [p.name for p in history_steps]
for major in range(1, 36):
    prefix = f"STEP-{major:02d}"
    require(any(name.startswith(prefix) for name in history_names), f"readable historical documentation retained for Step {major:02d}")
require(any(name.startswith("STEP-22.4") for name in history_names), "Step 22.4 design/history record is present")
require(any(name.startswith("STEP-23") for name in history_names), "Step 23 design/test/closure records are present")
require(any(name.startswith("STEP-24") for name in history_names), "Step 24 design/candidate/closure records are present")
require(any(name.startswith("STEP-25") for name in history_names), "Step 25 design/candidate/closure records are present")
require(any(name.startswith("STEP-26") for name in history_names), "Step 26 design/closure records are present")
require(any(name.startswith("STEP-27") for name in history_names), "Step 27 design/candidate/negative-closure record is present")
require(any(name.startswith("STEP-28") for name in history_names), "Step 28 ahead-of-load design/closure record is present")
require(any(name.startswith("STEP-29") for name in history_names), "Step 29 target-audit design/test/closure record is present")
require(any(name.startswith("STEP-30") for name in history_names), "Step 30 semantic-audit design/test/closure record is present")
require(any(name.startswith("STEP-31") for name in history_names), "Step 31 PrepareMethod semantic-audit design/test/closure record is present")
require(any(name.startswith("STEP-32") for name in history_names), "Step 32 first real StS2 rewrite design/test/closure record is present")
require(any(name.startswith("STEP-33") for name in history_names), "Step 33 transformed-admission design/test/closure record is present")
require(any(name.startswith("STEP-34") for name in history_names), "Step 34 controlled-execution design/test/closure record is present")
require(any(name.startswith("STEP-35") for name in history_names), "Step 35 very-early initialization design/test record is present")
require((ROOT / "docs/history/steps/STEP-35.0.1-B-C-HARD-TERMINATION-CRASH-LOCALIZATION.md").is_file(), "Step 35.0.1 crash-localization history record is present")
require((ROOT / "docs/history/reports/STEP-35.0-PHYSICAL-HARD-TERMINATION-SUMMARY.txt").is_file(), "sanitized Step-35.0 physical hard-termination summary is present")
require((ROOT / "docs/history/reports/STEP-35.0.1-PHYSICAL-EXECUTEVERYEARLY-INVOKE-CRASH-LOCALIZATION.txt").is_file(), "sanitized Step-35.0.1 physical invoke-crash localization summary is present")
require((ROOT / "docs/history/steps/STEP-35.0.2-EXECUTEVERYEARLY-INVOKE-CRASH-STATIC-ILCALLSITE-LOCALIZATION.md").is_file(), "Step 35.0.2 static IL/callsite localization history record is present")
require((ROOT / "docs/history/reports/STEP-35.0.2-PHYSICAL-REPEATED-HARD-TERMINATION-AND-TELEMETRY-CORRELATION.txt").is_file(), "Step 35.0.2 repeated hard-termination/correlation summary is present")
require((ROOT / "docs/history/steps/STEP-35.0.3-RUN-CORRELATED-DURABLE-TELEMETRY.md").is_file(), "Step 35.0.3 run-correlated telemetry design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.3-PHYSICAL-SAME-RUN-CORRELATION-AND-INVOKE-FRONTIER.txt").is_file(), "Step 35.0.3 physical same-run correlation report is present")
require((ROOT / "docs/history/steps/STEP-35.0.4-IN-METHOD-PRE-FIRST-AWAIT-LOCALIZATION.md").is_file(), "Step 35.0.4 in-method localization design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.4-PHYSICAL-GATE-A-CECIL-WRITE-RESOLUTION-FAILURE.txt").is_file(), "Step 35.0.4 physical Gate-A Cecil write-resolution failure record is present")
require((ROOT / "docs/history/steps/STEP-35.0.5-BOUNDED-CECIL-WRITER-RESOLUTION-FIX.md").is_file(), "Step 35.0.5 bounded Cecil writer-resolution fix record is present")
require((ROOT / "docs/history/reports/STEP-35.0.5-PHYSICAL-GATE-A-DEFERRED-OPEN-FAILURE.txt").is_file(), "Step 35.0.5 physical repeated Gate-A failure record is present")
require((ROOT / "docs/history/steps/STEP-35.0.6-DEFERRED-CECIL-OPEN-BEFORE-WRITER-CONFIGURATION.md").is_file(), "Step 35.0.6 deferred Cecil-open correction record is present")
require((ROOT / "docs/history/reports/STEP-35.0.6-PHYSICAL-DIAGNOSTIC-BRIDGE-MEMBERREF-FAILURE.txt").is_file(), "Step 35.0.6 physical diagnostic bridge MemberRef failure record is present")
require((ROOT / "docs/history/steps/STEP-35.0.7-GENERIC-DELEGATE-MEMBERREF-CORRECTION.md").is_file(), "Step 35.0.7 generic delegate MemberRef correction record is present")
require((ROOT / "docs/history/reports/STEP-35.0.7-PHYSICAL-SAVEMANAGER-LOCALIZATION-0.0.130.txt").is_file(), "Step 35.0.7 physical 0.0.130 SaveManager localization record is present")
require((ROOT / "docs/history/steps/STEP-35.0.8-SAVE-PLATFORM-GODOT-NATIVE-BOUNDARY-LOCALIZATION.md").is_file(), "Step 35.0.8 Save/Platform/Godot native-boundary localization design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.8-PHYSICAL-NULLPLATFORM-CONSTRUCTOR-LOCALIZATION-0.0.131.txt").is_file(), "Step 35.0.8 physical 0.0.131 NullPlatform constructor localization report is present")
require((ROOT / "docs/history/steps/STEP-35.0.9-NULLPLATFORM-CONSTRUCTOR-CALLSITE-LOCALIZATION.md").is_file(), "Step 35.0.9 NullPlatform constructor callsite-localization design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.9-PHYSICAL-COMMANDLINE-BOUNDARY-AND-ORDINAL-DEFECT-0.0.132.txt").is_file(), "Step 35.0.9 physical 0.0.132 CommandLine boundary/ordinal-defect report is present")
require((ROOT / "docs/history/steps/STEP-35.0.10-COMMANDLINE-GODOT-BOUNDARY-LOCALIZATION.md").is_file(), "Step 35.0.10 Command-Line/Godot boundary-localization design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.10-PHYSICAL-MAXSTACK-INSTRUMENTATION-FAILURE-0.0.133.txt").is_file(), "Step 35.0.10 physical 0.0.133 MaxStack instrumentation-failure report is present")
require((ROOT / "docs/history/reports/STEP-35.0.13-PHYSICAL-GODOT-DICTIONARY-CONSTRUCTOR-BOUNDARY-0.0.136.txt").is_file(), "Step 35.0.13 physical 0.0.136 Godot dictionary-constructor boundary report is present")
require((ROOT / "docs/history/steps/STEP-35.0.14-MANAGED-COMMANDLINE-DICTIONARY-COMPATIBILITY.md").is_file(), "superseded narrow Step 35.0.14 managed dictionary draft remains in history")
require((ROOT / "docs/history/steps/STEP-35.0.14-COMPREHENSIVE-GODOT-NATIVE-RECONNAISSANCE.md").is_file(), "Step 35.0.14 comprehensive Godot/native reconnaissance design record is preserved as the 0.0.137 pre-device design")
require((ROOT / "docs/history/reports/STEP-35.0.14-CODEMAGIC-HOST-REGRESSION-FAILURE-0.0.137.txt").is_file(), "0.0.137 Codemagic 208/209 pre-device failure report is present")
require((ROOT / "docs/history/steps/STEP-35.0.15-GODOTSHARP-BRIDGE-VERIFIER-CORRECTION.md").is_file(), "Step 35.0.15 GodotSharp bridge-verifier correction record is present")
require((ROOT / "docs/history/steps/STEP-35.0.16-GODOT-CALLBACK-BOUNDARY-AND-MANAGED-COMMANDLINE-FORWARD-PROBE.md").is_file(), "Step 35.0.16 forward-probe design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.16-CODEMAGIC-STALE-SUMMARY-ASSERTION-FAILURE-0.0.139.txt").is_file(), "0.0.139 Codemagic 209/210 stale-summary failure report is present")
require((ROOT / "docs/history/steps/STEP-35.0.17-RELEASE-SUMMARY-CONSISTENCY-CORRECTION.md").is_file(), "Step 35.0.17 release-summary consistency correction record is present")
require((ROOT / "docs/history/reports/STEP-35.0.17-PHYSICAL-THREE-MODE-CALLBACK-BOUNDARY-0.0.140.txt").is_file(), "physical 0.0.140 NATURAL/OS-RECON/FORWARD callback-boundary report is present")
require((ROOT / "docs/history/steps/STEP-35.0.18-GODOT-CORE-CALLBACK-HANDOFF-PROBE.md").is_file(), "Step 35.0.18 Godot core callback-handoff design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.18-CODEMAGIC-CALLBACK-TELEMETRY-ASSERTION-FAILURE-0.0.141.txt").is_file(), "0.0.141 Codemagic callback-telemetry assertion failure record is present")
require((ROOT / "docs/history/steps/STEP-35.0.19-CALLBACK-FAILURE-TELEMETRY-CONTRACT-CORRECTION.md").is_file(), "Step 35.0.19 callback failure-telemetry contract correction record is present")
require((ROOT / "docs/history/reports/STEP-35.0.19-CODEMAGIC-IOS-COMPILE-NAMESPACE-FAILURE-0.0.142.txt").is_file(), "0.0.142 Codemagic 855/855 static + 211/211 host + iOS CS0103 namespace failure record is present")
require((ROOT / "docs/history/steps/STEP-35.0.20-CALLBACK-HANDOFF-COMPILE-INTEGRATION-CORRECTION.md").is_file(), "Step 35.0.20 callback-handoff compile-integration correction record is present")
require((ROOT / "docs/history/reports/STEP-35.0.20-PHYSICAL-CORE-HANDOFF-SINGLETON-FRONTIER-0.0.143.txt").is_file(), "physical 0.0.143 CORE-HANDOFF callback-table success and singleton frontier record is present")
require((ROOT / "docs/history/steps/STEP-35.0.21-GODOT-SINGLETON-ACQUISITION-LOCALIZATION.md").is_file(), "Step 35.0.21 singleton-acquisition localization design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.21-PHYSICAL-REVERSE-BINDING-FRONTIER-0.0.144.txt").is_file(), "physical 0.0.144 reverse-binding GS035 frontier record is present")
require((ROOT / "docs/history/reports/STEP-35.0.22-PHYSICAL-REVERSE-BINDING-PREFLIGHT-0.0.145.txt").is_file(), "physical 0.0.145 reverse-binding preflight record is present")
require((ROOT / "docs/history/steps/STEP-35.0.23-GODOT-MANAGED-PLUGIN-BRIDGE-BOOTSTRAP.md").is_file(), "Step 35.0.23 managed-plugin bridge bootstrap design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.23-PHYSICAL-MANAGED-PLUGIN-BOOTSTRAP-RESOLVER-GUARD-0.0.146.txt").is_file(), "physical 0.0.146 managed-plugin bridge success/stale resolver guard record is present")
require((ROOT / "docs/history/steps/STEP-35.0.24-POST-BOOTSTRAP-RESOLVER-BASELINE-CORRECTION.md").is_file(), "Step 35.0.24 post-bootstrap resolver-baseline correction record is present")
require((ROOT / "docs/history/reports/STEP-35.0.24-CODEMAGIC-HOST-REGRESSION-MESSAGE-FAILURE-0.0.147.txt").is_file(), "0.0.147 Codemagic 212/213 host regression message failure record is present")
require((ROOT / "docs/history/steps/STEP-35.0.25-HOST-REGRESSION-CONTRACT-CORRECTION.md").is_file(), "Step 35.0.25 host regression contract correction record is present")
require((ROOT / "docs/history/steps/STEP-35.0.26-GATE-D-PROGRESS-WARM-CACHE.md").is_file(), "Step 35.0.26 Gate-D progress/warm-cache design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.26-PHYSICAL-DIAGNOSTIC-GATE-C-PASS-GATE-D-FINAL-UI-BOUNDARY-0.0.149.txt").is_file(), "physical 0.0.149 Gate-C PASS / Gate-D terminal UI boundary record is present")
require((ROOT / "docs/history/steps/STEP-35.0.27-EXACT-AUTHORITY-CLOSURE-GATE-D-FINALIZATION.md").is_file(), "Step 35.0.27 exact-authority/Gate-D-finalization design record is present")
require((ROOT / "docs/history/reports/STEP-35.0.27-CODEMAGIC-IOS-COMPILE-NAMESPACE-FAILURE-0.0.150.txt").is_file(), "0.0.150 Codemagic 895/895 static + 214/214 host + iOS CS0103 namespace failure record is present")
require((ROOT / "docs/history/steps/STEP-35.0.28-EXACT-CLOSURE-COMPILE-INTEGRATION-CORRECTION.md").is_file(), "Step 35.0.29 exact-closure compile-integration correction record is present")
step35_build150_report = read("docs/history/reports/STEP-35.0.27-CODEMAGIC-IOS-COMPILE-NAMESPACE-FAILURE-0.0.150.txt")
require(all(marker in step35_build150_report for marker in ["895/895", "214/214", "Step-15 standalone native-link preflight: PASS", "error CS0103", "Step35DiagnosticMode", "StS2Launcher.Core.Runtime", "No 0.0.150 IPA was produced"]), "0.0.150 CI evidence localizes the sole blocker to the missing Core.Runtime namespace import after static/host/native-link PASS")
require(len(history_steps) >= 62, "historical documentation set is comprehensive", f"count={len(history_steps)}")

# ---------------------------------------------------------------------------
# Codemagic/current build wiring
# ---------------------------------------------------------------------------
codemagic = read("codemagic.yaml")
require("ios-canonical:" in codemagic, "Codemagic exposes the stable canonical iOS workflow so caches survive numbered-step changes")
require("ios-step-33:" not in codemagic, "Codemagic no longer uses a step-numbered workflow key that would force cold caches on the next step")
for cache_path in [
    "$HOME/.nuget/packages",
    "$CM_BUILD_DIR/.nuget/packages",
    "$HOME/.cache/sts2launcher/godot-step15",
    "$HOME/.dotnet",
    "$CM_BUILD_DIR/src/StS2Launcher.iOS/obj/Release/net9.0-ios/ios-arm64",
]:
    require(cache_path in codemagic, f"Codemagic preserves canonical cache path: {cache_path}")
cm_script = read("scripts/codemagic.sh")
require(all(marker in cm_script for marker in ["Pinned .NET SDK/workloads", ".sts2launcher-ios-workload-set", "workload list", "Using verified cached iOS workload set", "DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=1"]), "Codemagic verifies the exact cached .NET/iOS workload before skipping network workload installation")
require("Step 36 canonical host regression tests" in read("scripts/test.sh"), "host-test report heading identifies Step 36")
require("LogFileName=step36.trx" in read("scripts/test.sh") and "artifacts/test-results/step36.trx" in read("scripts/test.sh"), "host-test TRX artifact identifies Step 36")
require("Step 35.0.30 / Step 36.0 Gate-D UI Return Fix + Controlled ExecuteEssential build environment" in read("scripts/codemagic.sh"), "Codemagic build-environment report heading identifies the combined Step 35.0.30 / Step 36.0 candidate")
workflow_count = len(re.findall(r'^  ios-[^:]+:', codemagic, re.M))
require(workflow_count == 1, "Codemagic contains one active launcher workflow")
require("scripts/codemagic.sh" in codemagic, "Codemagic calls the consolidated build entry point")
require(all(marker in read("scripts/codemagic.sh") for marker in ["SDK_SECONDS", "VALIDATE_SECONDS", "HOST_TEST_SECONDS", "WORKLOAD_SECONDS", "IOS_BUILD_SECONDS", "IPA_VERIFY_SECONDS", "TOTAL_SECONDS"]), "maintenance Codemagic summary records per-stage and total elapsed time")
require(all(marker in read("scripts/codemagic.sh") for marker in ["cache-state.txt", "AOT_CACHE_DIR", "AOT output files before build", "AOT output files after build"]), "Codemagic records cache-hit/size telemetry without bypassing the canonical build")
require("history" not in codemagic.lower(), "Codemagic workflow has no history dependency")

build_ios = read("scripts/build-ios.sh")
verify_ipa = read("scripts/verify-ipa.sh")
require('source scripts/lib/current-release.sh' in build_ios, "iOS build sources canonical release configuration")
require('PROJECT="$STS2_IOS_PROJECT"' in build_ios, "iOS build uses canonical project variable")
require("bash scripts/build-godot.sh" in build_ios, "iOS build uses canonical Godot wrapper")
require('source scripts/lib/current-release.sh' in verify_ipa and '"$VERSION" == "$STS2_DISPLAY_VERSION"' in verify_ipa and '"$BUILD_VERSION" == "$STS2_BUILD_VERSION"' in verify_ipa, "IPA verifier enforces release-config version")
require("src/StS2Launcher.iOS/Platform/GodotStep15NativeBridge.cs" in verify_ipa, "IPA verifier reads native bridge from canonical project path")
require("Expected device UI: STEP 35.0.30 / STEP 36.0 — GATE-D UI RETURN FIX + CONTROLLED EXACT EXECUTEESSENTIAL" in verify_ipa and "step36-ipa-verification-summary.log" in verify_ipa, "IPA verifier advertises the active combined Step-36 candidate")

# Fixture isolation: external IL fixtures remain post-publish data, never iOS project inputs.
require("StS2Launcher.Step20.DynamicFixture" not in project_text and "StS2Launcher.Step20.DependencyFixture" not in project_text and "StS2Launcher.Step20.RootFixture" not in project_text, "Step 20 dynamic fixtures remain absent from iOS build inputs")

# Closed Step-29/30/31 provenance and active Step-32 candidate provenance.
step29_manifest = ROOT / "tools/validation/protected-step29.0-real-sts2-target-audit.sha256"
require(step29_manifest.is_file(), "physically closed Step 29 implementation/evidence hash manifest exists")
if step29_manifest.is_file():
    mismatches=[]
    for line in step29_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "physically closed Step-29 implementation, tests, and closure evidence remain hash-pinned", ", ".join(mismatches))

step30_manifest = ROOT / "tools/validation/protected-step30.0-selected-target-semantic-audit.sha256"
require(step30_manifest.is_file(), "physically closed Step 30 implementation/evidence hash manifest exists")
if step30_manifest.is_file():
    mismatches=[]
    for line in step30_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "physically closed Step-30 implementation, tests, and closure evidence remain hash-pinned", ", ".join(mismatches))

step31_manifest = ROOT / "tools/validation/protected-step31.0-preparemethod-semantic-audit.sha256"
require(step31_manifest.is_file(), "physically closed Step 31 implementation/evidence hash manifest exists")
if step31_manifest.is_file():
    mismatches=[]
    for line in step31_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "physically closed Step-31 implementation, tests, and closure evidence remain hash-pinned", ", ".join(mismatches))

step32_manifest = ROOT / "tools/validation/protected-step32.0.5-real-sts2-preparemethod-rewrite.sha256"
require(step32_manifest.is_file(), "physically closed Step 32 implementation/evidence hash manifest exists")
if step32_manifest.is_file():
    mismatches=[]
    for line in step32_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "physically closed Step-32 implementation, tests, and closure evidence remain hash-pinned", ", ".join(mismatches))

step33_manifest = ROOT / "tools/validation/protected-step33.0-transformed-real-sts2-admission.sha256"
require(step33_manifest.is_file(), "physically closed Step 33 implementation/evidence hash manifest exists")
if step33_manifest.is_file():
    mismatches=[]
    for line in step33_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "physically closed Step-33 implementation, tests, and closure evidence remain hash-pinned", ", ".join(mismatches))

step34_manifest = ROOT / "tools/validation/protected-step34.0-transformed-real-sts2-prewarmjit-execution.sha256"
require(step34_manifest.is_file(), "physically closed Step 34 implementation/evidence hash manifest exists")
if step34_manifest.is_file():
    mismatches=[]
    for line in step34_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "physically closed Step-34 implementation, tests, and closure evidence remain hash-pinned", ", ".join(mismatches))

step35_manifest = ROOT / "tools/validation/candidate-step35-transformed-real-sts2-very-early-initialization.sha256"
require(step35_manifest.is_file(), "Step 35 candidate boundary hash manifest exists")
if step35_manifest.is_file():
    step35_manifest_text = step35_manifest.read_text()
    required_step35_manifest_paths = [
        "src/StS2Launcher.Core/Runtime/TransformedRealStS2VeryEarlyInitialization.cs",
        "src/StS2Launcher.Core/Runtime/Step35DiagnosticMode.cs",
        "src/StS2Launcher.Core/Runtime/Step35GodotReconnaissance.cs",
        "tests/StS2Launcher.Core.Tests/Runtime/TransformedRealStS2VeryEarlyInitializationTests.cs",
        "src/StS2Launcher.iOS/UI/RootViewController.TransformedRealStS2VeryEarlyInitialization.cs",
        "src/StS2Launcher.iOS/UI/RootViewController.Step35ManagedPluginBootstrap.cs",
        "src/StS2Launcher.iOS/UI/RootViewController.Step35GateDProgress.cs",
        "src/StS2Launcher.iOS/UI/CurrentReleasePresentation.cs",
        "src/StS2Launcher.iOS/Platform/GodotStep15NativeBridge.cs",
        "native/step15/godot_module/sts2_ios_host/step15_ios_host_bridge.mm",
        "native/step15/smoke_project/project.godot",
        "scripts/build-godot.sh",
        "scripts/preflight-godot-link.sh",
        "tools/validate_current.py",
        "docs/history/reports/STEP-35.0.10-PHYSICAL-MAXSTACK-INSTRUMENTATION-FAILURE-0.0.133.txt",
        "docs/history/reports/STEP-35.0.13-PHYSICAL-GODOT-DICTIONARY-CONSTRUCTOR-BOUNDARY-0.0.136.txt",
        "docs/history/steps/STEP-35.0.14-MANAGED-COMMANDLINE-DICTIONARY-COMPATIBILITY.md",
        "docs/history/steps/STEP-35.0.14-COMPREHENSIVE-GODOT-NATIVE-RECONNAISSANCE.md",
        "docs/history/reports/STEP-35.0.14-CODEMAGIC-HOST-REGRESSION-FAILURE-0.0.137.txt",
        "docs/history/steps/STEP-35.0.15-GODOTSHARP-BRIDGE-VERIFIER-CORRECTION.md",
        "docs/history/reports/STEP-35.0.15-PHYSICAL-NATURAL-COMPAT-CALLBACK-BOUNDARIES-0.0.138.txt",
        "docs/history/steps/STEP-35.0.16-GODOT-CALLBACK-BOUNDARY-AND-MANAGED-COMMANDLINE-FORWARD-PROBE.md",
        "docs/history/reports/STEP-35.0.16-CODEMAGIC-STALE-SUMMARY-ASSERTION-FAILURE-0.0.139.txt",
        "docs/history/steps/STEP-35.0.17-RELEASE-SUMMARY-CONSISTENCY-CORRECTION.md",
        "docs/history/reports/STEP-35.0.17-PHYSICAL-THREE-MODE-CALLBACK-BOUNDARY-0.0.140.txt",
        "docs/history/steps/STEP-35.0.18-GODOT-CORE-CALLBACK-HANDOFF-PROBE.md",
        "docs/history/reports/STEP-35.0.18-CODEMAGIC-CALLBACK-TELEMETRY-ASSERTION-FAILURE-0.0.141.txt",
        "docs/history/steps/STEP-35.0.19-CALLBACK-FAILURE-TELEMETRY-CONTRACT-CORRECTION.md",
        "docs/history/reports/STEP-35.0.19-CODEMAGIC-IOS-COMPILE-NAMESPACE-FAILURE-0.0.142.txt",
        "docs/history/steps/STEP-35.0.20-CALLBACK-HANDOFF-COMPILE-INTEGRATION-CORRECTION.md",
        "docs/history/reports/STEP-35.0.20-PHYSICAL-CORE-HANDOFF-SINGLETON-FRONTIER-0.0.143.txt",
        "docs/history/steps/STEP-35.0.21-GODOT-SINGLETON-ACQUISITION-LOCALIZATION.md",
        "docs/history/reports/STEP-35.0.21-PHYSICAL-REVERSE-BINDING-FRONTIER-0.0.144.txt",
        "docs/history/reports/STEP-35.0.22-PHYSICAL-REVERSE-BINDING-PREFLIGHT-0.0.145.txt",
        "docs/history/steps/STEP-35.0.23-GODOT-MANAGED-PLUGIN-BRIDGE-BOOTSTRAP.md",
    "docs/history/reports/STEP-35.0.23-PHYSICAL-MANAGED-PLUGIN-BOOTSTRAP-RESOLVER-GUARD-0.0.146.txt",
    "docs/history/steps/STEP-35.0.24-POST-BOOTSTRAP-RESOLVER-BASELINE-CORRECTION.md",
        "docs/history/reports/STEP-35.0.24-CODEMAGIC-HOST-REGRESSION-MESSAGE-FAILURE-0.0.147.txt",
        "docs/history/steps/STEP-35.0.25-HOST-REGRESSION-CONTRACT-CORRECTION.md",
        "docs/history/steps/STEP-35.0.26-GATE-D-PROGRESS-WARM-CACHE.md",
        "docs/history/reports/STEP-35.0.26-PHYSICAL-DIAGNOSTIC-GATE-C-PASS-GATE-D-FINAL-UI-BOUNDARY-0.0.149.txt",
        "docs/history/steps/STEP-35.0.27-EXACT-AUTHORITY-CLOSURE-GATE-D-FINALIZATION.md",
        "docs/history/reports/STEP-35.0.27-CODEMAGIC-IOS-COMPILE-NAMESPACE-FAILURE-0.0.150.txt",
        "docs/history/steps/STEP-35.0.28-EXACT-CLOSURE-COMPILE-INTEGRATION-CORRECTION.md",
        "docs/history/reports/STEP-35.0.28-CODEMAGIC-DECLARED-NAMESPACE-FAILURE-0.0.151.txt",
        "docs/history/steps/STEP-35.0.29-EXACT-CLOSURE-DECLARED-NAMESPACE-CORRECTION.md",
    ]
    require(
        all(f"  {relative}" in step35_manifest_text for relative in required_step35_manifest_paths),
        "Step 35.0.29 candidate manifest includes implementation/tests, native/build bridge, prior provenance, physical 0.0.149 evidence, 0.0.150/0.0.151 CI compile failures, and active declared-namespace correction",
    )
    mismatches=[]
    for line in step35_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "Step 35 implementation, active release wiring, and design/evidence docs are hash-pinned", ", ".join(mismatches))

step36_manifest = ROOT / "tools/validation/candidate-step36-controlled-exact-executeessential.sha256"
require(step36_manifest.is_file(), "Step 36 active candidate hash manifest exists")
if step36_manifest.is_file():
    step36_manifest_text = step36_manifest.read_text()
    required_step36_manifest_paths = [
        "src/StS2Launcher.Core/Runtime/TransformedRealStS2VeryEarlyInitialization.cs",
        "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitialization.cs",
        "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationGate.cs",
        "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationGateResult.cs",
        "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationProgress.cs",
        "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationSummary.cs",
        "src/StS2Launcher.Core/Runtime/TransformedRealStS2EssentialInitializationGateSequence.cs",
        "tests/StS2Launcher.Core.Tests/Runtime/TransformedRealStS2VeryEarlyInitializationTests.cs",
        "tests/StS2Launcher.Core.Tests/Runtime/TransformedRealStS2EssentialInitializationTests.cs",
        "src/StS2Launcher.iOS/UI/RootViewController.TransformedRealStS2VeryEarlyInitialization.cs",
        "src/StS2Launcher.iOS/UI/RootViewController.Step35Telemetry.cs",
        "src/StS2Launcher.iOS/UI/RootViewController.Step35GateDProgress.cs",
        "src/StS2Launcher.iOS/UI/RootViewController.TransformedRealStS2EssentialInitialization.cs",
        "src/StS2Launcher.iOS/UI/RootViewController.cs",
        "src/StS2Launcher.iOS/UI/CurrentReleasePresentation.cs",
        "src/StS2Launcher.iOS/StS2Launcher.iOS.csproj",
        "src/StS2Launcher.iOS/Info.plist",
        "scripts/lib/current-release.sh",
        "scripts/build-ios.sh",
        "scripts/test.sh",
        "scripts/codemagic.sh",
        "scripts/verify-ipa.sh",
        "codemagic.yaml",
        "tools/validate_current.py",
        "README.md",
        "docs/CURRENT-STATUS.md",
        "docs/MASTER-PLAN.md",
        "docs/ARCHITECTURE.md",
        "docs/TESTING.md",
        "docs/REGRESSION-CONTRACTS.md",
        "docs/RELEASE-CHECKLIST.md",
        "docs/history/INDEX.md",
        "docs/history/reports/STEP-35.0.29-PHYSICAL-EXACT-AUTHORITY-CLOSURE-UI-RETURN-STALL-0.0.152.txt",
        "docs/history/steps/STEP-36.0-CONTROLLED-EXACT-EXECUTEESSENTIAL.md",
    ]
    require(all(f"  {relative}" in step36_manifest_text for relative in required_step36_manifest_paths), "Step 36 candidate manifest pins the UI-return fix, exact ExecuteEssential implementation/tests, active release wiring, and physical Step-35 closure provenance")
    mismatches=[]
    for line in step36_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "Step 36 active implementation/release/provenance files are hash-pinned", ", ".join(mismatches))

# ---------------------------------------------------------------------------
# Source archive cleanliness/security
# ---------------------------------------------------------------------------
forbidden_names = re.compile(r'(^|/)(sts2\.dll|SlayTheSpire2\.app)(/|$)|fmod|spine_godot', re.I)
forbidden_files: list[str] = []
for path in ROOT.rglob("*"):
    if not path.is_file() or path.name == "history.zip":
        continue
    rel = str(path.relative_to(ROOT))
    if forbidden_names.search(rel):
        forbidden_files.append(rel)
require(not forbidden_files, "authoritative source tree contains no StS2/proprietary runtime payload", ", ".join(forbidden_files[:20]))

secret_patterns = [
    re.compile(r'password\s*=\s*"[^"\n]+"', re.I),
    re.compile(r'refresh[_ -]?token\s*=\s*"[^"\n]+"', re.I),
    re.compile(r'codesignkey\s*=\s*"[^"\n]+"', re.I),
]
secret_hits: list[str] = []
for area in [ROOT / "src", ROOT / "scripts", ROOT / "tools", ROOT / "tests", ROOT / "fixtures", ROOT / "native", ROOT / "docs"]:
    for path, text in text_files_under(area):
        if any(rx.search(text) for rx in secret_patterns):
            secret_hits.append(str(path.relative_to(ROOT)))
require(not secret_hits, "authoritative source/docs contain no obvious embedded credential/signing secrets", ", ".join(secret_hits))

print()
if FAILURES:
    print(f"VALIDATION FAILED: {len(FAILURES)} failure(s), {PASSES} passes")
    for item in FAILURES:
        print(f"  - {item}")
    sys.exit(1)
print(f"VALIDATION PASS: {PASSES} checks")
