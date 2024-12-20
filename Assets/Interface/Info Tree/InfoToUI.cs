using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class InfoToUI {

    public static void DisplayInfoTree(InfoBranch root) {
    
        VisualElement infoContainer = new VisualElement();
        
        foreach (InfoNode node in root.GetChildren()) {
            DisplayInfoRecursive(node, infoContainer);
        }

        // Make sure toggles are unfocusable - issues with default Unity manipulators and this is the easiest way to disable them
        foreach (Toggle toggle in infoContainer.Query<Toggle>().Build()) {
            toggle.focusable = false;
        }

        InterfaceManager.Instance.SetInfoContainerContent(infoContainer);
    }

    public static void DisplayConfigInfoTree(InfoBranch root, Action<String[], bool> callback) {
    
        VisualElement infoContainer = new VisualElement();
        
        foreach (InfoNode node in root.GetChildren()) {
            DisplayInfoRecursive(node, infoContainer, callback);
        }

        // Make sure toggles are unfocusable - issues with default Unity manipulators and this is the easiest way to disable them
        foreach (Toggle toggle in infoContainer.Query<Toggle>().Build()) {
            toggle.focusable = false;
        }

        InterfaceManager.Instance.SetConfigInfoContainerContent(infoContainer);
    }

    static void DisplayInfoRecursive(InfoNode node, VisualElement parentContainer, Action<String[], bool> callback = null) {
        if (node is InfoBranch category) {
            Foldout foldout = new Foldout();

            // Style
            foldout.text = category.GetCategoryName();
            foldout.AddToClassList("sub-foldout");

            // Call children
            foreach (InfoNode child in category.GetChildren()) DisplayInfoRecursive(child, foldout, callback);

            parentContainer.Add(foldout);
        }

        else if (node is InfoLeaf property) {
            VisualElement row = new VisualElement();
            row.AddToClassList("foldout__property");

            String labelText = $"• {property.GetCategoryName()}";
            if (property.GetValue() != null) labelText = labelText + $":    {property.GetValue()}";
            
            Label keyLabel = new Label(labelText);
            row.Add(keyLabel);

            if (property.GetDescription() != null) {
                Label descLabel = new Label(property.GetDescription());
                row.Add(descLabel);
            }

            parentContainer.Add(row);
        }

        else if (node is InfoCheckbox checkbox) {
            CheckboxLabel box = new CheckboxLabel(checkbox.GetCategoryName(),
                                                checkbox.GetPath(),
                                                checkbox.GetValue(),
                                                checkbox.IsModifiable());
            box.AddCallback(callback);

            parentContainer.Add(box);
        }
    }
}
