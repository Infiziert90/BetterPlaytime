#nullable enable
using System.Diagnostics;
using BetterPlaytime.Data;
using CheapLoc;
using Dalamud.Plugin.Services;

namespace BetterPlaytime.Logic;

public class TimeManager
{
    private static readonly TimeSpan Zero = new(0, 0, 0, 0);

    private readonly Plugin Plugin;

    public string PlayerName = string.Empty;

    private TimeSpan CharacterPlaytime;
    private Stopwatch? TotalSessionTime;
    private readonly Stopwatch AutoSaveWatch = new();

    public TimeManager(Plugin plugin)
    {
        Plugin = plugin;
    }

    public void PrintPlaytime()
    {
        var playerName = Plugin.GetLocalPlayerName();
        if (playerName == string.Empty)
            return;

        Plugin.ReloadConfig();
        var currentChar = Plugin.Configuration.StoredPlaytimes.Find(x => x.Playername == playerName);
        if (currentChar == null)
        {
            // Impossible to reach?
            Plugin.Chat.Print(Loc.Localize("Chat - Not registered", "Current character has yet to be logged, type /playtime to update."));
            return;
        }

        var totalPlaytime = currentChar.PTime;
        if (Plugin.Configuration.StoredPlaytimes.Count == 1 || Plugin.Configuration.ShowCurrent)
        {
            Plugin.Chat.Print($"{currentChar.Playername}: {GeneratePlaytime(currentChar.PTime)}");
            Plugin.Log.Information($"{currentChar.Playername}: {GeneratePlaytime(currentChar.PTime)}");
        }

        foreach (var character in Plugin.Configuration.StoredPlaytimes.Where(character => character.Playername != currentChar.Playername).OrderByDescending(x => x.PTime))
        {
            if (Plugin.Configuration.ShowAll)
            {
                Plugin.Chat.Print($"{character.Playername}: {GeneratePlaytime(character.PTime)}");
                Plugin.Log.Information($"{character.Playername}: {GeneratePlaytime(character.PTime)}");
            }

            totalPlaytime += character.PTime;
        }

        if (Plugin.Configuration.StoredPlaytimes.Count > 1)
            Plugin.Chat.Print($"{Loc.Localize("Chat - All characters", "Across all characters, you have played for")}: {GeneratePlaytime(totalPlaytime)}");
    }

    private string GeneratePlaytime(TimeSpan time, bool withSeconds = false)
    {
        return Plugin.Configuration.TimeOption switch
        {
            TimeOptions.Normal => GeneratePlaytimeString(time, withSeconds),
            TimeOptions.Seconds => $"{time.TotalSeconds:n0} {Loc.Localize("Time - Seconds", "Seconds")}",
            TimeOptions.Minutes => $"{time.TotalMinutes:n0} {Loc.Localize("Time - Minutes", "Minutes")}",
            TimeOptions.Hours => $"{time.TotalHours:n2} {Loc.Localize("Time - Hours", "Hours")}",
            TimeOptions.Days => $"{time.TotalDays:n2} {Loc.Localize("Time - Days", "Days")}",
            _ => GeneratePlaytimeString(time, withSeconds)
        };
    }

    private static string GeneratePlaytimeString(TimeSpan time, bool withSeconds = false)
    {
        var formatted =
            $"{(time.Days > 0 ? $"{time.Days:n0} {(time.Days == 1 ? Loc.Localize("Time - Day", "Day") : Loc.Localize("Time - Days", "Days"))}, " : string.Empty)}" +
            $"{(time.Hours > 0 ? $"{time.Hours:n0} {(time.Hours == 1 ? Loc.Localize("Time - Hour", "Hour") : Loc.Localize("Time - Hours", "Hours"))}, " : string.Empty)}" +
            $"{(time.Minutes > 0 ? $"{time.Minutes:n0} {(time.Minutes == 1 ? Loc.Localize("Time - Minute", "Minute") : Loc.Localize("Time - Minutes", "Minutes"))}, " : string.Empty)}";

        if (withSeconds)
            formatted += $"{time.Seconds:n0} {(time.Seconds == 1 ? Loc.Localize("Time - Second", "Second") : Loc.Localize("Time - Seconds", "Seconds"))}";

        if (formatted.EndsWith(", "))
            formatted = formatted[..^2];

        return formatted;
    }

