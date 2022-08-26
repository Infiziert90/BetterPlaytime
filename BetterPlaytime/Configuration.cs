using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using BetterPlaytime.Data;

namespace BetterPlaytime
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public TimeOptions TimeOption { get; set; } = TimeOptions.Normal;

        public bool ShowAll = true;
        public bool ShowCurrent = true;
        
        public bool AutoSaveEnabled = true;
        public int AutoSaveAfter = 15;

        public List<Playtime> StoredPlaytimes = new();
        
        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;
        
        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
