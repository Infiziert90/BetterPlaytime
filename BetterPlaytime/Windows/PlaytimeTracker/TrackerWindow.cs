using CheapLoc;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace BetterPlaytime.Windows.PlaytimeTracker;

public class TrackerWindow : Window, IDisposable
{
    private Plugin Plugin;

    public TrackerWindow(Plugin plugin) : base("Playtime Tracker")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 60),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var total = Plugin.TimeManager.GetTotalPlaytime();
        if (Plugin.TimeManager.CheckIfCharacterIsUsed())
        {
            var character = Plugin.TimeManager.GetCurrentPlaytime();
            ImGui.TextColored(ImGuiColors.HealerGreen,
                $"{Loc.Localize("Tracker - On Character", "On Character")}: {(character != ""
                    ? character
                    : Loc.Localize("Tracker - Playtime under minute", "less than a minute"))}");
        }
        ImGui.TextColored(ImGuiColors.HealerGreen,
            $"{Loc.Localize("Tracker - Total Playtime", "Total:")} {(total != ""
                ? total
                : Loc.Localize("Tracker - Playtime under minute", "less than a minute"))}");

        if (!Plugin.Configuration.ShowCharacter)
            return;

        var current = Plugin.TimeManager.GetCharacterPlaytime();
        if (current == string.Empty)
            return;

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.Text(Loc.Localize("Tracker - Total Character Time", "Character Total:"));
        ImGui.TextColored(ImGuiColors.HealerGreen, current);
    }
}
