#nullable enable
using System.Globalization;
using System.Runtime.InteropServices;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game;
using BetterPlaytime.Attributes;
using BetterPlaytime.Data;
using BetterPlaytime.Logic;
using BetterPlaytime.Resources;
using BetterPlaytime.Windows.Config;
using BetterPlaytime.Windows.PlaytimeTracker;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace BetterPlaytime;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;

    private Hook<UIModule.Delegates.HandlePacket> PlaytimeHook;

    public Configuration Configuration { get; set; }
    private WindowSystem WindowSystem = new("BetterPlaytime");

    private ConfigWindow ConfigWindow { get; init; }
    public TrackerWindow TrackerWindow { get; init; }

    public readonly TimeManager TimeManager;
    private readonly ServerBar ServerBar;

    private bool SendChatCommand;

    private readonly PluginCommandManager<Plugin> Commands;

    public unsafe Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        LanguageChanged(PluginInterface.UiLanguage);

        TimeManager = new TimeManager(this);
        ServerBar = new ServerBar(this);

        ConfigWindow = new ConfigWindow(this);
        TrackerWindow = new TrackerWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(TrackerWindow);

        Commands = new PluginCommandManager<Plugin>(this, CommandManager);

        PlaytimeHook = Hook.HookFromAddress<UIModule.Delegates.HandlePacket>(UIModule.StaticVirtualTablePointer->HandlePacket, PlaytimePacket);
        PlaytimeHook.Enable();

        Chat.ChatMessage += OnChatMessage;
        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
        PluginInterface.LanguageChanged += LanguageChanged;
        ClientState.Login += OnLogin;
        ClientState.Logout += OnLogout;

        if (ClientState.IsLoggedIn)
            Framework.Update += TimeTracker;
    }

    public void Dispose()
    {
        PlaytimeHook.Dispose();
        WindowSystem.RemoveAllWindows();

        PluginInterface.UiBuilder.Draw -= DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;
        PluginInterface.LanguageChanged -= LanguageChanged;

        Chat.ChatMessage -= OnChatMessage;
        ClientState.Login -= OnLogin;
        ClientState.Logout -= OnLogout;
        Framework.Update -= TimeTracker;
        Framework.Update -= TimeManager.AutoSaveEvent;
        Framework.Update -= ServerBar.UpdateTracker;

        TimeManager.StopAutoSave();
        Commands.Dispose();
        ServerBar.Dispose();
    }

    private void LanguageChanged(string langCode)
    {
        Language.Culture = new CultureInfo(langCode);
    }

    [Command("/btime")]
    [Aliases("/betterplaytime")]
    [HelpMessage("Outputs playtime\nArguments:\nconfig - Toggles config window\nui - Toggles session playtime window")]
    public void PluginCommand(string command, string args)
    {
        switch (args)
        {
            case "config":
                ConfigWindow.IsOpen ^= true;
                break;
            case "ui":
                TrackerWindow.IsOpen ^= true;
                break;
            default:
                PlaytimeCommand();
                break;
        }
    }

    private void OnLogin()
    {
        Log.Debug("Login");
        Framework.Update += TimeTracker;
    }

    private void OnLogout(int _, int __)
    {
        Log.Debug("Logout");
        Framework.Update -= TimeTracker;
        Framework.Update -= TimeManager.AutoSaveEvent;

        TimeManager.ShutdownTimers();
        TimeManager.StopAutoSave();
        TimeManager.PlayerName = string.Empty;
    }

    private void PlaytimeCommand()
    {
        // send playtime command after user uses btime command
        Log.Debug("Requesting playtime from server.");
        ChatBox.SendMessage("/playtime");
        SendChatCommand = true;
    }

    private void TimeTracker(IFramework framework)
    {
        if (ClientState.LocalPlayer == null)
            return;

        Log.Debug("Checking for player name");
        if (TimeManager.PlayerName != string.Empty)
            return;

        TimeManager.PlayerName = GetLocalPlayerName();
        Log.Debug($"New Name: {TimeManager.PlayerName}");
        TimeManager.StartTimer();

        Framework.Update -= TimeTracker;

        TimeManager.StartAutoSave();
        Framework.Update += TimeManager.AutoSaveEvent;
        Framework.Update += ServerBar.UpdateTracker;
    }

    private unsafe void PlaytimePacket(UIModule* thisPtr, UIModulePacketType type, uint uintParam, void* packet)
    {
        PlaytimeHook.Original(thisPtr, type, uintParam, packet);

        if (type != UIModulePacketType.PrintPlayTime)
            return;

        var playerName = GetLocalPlayerName();
        if (playerName == string.Empty)
            return;

        Log.Debug($"Extracted Player Name: {playerName}.");

        var totalPlaytime = (uint) Marshal.ReadInt32((nint) packet + 0x10);
        Log.Debug($"Value from address {totalPlaytime}");
        var playtime = TimeSpan.FromMinutes(totalPlaytime);
        Log.Debug($"{playtime}");

        ReloadConfig();
        if (Configuration.StoredPlaytimes.Any())
            Configuration.StoredPlaytimes.RemoveAll(x => x.Playername == playerName);

        Configuration.StoredPlaytimes.Add(new Playtime(playerName, playtime));
        Configuration.Save();

        TimeManager.RestartAutoSave();
    }

    private void OnChatMessage(XivChatType type, int _, ref SeString sender, ref SeString message, ref bool handled)
    {
        if (type != XivChatType.SystemMessage)
            return;

        if (!SendChatCommand)
            return;

        // plugin requested this message, so don't show it in chat
        SendChatCommand = false;
        handled = true;

        // continue /btime command
        TimeManager.PrintPlaytime();
    }

    public static string GetLocalPlayerName()
    {
        var local = ClientState.LocalPlayer;
        return local?.HomeWorld.ValueNullable == null ? string.Empty : $"{local.Name}\uE05D{local.HomeWorld.Value.Name}";
    }

    public void ReloadConfig()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
    }

    private void DrawUi()
    {
        WindowSystem.Draw();
    }

    private void DrawConfigUi()
    {
        ConfigWindow.IsOpen = true;
    }
}