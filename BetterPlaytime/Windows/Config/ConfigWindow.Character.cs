using CheapLoc;
using Dalamud.Interface.Components;

namespace BetterPlaytime.Windows.Config;

public partial class ConfigWindow
{
    private void Characters()
    {
        if (!ImGui.BeginTabItem("Characters"))
            return;

        if (!Plugin.Configuration.StoredPlaytimes.Any())
        {
            ImGui.EndTabItem();
            return;
        }

        var buttonHeight = ImGui.CalcTextSize("R").Y + (20.0f * ImGuiHelpers.GlobalScale);
        if (ImGui.BeginChild("CharactersContent", new Vector2(0, -buttonHeight)))
        {
            ImGuiHelpers.ScaledDummy(5.0f);
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config - Saved Characters Header", "Saved Character:"));

            ImGui.Indent(10.0f);
            if (ImGui.BeginTable("##CharacterListTable", 2))
            {
                ImGui.TableSetupColumn("##del", 0, 0.10f);
                ImGui.TableSetupColumn("##names");

                var deletionIdx = -1;
                foreach (var (item, idx) in Plugin.Configuration.StoredPlaytimes.Select((value, i) => (value, i)))
                {
                    ImGui.TableNextColumn();
                    if (ImGuiComponents.IconButton(idx, FontAwesomeIcon.Trash))
                        deletionIdx = idx;

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(item.Playername);
                }

                if (deletionIdx != -1)
                {
                    Plugin.Configuration.StoredPlaytimes.RemoveAt(deletionIdx);
                    Plugin.Configuration.Save();
                }

                ImGui.EndTable();
            }
            ImGui.Unindent(10.0f);

        }
        ImGui.EndChild();

        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(1.0f);

        if (ImGui.BeginChild("CharactersBottomBar", new Vector2(0, 0), false, 0))
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