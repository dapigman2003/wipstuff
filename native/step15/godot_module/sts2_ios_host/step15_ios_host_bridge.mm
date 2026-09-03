#import <Foundation/Foundation.h>
#import <QuartzCore/QuartzCore.h>
#import <QuartzCore/CAMetalLayer.h>
#import <UIKit/UIKit.h>

#import "drivers/apple_embedded/app_delegate_service.h"
#import "drivers/apple_embedded/godot_view_apple_embedded.h"
#import "drivers/apple_embedded/godot_view_renderer.h"
#import "drivers/apple_embedded/os_apple_embedded.h"
#import "drivers/apple_embedded/view_controller.h"

#include "core/config/engine.h"
#include "core/config/project_settings.h"
#include "modules/mono/csharp_script.h"
#include "modules/mono/glue/runtime_interop.h"
#include "modules/mono/mono_gd/gd_mono.h"
#include "modules/mono/mono_gd/gd_mono_cache.h"
#include "core/io/dir_access.h"
#include "core/io/file_access.h"
#include "core/os/os.h"
#include "core/string/ustring.h"
#include "core/version.h"

#include <atomic>
#include <cstdlib>
#include <cstring>
#include <string>
#include <vector>

extern int apple_embedded_main(int argc, char **argv);

#define STS2_STEP15_EXPORT extern "C" __attribute__((visibility("default"))) __attribute__((used))

namespace {
GDTViewController *g_controller = nil;
GDTView *g_view = nil;
NSArray<id> *g_lifecycle_tokens = nil;
std::atomic<int> g_started{ 0 };
std::atomic<int> g_process_restart_required{ 0 };
std::atomic<int> g_background_count{ 0 };
std::atomic<int> g_foreground_count{ 0 };
std::atomic<int> g_focus_out_count{ 0 };
std::atomic<int> g_focus_in_count{ 0 };
std::atomic<int> g_external_managed_bridge_installed{ 0 };
std::atomic<int> g_external_core_api_signal_returned{ 0 };
std::string g_last_error;

void set_error(const char *message) {
    g_last_error = message ? message : "unknown native Step 15 error";
}

bool on_main_thread() {
    return [NSThread isMainThread];
}

String marker_absolute_path(const char *path) {
    if (ProjectSettings::get_singleton() == nullptr) {
        return String();
    }
    return ProjectSettings::get_singleton()->globalize_path(String::utf8(path));
}

bool marker_exists(const char *path) {
    if (!g_started.load()) {
        return false;
    }
    const String absolute_path = marker_absolute_path(path);
    return !absolute_path.is_empty() && FileAccess::exists(absolute_path);
}

bool reset_marker(const char *path) {
    const String absolute_path = marker_absolute_path(path);
    if (absolute_path.is_empty()) {
        return false;
    }
    if (FileAccess::exists(absolute_path)) {
        DirAccess::remove_absolute(absolute_path);
    }
    return !FileAccess::exists(absolute_path);
}

void install_lifecycle_forwarders() {
    if (g_lifecycle_tokens != nil) {
        return;
    }

    NSNotificationCenter *center = [NSNotificationCenter defaultCenter];
    id focus_out = [center addObserverForName:UIApplicationWillResignActiveNotification
                                       object:nil
                                        queue:[NSOperationQueue mainQueue]
                                   usingBlock:^(NSNotification *note) {
        (void)note;
        g_focus_out_count.fetch_add(1);
        if (OS_AppleEmbedded::get_singleton()) {
            OS_AppleEmbedded::get_singleton()->on_focus_out();
        }
    }];
    id background = [center addObserverForName:UIApplicationDidEnterBackgroundNotification
                                        object:nil
                                         queue:[NSOperationQueue mainQueue]
                                    usingBlock:^(NSNotification *note) {
        (void)note;
        g_background_count.fetch_add(1);
        if (OS_AppleEmbedded::get_singleton()) {
            OS_AppleEmbedded::get_singleton()->on_enter_background();
        }
    }];
    id foreground = [center addObserverForName:UIApplicationWillEnterForegroundNotification
                                        object:nil
                                         queue:[NSOperationQueue mainQueue]
                                    usingBlock:^(NSNotification *note) {
        (void)note;
        g_foreground_count.fetch_add(1);
        if (OS_AppleEmbedded::get_singleton()) {
            OS_AppleEmbedded::get_singleton()->on_exit_background();
        }
    }];
    id focus_in = [center addObserverForName:UIApplicationDidBecomeActiveNotification
                                      object:nil
                                       queue:[NSOperationQueue mainQueue]
                                  usingBlock:^(NSNotification *note) {
        (void)note;
        g_focus_in_count.fetch_add(1);
        if (OS_AppleEmbedded::get_singleton()) {
            OS_AppleEmbedded::get_singleton()->on_focus_in();
        }
    }];
    g_lifecycle_tokens = @[ focus_out, background, foreground, focus_in ];
}
}

