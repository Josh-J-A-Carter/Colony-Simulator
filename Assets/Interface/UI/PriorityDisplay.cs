using System;
using UnityEngine.UIElements;
using System.Linq;
using Unity.VisualScripting;
using System.Collections.Generic;

public class PriorityDisplay : Label {

    TaskPriority currentPriority;

    int minPriority, maxPriority;

    public PriorityDisplay(TaskPriority initialPriority, Action<TaskPriority> onSetPriority) {

        currentPriority = initialPriority;

        minPriority = 0;
        maxPriority = Enum.GetValues(typeof(TaskPriority))
                        .ConvertTo<List<TaskPriority>>()
                        .Aggregate(0, (acc, t) => acc < (int) t ? (int) t : acc);

        text = Enum.GetName(typeof(TaskPriority), currentPriority);
        AddToClassList("priority__label");

        Button priority__left = new();
        priority__left.AddToClassList("priority__left");
        Add(priority__left);

        Button priority__right = new();
        priority__right.AddToClassList("priority__right");
        Add(priority__right);

        priority__left.RegisterCallback<ClickEvent>(_ => {
            // Increment, or do nothing if at max value
            if ((int) currentPriority == maxPriority) return;
            currentPriority += 1;

            text = Enum.GetName(typeof(TaskPriority), currentPriority);
            onSetPriority(currentPriority);
        });

        priority__right.RegisterCallback<ClickEvent>(_ => {
            // Increment, or do nothing if at max value
            if ((int) currentPriority == minPriority) return;
            currentPriority -= 1;

            text = Enum.GetName(typeof(TaskPriority), currentPriority);
            onSetPriority(currentPriority);
        });
    }
}
