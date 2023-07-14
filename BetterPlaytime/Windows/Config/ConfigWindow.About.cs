﻿namespace BetterPlaytime.Windows.Config;

public partial class ConfigWindow
{
    private static void About()
    {
        if (ImGui.BeginTabItem("About"))
        {
            var buttonHeight = ImGui.CalcTextSize("R").Y + (20.0f * ImGuiHelpers.GlobalScale);
            if (ImGui.BeginChild("AboutContent", new Vector2(0, -buttonHeight)))
            {
                ImGuiHelpers.ScaledDummy(5.0f);

                ImGui.TextUnformatted("Author:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, Plugin.Authors);

                ImGui.TextUnformatted("Discord:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, "@infi");

                ImGui.TextUnformatted("Version:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedOrange, Plugin.Version);
            }

            ImGui.EndChild();

            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(1.0f);

            if (ImGui.BeginChild("AboutBottomBar", new Vector2(0, 0), false, 0))
            {
                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
                if (ImGui.Button("Discord Thread"))
                    Dalamud.Utility.Util.OpenLink("https://canary.discord.com/channels/581875019861328007/1019677676883169350");
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DPSRed);
                if (ImGui.Button("Issues"))
                    Dalamud.Utility.Util.OpenLink("https://github.com/Infiziert90/BetterPlaytime/issues");
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.12549f, 0.74902f, 0.33333f, 0.6f));
                if (ImGui.Button("Ko-Fi Tip"))
                    Dalamud.Utility.Util.OpenLink("https://ko-fi.com/infiii");
                ImGui.PopStyleColor();
            }

            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }
}
