using BetterPlaytime.Data;
using CheapLoc;

namespace BetterPlaytime.Windows.Config;

public partial class ConfigWindow
{
    private int AutoSaveAfter;

    private void InitializeGeneral()
    {
        AutoSaveAfter = Plugin.Configuration.AutoSaveAfter;
    }

    private void General()
    {
        if (!ImGui.BeginTabItem($"General"))
            return;

        var buttonHeight = ImGui.CalcTextSize("R").Y + (20.0f * ImGuiHelpers.GlobalScale);
        if (ImGui.BeginChild("GeneralContent", new Vector2(0, -buttonHeight)))
        {
            ImGuiHelpers.ScaledDummy(5.0f);
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config - Formatting Header", "Formatting:"));

            var options = (int)Plugin.Configuration.TimeOption;
            ImGui.Indent(5.0f);
            ImGui.Columns(2, "timeColumns", false);
            ImGui.SetColumnWidth(0, ImGui.CalcTextSize("Seconds").X * (2 * ImGuiHelpers.GlobalScale));
            ImGui.RadioButton(Loc.Localize("Config - Display Normal", "Normal"), ref options, 0);
            ImGui.NextColumn();

            ImGui.NextColumn();
            ImGui.RadioButton(Loc.Localize("Time - Seconds", "Seconds"), ref options, 1);
            ImGui.NextColumn();
            ImGui.RadioButton(Loc.Localize("Time - Minutes", "Minutes"), ref options, 2);

            ImGui.NextColumn();
            ImGui.RadioButton(Loc.Localize("Time - Hours", "Hours"), ref options, 4);
            ImGui.NextColumn();
            ImGui.RadioButton(Loc.Localize("Time - Days", "Days"), ref options, 8);
            ImGui.Columns(1);
            ImGui.Unindent(5.0f);

            if (options != (int)Plugin.Configuration.TimeOption)
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.TimeOption = (TimeOptions)options;
                Plugin.Configuration.Save();
            }

            ImGuiHelpers.ScaledDummy(5.0f);
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config - Character Output Header", "Character Output:"));

            ImGui.Indent(10.0f);
            var current = Plugin.Configuration.ShowCurrent;
            if (ImGui.Checkbox($"{Loc.Localize("Config - Current Character", "Show current character")}##OutputCurrentCharacter", ref current))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ShowCurrent = current;
                Plugin.Configuration.Save();
            }

            var all = Plugin.Configuration.ShowAll;
            if (ImGui.Checkbox(Loc.Localize("Config - Other Characters", "Show other characters"), ref all))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ShowAll = all;
                Plugin.Configuration.Save();
            }
            ImGui.Unindent(10.0f);

            ImGuiHelpers.ScaledDummy(5.0f);
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config - Header Server Bar", "Server Bar:"));

            ImGui.Indent(10.0f);
            var showServerBar = Plugin.Configuration.ShowServerBar;
            if (ImGui.Checkbox($"{Loc.Localize("Config - Enabled", "Enabled")}##dtrEnabled", ref showServerBar))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ShowServerBar = showServerBar;
                Plugin.Configuration.Save();
            }

            var serverBarCharacter = Plugin.Configuration.ServerBarCharacter;
            if (ImGui.Checkbox($"{Loc.Localize("Config - Current Character", "Show current character")}##dtrCurrentCharacter", ref serverBarCharacter))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ServerBarCharacter = serverBarCharacter;
                Plugin.Configuration.Save();
            }
            ImGui.Unindent(10.0f);

            ImGuiHelpers.ScaledDummy(5.0f);
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config - Header AutoSave", "AutoSave:"));

            ImGui.Indent(10.0f);
            var autoSaveEnabled = Plugin.Configuration.AutoSaveEnabled;
            if (ImGui.Checkbox($"{Loc.Localize("Config - Enabled", "Enabled")}##autoSaveEnabled", ref autoSaveEnabled))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.AutoSaveEnabled = autoSaveEnabled;
                Plugin.Configuration.Save();
            }

            ImGui.SliderInt($"{Loc.Localize("Time - Minutes", "Minutes")}##autoSaveInput", ref AutoSaveAfter, 5, 60);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                AutoSaveAfter = Math.Clamp(AutoSaveAfter, 5, 60);
                if (AutoSaveAfter != Plugin.Configuration.AutoSaveAfter)
                {
                    Plugin.ReloadConfig();
                    Plugin.Configuration.AutoSaveAfter = AutoSaveAfter;
                    Plugin.Configuration.Save();
                }
            }
            ImGui.Unindent(10.0f);
        }
        ImGui.EndChild();

        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(1.0f);

        if (ImGui.BeginChild("GeneralBottomBar", new Vector2(0, 0), false, 0))
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