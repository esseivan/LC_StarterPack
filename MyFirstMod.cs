using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using MyFirstMod.Patches;

namespace MyFirstMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
//[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
//[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class MyFirstMod : BaseUnityPlugin
{
    public static MyFirstMod Instance { get; private set; } = null!;
    internal static new ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private ConfigEntry<string> configGreeting;
    private ConfigEntry<bool> configDisplayGreeting;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();

        configGreeting = Config.Bind(
            "General", // The section under which the option is shown
            "GreetingText", // The key of the configuration option in the configuration file
            "Hello, world!", // The default value
            "A greeting text to show when the game is launched"
        ); // Description of the option to show in the config file

        configDisplayGreeting = Config.Bind(
            "General.Toggles",
            "DisplayGreeting",
            true,
            "Whether or not to show the greeting text"
        );

        if (configDisplayGreeting.Value)
            Logger.LogDebug(configGreeting.Value);

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll(typeof(LightPatch));
        Harmony.PatchAll(typeof(PatchGrabbable));
        Harmony.PatchAll(typeof(ExampleScreenPatch));
        //Harmony.PatchAll(typeof(ExampleTVPatch)); // No longer working in v72

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}
