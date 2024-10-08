using System;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace SamplePlugin;

public class DalamudApi
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;

    [PluginService] public static IDataManager DataManager { get; set; } = null!;

    [PluginService] public static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; set; }
    [PluginService] public static IPluginLog PluginLog { get; set; }
    [PluginService] public static IClientState ClientState { get; set; }
    [PluginService] public static IObjectTable ObjectTable { get; set; }
    [PluginService] public static ISigScanner SigScanner { get; set; }
    [PluginService] public static IKeyState KeyState { get; set; }

    public static GameObjectManager GameObjectManager;

    public static Configuration config = new();


    internal static bool IsInitialized = false;
    public static void Init(IDalamudPluginInterface pi)
    {
        if (IsInitialized)
        {
            PluginLog.Info("Services already initialized, skipping");
        }
        IsInitialized = true;
        try
        {
            pi.Create<DalamudApi>();
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error initalising {nameof(DalamudApi)}", ex);
        }
    }
}
