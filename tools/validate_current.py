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


print("StS2 Launcher — Step 32.0.3 retired Harmony active-surface trim static validation")
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
require("<ApplicationVersion>118</ApplicationVersion>" in project_text, "build version is 118")
require("<ApplicationDisplayVersion>0.0.118</ApplicationDisplayVersion>" in project_text, "display version is 0.0.118")
require(plist.get("CFBundleVersion") == "118", "Info.plist build version is 118")
require(plist.get("CFBundleShortVersionString") == "0.0.118", "Info.plist display version is 0.0.118")
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
require("STEP 32.0.3 — RETIRED HARMONY ACTIVE-SURFACE TRIM" in release_presentation, "top launcher banner identifies active Step 32.0.3 maintenance candidate")
require("STEP 31 CLOSED POSITIVE 4/4" in release_presentation and "0x06007D05" in release_presentation and "PrepareMethod" in release_presentation and "private" in release_presentation and "zero CLR load" in release_presentation, "top launcher banner preserves Step-31 physical closure and identifies the private Step-32 rewrite boundary")
require("NSBundle.MainBundle.ObjectForInfoDictionary(\"CFBundleShortVersionString\")" in release_presentation, "top launcher version is derived from the built Info.plist instead of a stale hard-coded version")
require('ExpectedDisplayVersion = "0.0.118"' in release_presentation and 'ExpectedBuildVersion = "118"' in release_presentation, "Step 32.0.3 source pins expected bundle release identity")
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
require("STEP32 RUNTIME POLICY" in project_text, "runtime policy emits Step 32 build telemetry")
require("STEP32 DYNAMIC PAYLOAD TRIMMING POLICY: MtouchLink=$(MtouchLink); TrimMode=$(TrimMode)" in project_text, "dynamic-payload trimming policy emits exact Step-32 build telemetry")
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
require("STEP32 DYNAMIC PAYLOAD TRIMMING POLICY: MtouchLink=None; TrimMode=copy" in build_ios, "iOS publish requires exact Step-32 copy/no-link dynamic-payload telemetry")
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
require(not platform_changed, "platform/native behavior is unchanged except canonical managed namespace", ", ".join(platform_changed))

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
    "STS2_IPA_REL": "artifacts/StS2-Launcher-Step-32.ipa",
    "STS2_DISPLAY_VERSION": "0.0.118",
    "STS2_BUILD_VERSION": "118",
    "STS2_RUNTIME_POLICY_MARKER": "STEP32 RUNTIME POLICY:",
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
require("Step 32.0.3 IPA verification passed." in verify_ipa_text and "Step 27 IPA verification passed." not in verify_ipa_text, "IPA verification summary identifies the maintenance candidate rather than retired Step 27")
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
require("ConstantMetadataWriteResolver" in step32_source and "CecilWriteSystemRuntimeIdentity" in step32_source and "System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" in step32_source, "Step 32.0.2 pins the exact write-only System.Runtime constant-metadata resolver identity")
require("CollectExternalConstantTypeRequirements" in step32_source and "GetPrimitiveConstantType" in step32_source and "External framework/game assembly bytes opened by the write resolver: 0" in step32_source, "Step 32.0.2 synthesizes constant metadata from verified source values without opening external assembly bytes")
require("DefaultAssemblyResolver" not in step32_source and "AddSearchDirectory" not in step32_source, "Step 32.0.2 forbids broad Cecil resolver/search fallback")
require("ComputeConstantMetadataFingerprint" in step32_source and "source/transformed constant metadata semantics changed" in step32_source and "ExpectedConstantMetadataSha256" in step32_source, "Step 32.0.2 verifies unrelated Constant-table semantics survive serialization unchanged")
require("SyntheticExternalEnum" in step32_tests and "SingleAssemblyResolver" in step32_tests and "Synthetic constant-metadata resolver types: 1" in step32_tests, "Step 32 host regression reproduces the external-enum Constant-table serialization path")
require("ExactPrewarmJitPrepareMethodFamilyIsRewrittenOnPrivateCopyOnly" in step32_tests and "CollectionAssert.AreEqual(before, after)" in step32_tests and "CountPrepareMethod(transformedMethod)" in step32_tests and "BranchTargetedPrepareMethodSiteIsRejectedBeforeAnyRewrite" in step32_tests, "Step 32 host regressions prove private-only rewrite and branch-target refusal")
require("Step32-RealStS2PrepareMethodRewrite.txt" in step32_ui and "REAL STS2 PREPAREMETHOD REWRITE" in step32_ui, "iOS UI persists the dedicated Step-32 physical report")
require("new RealStS2PrepareMethodRewrite(_launcherDataRoot)" in root_ui_text and "AddRealStS2PrepareMethodRewriteControls(content)" in root_ui_text, "RootViewController wires Step 32 into the active device surface")
require("Step32ImplementationMarker" in release_presentation and "6 one-arg PrepareMethod calls to Pop" in release_presentation and "4 two-arg calls to Pop+Pop" in release_presentation and "zero CLR load" in release_presentation, "release presentation pins the Step-32 first-real-rewrite boundary")

