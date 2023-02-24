using System;
using System.IO;
using System.Numerics;
using BetterPlaytime.Data;
using CheapLoc;
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

            PlaytimeButton();
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Text(Loc.Localize("Config - Header Chat", "Chat Output:"));
            
            var options = (int) plugin.Configuration.TimeOption;
            ImGui.RadioButton(Loc.Localize("Config - Display Normal", "Normal"), ref options, 0);
            ImGui.RadioButton(Loc.Localize("Time - Seconds", "Seconds"), ref options, 1); ImGui.SameLine();
            ImGui.RadioButton(Loc.Localize("Time - Minutes", "Minutes"), ref options, 2);
            ImGui.RadioButton(Loc.Localize("Time - Hours", "Hours"), ref options, 4); ImGui.SameLine();
            ImGui.RadioButton(Loc.Localize("Time - Days", "Days"), ref options, 8);
            
            if ((TimeOptions) options != plugin.Configuration.TimeOption)
            {
                plugin.ReloadConfig();
                plugin.Configuration.TimeOption = (TimeOptions) options;
                plugin.Configuration.Save();
            }
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            
            var current = plugin.Configuration.ShowCurrent;
            if (ImGui.Checkbox(Loc.Localize("Config - Current Character", "Show current character"), ref current))
            {
                plugin.ReloadConfig();
                plugin.Configuration.ShowCurrent = current;
                plugin.Configuration.Save();
            }
            
            var all = plugin.Configuration.ShowAll;
            if (ImGui.Checkbox(Loc.Localize("Config - Other Characters", "Show other characters"), ref all))
            {
                plugin.ReloadConfig();
                plugin.Configuration.ShowAll = all;
                plugin.Configuration.Save();
            }
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Text(Loc.Localize("Config - Header Server Bar", "Server Bar:"));
            
            var showServerBar = plugin.Configuration.ShowServerBar;
            if (ImGui.Checkbox(Loc.Localize("Config - Enabled", "Enabled"), ref showServerBar))
            {
                plugin.ReloadConfig();
                plugin.Configuration.ShowServerBar = showServerBar;
                plugin.Configuration.Save();
            }
            
            var serverBarCharacter = plugin.Configuration.ServerBarCharacter;
            if (ImGui.Checkbox(Loc.Localize("Config - Current Character", "Show current character"), ref serverBarCharacter))
            {
                plugin.ReloadConfig();
                plugin.Configuration.ServerBarCharacter = serverBarCharacter;
                plugin.Configuration.Save();
            }
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Text(Loc.Localize("Config - Header AutoSave", "AutoSave:"));
            
            var autoSaveEnabled = plugin.Configuration.AutoSaveEnabled;
            if (ImGui.Checkbox(Loc.Localize("Config - Enabled", "Enabled"), ref autoSaveEnabled))
            {
                plugin.ReloadConfig();
                plugin.Configuration.AutoSaveEnabled = autoSaveEnabled;
                plugin.Configuration.Save();
            }
            
            ImGui.SliderInt($"{Loc.Localize("Time - Minutes", "Minutes")}##autosave_in_minutes", ref _currentNumber, 5, 60);
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
        
        if (ImGui.BeginTabItem($"UI###ui-tab"))
        {
            ImGui.Dummy(new Vector2(0,0));

            PlaytimeButton();
            
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Text(Loc.Localize("Config - Display Header", "Display Option:"));

            var showCharacter = plugin.Configuration.ShowCharacter;
            if (ImGui.Checkbox(Loc.Localize("Config - Current Character", "Show current character"), ref showCharacter))
            {
                plugin.ReloadConfig();
                plugin.Configuration.ShowCharacter = showCharacter;
                plugin.Configuration.Save();
            }
            
            ImGui.EndTabItem();
        }
    }

    public static void DebugTab()
    {
        if (ImGui.BeginTabItem($"Debug###debug-tab"))
        {
            if(ImGui.Button("Export Loc"))
            {
                var pwd = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(Plugin.PluginInterface!.AssemblyLocation.DirectoryName!);
                Loc.ExportLocalizable();
                Directory.SetCurrentDirectory(pwd);
            }
            
            ImGui.EndTabItem();
        }
    }

    public void PlaytimeButton()
    {
        var text = Loc.Localize("Config - Button Playtime", "Show Playtime");
        var textLength = ImGui.CalcTextSize(text).X;
        
        var scrollBarSpacing = ImGui.GetScrollMaxY() == 0 ? 0.0f : 15.0f;
        ImGui.SameLine(ImGui.GetWindowWidth() - 15.0f - textLength - scrollBarSpacing);
        
        if (ImGui.Button(Loc.Localize("Config - Button Playtime", "Show Playtime")))
        {
            playtimeTracker.Visible = true;
        }
    }
}