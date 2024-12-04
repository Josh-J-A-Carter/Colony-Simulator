using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class NavToUI {

    static Sprite backArrowIcon = Resources.Load<Sprite>("Image/ui_back_arrow");

    public static void DisplayNavTree(NavNode root, Action<Constructable> callback) {
        SetPage(root, null, callback);
    }

    static void SetPage(NavNode currentNode, VisualElement parentPage, Action<Constructable> callback) {
        VisualElement pageContainer = new VisualElement();
        pageContainer.AddToClassList("page-container");

        Button backButton = new Button();
        backButton.AddToClassList("back-button");
        pageContainer.Add(backButton);
        if (parentPage != null) {
            backButton.style.backgroundImage = new StyleBackground(backArrowIcon);
            backButton.RegisterCallback<ClickEvent>((_) => {
                InterfaceManager.Instance.SetConfigurableContainerContent(parentPage);
            });
        }


        if (currentNode is NavBranch navBranch) {
            foreach (NavNode childNode in navBranch.GetChildren()) {
                Sprite sprite = childNode.GetPreview();
                String category = childNode.GetCategoryName();

                Preview p = new Preview(sprite, category);
                pageContainer.Add(p);

                p.AddCallback((evt) => { 
                    if (evt.clickCount == 2) {
                        SetPage(childNode, pageContainer, callback);
                    }
                });
            }
        }

        else if (currentNode is NavLeaf navLeaf) {
            foreach (Constructable childNode in navLeaf.GetChildren()) {
                Sprite sprite = childNode.GetPreview();
                String name = childNode.GetName();

                Preview p = new Preview(sprite, name);
                pageContainer.Add(p);

                p.AddCallback((evt) => { 
                    callback.Invoke(childNode);
                });
            }
        }

        InterfaceManager.Instance.SetConfigurableContainerContent(pageContainer);
    }

}
