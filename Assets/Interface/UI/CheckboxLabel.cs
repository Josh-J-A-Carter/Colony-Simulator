using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckboxLabel : VisualElement {
    Label label;
    Toggle toggle;

    String[] path;

    public CheckboxLabel(String booleanProperty, String[] path, bool value, bool modifiable) {
        this.path = path;

        label = new Label(booleanProperty);
        label.AddToClassList("checkboxlabel__label");

        toggle = new Toggle();
        toggle.value = value;
        toggle.AddToClassList("checkboxlabel__toggle");

        AddToClassList("checkboxlabel");

        if (!modifiable) {
            SetEnabled(false);
            toggle.SetEnabled(false);
        }

        Add(toggle);
        Add(label);
    }

    public void AddCallback(Action<String[], bool> callback) {
        toggle.RegisterValueChangedCallback((changeEvent) => callback.Invoke(path, changeEvent.newValue));
    }
}
