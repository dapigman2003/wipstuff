using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamCompatibilityInventoryTests
{
    [TestMethod]
    public async Task CompatibilityInventoryClassifiesInstalledContentReadOnly()
    {
        var root = NewRoot();
        try
        {
            var managed = CreateManagedDepot(root);
            var files = new Dictionary<string, byte[]>
            {
                ["SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck"] = Encoding.ASCII.GetBytes("GODOT-PACK-DATA"),
                ["SlayTheSpire2.app/Contents/Resources/Managed/StS2.dll"] = FakeManagedBinary(
                    "System.Reflection", "System.Reflection.Emit", "DynamicMethod", "Expression.Compile", "Microsoft.Win32"),
                ["SlayTheSpire2.app/Contents/Resources/Managed/GodotSharp.dll"] = FakeManagedBinary(
                    "GodotSharp", "Godot.NativeInterop"),
                ["SlayTheSpire2.app/Contents/Resources/Managed/FMOD.Studio.dll"] = FakeManagedBinary(
                    "FMOD", "fmodstudio"),
                ["SlayTheSpire2.app/Contents/Resources/Managed/Spine.Runtime.dll"] = FakeManagedBinary(
                    "Spine", "spine-csharp"),
                ["SlayTheSpire2.app/Contents/MacOS/Slay the Spire 2"] = FakeMachO64(),
                ["SlayTheSpire2.app/Contents/Frameworks/libfmod.dylib"] = FakeMachO64(),
                ["SlayTheSpire2.app/Contents/Resources/readme.txt"] = Encoding.UTF8.GetBytes("project data"),
            };
            await WriteManagedTreeAsync(managed, files);

            var before = Directory.EnumerateFiles(managed, "*", SearchOption.AllDirectories)
                .ToDictionary(
                    path => Path.GetRelativePath(managed, path),
                    path => File.ReadAllBytes(path),
                    StringComparer.OrdinalIgnoreCase);

            var result = await new SteamCompatibilityInventoryInspection(root).RunAsync();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(SteamCompatibilityInventoryOutcome.Complete, result.Outcome);
            Assert.IsTrue(result.OfflineReadyPreconditionVerified);
            Assert.AreEqual(files.Count, result.TotalFiles);
            Assert.IsTrue(result.TotalBytes > 0);
            Assert.IsTrue(result.AssetFiles >= 2); // .pck + .txt
            Assert.IsTrue(result.GodotContentFiles >= 1);
            Assert.IsTrue(result.ManagedAssemblyFiles >= 4);
            Assert.AreEqual(result.ManagedAssemblyFiles, result.ManagedAssembliesScanned);
            Assert.IsTrue(result.NativeBinaryFiles >= 2);
            Assert.IsTrue(result.GodotSharpIndicatorFiles >= 1);
            Assert.IsTrue(result.FmodIndicatorFiles >= 2);
            Assert.IsTrue(result.SpineIndicatorFiles >= 1);
            Assert.IsTrue(result.ReflectionIndicatorFiles >= 1);
            Assert.IsTrue(result.DynamicCodeIndicatorFiles >= 1);
            Assert.IsTrue(result.PlatformSpecificFiles >= 1);
            Assert.IsTrue(result.PotentialIosBlockerSignals.Count >= 3);
            Assert.IsTrue(result.DependencyNotes.Count >= 3);
            Assert.IsFalse(result.SteamSessionConsulted);
            Assert.IsFalse(result.NetworkAccessAttempted);
            Assert.IsFalse(result.ManagedInstallModified);
            Assert.IsFalse(result.GameLaunchAttempted);
            Assert.IsNull(result.Error);

            var after = Directory.EnumerateFiles(managed, "*", SearchOption.AllDirectories)
                .ToDictionary(
                    path => Path.GetRelativePath(managed, path),
                    path => File.ReadAllBytes(path),
                    StringComparer.OrdinalIgnoreCase);

            CollectionAssert.AreEquivalent(before.Keys.ToArray(), after.Keys.ToArray());
            foreach (var pair in before)
                CollectionAssert.AreEqual(pair.Value, after[pair.Key], $"Step 14 modified managed file: {pair.Key}");
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [TestMethod]
    public async Task CompatibilityInventoryRefusesCorruptInstallBeforeClassification()
    {
        var root = NewRoot();
        try
        {
            var managed = CreateManagedDepot(root);
            var files = new Dictionary<string, byte[]>
            {
                ["game/StS2.dll"] = FakeManagedBinary("GodotSharp"),
            };
            await WriteManagedTreeAsync(managed, files);
            await File.AppendAllTextAsync(Path.Combine(managed, "game", "StS2.dll"), "CORRUPT");

            var result = await new SteamCompatibilityInventoryInspection(root).RunAsync();

            Assert.AreEqual(SteamCompatibilityInventoryOutcome.LocalInstallNotReady, result.Outcome);
            Assert.IsFalse(result.Success);
            Assert.IsFalse(result.OfflineReadyPreconditionVerified);
            Assert.AreEqual(0, result.ManagedAssembliesScanned);
            Assert.IsFalse(result.NetworkAccessAttempted);
            Assert.IsFalse(result.ManagedInstallModified);
            Assert.IsFalse(result.GameLaunchAttempted);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [TestMethod]
    public async Task CompatibilityInventoryRequiresExistingManagedInstall()
    {
        var root = NewRoot();
        try
        {
            var result = await new SteamCompatibilityInventoryInspection(root).RunAsync();

            Assert.AreEqual(SteamCompatibilityInventoryOutcome.LocalInstallNotReady, result.Outcome);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(0, result.TotalFiles);
            Assert.IsFalse(result.SteamSessionConsulted);
            Assert.IsFalse(result.NetworkAccessAttempted);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [TestMethod]
    public void CompatibilityInventoryResultContractExplicitlyProvesReadOnlyBoundary()
    {
        var properties = typeof(SteamCompatibilityInventoryResult)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet();

        Assert.IsTrue(properties.Contains(nameof(SteamCompatibilityInventoryResult.OfflineReadyPreconditionVerified)));
        Assert.IsTrue(properties.Contains(nameof(SteamCompatibilityInventoryResult.PotentialIosBlockerSignals)));
        Assert.IsTrue(properties.Contains(nameof(SteamCompatibilityInventoryResult.DynamicCodeEvidence)));
        Assert.IsTrue(properties.Contains(nameof(SteamCompatibilityInventoryResult.SteamSessionConsulted)));
        Assert.IsTrue(properties.Contains(nameof(SteamCompatibilityInventoryResult.NetworkAccessAttempted)));
        Assert.IsTrue(properties.Contains(nameof(SteamCompatibilityInventoryResult.ManagedInstallModified)));
        Assert.IsTrue(properties.Contains(nameof(SteamCompatibilityInventoryResult.GameLaunchAttempted)));
        Assert.AreEqual(2868840u, SteamCompatibilityInventoryInspection.TargetAppId);
    }

    private static byte[] FakeManagedBinary(params string[] markers)
    {
        var body = string.Join("\0", new[] { "MZ", "BSJB" }.Concat(markers)) + "\0";
        return Encoding.Latin1.GetBytes(body);
    }

    private static byte[] FakeMachO64() =>
        [0xCF, 0xFA, 0xED, 0xFE, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00];

    private static string NewRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "sts2-compat-inventory-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static string CreateManagedDepot(string root)
    {
        var managed = Path.Combine(root, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        Directory.CreateDirectory(managed);
        return managed;
    }

    private static async Task WriteManagedTreeAsync(string managed, IReadOnlyDictionary<string, byte[]> files)
    {
        Directory.CreateDirectory(managed);
        var receiptFiles = new List<SteamManagedInstallFile>();
        foreach (var pair in files)
        {
            var path = Path.Combine(managed, pair.Key.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, pair.Value);
            receiptFiles.Add(new SteamManagedInstallFile(pair.Key, pair.Value.LongLength, Sha1Hex(pair.Value)));
        }

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840u,
            2868842u,
            8653035385353091849UL,
            "public",
            DateTimeOffset.UnixEpoch,
            receiptFiles);
        await using var stream = new FileStream(
            Path.Combine(managed, SteamManagedInstallReceipt.FileName),
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);
        await JsonSerializer.SerializeAsync(
            stream,
            receipt,
            SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
    }

    private static string Sha1Hex(byte[] bytes) =>
        Convert.ToHexString(SHA1.HashData(bytes));

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
