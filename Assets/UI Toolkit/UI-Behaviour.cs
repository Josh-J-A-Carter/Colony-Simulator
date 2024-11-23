using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class NewBehaviourScript : MonoBehaviour {

    VisualElement root;

    List<Button> buttons;

    void OnEnable() {
        root = GetComponent<UIDocument>().rootVisualElement;

        // Register button callbacks
        buttons = root.Query<Button>().ToList();

        Button select = root.Q(name: "select-tool") as Button;
        select.AddToClassList("selected");

        select.RegisterCallback<ClickEvent>(ClickedSelectTool);
        root.Q(name: "construct-tool").RegisterCallback<ClickEvent>(ClickedConstructTool);
        root.Q(name: "destroy-tool").RegisterCallback<ClickEvent>(ClickedDestroyTool);
    }

    void DeselectAllButtons() {
        foreach (Button b in buttons) b.RemoveFromClassList("selected");
    }

    void ClickedSelectTool(ClickEvent evt) {
        Controller.toolSelection = Controller.Tool.Select;

        DeselectAllButtons();

        root.Q(name: "select-tool").AddToClassList("selected");
    }

    void ClickedConstructTool(ClickEvent evt) {
        Controller.toolSelection = Controller.Tool.Construct;

        DeselectAllButtons();

        root.Q(name: "construct-tool").AddToClassList("selected");
    }

    void ClickedDestroyTool(ClickEvent evt) {
        Controller.toolSelection = Controller.Tool.Destroy;

        DeselectAllButtons();

        root.Q(name: "destroy-tool").AddToClassList("selected");
    }
}
