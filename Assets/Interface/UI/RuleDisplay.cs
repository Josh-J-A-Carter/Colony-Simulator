using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RuleDisplay : VisualElement {

    int currentIndex;

    public RuleDisplay(List<(Sprite, String, int)> options, int initialIndex, Action<int> onSetType,
                        String confirmationText, Action onConfirmation) {
        AddToClassList("rule-display");

    #if UNITY_EDITOR
        Debug.Assert(options.Count > initialIndex);
    #endif

        currentIndex = initialIndex;
        (Sprite sprite, String name, _) = options[currentIndex];

        // Visual & textual preview of ForageRule.Type
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
            currentIndex = currentIndex == options.Count - 1 ? 0 : currentIndex + 1;
            (Sprite newSprite, String newName, int newType) = options[currentIndex];

            preview.style.backgroundImage = new StyleBackground(newSprite);
            label.text = $"Foraging for {newName}";
            onSetType(newType);
        });

        // TO DO:
        // - Priority
        // - Other tag types
        // - Make it not DISGUSTING to look at


        // Confirmation operation (i.e. Add or Remove)
        Button confirmOperation = new();
        confirmOperation.AddToClassList("rule-display__operation");
        Add(confirmOperation);

        confirmOperation.text = confirmationText;
        confirmOperation.RegisterCallback<ClickEvent>(_ => onConfirmation());
    }

}
