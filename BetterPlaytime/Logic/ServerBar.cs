using Dalamud.Game.Gui.Dtr;
using Dalamud.Plugin.Services;

namespace BetterPlaytime.Logic;

public class ServerBar
{
    private Plugin Plugin;
    private readonly IDtrBarEntry DtrEntry;

    public ServerBar(Plugin plugin)
    {
        Plugin = plugin;

        if (Plugin.DtrBar.Get("BetterPlaytime") is not { } entry)
            return;

        DtrEntry = entry;

        DtrEntry.Text = "Playtime...";
        DtrEntry.Shown = false;
        DtrEntry.OnClick += OnClick;
    }

    public void UpdateTracker(IFramework framework)
    {
        if (!Plugin.Configuration.ShowServerBar)
        {
            UpdateVisibility(false);
            return;
        }

        UpdateVisibility(true);
        UpdateBarString();
    }

    private void UpdateBarString()
    {
        var total = Plugin.TimeManager.GetServerBarPlaytime();
        if (Plugin.Configuration.ServerBarCharacter && Plugin.TimeManager.CheckIfCharacterIsUsed())
            total = $"{Plugin.TimeManager.GetServerBarCharacter()}/{total}";

        if (Plugin.Configuration.FullPlaytimeInDtr)
            total = Plugin.TimeManager.GetCharacterPlaytime(true);

        DtrEntry.Text = total;
    }

    private void UpdateVisibility(bool shown) => DtrEntry.Shown = shown;

    private void OnClick()
    {
        Plugin.Configuration.FullPlaytimeInDtr ^= true;
        Plugin.Configuration.Save();
    }

    public void Dispose()
    {
        if (DtrEntry == null)
            return;

        DtrEntry.OnClick -= OnClick;
        DtrEntry.Remove();
    }
}