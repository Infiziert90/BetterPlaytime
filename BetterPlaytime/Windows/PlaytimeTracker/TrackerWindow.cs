using BetterPlaytime.Resources;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace BetterPlaytime.Windows.PlaytimeTracker;

public class TrackerWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

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
        if (Plugin.TimeManager.CheckIfCharacterIsUsed())
        {
            var character = Plugin.TimeManager.GetCurrentPlaytime();
            ImGui.TextColored(ImGuiColors.HealerGreen, $"{Language.OnCharacter}: {(character != "" ? character : Language.PlaytimeUnderMinute)}");
        }

        var total = Plugin.TimeManager.GetTotalPlaytime();
        ImGui.TextColored(ImGuiColors.HealerGreen, $"{Language.TotalPlaytime} {(total != "" ? total : Language.PlaytimeUnderMinute)}");

        if (!Plugin.Configuration.ShowCharacter)
            return;

        var current = Plugin.TimeManager.GetCharacterPlaytime();
        if (current == string.Empty)
            return;

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.TextUnformatted(Language.TotalCharacterPlaytime);
        ImGui.TextColored(ImGuiColors.HealerGreen, current);
    }
}
