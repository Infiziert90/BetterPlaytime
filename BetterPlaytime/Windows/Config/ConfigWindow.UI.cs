using BetterPlaytime.Resources;
using Dalamud.Interface.Utility.Raii;

namespace BetterPlaytime.Windows.Config;

public partial class ConfigWindow
{
    private void UI()
    {
        using var tabItem = ImRaii.TabItem("UI");
        if (!tabItem.Success)
            return;

        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.DisplayOptionHeader);

        var showCharacter = Plugin.Configuration.ShowCharacter;
        if (ImGui.Checkbox($"{Language.ShowCurrentCharacter}##UICurrentCharacter", ref showCharacter))
        {
            Plugin.ReloadConfig();
            Plugin.Configuration.ShowCharacter = showCharacter;
            Plugin.Configuration.Save();
        }
    }
}