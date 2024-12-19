using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RuleDisplay : VisualElement {

    int currentIndex;

    public RuleDisplay(List<(Sprite, String, int)> typeOptions, int initialTypeIndex, Action<int> onSetType,
                        List<(String, TaskPriority)> priorityOptions, int initialPriorityIndex, Action<TaskPriority> onSetPriority,
                        String confirmationText, Action onConfirmation) {
        AddToClassList("rule-display");

    #if UNITY_EDITOR
        Debug.Assert(typeOptions.Count > initialTypeIndex);
    #endif

        currentIndex = initialTypeIndex;
        (Sprite sprite, String name, _) = typeOptions[currentIndex];

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
            currentIndex = currentIndex == typeOptions.Count - 1 ? 0 : currentIndex + 1;
            (Sprite newSprite, String newName, int newType) = typeOptions[currentIndex];

            preview.style.backgroundImage = new StyleBackground(newSprite);
            label.text = $"Foraging for {newName}";
            onSetType(newType);
        });

    #if UNITY_EDITOR
        Debug.Assert(priorityOptions.Count > initialPriorityIndex);
    #endif

        // Priority
        String priorityText = priorityOptions[initialPriorityIndex].Item1;
        Label priority = new(priorityText);
        priority.AddToClassList("rule-display__priority-label");
        Add(priority);

        Button priority__left = new();
        priority__left.AddToClassList("rule-display__priority-left");
        priority.Add(priority__left);

        Button priority__right = new();
        priority__right.AddToClassList("rule-display__priority-right");
        priority.Add(priority__right);


        // TO DO:
        // - Priority
        // - Other tag types



        // Confirmation operation (i.e. Add or Remove)
        Button confirmOperation = new();
        confirmOperation.AddToClassList("rule-display__operation");
        Add(confirmOperation);

        confirmOperation.text = confirmationText;
        confirmOperation.RegisterCallback<ClickEvent>(_ => onConfirmation());
    }

}
