using System.Text;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace BetterPlaytime.Logic;

// Shared Across all my plugin from:
// https://github.com/Infiziert90/ChatTwo/blob/main/ChatTwo/GameFunctions/ChatBox.cs
public unsafe class ChatCommon
{
    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B F2 48 8B F9 45 84 C9")]
    private readonly delegate* unmanaged<UIModule*, Utf8String*, nint, byte, void> ProcessChatBox = null!;

    internal ChatCommon()
    {
        Plugin.Hook.InitializeFromAttributes(this);
    }

    private void SendMessageUnsafe(byte[] message)
    {
        if (ProcessChatBox == null)
            throw new InvalidOperationException("Could not find signature for chat sending");

        var mes = Utf8String.FromSequence(message);
        ProcessChatBox(UIModule.Instance(), mes, IntPtr.Zero, 0);
        mes->Dtor(true);
    }

    public void SendMessage(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (bytes.Length == 0)
            throw new ArgumentException("message is empty", nameof(message));

        if (bytes.Length > 500)
            throw new ArgumentException("message is longer than 500 bytes", nameof(message));

        if (message.Length != SanitiseText(message).Length)
            throw new ArgumentException("message contained invalid characters", nameof(message));

        SendMessageUnsafe(bytes);
    }

    private static string SanitiseText(string text)
    {
        var uText = Utf8String.FromString(text);

        uText->SanitizeString( 0x27F, (Utf8String*)nint.Zero);
        var sanitised = uText->ToString();
        uText->Dtor(true);

        return sanitised;
    }
}