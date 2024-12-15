using System.Runtime.InteropServices;
using DavyKager;
using p4g64.accessibility.Native;
using Reloaded.Hooks.Definitions;
using static p4g64.accessibility.Utils;

namespace p4g64.accessibility.Components.Battle;

internal unsafe class Battle
{
    private static readonly Dictionary<Command, string> _commandNames = new()
    {
        { Command.Analysis, "Analysis" },
        { Command.Tactics, "Tactics" },
        { Command.Guard, "Guard" },
        { Command.Attack, "Attack" },
        { Command.Skill, "Skill" },
        { Command.Persona, "Persona" },
        { Command.Item, "Item" },
        { Command.Escape, "Escape" }
    };

    private Command _lastSelectedCommand = Command.None;
    private MessageBubble _messageBubble;

    private IHook<ProcessDelegate> _processHook;
    private SkillSelect _skillSelect;

    internal Battle(IReloadedHooks hooks)
    {
        _messageBubble = new MessageBubble(hooks);
        _skillSelect = new SkillSelect(hooks);

        SigScan("48 8B C4 48 89 48 ?? 53 55 56 57 41 54 41 55 41 56 41 57 48 81 EC 08 01 00 00", "Btl::UI::Main",
            address => { _processHook = hooks.CreateHook<ProcessDelegate>(Process, address).Activate(); });
    }

    private void Process(BtlCommandsInfo* commands, BtlInfo* info, float* param_3)
    {
        _processHook.OriginalFunction(commands, info, param_3);

        var selectedCommand = commands->SelectedCommand;
        if (_lastSelectedCommand != selectedCommand)
        {
            var command = _commandNames[selectedCommand];

            Log($"Outputting currently selected battle command \"{command}\"");
            Tolk.Output(command, true);

            _lastSelectedCommand = selectedCommand;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct BtlInfo
    {
        [FieldOffset(0xd1a)] internal fixed short ActiveMemberSkills[8];

        [FieldOffset(0xcb8)] internal BtlTurnInfo* Turn;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct BtlCommandsInfo
    {
        [FieldOffset(4)] internal Command SelectedCommand;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct BtlTurnInfo
    {
        [FieldOffset(0x38)] internal BtlUnitInfo* Unit;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct BtlUnitInfo
    {
        [FieldOffset(0xcf0)] internal PartyMember.PartyMemberInfo* PartyMemberInfo;
    }

    internal enum Command : short
    {
        None = -1,
        Analysis = 0,
        Tactics = 1,
        Guard = 2,
        Attack = 3,
        Skill = 4,
        Persona = 5,
        Item = 6,
        Escape = 7,
    }

    private delegate void ProcessDelegate(BtlCommandsInfo* commands, BtlInfo* info, float* param_3);
}