STS2_STEP15_EXPORT const char *sts2_step15_get_engine_version() {
    return GODOT_VERSION_NUMBER "-" GODOT_VERSION_STATUS;
}

STS2_STEP15_EXPORT const char *sts2_step15_last_error() {
    return g_last_error.c_str();
}

STS2_STEP15_EXPORT int sts2_step15_is_engine_started() {
    return g_started.load();
}

STS2_STEP15_EXPORT int sts2_step15_requires_process_restart() {
    return g_process_restart_required.load();
}

STS2_STEP15_EXPORT int sts2_step15_start(void *parent_controller_handle, void *container_view_handle, const char *project_path_utf8) {
    if (!on_main_thread()) {
        set_error("Godot host start must run on the UIKit main thread.");
        return 10;
    }
    if (g_started.load()) {
        set_error("Godot host is already started in this process.");
        return 11;
    }
    if (g_process_restart_required.load()) {
        set_error("A Godot initialization attempt already touched process-global engine state. Force-quit and relaunch before another attempt.");
        return 18;
    }
    if (parent_controller_handle == nullptr || container_view_handle == nullptr || project_path_utf8 == nullptr || project_path_utf8[0] == '\0') {
        set_error("Godot host start received a null/empty UIKit handle or project path.");
        return 12;
    }

    UIViewController *parent = (__bridge UIViewController *)parent_controller_handle;
    UIView *container = (__bridge UIView *)container_view_handle;
    if (CGRectGetWidth(container.bounds) < 1.0 || CGRectGetHeight(container.bounds) < 1.0) {
        set_error("The Godot host container has zero/invalid bounds. Let UIKit finish layout before starting the engine.");
        return 17;
    }
    NSString *project_path = [NSString stringWithUTF8String:project_path_utf8];
    if (project_path == nil || ![[NSFileManager defaultManager] fileExistsAtPath:[project_path stringByAppendingPathComponent:@"project.godot"]]) {
        set_error("Step 15 project.godot was not found in the bundled smoke-project directory.");
        return 13;
    }

    NSString *executable = [[NSBundle mainBundle] executablePath];
    if (executable == nil) {
        set_error("NSBundle executablePath was unavailable.");
        return 14;
    }

    // Godot's Apple-embedded display server still reads
    // UIApplication.sharedApplication.delegate.window for selected window and
    // orientation queries. Under our UIScene launcher, point that inherited
    // delegate property at the already-existing scene window; never create or
    // replace the launcher window here.
    UIWindow *host_window = parent.view.window ?: container.window;
    id app_delegate = [UIApplication sharedApplication].delegate;
    if (host_window == nil) {
        set_error("The launcher host UIWindow was unavailable for embedded Godot.");
        return 15;
    }
    if (app_delegate == nil || ![app_delegate respondsToSelector:@selector(setWindow:)]) {
        set_error("UIApplication delegate does not expose the window property required by Godot's Apple-embedded display server.");
        return 16;
    }
    [(id)app_delegate setWindow:host_window];

    std::vector<std::string> args = {
        std::string([executable UTF8String]),
        "--path",
        std::string(project_path_utf8),
        "--rendering-method",
        "mobile",
        "--rendering-driver",
        "metal",
    };
    std::vector<char *> argv;
    argv.reserve(args.size());
    for (std::string &arg : args) {
        argv.push_back(arg.data());
    }

    // apple_embedded_main creates Godot process-global OS/Main state before
    // Main::setup can succeed or fail. Upstream's standalone iOS host exits on
    // setup failure rather than retrying in-process. Once this call is entered,
    // this diagnostic host likewise requires a process relaunch before any new
    // Godot attempt or unrelated launcher regression.
    g_process_restart_required.store(1);
    const int main_result = apple_embedded_main((int)argv.size(), argv.data());
    if (main_result != EXIT_SUCCESS) {
        set_error("apple_embedded_main returned a failure result.");
        return 20 + main_result;
    }

    // Gate C/D markers must belong to this engine session, not a previous process run.
    // Refuse to continue if a stale marker cannot be removed; otherwise Gate C/D
    // could falsely pass on evidence from an earlier process.
    if (!reset_marker("user://sts2_step15_render_ready.txt")) {
        set_error("Could not reset the Step 15 render marker for this engine session.");
        return 31;
    }
    if (!reset_marker("user://sts2_step15_touch_ready.txt")) {
        set_error("Could not reset the Step 15 touch marker for this engine session.");
        return 32;
    }

    g_controller = [[GDTViewController alloc] initWithNibName:nil bundle:nil];
    (void)g_controller.view;
    g_view = g_controller.godotView;
    if (g_view == nil) {
        set_error("Godot GDTViewController did not create a GDTView.");
        return 30;
    }

    // Godot's Apple-embedded DisplayServer resolves its rendering view through
    // GDTAppDelegateService.viewController. The normal standalone app delegate
    // sets that static slot; our embedded host must set it explicitly.
    [GDTAppDelegateService sts2_setEmbeddedViewController:g_controller];

    g_view.useCADisplayLink = YES;
    g_view.renderingInterval = 1.0 / 60.0;
    g_controller.view.frame = container.bounds;
    g_controller.view.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    [parent addChildViewController:g_controller];
    [container addSubview:g_controller.view];
    [g_controller didMoveToParentViewController:parent];

    g_background_count.store(0);
    g_foreground_count.store(0);
    g_focus_out_count.store(0);
    g_focus_in_count.store(0);
    install_lifecycle_forwarders();

    [g_view startRendering];
    g_started.store(1);
    g_last_error.clear();
    return 0;
}

