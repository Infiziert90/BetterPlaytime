using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace BetterPlaytime.Windows.Config;

public partial class ConfigWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    public ConfigWindow(Plugin plugin) : base("Configuration##BetterPlaytime")
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
        using var tabBar = ImRaii.TabBar("##ConfigTabBar");
        if (!tabBar.Success)
            return;

        General();

        UI();

        Characters();

        About();
    }
}
