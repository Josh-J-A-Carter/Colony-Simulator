using UnityEngine;
using UnityEngine.UIElements;

public static class InfoToUI {

    public static void DisplayInfoTree(InfoBranch root) {
    
        VisualElement infoContainer = new VisualElement();
        
        foreach (InfoNode node in root.GetChildren()) {
            DisplayInfoRecursive(node, infoContainer);
        }

        InterfaceManager.Instance.SetInfoContainerContent(infoContainer);
    }

    static void DisplayInfoRecursive(InfoNode node, VisualElement parentContainer) {
        if (node is InfoBranch category) {
            Foldout foldout = new Foldout();

            // Style
            foldout.text = category.GetCategoryName();
            foldout.AddToClassList("sub-foldout");

            // Call children
            foreach (InfoNode child in category.GetChildren()) DisplayInfoRecursive(child, foldout);

            parentContainer.Add(foldout);
        }

        else if (node is InfoLeaf property) {
            VisualElement row = new VisualElement();
            row.AddToClassList("foldout__property");

            // Style
            Label keyLabel = new Label();
            keyLabel.text = $"• {property.GetCategoryName()}:    {property.GetValue()}";
            row.Add(keyLabel);

            parentContainer.Add(row);
        }
    }
}
