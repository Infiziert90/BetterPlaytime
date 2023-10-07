using Dalamud.Game.Gui.Dtr;
using Dalamud.Plugin.Services;

namespace BetterPlaytime.Logic;

public class ServerBar
{
    private Plugin Plugin;
    private readonly DtrBarEntry DtrEntry;

    private const string DtrBarTitle = "BetterPlaytime";

    public ServerBar(Plugin plugin)
    {
        Plugin = plugin;

        // https://github.com/karashiiro/PingPlugin/blob/4a1797c8155b5eb8278b71d7577610acfbbd5662/PingPlugin/PingUI.cs#L52
        try
        {
            DtrEntry = Plugin.DtrBar.Get(DtrBarTitle);
        }
        catch (Exception e)
        {
            // This usually only runs once after any given plugin reload
            for (var i = 0; i < 5; i++)
            {
                Plugin.Log.Error(e, $"Failed to acquire DtrBarEntry {DtrBarTitle}, trying {DtrBarTitle}{i}");
                try
                {
                    DtrEntry = Plugin.DtrBar.Get(DtrBarTitle + i);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                break;
            }
        }

        if (DtrEntry != null)
        {
            DtrEntry.Text = "Playtime...";
            DtrEntry.Shown = false;
            DtrEntry.OnClick += OnClick;
        }
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
        DtrEntry.Dispose();
    }
}