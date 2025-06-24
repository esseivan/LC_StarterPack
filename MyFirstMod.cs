using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using MyFirstMod.Patches;
using UnityEngine;

namespace MyFirstMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
//[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
//[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class MyFirstMod : BaseUnityPlugin
{
    public static MyFirstMod Instance { get; private set; } = null!;
    internal static new ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony Harmony { get; set; }

    public static ConfigEntry<string> configGreeting;
    public static ConfigEntry<bool> configDisplayGreeting;
    public static ConfigEntry<bool> configEnableTests;
    public static ConfigEntry<bool> configFreeTeleporter;
    public static ConfigEntry<bool> configFreeInverseTeleporter;
    public static ConfigEntry<bool> configWalkie;
    public static ConfigEntry<bool> configStartWithExtraCredits;
    public static ConfigEntry<int> configStartWithExtraCreditsValue;

    public static AssetBundle Assets;

    private void SetupConfigBinds()
    {
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

        configEnableTests = Config.Bind(
            "General.Toggles",
            "EnableTests",
            false,
            "Enable test features and debug functionality"
        );

        configFreeTeleporter = Config.Bind(
            "Items",
            "FreeTeleporter",
            true,
            "Automatically unlock teleporter at game start"
        );

        configFreeInverseTeleporter = Config.Bind(
            "Items",
            "FreeInverseTeleporter",
            false,
            "Automatically unlock inverse teleporter at game start"
        );

        configWalkie = Config.Bind("Items", "FreeWalkie", false, "Start with free walkie-talkie");

        configStartWithExtraCredits = Config.Bind(
            "Economy",
            "StartWithExtraCredits",
            false,
            "Start new games with extra credits"
        );

        configStartWithExtraCreditsValue = Config.Bind(
            "Economy",
            "StartingCreditsAmount",
            100,
            "Starting credit value (if enabled)"
        );
    }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        SetupConfigBinds();

        Patch();
        NetcodePatcher();

        if (configDisplayGreeting.Value)
            Logger.LogDebug(configGreeting.Value);

        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Assets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "esnassets"));

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void NetcodePatcher()
    {
        Logger.LogDebug("Patching netcode...");
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
            );
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(
                    typeof(RuntimeInitializeOnLoadMethodAttribute),
                    false
                );
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
        Logger.LogDebug("Patching netcode... complete");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        if (configEnableTests.Value)
        {
            Harmony.PatchAll();
        }
        else
        {
            bool patch_StartWithExtras =
                configStartWithExtraCredits.Value
                || configFreeTeleporter.Value
                || configFreeInverseTeleporter.Value;
            if (patch_StartWithExtras)
            {
                Logger.LogDebug("Patching StartWithExtras");
                Harmony.PatchAll(typeof(StartWithExtras));
            }
        }

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}
