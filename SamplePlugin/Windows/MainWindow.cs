using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Aetheryte = Lumina.Excel.GeneratedSheets.Aetheryte;

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

    private bool isTargetSet = false;
    private ulong targetId = 0;
    private bool flag = false;
    private Byte followFlagByte;
    public override unsafe void Draw()
    {
        var currentTerritoryTypeRowID = DalamudApi.ClientState.TerritoryType;
        var currentTerritoryType = TerritoryTypes.GetRow(currentTerritoryTypeRowID);
        var currentPlaceName = currentTerritoryType.PlaceName;
        var currentMapId = DalamudApi.ClientState.MapId;
        var currentMapText = Maps.GetRow(currentMapId).Id;
        ushort sizeFactor = 100;
        foreach (var map in Maps)
        {
            if (map.PlaceName == currentPlaceName)
            {
                sizeFactor = map.SizeFactor;
            }
        }
        ImGui.Separator();
        ImGui.Text($"currentTerritoryTypeRowID:{currentTerritoryTypeRowID}");
        ImGui.Text($"currentTerritoryType:{currentTerritoryType}");
        ImGui.Text($"currentPlaceName:{currentPlaceName}");
        ImGui.Text($"currentMapId:{currentMapId}");
        ImGui.Text($"currentMapText:{currentMapText}");

        ImGui.Separator();
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

        ImGui.Separator();
        var PlayerController = DalamudApi.SigScanner.GetStaticAddressFromSig(
            "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 3C ?? 75 ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? EB ?? 49 3B FE");
        var followX = PlayerController + 1120;
        var followY = PlayerController + 1124;
        var followZ = PlayerController + 1128;
        /*似乎不对
        var isFollowing = PlayerController + 1189;
        SafeMemory.Read(isFollowing, out bool isFollowingFlag);
        

        var followStateStart = PlayerController + 1080;
        SafeMemory.Read(followStateStart, out byte followStateResult);
        */

        // ImGui.Text($"isFollowing:{followStateResult}");

        ImGui.Text($"PlayerController:{PlayerController:X}");
        ImGui.SameLine();
        if (ImGui.Button("copy##PlayerController"))
        {
            ImGui.SetClipboardText($"0x{PlayerController:X}");
        }

        var followFlag = PlayerController + 1377;
        SafeMemory.Read(followFlag, out followFlagByte);
        ImGui.Text($"FollowFlag:{followFlag:X}: {followFlagByte}");
        ImGui.SameLine();
        if (ImGui.Button("copy##FollowFlag"))
        {
            ImGui.SetClipboardText($"0x{followFlag:X}");
        }

        SafeMemory.Read<float>(followX, out float followXResult);
        SafeMemory.Read<float>(followY, out float followYResult);
        SafeMemory.Read<float>(followZ, out float followZResult);

        ImGui.Text($"FollowX:{followXResult}");
        ImGui.Text($"FollowY:{followYResult}");
        ImGui.Text($"FollowZ:{followZResult}");


        var me = DalamudApi.ObjectTable.First();
        var isMoving = DalamudApi.KeyState[VirtualKey.W] || DalamudApi.KeyState[VirtualKey.A] ||
                       DalamudApi.KeyState[VirtualKey.S] || DalamudApi.KeyState[VirtualKey.D];
        var target = me.TargetObject;
        var targetAddr = target.Address + 902;
        SafeMemory.Read(targetAddr, out Byte testFlag);
        ImGui.Text($"TargetAddress:{target?.Address:X}");
        ImGui.SameLine();
        ImGui.Text($"TargetFlag:{testFlag}");
        ImGui.SameLine();
        if (ImGui.Button("copyAddress##Target"))
        {
            ImGui.SetClipboardText($"0x{target?.Address:X}");
        }


        ImGui.Text($"isMoving:{isMoving}");
        if (ImGui.Button("Start"))
        {
            flag = !flag;
        }
        if (flag)
        {
            if (isTargetSet==false&&target != null)
            {
                targetId = target.GameObjectId;
                isTargetSet = true;
            }
            if(isTargetSet)
            {
                if (!isMoving)
                    SafeMemory.Write((IntPtr)PlayerController + 1377, 4);
                var searchResult = DalamudApi.ObjectTable.SearchById(targetId);
                if (searchResult != null)
                {
                    SafeMemory.Write(followX, searchResult.Position.X);
                    SafeMemory.Write(followY, searchResult.Position.Y);
                    SafeMemory.Write(followZ, searchResult.Position.Z);
                }
            }
        }

        if (!flag)
        {
            if(followFlagByte==(Byte)4) SafeMemory.Write((IntPtr)PlayerController + 1377, 1);
            isTargetSet = false;
        }
        ImGui.Text($"FollowTargetId:{targetId}");
        ImGui.Text($"isRunning:{flag}");
        ImGui.Text($"isTargetSet:{isTargetSet}");
        /*
        foreach (var gameObject in DalamudApi.ObjectTable)
        {
            var a = (ICharacter*)&gameObject;
            ICharacter b = *a;
            if (b.Level == 0) continue;
            ImGui.Text($"Character:{b.Level}");
        }
        */
    }
}
