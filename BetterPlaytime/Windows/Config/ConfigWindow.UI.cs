using CheapLoc;

namespace BetterPlaytime.Windows.Config;

public partial class ConfigWindow
{
    private void UI()
    {
        if (!ImGui.BeginTabItem($"UI"))
            return;

        var buttonHeight = ImGui.CalcTextSize("R").Y + (20.0f * ImGuiHelpers.GlobalScale);
        if (ImGui.BeginChild("UIContent", new Vector2(0, -buttonHeight)))
        {
            ImGuiHelpers.ScaledDummy(5.0f);
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config - Display Header", "Display Option:"));

            var showCharacter = Plugin.Configuration.ShowCharacter;
            if (ImGui.Checkbox(
                    $"{Loc.Localize("Config - Current Character", "Show current character")}##UICurrentCharacter",
                    ref showCharacter))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ShowCharacter = showCharacter;
                Plugin.Configuration.Save();
            }
        }
        ImGui.EndChild();

        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(1.0f);

        if (ImGui.BeginChild("UIBottomBar", new Vector2(0, 0), false, 0))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.TankBlue);
            if (ImGui.Button("Playtime"))
                Dalamud.Utility.Util.OpenLink("https://ko-fi.com/infiii");
            ImGui.PopStyleColor();
        }
        ImGui.EndChild();

        ImGui.EndTabItem();
    }
}