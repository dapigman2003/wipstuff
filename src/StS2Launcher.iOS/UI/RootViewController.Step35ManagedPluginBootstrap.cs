using StS2Launcher.iOS.Platform;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private void RunStep35ManagedPluginBootstrap()
    {
        WriteStep35CrashCheckpoint($"CB_NATIVE_READY_RECHECK — engineStarted={GodotStep15NativeBridge.IsEngineStarted}; setup={GodotStep15NativeBridge.IsSetupFinished}; interopReady={GodotStep15NativeBridge.IsRuntimeInteropReady}; dotnetFeature={GodotStep15NativeBridge.HasDotNetFeature}; godotDotNetInitialized={GodotStep15NativeBridge.IsDotNetRuntimeInitialized}.");
        if (!GodotStep15NativeBridge.IsEngineStarted || !GodotStep15NativeBridge.IsSetupFinished ||
            !GodotStep15NativeBridge.IsRuntimeInteropReady || GodotStep15NativeBridge.HasDotNetFeature || GodotStep15NativeBridge.IsDotNetRuntimeInitialized)
            throw new InvalidOperationException("CORE-HANDOFF native readiness changed after Gate B; refusing to initialize the private GodotSharp derivative.");

        WriteStep35CrashCheckpoint("CB_NATIVE_TABLE_REQUEST_START — requesting pointer/size for the exact source-built Godot 4.5.1 runtime interop callback table on the main thread.");
        var callbackTable = GodotStep15NativeBridge.GetRuntimeInteropFunctions(out var callbackTableSizeBytes);
        WriteStep35CrashCheckpoint($"CB_NATIVE_TABLE_REQUEST_RETURNED — table=0x{callbackTable.ToInt64():X}; bytes={callbackTableSizeBytes}; nativeError='{GodotStep15NativeBridge.LastError}'.");
        if (callbackTable == IntPtr.Zero || callbackTableSizeBytes <= 0 || callbackTableSizeBytes % IntPtr.Size != 0)
            throw new InvalidOperationException("CORE-HANDOFF native callback table was null/empty/misaligned: " + GodotStep15NativeBridge.LastError);

        var handoffDetail = _transformedRealStS2VeryEarlyInitialization.RunGodotCoreCallbackHandoffInitialization(
            callbackTable,
            callbackTableSizeBytes,
            WriteStep35CrashCheckpoint);
        WriteStep35CrashCheckpoint("CB_MANAGED_HANDOFF_RETURNED — " + handoffDetail);

        var hasCSharpLanguage = GodotStep15NativeBridge.HasCSharpLanguageSingleton;
        var apiCacheUpdated = GodotStep15NativeBridge.IsGodotApiCacheUpdated;
        var hasCreateBindingCallback = GodotStep15NativeBridge.HasManagedCreateBindingCallback;
        var reverseBindingReady = GodotStep15NativeBridge.IsReverseBindingReady;
        var externalBridgeInstalled = GodotStep15NativeBridge.IsExternalManagedBridgeInstalled;
        WriteStep35CrashCheckpoint(
            $"CB_REVERSE_BINDING_STATE_BEFORE — csharpLanguage={hasCSharpLanguage}; godotApiCacheUpdated={apiCacheUpdated}; createManagedBindingCallback={hasCreateBindingCallback}; reverseBindingReady={reverseBindingReady}; externalBridgeInstalled={externalBridgeInstalled}; godotDotNetInitialized={GodotStep15NativeBridge.IsDotNetRuntimeInitialized}.");

        if (!hasCSharpLanguage || apiCacheUpdated || hasCreateBindingCallback || reverseBindingReady || externalBridgeInstalled || GodotStep15NativeBridge.IsDotNetRuntimeInitialized)
            throw new InvalidOperationException("CORE-HANDOFF Step-35.0.25 requires the physically proven 0.0.145 baseline: CSharpLanguage present, Godot API cache/reverse callbacks absent, no prior external bridge, and no Godot-owned .NET runtime.");
        WriteStep35CrashCheckpoint("CB_REVERSE_BASELINE_PASS — physical 0.0.145 missing-reverse-binding baseline reproduced; starting one coordinated generated-plugin bootstrap experiment instead of another callsite bypass/probe.");

        var managedCallbacksSizeBytes = GodotStep15NativeBridge.ManagedCallbacksSizeBytes;
        WriteStep35CrashCheckpoint($"CB_MANAGED_CALLBACKS_SIZE — native sizeof(GDMonoCache::ManagedCallbacks)={managedCallbacksSizeBytes} bytes.");
        if (managedCallbacksSizeBytes <= 0 || managedCallbacksSizeBytes % IntPtr.Size != 0)
            throw new InvalidOperationException("CORE-HANDOFF native ManagedCallbacks size was invalid: " + managedCallbacksSizeBytes);

        var managedCallbacks = _transformedRealStS2VeryEarlyInitialization.PrepareGodotManagedPluginReverseBridge(
            managedCallbacksSizeBytes,
            WriteStep35CrashCheckpoint);
        WriteStep35CrashCheckpoint($"CB_REVERSE_PREP_RETURNED — managed generated-plugin bootstrap preparation returned {managedCallbacks.Length} callback bytes; native cache has not been mutated yet.");
        if (managedCallbacks.Length != managedCallbacksSizeBytes)
            throw new InvalidOperationException($"Managed callback preparation returned {managedCallbacks.Length} bytes but native expects {managedCallbacksSizeBytes}.");

        WriteStep35CrashCheckpoint("CB_NATIVE_REVERSE_INSTALL_START — copying the complete ManagedCallbacks struct into GDMonoCache via the same update_godot_api_cache operation used by Godot 4.5.1 after its generated plugin initializer returns. No callback is intentionally invoked by this install export.");
        var installReturned = GodotStep15NativeBridge.InstallExternalManagedCallbacks(managedCallbacks);
        WriteStep35CrashCheckpoint($"CB_NATIVE_REVERSE_INSTALL_RETURNED — returned={installReturned}; nativeError='{GodotStep15NativeBridge.LastError}'.");
        if (!installReturned)
            throw new InvalidOperationException("CORE-HANDOFF external ManagedCallbacks cache adoption failed: " + GodotStep15NativeBridge.LastError);

        apiCacheUpdated = GodotStep15NativeBridge.IsGodotApiCacheUpdated;
        hasCreateBindingCallback = GodotStep15NativeBridge.HasManagedCreateBindingCallback;
        reverseBindingReady = GodotStep15NativeBridge.IsReverseBindingReady;
        externalBridgeInstalled = GodotStep15NativeBridge.IsExternalManagedBridgeInstalled;
        WriteStep35CrashCheckpoint(
            $"CB_REVERSE_BINDING_STATE_AFTER_INSTALL — csharpLanguage={GodotStep15NativeBridge.HasCSharpLanguageSingleton}; godotApiCacheUpdated={apiCacheUpdated}; createManagedBindingCallback={hasCreateBindingCallback}; reverseBindingReady={reverseBindingReady}; externalBridgeInstalled={externalBridgeInstalled}; godotDotNetInitialized={GodotStep15NativeBridge.IsDotNetRuntimeInitialized}.");
        if (!apiCacheUpdated || !hasCreateBindingCallback || !reverseBindingReady || !externalBridgeInstalled || GodotStep15NativeBridge.IsDotNetRuntimeInitialized)
            throw new InvalidOperationException("CORE-HANDOFF cache adoption returned but native reverse-binding readiness did not become complete, or Godot unexpectedly claimed runtime ownership.");
        WriteStep35CrashCheckpoint("CB_REVERSE_CACHE_ADOPTION_PASS — complete managed callback struct is installed and reverse instance-binding readiness is now present; Godot-owned runtime_initialized intentionally remains false.");

        // Godot 4.5.1 calls GD_OnCoreApiAssemblyLoaded immediately after update_godot_api_cache.
        // Keep this as its own durable boundary because it is the first deliberate native->managed callback
        // in the externally hosted bridge and therefore physically proves whether private GodotSharp reverse
        // unmanaged entry thunks are callable on this iOS runtime.
        WriteStep35CrashCheckpoint("CB_CORE_API_SIGNAL_START — invoking the standard GD_OnCoreApiAssemblyLoaded callback through Godot's installed ManagedCallbacks table.");
        var coreApiSignalReturned = GodotStep15NativeBridge.SignalExternalCoreApiLoaded();
        WriteStep35CrashCheckpoint($"CB_CORE_API_SIGNAL_RETURNED — returned={coreApiSignalReturned}; nativeReturnedFlag={GodotStep15NativeBridge.DidExternalCoreApiSignalReturn}; nativeError='{GodotStep15NativeBridge.LastError}'.");
        if (!coreApiSignalReturned || !GodotStep15NativeBridge.DidExternalCoreApiSignalReturn)
            throw new InvalidOperationException("CORE-HANDOFF GD_OnCoreApiAssemblyLoaded signal failed: " + GodotStep15NativeBridge.LastError);

        WriteStep35CrashCheckpoint("CB_MANAGED_PLUGIN_BOOTSTRAP_PASS — generated game-plugin managed substeps, complete reverse callback cache adoption, and standard core-API-loaded callback all returned; sealing the exact physical 0.0.146 bootstrap resolver delta before unchanged NATURAL Gate C. Godot GDMono runtime_initialized remains deliberately unclaimed by the launcher.");
        var resolverBaselineDetail = _transformedRealStS2VeryEarlyInitialization.SealGodotManagedPluginBootstrapResolverBaseline(WriteStep35CrashCheckpoint);
        WriteStep35CrashCheckpoint("CB_POST_BOOTSTRAP_RESOLVER_BASELINE_RETURNED — " + resolverBaselineDetail);
    }
}
