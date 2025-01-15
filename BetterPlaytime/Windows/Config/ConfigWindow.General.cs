using BetterPlaytime.Data;
using BetterPlaytime.Resources;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

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
        using var tabItem = ImRaii.TabItem("General");
        if (!tabItem.Success)
            return;

        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.FormattingHeader);

        var options = (int)Plugin.Configuration.TimeOption;
        using (ImRaii.PushIndent(5.0f))
        {
            ImGui.Columns(2, "timeColumns", false);
            ImGui.SetColumnWidth(0, ImGui.CalcTextSize("Seconds").X * (2 * ImGuiHelpers.GlobalScale));
            ImGui.RadioButton(Language.DisplayNormal, ref options, 0);
            ImGui.NextColumn();

            ImGui.NextColumn();
            ImGui.RadioButton(Language.TimeSeconds, ref options, 1);
            ImGui.NextColumn();
            ImGui.RadioButton(Language.TimeMinutes, ref options, 2);

            ImGui.NextColumn();
            ImGui.RadioButton(Language.TimeHours, ref options, 4);
            ImGui.NextColumn();
            ImGui.RadioButton(Language.TimeDays, ref options, 8);
            ImGui.Columns(1);
        }

        if (options != (int)Plugin.Configuration.TimeOption)
        {
            Plugin.ReloadConfig();
            Plugin.Configuration.TimeOption = (TimeOptions)options;
            Plugin.Configuration.Save();
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.OutputHeader);

        using (ImRaii.PushIndent(10.0f))
        {
            var current = Plugin.Configuration.ShowCurrent;
            if (ImGui.Checkbox($"{Language.ShowCurrentCharacter}##OutputCurrentCharacter", ref current))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ShowCurrent = current;
                Plugin.Configuration.Save();
            }

            var all = Plugin.Configuration.ShowAll;
            if (ImGui.Checkbox(Language.ShowOtherCharacter, ref all))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ShowAll = all;
                Plugin.Configuration.Save();
            }
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ServerBarHeader);

        using (ImRaii.PushIndent(10.0f))
        {
            var showServerBar = Plugin.Configuration.ShowServerBar;
            if (ImGui.Checkbox($"{Language.Enabled}##dtrEnabled", ref showServerBar))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ShowServerBar = showServerBar;
                Plugin.Configuration.Save();
            }

            var serverBarCharacter = Plugin.Configuration.ServerBarCharacter;
            if (ImGui.Checkbox($"{Language.ShowCurrentCharacter}##dtrCurrentCharacter", ref serverBarCharacter))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.ServerBarCharacter = serverBarCharacter;
                Plugin.Configuration.Save();
            }
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.AutoSaveHeader);

        using (ImRaii.PushIndent(10.0f))
        {
            var autoSaveEnabled = Plugin.Configuration.AutoSaveEnabled;
            if (ImGui.Checkbox($"{Language.Enabled}##autoSaveEnabled", ref autoSaveEnabled))
            {
                Plugin.ReloadConfig();
                Plugin.Configuration.AutoSaveEnabled = autoSaveEnabled;
                Plugin.Configuration.Save();
            }

            ImGui.SliderInt($"{Language.TimeMinutes}##autoSaveInput", ref AutoSaveAfter, 5, 60);
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
        }
    }
}