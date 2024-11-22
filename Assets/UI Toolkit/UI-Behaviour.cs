using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NewBehaviourScript : MonoBehaviour {

    VisualElement root;

    List<VisualElement> noClickThrough = new List<VisualElement>();

    void OnEnable() {
        root = GetComponent<UIDocument>().rootVisualElement;

        // Store the elements which cannot be clicked through, i.e. should not be raycasted through
        noClickThrough = root.Query(className: "no-click-through").ToList();
        // Debug.Log(noClickThrough.Aggregate<VisualElement, String>("", (String acc, VisualElement element) => acc + ", " + element.name));

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
