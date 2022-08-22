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
        private Configuration Configuration { get; set; }
        private PluginUI PluginUi { get; init; }
        private ClientState clientState;
        
        private readonly PluginCommandManager<Plugin> commandManager;
        
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commands,
            [RequiredVersion("1.0")] ClientState clientState)
        {
            this.PluginInterface = pluginInterface;
            this.clientState = clientState;
            
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            
            this.PluginUi = new PluginUI(this.Configuration);
            
            this.commandManager = new PluginCommandManager<Plugin>(this, commands);
            
            Chat.ChatMessage += OnChatMessage;
            PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }
        
        [Command("/btime")]
        [Aliases("/betterplaytime")]
        [HelpMessage("Outputs playtime\nArguments:\nconfig - Opens config")]
        public void PluginCommand(string command, string args)
        {
            switch (args)
            {
                case "config":
                    this.PluginUi.SettingsVisible = true;
                    break;
                default:
                    PrintPlaytime();
                    break;
            }
        }

        private void PrintPlaytime()
        {
            var playerName = GetLocalPlayerName();
            if (playerName == null) return;
            
            ReloadConfig();
            var currentChar = Configuration.StoredPlaytimes.Find(x => x.Playername == playerName);
            if (currentChar == null)
            {
                Chat.Print("Current character has yet to be logged, type /playtime to update.");
                return;
            }

            var span = currentChar.PTime;
            Chat.Print($"{currentChar.Playername}: {GeneratePlaytime(currentChar.PTime)}");
            
            var sList = Configuration.StoredPlaytimes.OrderByDescending(x => x.PTime).ToList();
            foreach (var character in sList.Where(character => character.Playername != currentChar.Playername))
            {
                Chat.Print($"{character.Playername}: {GeneratePlaytime(character.PTime)}");
                span += character.PTime;
            }
            
            if (Configuration.StoredPlaytimes.Count > 1)
                Chat.Print($"Across all characters, you have played for: {GeneratePlaytime(span)}");
        }
        
        private string GeneratePlaytime(TimeSpan time)
        {
            return Configuration.TimeOption switch
            {
                TimeOptions.Normal => GeneratePlaytimeString(time),
                TimeOptions.Seconds => $"{time.TotalSeconds:n0} seconds",
                TimeOptions.Minutes => $"{time.TotalMinutes:n0} minutes",
                TimeOptions.Hours => $"{time.TotalHours:n0} hours",
                TimeOptions.Days => $"{time.TotalDays:n0} days",
                _ => GeneratePlaytimeString(time)
            };
        }
        
        private string GeneratePlaytimeString(TimeSpan time)
        {
            var formatted =
                $"{(time.Days > 0 ? $"{time.Days:n0} day{(time.Days == 1 ? string.Empty : 's')}, " : string.Empty)}" +
                $"{(time.Hours > 0 ? $"{time.Hours:n0} hour{(time.Hours == 1 ? string.Empty : 's')}, " : string.Empty)}" +
                $"{(time.Minutes > 0 ? $"{time.Minutes:n0} minute{(time.Minutes == 1 ? string.Empty : "s")}, " : string.Empty)}";
            if (formatted.EndsWith(", ")) formatted = formatted[..^2];

            return formatted;
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
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
        }
        
        public void Dispose()
        {
            Chat.ChatMessage -= OnChatMessage;
            this.PluginUi.Dispose();
            this.commandManager.Dispose();
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }
        
        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}
