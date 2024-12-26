using System;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Diagnostics;

public class OptionDisplay<T> : Button {

    int currentIndex;

    int minIndex, maxIndex;

    public OptionDisplay(int initialIndex, List<(T, String)> options, Action<T> onSetOption, Action onClick = null) {

        currentIndex = initialIndex;

        if (onClick != null) {
            RegisterCallback<ClickEvent>(_ => onClick());
            AddToClassList("selectable");
        }

        minIndex = 0;
        maxIndex = options.Count - 1;

    #if UNITY_EDITOR
        Debug.Assert(minIndex <= maxIndex);
        Debug.Assert(currentIndex <= maxIndex);
    #endif

        text = options[currentIndex].Item2;
        AddToClassList("option-display__label");

        Button left = new();
        left.AddToClassList("option-display__left");
        Add(left);

        Button right = new();
        right.AddToClassList("option-display__right");
        Add(right);

        left.RegisterCallback<ClickEvent>(_ => {
            // Decrement, or do nothing if at min index
            if (currentIndex == minIndex) return;
            currentIndex -= 1;

            text = options[currentIndex].Item2;
            onSetOption(options[currentIndex].Item1);
        });

        right.RegisterCallback<ClickEvent>(_ => {
            // Increment, or do nothing if at max index
            if (currentIndex == maxIndex) return;
            currentIndex += 1;

            text = options[currentIndex].Item2;
            onSetOption(options[currentIndex].Item1);
        });
    }
}
