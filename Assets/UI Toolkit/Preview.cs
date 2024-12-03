using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Preview : VisualElement {

    String name;
    Label label;

    Sprite icon;
    Button button;

    public Preview(Sprite icon, String name) {
        this.name = name;
        label = new Label(name);

        this.icon = icon;
        button = new Button();
        button.style.backgroundImage = new StyleBackground(icon);
        button.style.backgroundColor = new StyleColor(Color.clear);

        Add(button);
        Add(label);
        AddToClassList("cringe");
    }

    public void AddCallback(EventCallback<ClickEvent> callback) {
        button.RegisterCallback<ClickEvent>(callback);
    }
}
