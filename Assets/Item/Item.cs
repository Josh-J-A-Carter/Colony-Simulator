using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject, Informative {

    [SerializeField]
    String infoName, infoDescription;

    [SerializeField]
    Sprite previewSprite;

    public string GetDescription() {
        return infoDescription;
    }
    public InfoType GetInfoType() {
        return InfoType.Item;
    }

    public string GetName() {
        return infoName;
    }

    public Sprite GetPreviewSprite() {
        return previewSprite;
    }

    public InfoBranch GetInfoTree(object _ = null) {
        InfoBranch root = new InfoBranch(String.Empty);

        InfoBranch genericCategory = new InfoBranch("Generic properties");
        root.AddChild(genericCategory);

        InfoLeaf typeProperty = new InfoLeaf("Type", "Item");
        genericCategory.AddChild(typeProperty);

        InfoLeaf nameProperty = new InfoLeaf("Name", infoName);
        genericCategory.AddChild(nameProperty);

        // InfoLeaf descriptionProperty = new InfoLeaf("Description", infoDescription);
        // genericCategory.AddChild(descriptionProperty);

        return root;
    }
}