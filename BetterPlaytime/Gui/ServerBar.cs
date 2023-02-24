using System;
using BetterPlaytime.Logic;
using Dalamud.Game;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Logging;

namespace BetterPlaytime.Gui;

public class ServerBar
{
    private Plugin plugin;
    private TimeManager timeManager;
    private readonly DtrBarEntry dtrEntry;
    
    public ServerBar(Plugin plugin, TimeManager timeManager, DtrBar dtrBar)
    {
        this.plugin = plugin;
        this.timeManager = timeManager;
        
        // https://github.com/karashiiro/PingPlugin/blob/4a1797c8155b5eb8278b71d7577610acfbbd5662/PingPlugin/PingUI.cs#L52
        var dtrBarTitle = "BetterPlaytime";
        try
        {
            dtrEntry = dtrBar.Get(dtrBarTitle);
        }
        catch (Exception e)
        {
            // This usually only runs once after any given plugin reload
            for (var i = 0; i < 5; i++)
            {
                PluginLog.LogError(e, $"Failed to acquire DtrBarEntry {dtrBarTitle}, trying {dtrBarTitle}{i}");
                try
                {
                    this.dtrEntry = dtrBar.Get(dtrBarTitle + i);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                break;
            }
        }

        if (this.dtrEntry != null)
        {
            this.dtrEntry.Text = "Playtime...";
            this.dtrEntry.Shown = false;
        }
    }

    public void UpdateTracker(Framework framework)
    {
        if (!plugin.Configuration.ShowServerBar)
        {
            UpdateVisibility(false);
            return;
        }
        
        UpdateVisibility(true);
        UpdateBarString();
    }

    private void UpdateBarString()
    {
        var total = timeManager.GetServerBarPlaytime();
        if (plugin.Configuration.ServerBarCharacter && timeManager.CheckIfCharacterIsUsed())
            total = $"{timeManager.GetServerBarCharacter()}/{total}";
        
        dtrEntry.Text = total;
    }

    private void UpdateVisibility(bool shown) => dtrEntry.Shown = shown;
    
    public void Dispose()
    {
        this.dtrEntry?.Dispose();
    }
}