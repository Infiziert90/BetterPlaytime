using System.Numerics;
using BetterPlaytime.Logic;
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
        
        ImGui.SetNextWindowSize(new Vector2(260, 85), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(260, 85), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Playtime##playtimeTrackerWindow", ref this.Visible))
        {
            var character = timeManager.GetCurrentPlaytime();
            var total = timeManager.GetTotalPlaytime();
            if (total != character)
                ImGui.TextColored(_greenColor, $"Character: {(character != "" ? character : "less than a minute")}");
            ImGui.TextColored(_greenColor, $"Total: {(total != "" ? total : "less than a minute")}");
        }
        ImGui.End();
    }
}