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
    internal static Harmony? Harmony { get; set; }

    private ConfigEntry<string> configGreeting;
    private ConfigEntry<bool> configDisplayGreeting;

    private ConfigEntry<bool> configEnableTests;

    private ConfigEntry<bool> configFreeTeleporter;
    private ConfigEntry<bool> configFreeInverseTeleporter;
    private ConfigEntry<bool> configWalkie;

    public static AssetBundle Assets;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();
        NetcodePatcher();

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

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        //Harmony.PatchAll(typeof(LightPatch));
        //Harmony.PatchAll(typeof(PatchGrabbable));
        //Harmony.PatchAll(typeof(ExampleScreenPatch));
        //Harmony.PatchAll(typeof(RoundManager));
        //Harmony.PatchAll(typeof(GameNetworkManager));
        //Harmony.PatchAll(typeof(StartOfRound));

        //Harmony.PatchAll(typeof(ExampleTVPatch)); // No longer working in v72

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}
