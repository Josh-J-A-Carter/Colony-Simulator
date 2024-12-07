using UnityEngine;

public class ItemEntity : MonoBehaviour, Informative {

    public Item item { get; private set; }
    public uint quantity { get; private set; }

    public void Setup(Item item, uint quantity) {
        this.item = item;
        this.quantity = quantity;

        GetComponent<SpriteRenderer>().sprite = item.GetPreviewSprite();
    }

    public string GetDescription() {
        return item.GetDescription();
    }
    public InfoType GetInfoType() {
        return InfoType.Item;
    }

    public string GetName() {
        return item.GetName();
    }

    public Sprite GetPreviewSprite() {
        return item.GetPreviewSprite();
    }

    public InfoBranch GetInfoTree(object _ = null) {
        InfoBranch root = item.GetInfoTree();

        InfoBranch genericCategory = (InfoBranch) root.GetChildren()[0];

        InfoLeaf quantityProperty = new InfoLeaf("Quantity", value: quantity + " unit(s)");
        genericCategory.AddChild(quantityProperty);

        return root;
    }
}
