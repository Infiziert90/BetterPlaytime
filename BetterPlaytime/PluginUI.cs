using ImGuiNET;
using System;
using System.Numerics;
using BetterPlaytime.Gui;
using BetterPlaytime.Logic;

namespace BetterPlaytime
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public class PluginUI : IDisposable
    {
        private GeneralSettings GeneralSettings { get; init; }
        public PlaytimeTracker PlaytimeTracker { get; init; }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }
        
        // passing in the image here just for simplicityw
        public PluginUI(Plugin plugin, TimeManager timeManager)
        {
            this.PlaytimeTracker = new PlaytimeTracker(plugin, timeManager);
            this.GeneralSettings = new GeneralSettings(plugin, PlaytimeTracker);
        }

        public void Dispose()
        {
            
        }

        public void Draw()
        {
            DrawSettingsWindow();
            PlaytimeTracker.DrawPlaytimeTrackerWindow();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }
            
            ImGui.SetNextWindowSize(new Vector2(260, 380), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(260, 380), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Better Playtime Config", ref this.settingsVisible, ImGuiWindowFlags.NoCollapse ))
            {

                if (ImGui.BeginTabBar("##settings-tabs"))
                {
                    // Renders General Settings UI
                    this.GeneralSettings.RenderGeneralSettings();
                    
                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }
    }
}
