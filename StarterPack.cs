using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using Newtonsoft.Json;
using StarterPack.Patches;
using TerminalApi;
using TerminalApi.Classes;
using UnityEngine;
using static TerminalApi.TerminalApi;

namespace StarterPack;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("atomic.terminalapi", MinimumDependencyVersion: "1.5.0")]
//[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
//[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class StarterPack : BaseUnityPlugin
{
    public static StarterPack Instance { get; private set; } = null!;
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

        configWalkie = Config.Bind(
            "Items",
            "FreeWalkie",
            false,
            "Start with free walkie-talkie. Not yet implemented !"
        );

        configStartWithExtraCredits = Config.Bind(
            "Economy",
            "StartWithExtraCredits",
            false,
            "Start new games with extra credits"
        );

        configStartWithExtraCreditsValue = Config.Bind(
            "Economy",
            "StartingCreditsAmount",
            101,
            "Starting credit value (if enabled)"
        );
    }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        SetupConfigBinds();

        LoadAssets();

        Patch();

        CreateTerminalCommands();

        if (configDisplayGreeting.Value)
            Logger.LogDebug(configGreeting.Value);

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void LoadAssets()
    {
        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Assets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "assets/esnassets"));
        if (Assets == null)
        {
            Logger.LogError("Failed to load custom assets.");
            return;
        }

        Logger.LogDebug($"Assets loaded");
        string[] names = Assets.GetAllAssetNames();
        foreach (string name in names)
        {
            Logger.LogDebug($"{name}");
        }
    }

    private static void CreateTerminalCommands()
    {
        AddCommand(
            "time",
            new CommandInfo
            {
                Category = "other",
                Description = "Displays the current time.",
                DisplayTextSupplier = Commands.CheckTimeCMD,
            },
            "check"
        );

        AddCommand(
            "test",
            new CommandInfo
            {
                Category = "other",
                Description = "Testing command",
                DisplayTextSupplier = Commands.TestCMD,
            }
        );
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
            NetcodePatcher();
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

                Harmony.PatchAll(typeof(TestTerminalNodePatch));
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
