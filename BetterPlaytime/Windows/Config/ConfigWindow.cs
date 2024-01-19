using System.IO;
using CheapLoc;
using Dalamud.Interface.Windowing;

namespace BetterPlaytime.Windows.Config;

public partial class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin;

    public ConfigWindow(Plugin plugin) : base("Configuration")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(330, 470),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;

        InitializeGeneral();
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("##ConfigTabBar"))
        {
            General();

            UI();

            Characters();

            About();

            #if DEBUG
            DebugTab();
            #endif

            ImGui.EndTabBar();
        }
    }

    private static void DebugTab()
    {
        if (!ImGui.BeginTabItem($"Debug##debug-tab"))
            return;

        if(ImGui.Button("Export Loc"))
        {
            var pwd = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Plugin.PluginInterface!.AssemblyLocation.DirectoryName!);
            Loc.ExportLocalizable();
            Directory.SetCurrentDirectory(pwd);
        }

        ImGui.EndTabItem();
    }
}
