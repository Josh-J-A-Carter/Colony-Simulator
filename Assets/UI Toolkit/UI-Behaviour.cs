using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class NewBehaviourScript : MonoBehaviour {

    VisualElement root;

    List<Button> buttons;

    [SerializeField]
    Constructable comb, pyramid;

    ToolManager tm => ToolManager.Instance;

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
        tm.SetTool(ToolType.Select);

        DeselectAllButtons();

        root.Q(name: "select-tool").AddToClassList("selected");
    }

    void ClickedConstructTool(ClickEvent evt) {
        tm.SetTool(ToolType.Build);

        DeselectAllButtons();

        root.Q(name: "construct-tool").AddToClassList("selected");
    }

    void ClickedDestroyTool(ClickEvent evt) {
        tm.SetTool(ToolType.Destroy);

        DeselectAllButtons();

        root.Q(name: "destroy-tool").AddToClassList("selected");
    }
}
