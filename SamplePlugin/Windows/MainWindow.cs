using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    public static ExcelSheet<TerritoryType> TerritoryTypes = DalamudApi.DataManager.GetExcelSheet<TerritoryType>();

    public static ExcelSheet<Aetheryte> Aetherytes = DalamudApi.DataManager.GetExcelSheet<Aetheryte>();

    public static ExcelSheet<PlaceName> PlaceNames = DalamudApi.DataManager.GetExcelSheet<PlaceName>();

    public static ExcelSheet<Map> Maps = DalamudApi.DataManager.GetExcelSheet<Map>();
    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var currentTerritoryTypeRowID = DalamudApi.ClientState.TerritoryType;
        var currentTerritoryType = TerritoryTypes.GetRow(currentTerritoryTypeRowID);
        var currentPlaceName = currentTerritoryType.PlaceName;
        ushort sizeFactor = 100;
        foreach (var map in Maps)
        {
            if (map.PlaceName == currentPlaceName)
            {
                sizeFactor = map.SizeFactor;
            }
        }
        ImGui.Text("Size Factor:"+sizeFactor);
        foreach (var aetheryte in Aetherytes)
        {
            if (!aetheryte.IsAetheryte || aetheryte.Territory.Value == null ||
                aetheryte.PlaceName.Value == null) continue;
            if (aetheryte.Territory.Value == currentTerritoryType)
            {
                ImGui.Text("aetheryte:" + aetheryte.PlaceName.Value.RowId);
                var placeNameOfAetheryteOfCurrentMap = PlaceNames.GetRow(aetheryte.PlaceName.Value.RowId);
                ImGui.Text(placeNameOfAetheryteOfCurrentMap.Name);
                ImGui.SameLine();
                if (ImGui.Button("Teleport"))
                {
                    DalamudApi.CommandManager.ProcessCommand($"/tp {placeNameOfAetheryteOfCurrentMap.Name}");
                }
            }
        }
    }
}