# ---------------------------------------------------------------------------
# Documentation model
# ---------------------------------------------------------------------------
required_docs = [
    "README.md", "docs/README.md", "docs/MASTER-PLAN.md", "docs/CURRENT-STATUS.md", "docs/ARCHITECTURE.md",
    "docs/TESTING.md", "docs/REGRESSION-CONTRACTS.md", "docs/REPORTS.md", "docs/RELEASE-CHECKLIST.md", "docs/history/INDEX.md",
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

current_status = read("docs/CURRENT-STATUS.md")
require("Steps 01–26" in current_status and "Step 27" in current_status and "CLOSED NEGATIVE" in current_status and "Step 28" in current_status and "CLOSED POSITIVE 5/5" in current_status and "Step 31" in current_status and "CLOSED POSITIVE 4/4" in current_status and "Active candidate — Step 32.0.3 / 0.0.118 (118)" in current_status and "0x06007D05" in current_status and "6 ×" in current_status and "4 ×" in current_status and "System.Runtime" in current_status and "Sentry, Version=5.0.0.0" in current_status and "maintenance-only" in current_status and "Step32-RealStS2PrepareMethodRewrite.txt" in current_status, "current status preserves physical baselines, the 0.0.117 Sentry finding, and the maintenance-only Step-32 trim")

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
for major in range(1, 33):
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
require(any(name.startswith("STEP-32") for name in history_names), "Step 32 first real StS2 rewrite design/test record is present")
require(len(history_steps) >= 60, "historical documentation set is comprehensive", f"count={len(history_steps)}")

# ---------------------------------------------------------------------------
# Codemagic/current build wiring
# ---------------------------------------------------------------------------
codemagic = read("codemagic.yaml")
require("ios-step-32:" in codemagic, "Codemagic exposes the Step 32 workflow")
require("Step 32 canonical host regression tests" in read("scripts/test.sh"), "host-test report heading identifies Step 32")
require("LogFileName=step32.trx" in read("scripts/test.sh") and "artifacts/test-results/step32.trx" in read("scripts/test.sh"), "host-test TRX artifact identifies Step 32")
require("Step 32 PrepareMethod Semantic Context Audit build environment" in read("scripts/codemagic.sh") or "Step 32" in read("scripts/codemagic.sh"), "Codemagic build-environment report heading identifies Step 32")
workflow_count = len(re.findall(r'^  ios-[^:]+:', codemagic, re.M))
require(workflow_count == 1, "Codemagic contains one active launcher workflow")
require("scripts/codemagic.sh" in codemagic, "Codemagic calls the consolidated build entry point")
require(all(marker in read("scripts/codemagic.sh") for marker in ["SDK_SECONDS", "VALIDATE_SECONDS", "HOST_TEST_SECONDS", "WORKLOAD_SECONDS", "IOS_BUILD_SECONDS", "IPA_VERIFY_SECONDS", "TOTAL_SECONDS"]), "maintenance Codemagic summary records per-stage and total elapsed time")
require("history" not in codemagic.lower(), "Codemagic workflow has no history dependency")

build_ios = read("scripts/build-ios.sh")
verify_ipa = read("scripts/verify-ipa.sh")
require('source scripts/lib/current-release.sh' in build_ios, "iOS build sources canonical release configuration")
require('PROJECT="$STS2_IOS_PROJECT"' in build_ios, "iOS build uses canonical project variable")
require("bash scripts/build-godot.sh" in build_ios, "iOS build uses canonical Godot wrapper")
require('source scripts/lib/current-release.sh' in verify_ipa and '"$VERSION" == "$STS2_DISPLAY_VERSION"' in verify_ipa and '"$BUILD_VERSION" == "$STS2_BUILD_VERSION"' in verify_ipa, "IPA verifier enforces release-config version")
require("src/StS2Launcher.iOS/Platform/GodotStep15NativeBridge.cs" in verify_ipa, "IPA verifier reads native bridge from canonical project path")

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

step32_manifest = ROOT / "tools/validation/candidate-step32-real-sts2-preparemethod-rewrite.sha256"
require(step32_manifest.is_file(), "Step 32 candidate boundary hash manifest exists")
if step32_manifest.is_file():
    mismatches=[]
    for line in step32_manifest.read_text().splitlines():
        if not line.strip(): continue
        digest, relative = line.split("  ",1); path=ROOT/relative
        if not path.is_file() or sha256(path)!=digest: mismatches.append(relative)
    require(not mismatches, "Step 32 implementation, active release wiring, and evidence docs are hash-pinned", ", ".join(mismatches))

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
