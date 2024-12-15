using System.Runtime.InteropServices;
using DavyKager;
using Reloaded.Hooks.Definitions;
using static p4g64.accessibility.Native.Text.Text;
using static p4g64.accessibility.Utils;

namespace p4g64.accessibility.Components.Battle;

/// <summary>
/// A class for hooking the message bubble that shows up at the top of the screen
/// in battle when using skills and analysing enemies
/// </summary>
internal unsafe class MessageBubble
{
    private IHook<DrawMessageBubbleDelegate> _drawBubbleHook;

    private TextStruct* _lastText;

    internal MessageBubble(IReloadedHooks hooks)
    {
        SigScan("40 55 56 41 54 41 55 41 56 48 81 EC 20 01 00 00", "Btl::UI::DrawMessageBubble",
            address =>
            {
                _drawBubbleHook = hooks.CreateHook<DrawMessageBubbleDelegate>(DrawMessageBubble, address).Activate();
            });
    }

    private nuint DrawMessageBubble(nuint param_1, BtlMessageBubble* bubble, nuint param_3)
    {
        var res = _drawBubbleHook.OriginalFunction(param_1, bubble, param_3);

        var text = bubble->Text;
        if (_lastText != text && text != null)
        {
            var textStr = text->ToString();
            Log($"Outputting battle bubble text \"{textStr}\"");
            Tolk.Output(textStr, true);

            _lastText = text;
        }

        return res;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct BtlMessageBubble
    {
        [FieldOffset(0)] internal TextStruct* Text;
    }

    private delegate nuint DrawMessageBubbleDelegate(nuint param_1, BtlMessageBubble* bubble, nuint param_3);
}