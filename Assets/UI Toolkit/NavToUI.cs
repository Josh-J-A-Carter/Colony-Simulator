using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class NavToUI {

    public static void DisplayNavTree(NavNode root) {
        InterfaceManager.Instance.SetConfigurableContainerContent(GeneratePage(root, null));
    }

    static VisualElement GeneratePage(NavNode currentNode, VisualElement parentPage) {
        VisualElement pageContainer = new VisualElement();
        pageContainer.AddToClassList("page-container");

        if (currentNode is NavBranch navBranch) {
            foreach(NavNode childNode in navBranch.GetChildren()) {
                Sprite sprite = childNode.GetPreview();
                String category = childNode.GetCategoryName();

                Preview p = new Preview(sprite, category);
                pageContainer.Add(p);

                p.AddCallback((evt) => { Debug.Log("Clicked"); });
            }
        }

        return pageContainer;
    }

}