STS2_STEP15_EXPORT int sts2_step15_is_setup_finished() {
    if (!g_started.load() || g_view == nil || g_view.renderer == nil) {
        return 0;
    }
    return [g_view.renderer hasFinishedSetup] ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_is_rendering_active() {
    return (g_started.load() && g_view != nil && g_view.isActive && g_view.canRender) ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_is_metal_layer_ready() {
    if (!g_started.load() || g_view == nil || g_view.renderingLayer == nil) {
        return 0;
    }
    // Require the concrete Core Animation Metal layer instead of relying on a
    // class-name substring. This makes Gate C evidence match the renderer type
    // Godot's Metal display-server path actually installs.
    return [g_view.renderingLayer isKindOfClass:[CAMetalLayer class]] ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_stop_rendering() {
    if (!on_main_thread() || !g_started.load() || g_view == nil) {
        return 0;
    }
    [g_view stopRendering];
    return (!g_view.isActive && !g_view.canRender) ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_start_rendering() {
    if (!on_main_thread() || !g_started.load() || g_view == nil) {
        return 0;
    }
    [g_view startRendering];
    return (g_view.isActive && g_view.canRender) ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_render_marker_ready() {
    return marker_exists("user://sts2_step15_render_ready.txt") ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_touch_marker_ready() {
    return marker_exists("user://sts2_step15_touch_ready.txt") ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_background_count() {
    return g_background_count.load();
}

STS2_STEP15_EXPORT int sts2_step15_foreground_count() {
    return g_foreground_count.load();
}

STS2_STEP15_EXPORT int sts2_step15_focus_out_count() {
    return g_focus_out_count.load();
}

STS2_STEP15_EXPORT int sts2_step15_focus_in_count() {
    return g_focus_in_count.load();
}


// Step 35.0.18 diagnostic-only bridge: expose the exact Godot 4.5.1 runtime
// interop callback table from the same source-built engine that owns the live
// ClassDB/StringName/Engine state. This does not initialize managed GodotSharp,
// call any callback, or load a .NET runtime. The managed Step-35 probe decides
// whether/when to pass this table to its separately verified GodotSharp clone.
STS2_STEP15_EXPORT int sts2_step15_is_runtime_interop_ready() {
    if (!on_main_thread() || !g_started.load()) {
        return 0;
    }
    if (Engine::get_singleton() == nullptr || ProjectSettings::get_singleton() == nullptr) {
        return 0;
    }
    if (CSharpLanguage::get_singleton() == nullptr || GDMono::get_singleton() == nullptr) {
        return 0;
    }
    return 1;
}

STS2_STEP15_EXPORT int sts2_step15_has_dotnet_feature() {
    return OS::get_singleton() != nullptr && OS::get_singleton()->has_feature("dotnet") ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_is_dotnet_runtime_initialized() {
    GDMono *gdmono = GDMono::get_singleton();
    return gdmono != nullptr && gdmono->is_runtime_initialized() ? 1 : 0;
}

// Step 35.0.23 diagnostic-only reverse-binding readiness. These exports
// read existing Godot C# native-module state only. They do not initialize the
// Godot-managed runtime, mutate GDMonoCache, create instance bindings, or call
// any managed callback.
STS2_STEP15_EXPORT int sts2_step15_has_csharp_language_singleton() {
    return CSharpLanguage::get_singleton() != nullptr ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_is_godot_api_cache_updated() {
    return GDMonoCache::godot_api_cache_updated ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_has_managed_create_binding_callback() {
    return GDMonoCache::managed_callbacks.ScriptManagerBridge_CreateManagedForGodotObjectBinding != nullptr ? 1 : 0;
}

STS2_STEP15_EXPORT int sts2_step15_is_reverse_binding_ready() {
    return CSharpLanguage::get_singleton() != nullptr &&
                   GDMonoCache::godot_api_cache_updated &&
                   GDMonoCache::managed_callbacks.ScriptManagerBridge_CreateManagedForGodotObjectBinding != nullptr
            ? 1
            : 0;
}

// Step 35.0.23 diagnostic-only external-host bridge adoption. This does not
// start hostfxr/CoreCLR, load a game native image, or fabricate individual
// callback pointers. The managed launcher reproduces the generated GodotPlugins
// bootstrap in its existing CLR and supplies the complete ManagedCallbacks
// struct produced by GodotSharp ManagedCallbacks.Create(IntPtr).
STS2_STEP15_EXPORT int sts2_step15_get_managed_callbacks_size() {
    return (int)sizeof(GDMonoCache::ManagedCallbacks);
}

STS2_STEP15_EXPORT int sts2_step15_is_external_managed_bridge_installed() {
    return g_external_managed_bridge_installed.load();
}

STS2_STEP15_EXPORT int sts2_step15_install_external_managed_callbacks(const void *p_callbacks, int p_size) {
    if (!on_main_thread() || !g_started.load()) {
        set_error("External managed callback installation requires the live Step 15 engine on the main thread.");
        return 0;
    }
    if (CSharpLanguage::get_singleton() == nullptr || GDMono::get_singleton() == nullptr) {
        set_error("External managed callback installation requires CSharpLanguage and GDMono native scaffolding.");
        return 0;
    }
    if (GDMono::get_singleton()->is_runtime_initialized()) {
        set_error("External managed callback installation refuses to compete with a Godot-owned initialized .NET runtime.");
        return 0;
    }
    if (GDMonoCache::godot_api_cache_updated || g_external_managed_bridge_installed.load()) {
        set_error("External managed callback installation is single-shot and requires an untouched Godot API cache.");
        return 0;
    }
    if (p_callbacks == nullptr || p_size != (int)sizeof(GDMonoCache::ManagedCallbacks)) {
        set_error("External managed callback table was null or did not match sizeof(GDMonoCache::ManagedCallbacks).");
        return 0;
    }

    GDMonoCache::ManagedCallbacks callbacks{};
    memcpy(&callbacks, p_callbacks, sizeof(callbacks));
    if (callbacks.ScriptManagerBridge_CreateManagedForGodotObjectBinding == nullptr ||
            callbacks.GD_OnCoreApiAssemblyLoaded == nullptr) {
        set_error("External managed callback table is missing required reverse-binding/core-API callbacks.");
        return 0;
    }

    // This is the same cache-adoption operation performed by normal Godot managed initialization
    // immediately after GodotPlugins.Game.Main.InitializeFromGameProject returns.
    // It copies the complete callback struct by value; it does not invoke a callback.
    GDMonoCache::update_godot_api_cache(callbacks);
    if (!GDMonoCache::godot_api_cache_updated ||
            GDMonoCache::managed_callbacks.ScriptManagerBridge_CreateManagedForGodotObjectBinding == nullptr) {
        set_error("Godot API cache did not become reverse-binding ready after external callback adoption.");
        return 0;
    }

    g_external_managed_bridge_installed.store(1);
    g_last_error.clear();
    return 1;
}

STS2_STEP15_EXPORT int sts2_step15_signal_external_core_api_loaded() {
    if (!on_main_thread() || !g_started.load()) {
        set_error("External core-API-loaded signal requires the live Step 15 engine on the main thread.");
        return 0;
    }
    if (!g_external_managed_bridge_installed.load() || !GDMonoCache::godot_api_cache_updated ||
            GDMonoCache::managed_callbacks.GD_OnCoreApiAssemblyLoaded == nullptr) {
        set_error("External core-API-loaded signal requires the installed managed callback cache.");
        return 0;
    }
    if (g_external_core_api_signal_returned.load()) {
        set_error("External core-API-loaded signal is single-shot.");
        return 0;
    }

#ifdef DEBUG_ENABLED
    const bool debug = true;
#else
    const bool debug = false;
#endif
    GDMonoCache::managed_callbacks.GD_OnCoreApiAssemblyLoaded(debug);
    g_external_core_api_signal_returned.store(1);
    g_last_error.clear();
    return 1;
}

STS2_STEP15_EXPORT int sts2_step15_did_external_core_api_signal_return() {
    return g_external_core_api_signal_returned.load();
}

STS2_STEP15_EXPORT const void *sts2_step15_get_runtime_interop_funcs(int *r_size) {
    if (r_size != nullptr) {
        *r_size = 0;
    }
    if (!sts2_step15_is_runtime_interop_ready()) {
        set_error("Godot runtime interop callbacks requested before the Step 15 engine/C# native module reached the required main-thread ready state.");
        return nullptr;
    }

    int32_t size = 0;
    const void **callbacks = godotsharp::get_runtime_interop_funcs(size);
    if (callbacks == nullptr || size <= 0 || (size % (int32_t)sizeof(void *)) != 0) {
        set_error("Godot runtime interop callback table was null, empty, or not pointer-size aligned.");
        return nullptr;
    }

    const int32_t count = size / (int32_t)sizeof(void *);
    for (int32_t i = 0; i < count; i++) {
        if (callbacks[i] == nullptr) {
            set_error("Godot runtime interop callback table contained a null function pointer.");
            return nullptr;
        }
    }

    if (r_size != nullptr) {
        *r_size = size;
    }
    g_last_error.clear();
    return callbacks;
}
