#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using BetterPlaytime.Data;
using Dalamud.Game;
using Dalamud.Logging;

namespace BetterPlaytime.Logic;

public class TimeManager
{
    private static readonly TimeSpan Zero = new TimeSpan(0, 0, 0, 0);

    private readonly Plugin plugin;
    
    public string PlayerName = string.Empty;
    
    private TimeSpan _characterPlaytime;
    private Stopwatch? _totalSessionTime;
    private readonly Stopwatch _autoSaveTime = new();

    public TimeManager(Plugin plugin)
    {
        this.plugin = plugin;
    }
    
    public void PrintPlaytime()
    {
        var playerName = plugin.GetLocalPlayerName();
        if (playerName == null) return;
        
        plugin.ReloadConfig();
        var currentChar = plugin.Configuration.StoredPlaytimes.Find(x => x.Playername == playerName);
        if (currentChar == null)
        {
            // Impossible to reach?
            Plugin.Chat.Print("Current character has yet to be logged, type /playtime to update.");
            return;
        }
        
        var span = currentChar.PTime;
        if (plugin.Configuration.StoredPlaytimes.Count == 1 || plugin.Configuration.ShowCurrent)
        {
            Plugin.Chat.Print($"{currentChar.Playername}: {GeneratePlaytime(currentChar.PTime)}"); 
            PluginLog.Information($"{currentChar.Playername}: {GeneratePlaytime(currentChar.PTime)}");
        }

        var sList = plugin.Configuration.StoredPlaytimes.OrderByDescending(x => x.PTime).ToList();
        foreach (var character in sList.Where(character => character.Playername != currentChar.Playername))
        {
            if (plugin.Configuration.ShowAll)
            {
                Plugin.Chat.Print($"{character.Playername}: {GeneratePlaytime(character.PTime)}");
                PluginLog.Information($"{character.Playername}: {GeneratePlaytime(character.PTime)}");
            }

            span += character.PTime;
        } 
        
        if (plugin.Configuration.StoredPlaytimes.Count > 1)
            Plugin.Chat.Print($"Across all characters, you have played for: {GeneratePlaytime(span)}");
    }
    
    private string GeneratePlaytime(TimeSpan time)
    {
        return plugin.Configuration.TimeOption switch
        {
            TimeOptions.Normal => GeneratePlaytimeString(time),
            TimeOptions.Seconds => $"{time.TotalSeconds:n0} seconds",
            TimeOptions.Minutes => $"{time.TotalMinutes:n0} minutes",
            TimeOptions.Hours => $"{time.TotalHours:n2} hours",
            TimeOptions.Days => $"{time.TotalDays:n2} days",
            _ => GeneratePlaytimeString(time)
        };
    }
    
    public string GeneratePlaytimeString(TimeSpan time)
    {
        var formatted =
            $"{(time.Days > 0 ? $"{time.Days:n0} day{(time.Days == 1 ? string.Empty : 's')}, " : string.Empty)}" +
            $"{(time.Hours > 0 ? $"{time.Hours:n0} hour{(time.Hours == 1 ? string.Empty : 's')}, " : string.Empty)}" +
            $"{(time.Minutes > 0 ? $"{time.Minutes:n0} minute{(time.Minutes == 1 ? string.Empty : "s")}, " : string.Empty)}";
        if (formatted.EndsWith(", ")) formatted = formatted[..^2];

        return formatted;
    }
    
    public string GenerateServerBarString(TimeSpan time)
    {
        var formatted =
            $"{(time.Days > 0 ? $"{time.Days:n0}:" : string.Empty)}" +
            $"{(time.Hours > 0 ? $"{time.Hours:00;n0}:" : string.Empty)}" +
            $"{time.Minutes:00;n0}:" +
            $"{time.Seconds:00;n0}";

        return formatted;
    }

    public void StartTimer()
    {
        _totalSessionTime ??= new Stopwatch();
        var elapsed = _totalSessionTime.Elapsed;
        _characterPlaytime = new TimeSpan(elapsed.Days, elapsed.Hours, elapsed.Minutes, elapsed.Seconds);
        
        _totalSessionTime.Start();
    }
    
    public void ShutdownTimers()
    {
        if (_totalSessionTime == null) return;
        
        _totalSessionTime.Stop();
        PluginLog.Debug($"Playtime of {PlayerName}: {CalculateCharacterPlaytime():hh\\:mm\\:ss}");
        PluginLog.Debug($"Full Playtime: {_totalSessionTime.Elapsed:hh\\:mm\\:ss}");
    }

    public void AutoSaveEvent(Framework framework)
    {
        if (_autoSaveTime.Elapsed.Minutes >= plugin.Configuration.AutoSaveAfter)
        {
            AutoSave();
            _autoSaveTime.Restart();
        }
    }

    private void AutoSaveAndStop()
    {
        AutoSave();
        _autoSaveTime.Reset();
    }

    private void AutoSave()
    {
        if (!plugin.Configuration.AutoSaveEnabled)
        {
            PluginLog.Debug("Auto saving is disabled...");
            return;
        }
        
        PluginLog.Debug("Check for player name...");
        if (PlayerName == string.Empty) return;
        
        PluginLog.Debug("Check if player name exists...");
        plugin.ReloadConfig();
        var playtime = plugin.Configuration.StoredPlaytimes.Find(x => x.Playername == PlayerName);
        if (playtime == null) return;
        
        PluginLog.Debug("Saving playtime...");
        playtime.PTime += _autoSaveTime.Elapsed;
        plugin.Configuration.Save();
    }
    
    public string GetCharacterPlaytime()
    {
        var playerName = plugin.GetLocalPlayerName();
        if (playerName == null) return string.Empty;
        
        var currentChar = plugin.Configuration.StoredPlaytimes.Find(x => x.Playername == playerName);
        return currentChar == null ? string.Empty : new string($"{GeneratePlaytime(currentChar.PTime + _autoSaveTime.Elapsed)}");
    }

    public TimeSpan CalculateCharacterPlaytime() => _totalSessionTime.Elapsed.Subtract(_characterPlaytime);
    public bool CheckIfCharacterIsUsed() => !_characterPlaytime.Equals(Zero);
    public string GetCurrentPlaytime() => GeneratePlaytimeString(CalculateCharacterPlaytime());
    public string GetTotalPlaytime() => GeneratePlaytimeString(_totalSessionTime.Elapsed);
    public string GetServerBarPlaytime() => GenerateServerBarString(_totalSessionTime.Elapsed);
    public string GetServerBarCharacter() => GenerateServerBarString(CalculateCharacterPlaytime());

    public void StartAutoSave() => _autoSaveTime.Start();
    public void RestartAutoSave() => _autoSaveTime.Restart();
    public void StopAutoSave() => AutoSaveAndStop();
}