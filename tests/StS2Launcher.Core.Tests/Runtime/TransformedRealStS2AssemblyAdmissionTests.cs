using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class TransformedRealStS2AssemblyAdmissionTests
{
    [TestMethod]
    public void OrderedAdmissionGatesReachFourOfFourPass()
    {
        var gates = new TransformedRealStS2AssemblyAdmissionGateSequence();
        gates.Record(new(TransformedRealStS2AssemblyAdmissionGate.VerifiedTransformedImagePreflight, true, "preflight"));
        gates.Record(new(TransformedRealStS2AssemblyAdmissionGate.TransformedPrimaryClrAdmission, true, "load"));
        gates.Record(new(TransformedRealStS2AssemblyAdmissionGate.AdmissionOnlyResolverAudit, true, "resolver"));
        gates.Record(new(TransformedRealStS2AssemblyAdmissionGate.FinalIsolationAudit, true, "isolation"));

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.Gates.Count);
        Assert.AreEqual("TRANSFORMED REAL STS2 CLR ADMISSION PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void AdmissionStopsAfterFirstFailure()
    {
        var gates = new TransformedRealStS2AssemblyAdmissionGateSequence();
        gates.Record(new(TransformedRealStS2AssemblyAdmissionGate.VerifiedTransformedImagePreflight, true, "preflight"));
        gates.Record(new(TransformedRealStS2AssemblyAdmissionGate.TransformedPrimaryClrAdmission, false, "failed"));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2AssemblyAdmissionGate.AdmissionOnlyResolverAudit, true, "must not advance")));
        Assert.AreEqual(TransformedRealStS2AssemblyAdmissionGate.TransformedPrimaryClrAdmission, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public void AdmissionOnlyContextLoadsPrimaryButRefusesPrivateDependencyAdmission()
    {
        using var temp = new TempTestDirectory("sts2-step33-admission-context");
        var primaryPath = Path.Combine(temp.Path, "sts2.dll");
        var dependencyPath = Path.Combine(temp.Path, "GameDependency.dll");
        WriteAssembly(primaryPath, "sts2");
        WriteAssembly(dependencyPath, "GameDependency");

        var primaryIdentity = AssemblyName.GetAssemblyName(primaryPath);
        var dependencyIdentity = AssemblyName.GetAssemblyName(dependencyPath);
        var primaryBytes = File.ReadAllBytes(primaryPath);
        var dependencyBytes = File.ReadAllBytes(dependencyPath);
        var primarySha256 = Convert.ToHexString(SHA256.HashData(primaryBytes)).ToLowerInvariant();

        var primaryPlan = new RuntimeBindingPreparedAssembly(
            "sts2.dll",
            primaryIdentity.FullName!,
            Convert.ToHexString(SHA1.HashData(primaryBytes)).ToLowerInvariant(),
            primaryBytes.LongLength,
            true);
        var dependencyPlan = new RuntimeBindingPreparedAssembly(
            "GameDependency.dll",
            dependencyIdentity.FullName!,
            Convert.ToHexString(SHA1.HashData(dependencyBytes)).ToLowerInvariant(),
            dependencyBytes.LongLength,
            false);
        var plan = new RuntimeFrameworkBindingPlanDocument(
            RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion,
            SteamOfflineInstallInspection.TargetAppId,
            2868842,
            1,
            "public",
            "Managed",
            primaryPlan.RelativePath,
            primaryPlan.AssemblyFullName,
            [primaryPlan, dependencyPlan],
            [],
            [],
            [],
            true);
        var entries = new[]
        {
            new TransformedRealStS2AssemblyAdmission.PreparedAdmissionEntry(primaryPlan, primaryPath, primaryIdentity, 0),
            new TransformedRealStS2AssemblyAdmission.PreparedAdmissionEntry(dependencyPlan, dependencyPath, dependencyIdentity, 0),
        };

        var context = new TransformedRealStS2AssemblyAdmission.Step33AdmissionLoadContext(
            "Step33-Test",
            plan,
            entries,
            isCollectible: true);
        try
        {
            var loaded = context.LoadPrimary(primaryPath, primarySha256);
            Assert.AreSame(context, AssemblyLoadContext.GetLoadContext(loaded));
            Assert.AreEqual("sts2", loaded.GetName().Name);
            Assert.AreEqual(1, context.Assemblies.Count());
            Assert.AreEqual(0, context.ManagedResolverRequests.Count);

            Assert.ThrowsExactly<FileLoadException>(() =>
                context.LoadFromAssemblyName(new AssemblyName(dependencyPlan.AssemblyFullName)));
            Assert.AreEqual(1, context.PrivateDependencyRequests.Count);
            Assert.AreEqual(0, context.NativeLoadAttempts.Count);
            Assert.AreEqual(1, context.Assemblies.Count());
        }
        finally
        {
            context.Unload();
        }
    }

    private static void WriteAssembly(string path, string name)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(name, new Version(1, 0, 0, 0)),
            name,
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var type = new TypeDefinition("Fixture", "Marker", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(type);
        var method = new MethodDefinition("Ping", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, module.TypeSystem.Void);
        method.Body.GetILProcessor().Append(method.Body.GetILProcessor().Create(OpCodes.Ret));
        type.Methods.Add(method);
        assembly.Write(path);
    }
}
