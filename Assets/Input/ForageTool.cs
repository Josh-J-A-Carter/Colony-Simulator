using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForageTool : Tool {

    [SerializeField]
    Item nectar, pollen, sap;

    List<(Sprite, String, int)> typeOptions;

    ForageRule newRule;

    List<ForageRule> rules;

    public void Awake() {
        typeOptions = new() {
            (nectar.GetPreviewSprite(), nectar.GetName(), (int) ForageRule.Type.Nectar),
            (pollen.GetPreviewSprite(), pollen.GetName(), (int) ForageRule.Type.Pollen),
            (sap.GetPreviewSprite(), sap.GetName(), (int) ForageRule.Type.Sap)
        };


        rules = new();

        InterfaceManager.Instance.SetForageQuitCallback(_ => parent.RestorePreviousTool());
    }

    void AddNewRule() {
        if (newRule != null) {
            ForageRule thisRule = newRule;
            rules.Add(thisRule);
            TaskManager.Instance.RegisterRule(thisRule);

            InterfaceManager.Instance.AddOldForageContent(
                new (typeOptions, (int) thisRule.type, (input) => thisRule.SetType((ForageRule.Type) input),
                    thisRule.priority, (priority) => thisRule.SetPriority(priority),
                    "Remove", () => RemoveRule(thisRule)
                )
            );
        }

        SetNewRule();
    }

    void SetNewRule() {
        newRule = new(ForageRule.Type.Nectar, new(), parent.GetPriority());

        InterfaceManager.Instance.SetNewForageContent(
            new (typeOptions, (int) newRule.type, (input) => newRule.SetType((ForageRule.Type) input),
                newRule.priority, (priority) => newRule.SetPriority(priority),
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
            InterfaceManager.Instance.AddOldForageContent(
                new (typeOptions, (int) rule.type, (input) => rule.SetType((ForageRule.Type) input),
                    rule.priority, (priority) => rule.SetPriority(priority),
                    "Remove", () => RemoveRule(rule)
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
