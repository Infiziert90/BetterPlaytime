using System;
using System.Numerics;
using BetterPlaytime.Data;
using ImGuiNET;

namespace BetterPlaytime.Gui;

public class GeneralSettings
{
    private Plugin plugin;
    private PlaytimeTracker playtimeTracker;
    
    private int _currentNumber;
    
    public GeneralSettings(Plugin plugin, PlaytimeTracker playtimeTracker)
    {
        this.plugin = plugin;
        this.playtimeTracker = playtimeTracker;

        _currentNumber = plugin.Configuration.AutoSaveAfter;
    }
    
    public void RenderGeneralSettings()
    {
        if (ImGui.BeginTabItem($"General###general-tab"))
        {
            ImGui.Dummy(new Vector2(0,0));
            
            var spacing = ImGui.GetScrollMaxY() == 0 ? 100.0f : 120.0f;
            ImGui.SameLine(ImGui.GetWindowWidth() - spacing);
        
            if (ImGui.Button("Show Playtime"))
            {
                playtimeTracker.Visible = true;
            }
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Text("Display Option:");
            
            var options = (int) plugin.Configuration.TimeOption;
            ImGui.RadioButton("Normal", ref options, 0);
            ImGui.RadioButton("Seconds", ref options, 1); ImGui.SameLine();
            ImGui.RadioButton("Minutes", ref options, 2);
            ImGui.RadioButton("Hours", ref options, 4); ImGui.SameLine();
            ImGui.RadioButton("Days", ref options, 8);
            
            if ((TimeOptions) options != plugin.Configuration.TimeOption)
            {
                plugin.ReloadConfig();
                plugin.Configuration.TimeOption = (TimeOptions) options;
                plugin.Configuration.Save();
            }
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            
            var current = plugin.Configuration.ShowCurrent;
            if (ImGui.Checkbox("Show current character", ref current))
            {
                plugin.ReloadConfig();
                plugin.Configuration.ShowCurrent = current;
                plugin.Configuration.Save();
            }
            
            var all = plugin.Configuration.ShowAll;
            if (ImGui.Checkbox("Show other characters", ref all))
            {
                plugin.ReloadConfig();
                plugin.Configuration.ShowAll = all;
                plugin.Configuration.Save();
            }

            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Text("AutoSave:");
            
            var autoSaveEnabled = plugin.Configuration.AutoSaveEnabled;
            if (ImGui.Checkbox("Enabled", ref autoSaveEnabled))
            {
                plugin.ReloadConfig();
                plugin.Configuration.AutoSaveEnabled = autoSaveEnabled;
                plugin.Configuration.Save();
            }
            
            ImGui.SliderInt("Minutes##autosave_in_minutes", ref _currentNumber, 5, 60);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                _currentNumber = Math.Clamp(_currentNumber, 5, 60); 
                if (_currentNumber != plugin.Configuration.AutoSaveAfter)
                {
                    plugin.ReloadConfig();
                    plugin.Configuration.AutoSaveAfter = _currentNumber;
                    plugin.Configuration.Save();
                }
            }

            ImGui.EndTabItem();
        }
    }
}