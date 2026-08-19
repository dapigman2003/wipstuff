using System.Runtime.Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class HostFrameworkClosureFoundationTests
{
    [TestMethod]
    public void RootSet_IsMeasuredUniqueAndFrameworkOnly()
    {
        Assert.AreEqual(22, HostFrameworkClosureRootSet.DirectTrimmerRoots.Count);
        Assert.AreEqual(44, HostFrameworkClosureRootSet.ExpectedHostClosure.Count);
        Assert.AreEqual(HostFrameworkClosureRootSet.DirectTrimmerRoots.Count,
            HostFrameworkClosureRootSet.DirectTrimmerRoots.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.AreEqual(HostFrameworkClosureRootSet.ExpectedHostClosure.Count,
            HostFrameworkClosureRootSet.ExpectedHostClosure.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        foreach (var name in HostFrameworkClosureRootSet.DirectTrimmerRoots)
            Assert.IsTrue(IsFrameworkShape(name), $"Unexpected non-framework Step 22 root: {name}");
        foreach (var spec in HostFrameworkClosureRootSet.ExpectedHostClosure)
        {
            Assert.IsTrue(IsFrameworkShape(spec.Name), $"Unexpected non-framework Step 22 closure identity: {spec.Name}");
            Assert.IsTrue(spec.MinimumVersion > new Version(0, 0, 0, 0));
            Assert.AreEqual(16, spec.PublicKeyToken.Length);
        }
    }

    [TestMethod]
    public void DirectRootSeeds_AreLoadableOnNet9Host()
    {
        // Codemagic runs these host tests before iOS publish. This catches misspelled/nonexistent
        // framework seed names early. The physical iPhone Gate A remains authoritative for the
        // iOS-specific 44-name transitive closure after trimming/AOT/linking.
        foreach (var name in HostFrameworkClosureRootSet.DirectTrimmerRoots)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new System.Reflection.AssemblyName(name));
            Assert.IsTrue(string.Equals(name, assembly.GetName().Name, StringComparison.OrdinalIgnoreCase), $"Host returned wrong assembly for {name}");
        }
    }

    [TestMethod]
    public void GateSequence_StopsAtFirstFailureAndPassesFourOfFour()
    {
        var sequence = new HostFrameworkClosureGateSequence();
        sequence.Record(new HostFrameworkClosureGateResult(HostFrameworkClosureGate.RootedHostAvailability, true, "a"));
        sequence.Record(new HostFrameworkClosureGateResult(HostFrameworkClosureGate.BindingClosureRecompute, false, "b"));
        var failed = sequence.Snapshot();
        Assert.AreEqual(1, failed.PassedCount);
        Assert.AreEqual(HostFrameworkClosureGate.BindingClosureRecompute, failed.FirstFailingGate);

        sequence.Reset();
        foreach (HostFrameworkClosureGate gate in Enum.GetValues<HostFrameworkClosureGate>())
            sequence.Record(new HostFrameworkClosureGateResult(gate, true, gate.ToString()));
        var passed = sequence.Snapshot();
        Assert.AreEqual(4, passed.PassedCount);
        Assert.IsNull(passed.FirstFailingGate);
        StringAssert.Contains(passed.Summary, "PASS — 4/4");
    }

    private static bool IsFrameworkShape(string name)
        => name.Equals("netstandard", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("System", StringComparison.OrdinalIgnoreCase) ||
           name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.CSharp", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.VisualBasic", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.VisualBasic.Core", StringComparison.OrdinalIgnoreCase) ||
           name.StartsWith("Microsoft.Win32.", StringComparison.OrdinalIgnoreCase);
}
