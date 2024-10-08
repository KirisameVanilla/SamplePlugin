using Dalamud.Game.Command;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using SamplePlugin.Windows;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/pmycommand";

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private MainWindow MainWindow { get; init; }
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        MainWindow = new MainWindow(this);
        DalamudApi.Init(pluginInterface);
        WindowSystem.AddWindow(MainWindow);

        DalamudApi.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        DalamudApi.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}
