using System.Numerics;
using BetterPlaytime.Data;
using ImGuiNET;

namespace BetterPlaytime.Gui;

public class GeneralSettings
{
    private Configuration configuration;
    
    public GeneralSettings(Configuration configuration)
    {
        this.configuration = configuration;
    }
    
    public void RenderGeneralSettings()
    {
        if (ImGui.BeginTabItem($"General###general-tab"))
        {
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Text("Display Option:");
            
            var options = (int) configuration.TimeOption;
            ImGui.RadioButton("Normal", ref options, 0);
            ImGui.RadioButton("Seconds", ref options, 1);
            ImGui.RadioButton("Minutes", ref options, 2);
            ImGui.RadioButton("Hours", ref options, 4);
            ImGui.RadioButton("Days", ref options, 8);

            if ((TimeOptions) options != configuration.TimeOption)
            {
                configuration.TimeOption = (TimeOptions) options;
                configuration.Save();
            }

            ImGui.EndTabItem();
        }
    }
}