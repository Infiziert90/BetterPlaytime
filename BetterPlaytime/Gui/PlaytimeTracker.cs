using System.Numerics;
using BetterPlaytime.Logic;
using CheapLoc;
using ImGuiNET;

namespace BetterPlaytime.Gui;

public class PlaytimeTracker
{
    private readonly Vector4 _greenColor = new(0.0f, 1.0f, 0.0f, 1.0f);
    
    private Plugin plugin;
    private TimeManager timeManager;
    
    public bool Visible = false;
    
    public PlaytimeTracker(Plugin plugin, TimeManager timeManager)
    {
        this.plugin = plugin;
        this.timeManager = timeManager;
    }
    
    public void DrawPlaytimeTrackerWindow()
    {
        if (!Visible) return;
        
        ImGui.SetNextWindowSize(new Vector2(200, 60), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(200, 60), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Playtime##playtimeTrackerWindow", ref this.Visible, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var total = timeManager.GetTotalPlaytime();
            if (timeManager.CheckIfCharacterIsUsed())
            {
                var character = timeManager.GetCurrentPlaytime();
                ImGui.TextColored(_greenColor, 
                    $"{Loc.Localize("Tracker - On Character", "On Character")}: {(character != "" 
                        ? character 
                        : Loc.Localize("Tracker - Playtime under minute", "less than a minute"))}");
            }
            ImGui.TextColored(_greenColor, 
                $"{Loc.Localize("Tracker - Total Playtime", "Total:")} {(total != "" 
                    ? total 
                    : Loc.Localize("Tracker - Playtime under minute", "less than a minute"))}");

            if (!plugin.Configuration.ShowCharacter) return;
            
            var current = timeManager.GetCharacterPlaytime();
            if (current == string.Empty) return;
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Text(Loc.Localize("Tracker - Total Character Time", "Character Total:"));
            ImGui.TextColored(_greenColor, current);
        }
        ImGui.End();
    }
}