using DavyKager;
using p4g64.accessibility.Native;
using Reloaded.Hooks.Definitions;
using static p4g64.accessibility.Utils;

namespace p4g64.accessibility.Components.Battle;

public unsafe class SkillSelect
{
    private IHook<DrawSkillListItemDelegate> _drawSkillListItemHook;
    private bool _lastDrawDescription = false;

    private short _lastSkillId = -1;

    internal SkillSelect(IReloadedHooks hooks)
    {
        SigScan("48 8B C4 56 57 48 81 EC C8 00 00 00", "DrawSkillListItem",
            address =>
            {
                _drawSkillListItemHook =
                    hooks.CreateHook<DrawSkillListItemDelegate>(DrawSkillListItem, address).Activate();
            });
    }

    private void DrawSkillListItem(Battle.BtlInfo* btl, int skillSlot, float param_3, float param_4, byte alpha,
        int isSelected, bool drawDescription)
    {
        _drawSkillListItemHook.OriginalFunction(btl, skillSlot, param_3, param_4, alpha, isSelected, drawDescription);

        if (isSelected == 0) return;

        var skillId = btl->ActiveMemberSkills[skillSlot];
        if (skillId == _lastSkillId)
        {
            // Hack to get around the game's stupid hack that calls this twice when the help box is opening
            // The first time it's called with the correct opacity, the second it's called with 255 but drawDescription turned off
            // With this we only check the value when it's opening or closing
            if (alpha == 255)
            {
                // Output just the description if it was just toggled on
                if (!_lastDrawDescription && drawDescription)
                {
                    var description = Skill.GetDescription(skillId);
                    Log($"Outputting skill description \"{description}\"");
                    Tolk.Output(description, true);
                }

                _lastDrawDescription = drawDescription;
            }

            return;
        }

        _lastSkillId = skillId;
        _lastDrawDescription = drawDescription;
        var skillName = Skill.GetName(skillId);

        var member = btl->Turn->Unit->PartyMemberInfo;
        var skillCost = PartyMember.GetSkillCost(member, skillId);
        var costType = Skill.GetActiveSkillData(skillId)->CostType;
        var skillType = Skill.GetSkillType(skillId);

        string text;
        if (skillType == Skill.SkillType.Passive)
        {
            text = $"{skillName}: Passive Skill. ";
        }
        else
        {
            var skillElement = Skill.GetSkillElement(skillId);
            var elementText =
                skillElement > Skill.ElementalType.Dark
                    ? "Support"
                    : skillElement.ToString(); // The icon does not differentiate between the non-damaging types
            text = $"{skillName}: {elementText} skill that costs {skillCost} {costType}. "; // TODO availability
        }

        if (drawDescription)
        {
            text += Skill.GetDescription(skillId);
        }

        Log($"Outputting skill text \"{text}\"");
        Tolk.Output(text, true);
    }

    private delegate void DrawSkillListItemDelegate(Battle.BtlInfo* btl, int skillSlot, float param_3, float param_4,
        byte alpha, int isSelected, bool drawDescription);
}