using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item/Item")]
public class Item : ScriptableObject, IInformative {

    [SerializeField]
    String infoName;

    [SerializeField, TextArea]
    String infoDescription;

    [SerializeField]
    Sprite previewSprite;

    [SerializeField]
    ItemTag[] itemTags;

    [SerializeField]
    ItemComponent[] components;

    public string GetDescription() {
        return infoDescription;
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

        InfoLeaf descriptionProperty = new InfoLeaf("Description", infoDescription);
        genericCategory.AddChild(descriptionProperty);

        ItemComponent foodComponent;
        if (TryGetItemComponent(ItemTag.Food, out foodComponent)) {
            InfoBranch foodCategory = new InfoBranch("Food information");
            root.AddChild(foodCategory);

            InfoLeaf nutrientsProperty = new InfoLeaf("Nutritional value", (foodComponent as FoodComponent).NutritionalValue.ToString());
            foodCategory.AddChild(nutrientsProperty);
        }

        return root;
    }

    public bool HasItemTag(ItemTag input) {
        return itemTags.Contains(input);
    }

    public ItemComponent GetItemComponent(ItemTag tag) {
        foreach (ItemComponent component in components) if (component.ComponentType == tag) return component;

        throw new Exception($"ItemComponent associated to tag {tag} unable to be found on item {this}");
    }

    public bool TryGetItemComponent(ItemTag tag, out ItemComponent component) {
        foreach (ItemComponent comp in components) {
            if (comp.ComponentType == tag) {
                component = comp;
                return true;
            }
        }

        component = null;
        return false;
    }
}