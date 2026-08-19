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


print("StS2 Launcher — Step 22.4.2 Canonical Foundation Regression Correction static validation")
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
require("<ApplicationVersion>64</ApplicationVersion>" in project_text, "build version is 64")
require("<ApplicationDisplayVersion>0.0.64</ApplicationDisplayVersion>" in project_text, "display version is 0.0.64")
require(plist.get("CFBundleVersion") == "64", "Info.plist build version is 64")
require(plist.get("CFBundleShortVersionString") == "0.0.64", "Info.plist display version is 0.0.64")
require(plist.get("UIFileSharingEnabled") is True, "iOS Files sharing remains enabled")
require(plist.get("LSSupportsOpeningDocumentsInPlace") is True, "open-in-place Documents access remains enabled")
require("<RootNamespace>StS2Launcher.iOS</RootNamespace>" in project_text, "canonical iOS root namespace is explicit")
require("<AssemblyName>StS2Launcher.iOS</AssemblyName>" in project_text, "canonical iOS assembly name is explicit")

# Every live iOS source file uses the canonical namespace family.
ios_cs = list((ROOT / "src/StS2Launcher.iOS").rglob("*.cs"))
ios_text = "\n".join(p.read_text() for p in ios_cs)
require("StS2Launcher.Step05.iOS" not in ios_text, "live iOS source contains no legacy Step05 namespace")
require("namespace StS2Launcher.iOS" in ios_text, "live iOS source uses canonical namespace")

# ---------------------------------------------------------------------------
# Physically proven runtime/build policy
# ---------------------------------------------------------------------------
require("<TrimMode>full</TrimMode>" in project_text, "full trimming policy retained")
require("<MtouchInterpreter>-all</MtouchInterpreter>" in project_text, "Step 20 interpreter policy retained")
require("'$(UseInterpreter)' == 'true'" in project_text, "build guard rejects broad UseInterpreter=true")
require("'$(PublishAot)' == 'true'" in project_text, "build guard rejects NativeAOT")
require("STEP22.4.2 RUNTIME POLICY" in project_text, "runtime policy emits Step 22.4.2 build telemetry")

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
    "STS2_IPA_REL": "artifacts/StS2-Launcher-Step-22.4.2.ipa",
    "STS2_DISPLAY_VERSION": "0.0.64",
    "STS2_BUILD_VERSION": "64",
    "STS2_RUNTIME_POLICY_MARKER": "STEP22.4.2 RUNTIME POLICY:",
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
# Documentation model
# ---------------------------------------------------------------------------
required_docs = [
    "README.md", "docs/README.md", "docs/MASTER-PLAN.md", "docs/CURRENT-STATUS.md", "docs/ARCHITECTURE.md",
    "docs/TESTING.md", "docs/REGRESSION-CONTRACTS.md", "docs/REPORTS.md", "docs/RELEASE-CHECKLIST.md", "docs/history/INDEX.md",
    "docs/history/steps/STEP-22.4-CANONICAL-FOUNDATION.md",
    "docs/history/steps/STEP-22.4.2-STEP19-REGRESSION-CONTRACT-CORRECTION.md",
]
for doc in required_docs:
    require((ROOT / doc).is_file(), f"authoritative documentation exists: {doc}")

master = read("docs/MASTER-PLAN.md")
for heading in ["Product objective", "Non-negotiable security and content boundaries", "Authority model", "Canonical source architecture", "Major roadmap", "Definition of a closed step", "Resumption rule"]:
    require(heading in master, f"master plan contains durable section: {heading}")
require("docs/CURRENT-STATUS.md" in master and "docs/REGRESSION-CONTRACTS.md" in master and "docs/history/INDEX.md" in master, "master plan defines self-contained resumption path")

top_level_step_docs = [p.name for p in (ROOT / "docs").glob("STEP-*.md")]
require(not top_level_step_docs, "top-level docs are durable/current; step records live under docs/history/steps", ", ".join(top_level_step_docs))

history_steps = list((ROOT / "docs/history/steps").glob("*.md"))
history_names = [p.name for p in history_steps]
for major in range(1, 23):
    prefix = f"STEP-{major:02d}"
    require(any(name.startswith(prefix) for name in history_names), f"readable historical documentation retained for Step {major:02d}")
require(any(name.startswith("STEP-22.4") for name in history_names), "Step 22.4 design/history record is present")
require(len(history_steps) >= 58, "historical documentation set is comprehensive", f"count={len(history_steps)}")

# ---------------------------------------------------------------------------
# Codemagic/current build wiring
# ---------------------------------------------------------------------------
codemagic = read("codemagic.yaml")
require("ios-step-22-4-2:" in codemagic, "Codemagic exposes Step 22.4.2 workflow")
workflow_count = len(re.findall(r'^  ios-[^:]+:', codemagic, re.M))
require(workflow_count == 1, "Codemagic contains one active launcher workflow")
require("scripts/codemagic.sh" in codemagic, "Codemagic calls the consolidated build entry point")
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
