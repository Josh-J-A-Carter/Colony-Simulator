using System;
using UnityEngine;
using UnityEngine.UIElements;

public class NewBehaviourScript : MonoBehaviour {

    VisualElement root;

    void OnEnable() {
        root = GetComponent<UIDocument>().rootVisualElement;

        // Register button callbacks
        root.Q(name: "construct-tool").RegisterCallback<ClickEvent>(ClickedConstructTool);
        root.Q(name: "destroy-tool").RegisterCallback<ClickEvent>(ClickedDestroyTool);
    }

    void ClickedConstructTool(ClickEvent evt) {
        Debug.Log("Clicked construct");
    }

    void ClickedDestroyTool(ClickEvent evt) {
        Debug.Log("Clicked destroy");
    }
}
