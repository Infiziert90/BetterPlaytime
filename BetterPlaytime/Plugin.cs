#nullable enable
using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Data;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState;
using Dalamud.Game;
using BetterPlaytime.Attributes;
using BetterPlaytime.Data;
using BetterPlaytime.Logic;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;


namespace BetterPlaytime
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] public static DataManager Data { get; private set; } = null!;
        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        
        public string Name => "BetterPlaytime";
        
        private DalamudPluginInterface PluginInterface { get; init; }
        public Configuration Configuration { get; set; }
        private PluginUI PluginUi { get; init; }
        private TimeManager TimeManager { get; init; }
        private ClientState clientState;
        
        private readonly PluginCommandManager<Plugin> commandManager;
        
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commands,
            [RequiredVersion("1.0")] ClientState clientState)
        {
            PluginInterface = pluginInterface;
            this.clientState = clientState;
            
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);
            
            TimeManager = new TimeManager(this);
            PluginUi = new PluginUI(this, TimeManager);
            
            commandManager = new PluginCommandManager<Plugin>(this, commands);
            
            Chat.ChatMessage += OnChatMessage;
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            clientState.Login += OnLogin;
            clientState.Logout += OnLogout;

            if (clientState.IsLoggedIn) Framework.Update += TimeTracker;
        }
        
        [Command("/btime")]
        [Aliases("/betterplaytime")]
        [HelpMessage("Outputs playtime\nArguments:\nconfig - Opens config")]
        public void PluginCommand(string command, string args)
        {
            switch (args)
            {
                case "config":
                    PluginUi.SettingsVisible = true;
                    break;
                default:
                    TimeManager.PrintPlaytime();
                    break;
            }
        }
        
        public void OnLogin(object? sender, EventArgs e)
        {
            PluginLog.Debug("Login");
            Framework.Update += TimeTracker;
        }
        
        public void OnLogout(object? sender, EventArgs e)
        {
            PluginLog.Debug("Logout");
            Framework.Update -= TimeTracker;
            Framework.Update -= TimeManager.AutoSaveEvent;
            
            TimeManager.ShutdownTimers();
            TimeManager.StopAutoSave();
            TimeManager.PlayerName = string.Empty;
        }
        
        public void TimeTracker(Framework framework)
        {
            if (clientState.LocalPlayer == null) return;
            
            PluginLog.Debug($"Checking for player name");
            if (TimeManager.PlayerName != string.Empty) return;

            TimeManager.PlayerName = GetLocalPlayerName();
            PluginLog.Debug($"New Name: {TimeManager.PlayerName}");
            TimeManager.StartTimer();
            
            Framework.Update -= TimeTracker;
            
            TimeManager.StartAutoSave();
            Framework.Update += TimeManager.AutoSaveEvent;
        }
        
        private void OnChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message, ref bool handled)
        {
            // 57 = sysmsg
            var xivChatType = (ushort) type;
            if (xivChatType != 57) return;

            var m = Reg.Match(message.ToString(), clientState.ClientLanguage);
            if (!m.Success) return;
            
            var playerName = GetLocalPlayerName();
            if (playerName == null) return;

            PluginLog.Debug($"Extracted Player Name: {playerName}.");
            PluginLog.Debug($"Extracted Playtime: {message}\n    Matches: D: {m.Groups["days"]} H: {m.Groups["hours"]} M: {m.Groups["minutes"]}");

            ReloadConfig();
            if (Configuration.StoredPlaytimes.Count != 0) 
                Configuration.StoredPlaytimes.RemoveAll(x => x.Playername == playerName);
            try
            {
                var time = new TimeSpan(
                    m.Groups["days"].Success ? int.Parse(m.Groups["days"].Value) : 0,
                    m.Groups["hours"].Success ? int.Parse(m.Groups["hours"].Value) : 0,
                    int.Parse(m.Groups["minutes"].Value),
                    0
                );
                
                Configuration.StoredPlaytimes.Add(new Playtime(playerName, time));
                Configuration.Save();
                
                TimeManager.RestartAutoSave();
            }
            catch (FormatException e)
            {
                Chat.PrintError("Unable to parse playtime.");
                PluginLog.Error(e.ToString());
                PluginLog.Error(message.ToString());
            }
        }

        public string? GetLocalPlayerName()
        {
            var local = clientState?.LocalPlayer;
            if (local == null || local.HomeWorld.GameData?.Name == null)
            {
                return null;
            }
            return $"{local.Name}\uE05D{local.HomeWorld.GameData.Name}";
        }
        
        public void ReloadConfig()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);
        }
        
        public void Dispose()
        {
            Chat.ChatMessage -= OnChatMessage;
            clientState.Login -= OnLogin;
            clientState.Logout -= OnLogout;
            Framework.Update -= TimeTracker;
            Framework.Update -= TimeManager.AutoSaveEvent;
            
            TimeManager.StopAutoSave();
            PluginUi.Dispose();
            commandManager.Dispose();
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }
        
        private void DrawConfigUI()
        {
            PluginUi.SettingsVisible = true;
        }
    }
}
