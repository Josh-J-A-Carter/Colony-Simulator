using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Preview : Button {
    Label label;
    VisualElement display;

    public Preview(Sprite icon, String name) {
        label = new Label(name);
        label.AddToClassList("preview__label");

        display = new VisualElement();
        display.style.backgroundImage = new StyleBackground(icon);
        display.AddToClassList("preview__display");

        style.backgroundColor = new StyleColor(Color.clear);
        AddToClassList("preview");

        Add(display);
        Add(label);
    }

    public void AddCallback(EventCallback<ClickEvent> callback) {
        RegisterCallback(callback);
    }
}
