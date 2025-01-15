using BetterPlaytime.Resources;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;

namespace BetterPlaytime.Windows.Config;

public partial class ConfigWindow
{
    private void Characters()
    {
        using var tabItem = ImRaii.TabItem("Characters");
        if (!tabItem.Success)
            return;

        if (Plugin.Configuration.StoredPlaytimes.Count == 0)
            return;

        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.SavedCharacterHeader);

        using var indent = ImRaii.PushIndent(10.0f);
        using var table = ImRaii.Table("##CharacterListTable", 2);
        if (!table.Success)
            return;

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
    }
}