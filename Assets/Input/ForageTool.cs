using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ForageTool : Tool {

    [SerializeField]
    Item nectar, pollen, sap;

    List<(ForageRule.Type, Sprite, String)> typeOptions;

    List<(ItemTag, String)> qualityOptions;

    ForageRule newRule;

    ForageRule.Type currentType;
    int currentTypeIndex;
    ItemTag currentQuality;
    int currentQualityIndex;
    TaskPriority currentPriority;

    List<ForageRule> rules;

    public void Awake() {
        typeOptions = new() {
            (ForageRule.Type.Nectar, nectar.GetPreviewSprite(), nectar.GetName()),
            (ForageRule.Type.Pollen, pollen.GetPreviewSprite(), pollen.GetName()),
            (ForageRule.Type.Sap, sap.GetPreviewSprite(), sap.GetName())
        };

        qualityOptions = new() {
            (ItemTag.Standard, Utilities.GetDescription(ItemTag.Standard)),
            (ItemTag.Superior, Utilities.GetDescription(ItemTag.Superior)),
            (ItemTag.Perfect, Utilities.GetDescription(ItemTag.Perfect))
        };

        currentType = ForageRule.Type.Nectar;
        currentTypeIndex = 0;

        currentQuality = ItemTag.Standard;
        currentQualityIndex = 0;

        currentPriority = parent.GetPriority();

        rules = new();

        InterfaceManager.Instance.SetForageQuitCallback(_ => parent.RestorePreviousTool());
    }

    void AddNewRule() {
        if (newRule != null) {
            // Warn the user if the rule already exists
            foreach (ForageRule rule in rules) {
                if (rule.Equals(newRule)) {
                    InterfaceManager.Instance.ShowForageWarning();
                    return;
                }
            }

            InterfaceManager.Instance.HideForageWarning();

            ForageRule thisRule = newRule;
            rules.Add(thisRule);
            TaskManager.Instance.RegisterRule(thisRule);

            InterfaceManager.Instance.AddOldForageContent(
                new RuleDisplay<ForageRule.Type, ItemTag> (
                    // Type options
                    typeOptions, currentTypeIndex, (input) => thisRule.SetType(input),
                    // Priority options
                    thisRule.priority, (input) => thisRule.SetPriority(input),
                    // Quality options
                    qualityOptions, currentQualityIndex, (input) => {
                        foreach ((ItemTag tag, _) in qualityOptions) thisRule.RemoveTag(tag);
                        thisRule.AddTag(input);
                    },
                    // Remove button
                    "Remove", () => RemoveRule(thisRule)
                )
            );
        }

        SetNewRule();
    }

    void SetNewRule() {
        newRule = new(currentType, new() { currentQuality }, currentPriority);

        InterfaceManager.Instance.SetNewForageContent(
            new RuleDisplay<ForageRule.Type, ItemTag>(
                // Type options
                typeOptions, currentTypeIndex, (input) => {
                    newRule.SetType(input);
                    currentType = input;
                    currentTypeIndex = typeOptions.FindIndex(t => t.Item1 == input);
                },
                // Priority options
                newRule.priority, (input) => {
                    newRule.SetPriority(input);
                    currentPriority = input;
                },
                // Quality options
                qualityOptions, currentQualityIndex, (input) => {
                    newRule.RemoveTag(currentQuality);
                    newRule.AddTag(input);

                    currentQuality = input;
                    currentQualityIndex = qualityOptions.FindIndex(q => q.Item1 == input);
                },
                // Final addition button
                "Add", () => AddNewRule()
            )
        );
    }

    void RemoveRule(ForageRule rule) {
        rules.Remove(rule);
        TaskManager.Instance.DeregisterRule(rule);

        RefreshOldRuleDisplay();
    }

    void RefreshOldRuleDisplay() {
        InterfaceManager.Instance.ResetOldForageContent();

        
        foreach (ForageRule rule in rules) {
            ForageRule.Type type = rule.type;
            int typeIndex = typeOptions.FindIndex(t => t.Item1 == type);

            ItemTag quality = ItemTag.Standard;
            foreach ((ItemTag tag, _) in qualityOptions) {
                if (rule.HasTag(tag)) {
                    quality = tag;
                    break;
                }
            }
            int qualityIndex = qualityOptions.FindIndex(t => t.Item1 == quality);

            TaskPriority priority = rule.priority;

            InterfaceManager.Instance.AddOldForageContent(
                new RuleDisplay<ForageRule.Type, ItemTag>(
                    // Type options
                    typeOptions, typeIndex, (input) => rule.SetType(input),
                    // Priority
                    priority, (input) => rule.SetPriority(input),
                    // Quality
                    qualityOptions, qualityIndex, (input) => {
                        foreach ((ItemTag tag, _) in qualityOptions) rule.RemoveTag(tag);
                        rule.AddTag(input);
                    },
                    // Remove button
                    "Remove", () => RemoveRule(rule)
                )
            );
        }
    }

    public override void OnEquip() {
        currentPriority = parent.GetPriority();

        SetNewRule();

        RefreshOldRuleDisplay();

        InterfaceManager.Instance.ShowForageMenu();
    }

    public override void OnDequip() {
        InterfaceManager.Instance.HideForageMenu();

        InterfaceManager.Instance.HideForageWarning();
    }

}