    private static string GenerateServerBarString(TimeSpan time)
    {
        return $"{(time.Days > 0 ? $"{time.Days:n0}:" : string.Empty)}" +
               $"{(time.Days > 0 ? $"{time.Hours:00;n0}:" : time.Hours > 0 ? $"{time.Hours:00;n0}:" : string.Empty)}" +
               $"{time.Minutes:00;n0}:" +
               $"{time.Seconds:00;n0}";
    }

    public void StartTimer()
    {
        TotalSessionTime ??= new Stopwatch();
        CharacterPlaytime = TotalSessionTime.Elapsed;

        TotalSessionTime.Start();
    }

    public void ShutdownTimers()
    {
        if (TotalSessionTime == null)
            return;

        TotalSessionTime.Stop();
        Plugin.Log.Debug($"Playtime of {PlayerName}: {CalculateCharacterPlaytime():hh\\:mm\\:ss}");
        Plugin.Log.Debug($"Full Playtime: {TotalSessionTime.Elapsed:hh\\:mm\\:ss}");
    }

    public void AutoSaveEvent(IFramework framework)
    {
        if (AutoSaveWatch.Elapsed.Minutes < Plugin.Configuration.AutoSaveAfter)
            return;

        AutoSave();
        AutoSaveWatch.Restart();
    }

    private void AutoSaveAndStop()
    {
        AutoSave();
        AutoSaveWatch.Reset();
    }

    private void AutoSave()
    {
        if (!Plugin.Configuration.AutoSaveEnabled)
        {
            Plugin.Log.Debug("Auto save is disabled...");
            return;
        }

        Plugin.Log.Debug("Check for player name...");
        if (PlayerName == string.Empty)
            return;

        Plugin.Log.Debug("Check if player name exists...");
        Plugin.ReloadConfig();
        var playtime = Plugin.Configuration.StoredPlaytimes.Find(x => x.Playername == PlayerName);
        if (playtime == null)
            return;

        Plugin.Log.Debug("Saving playtime...");
        playtime.PTime += AutoSaveWatch.Elapsed;
        Plugin.Configuration.Save();
    }

    public string GetCharacterPlaytime(bool withSeconds = false)
    {
        var playerName = Plugin.GetLocalPlayerName();
        if (playerName == string.Empty)
            return playerName;

        var currentChar = Plugin.Configuration.StoredPlaytimes.Find(x => x.Playername == playerName);
        return currentChar == null ? string.Empty : $"{GeneratePlaytime(currentChar.PTime + AutoSaveWatch.Elapsed, withSeconds)}";
    }

    private TimeSpan CalculateCharacterPlaytime() => TotalSessionTime!.Elapsed.Subtract(CharacterPlaytime);
    public bool CheckIfCharacterIsUsed() => !CharacterPlaytime.Equals(Zero);
    public string GetCurrentPlaytime() => GeneratePlaytimeString(CalculateCharacterPlaytime());
    public string GetTotalPlaytime() => GeneratePlaytimeString(TotalSessionTime!.Elapsed);
    public string GetServerBarPlaytime() => GenerateServerBarString(TotalSessionTime!.Elapsed);
    public string GetServerBarCharacter() => GenerateServerBarString(CalculateCharacterPlaytime());

    public void StartAutoSave() => AutoSaveWatch.Start();
    public void RestartAutoSave() => AutoSaveWatch.Restart();
    public void StopAutoSave() => AutoSaveAndStop();
}