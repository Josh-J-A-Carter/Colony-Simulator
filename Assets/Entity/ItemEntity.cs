using System;
using UnityEngine;

public class ItemEntity : MonoBehaviour, IInformative, IEntity {

    public Item item { get; private set; }
    public uint quantity { get; private set; }

    GravityComponent gravity;

    public GameObject GetGameObject() {
        return gameObject;
    }

    public void Awake() {
        gravity = GetComponent<GravityComponent>();
        gravity.Enable();
    }

    public void Setup(Item item, uint quantity) {
        this.item = item;
        this.quantity = quantity;

        GetComponent<SpriteRenderer>().sprite = item.GetPreviewSprite();

        // ResourceManager.Instance.Register(this);
    }


    public void Collect(InventoryManager inventory) {
        if (inventory.RemainingCapacity() >= quantity) {
            inventory.Give(item, quantity);
            // ResourceManager.Instance.Deregister(this);
            EntityManager.Instance.DestroyEntity(this);
            return;
        }

        // Can't collect the entire item, so just take part of it
        uint remaining = inventory.RemainingCapacity();

        inventory.Give(item, remaining);
        quantity -= remaining;

        // ResourceManager.Instance.UpdateCount(item, (int) -remaining);
    }

    public string GetDescription() {
        return item.GetDescription();
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
