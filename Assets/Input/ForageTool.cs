using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ForageTool : Tool {

    [SerializeField]
    Item nectar, pollen, sap;

    List<(Sprite, String, int)> options;

    ForageRule newRule;

    List<ForageRule> rules;

    public void Awake() {
        options = new() {
            (nectar.GetPreviewSprite(), nectar.GetName(), (int) ForageRule.Type.Nectar),
            (pollen.GetPreviewSprite(), pollen.GetName(), (int) ForageRule.Type.Pollen),
            (sap.GetPreviewSprite(), sap.GetName(), (int) ForageRule.Type.Sap)
        };

        rules = new();

        InterfaceManager.Instance.SetForageQuitCallback(_ => parent.SetTool(ToolType.Select));
    }

    void AddNewRule() {
        if (newRule != null) {
            rules.Add(newRule);
            TaskManager.Instance.RegisterRule(newRule);

            InterfaceManager.Instance.AddOldForageContent(
                new (options,
                    (int) newRule.type, 
                    (input) => newRule.SetType((ForageRule.Type) input),
                    "Remove",
                    () => RemoveRule(newRule)
                )
            );
        }

        SetNewRule();
    }

    void SetNewRule() {
        newRule = new(ForageRule.Type.Nectar, new(), parent.GetPriority());

        InterfaceManager.Instance.SetNewForageContent(
            new (options,
                (int) newRule.type,
                (input) => newRule.SetType((ForageRule.Type) input),
                "Add",
                () => AddNewRule()
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
            InterfaceManager.Instance.AddOldForageContent(
                new (options,
                    (int) rule.type,
                    (input) => rule.SetType((ForageRule.Type) input),
                    "Remove",
                    () => RemoveRule(rule)
                )
            );
        }
    }

    public override void OnEquip() {
        SetNewRule();

        RefreshOldRuleDisplay();

        InterfaceManager.Instance.ShowForageMenu();
    }

    public override void OnDequip() {
        InterfaceManager.Instance.HideForageMenu();
    }

}
