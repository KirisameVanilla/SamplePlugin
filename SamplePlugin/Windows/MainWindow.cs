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

    private uint? uid =0 ;
    public string input = "";
    private bool showObjects = false;
    private bool showIGameObjects = false;
    public override unsafe void Draw()
    {
        var objects = GameObjectManager.Instance()->Objects.GameObjectIdSorted;
        var objArr = objects.ToArray();

        ImGui.InputText("Id", ref input, 100);
        if (input != "") uid = uint.Parse(input);

        if (ImGui.Button("ShowObjects"))
        {
            showObjects = !showObjects;
        }
        ImGui.SameLine();
        if (ImGui.Button("ShowIGameObjects"))
        {
            showIGameObjects = !showIGameObjects;
        }
        
        if (showObjects)
        {
            if(uid!=0&&uid!=null)
            {/*
                foreach (var ptr in objects)
                {

                    if (ptr.Value->BaseId != uid) continue;
                    var gameObject = *ptr.Value;
                    var goId = gameObject.GetGameObjectId().ObjectId;
                    ImGui.Text($"Name:{gameObject.NameString}\tBaseId:{gameObject.BaseId}\tGoId:{goId}");
                    
                }
            */

                var target = *objArr.First(i => ((i.Value)->BaseId == uid)).Value;
                ImGui.Text($"Name:{target.NameString}\tBaseId:{target.BaseId}\tGameObjectId:{target.GetGameObjectId().ObjectId}");

            }
            else
            {
                foreach (var ptr in objects)
                {
                    var gameObject = *ptr.Value;
                    ImGui.Text($"Name:{gameObject.NameString}\tBaseId:{gameObject.BaseId}\tGameObjectId:{gameObject.GetGameObjectId().ObjectId}");
                }
            }
        }



        
        if(showIGameObjects)
        {
            foreach (var iGameObject in DalamudApi.ObjectTable)
            {
                var entityId = iGameObject.EntityId;
                var gameObjectId = iGameObject.GameObjectId;
                var dataId = iGameObject.DataId;
                ImGui.Text(
                    $"Name:{iGameObject.Name.TextValue}\tAddr:{iGameObject.Address}\tDataId:{dataId}\tGOId:{gameObjectId}\tEntyId:{entityId}");
            }
        }


        
    }
}
