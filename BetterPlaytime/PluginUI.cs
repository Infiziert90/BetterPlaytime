using ImGuiNET;
using System;
using System.Numerics;
using BetterPlaytime.Gui;

namespace BetterPlaytime
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public class PluginUI : IDisposable
    {
        public Configuration configuration;
        private GeneralSettings GeneralSettings { get; init; }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }
        
        // passing in the image here just for simplicityw
        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
            this.GeneralSettings = new GeneralSettings(configuration);
        }

        public void Dispose()
        {
            
        }

        public void Draw()
        {
            DrawSettingsWindow();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }
            
            ImGui.SetNextWindowSize(new Vector2(260, 310), ImGuiCond.Always);
            if (ImGui.Begin("Better Playtime Config", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse ))
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
