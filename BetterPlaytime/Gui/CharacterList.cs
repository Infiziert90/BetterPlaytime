using System.Linq;
using Dalamud.Interface;
using ImGuiNET;

namespace BetterPlaytime.Gui;

public class CharacterList
{
    private Plugin plugin;
    
    public CharacterList(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void RenderCharacterList()
    {
        if (!ImGui.BeginTabItem("Characters###CL-tab")) 
            return;

        if (!ImGui.BeginTable("##CharacterListTable", 2, ImGuiTableFlags.None)) 
            return;

        ImGui.TableSetupColumn("##CLT_plusbutton", ImGuiTableColumnFlags.None, 0.10f);
        ImGui.TableSetupColumn("##CLT_nameheader");
        
        if (plugin.Configuration.StoredPlaytimes.Any())
        {
            var deletionIdx = -1;
            foreach (var (item, idx) in plugin.Configuration.StoredPlaytimes.Select((value, i) => (value, i)))
            {
                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##CL_delbtn{idx}")) 
                    deletionIdx = idx;
                ImGui.PopFont();

                ImGui.TableNextColumn();
                ImGui.PushItemWidth(220.0f);
                ImGui.TextUnformatted(item.Playername);
            }

            if (deletionIdx != -1)
            {
                plugin.Configuration.StoredPlaytimes.RemoveAt(deletionIdx);
                plugin.Configuration.Save();
            }
        }
        ImGui.EndTable();

        ImGui.EndTabItem();
    }
}