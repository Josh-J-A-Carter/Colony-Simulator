using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RuleDisplay<TypeParam, QualParam> : VisualElement {

    int currentTypeIndex;
    public RuleDisplay(List<(TypeParam, Sprite, String)> typeOptions, int initialTypeIndex, Action<TypeParam> onSetType,
                        TaskPriority initialPriority, Action<TaskPriority> onSetPriority,
                        List<(QualParam, String)> qualityOptions, int initialQualityIndex, Action<QualParam> onSetQuality,
                        String confirmationText, Action onConfirmation) {

        AddToClassList("rule-display");

    #if UNITY_EDITOR
        Debug.Assert(typeOptions.Count > initialTypeIndex);
        Debug.Assert(qualityOptions.Count > initialQualityIndex);
    #endif


        // Visual & textual preview of ForageRule.Type
        currentTypeIndex = initialTypeIndex;
        (_, Sprite sprite, String name) = typeOptions[currentTypeIndex];

        Button preview = new();
        preview.AddToClassList("rule-display__preview");
        Add(preview);

        Label label = new($"Foraging for {name}");
        label.AddToClassList("rule-display__label");
        Add(label);

        preview.style.backgroundImage = new StyleBackground(sprite);
        preview.text = String.Empty;
        preview.RegisterCallback<ClickEvent>(_ => {
            // Increment, or wrap to 0
            currentTypeIndex = currentTypeIndex == typeOptions.Count - 1 ? 0 : currentTypeIndex + 1;
            (TypeParam newType, Sprite newSprite, String newName) = typeOptions[currentTypeIndex];

            preview.style.backgroundImage = new StyleBackground(newSprite);
            label.text = $"Foraging for {newName}";
            onSetType(newType);
        });

        // Priority
        PriorityDisplay priority = new PriorityDisplay(initialPriority, onSetPriority);
        Add(priority);

        // Quality tag
        OptionDisplay<QualParam> quality = new(initialQualityIndex, qualityOptions, onSetQuality, null);
        Add(quality);

        // Confirmation operation (i.e. Add or Remove)
        Button confirmOperation = new();
        confirmOperation.AddToClassList("rule-display__operation");
        Add(confirmOperation);

        confirmOperation.text = confirmationText;
        confirmOperation.RegisterCallback<ClickEvent>(_ => onConfirmation());
    }

}
