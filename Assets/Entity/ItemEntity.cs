using System;
using UnityEngine;

public class ItemEntity : MonoBehaviour, IInformative, Entity {

    public Item item { get; private set; }
    public uint quantity { get; private set; }

    bool falling;
    float fallStartTime;

    float fallStartHeight;

    const float GRAVITY = 4.9f;

    public GameObject GetGameObject() {
        return gameObject;
    }

    public void Setup(Item item, uint quantity) {
        this.item = item;
        this.quantity = quantity;

        GetComponent<SpriteRenderer>().sprite = item.GetPreviewSprite();

        // ResourceManager.Instance.Register(this);
    }

    public void FixedUpdate() {
        Vector2Int below = new((int) Math.Floor(transform.position.x), (int) Math.Floor(transform.position.y - 0.25));
        (_, Constructable c1) = TileManager.Instance.GetConstructableAt(below);

        if (falling && c1 && !TileManager.Instance.IsUnobstructed(below)) {
            // Don't let it sink into the floor
            Vector2Int here = new((int) Math.Floor(transform.position.x), (int) Math.Floor(transform.position.y));
            (_, Constructable c2) = TileManager.Instance.GetConstructableAt(here);
            if (c2 && !TileManager.Instance.IsUnobstructed(here)) {
                Vector2 goal = new Vector2(transform.position.x, here.y + 1.35f);
                Vector2 translation = goal - (Vector2) transform.position;
                transform.Translate(translation);
            }
            falling = false;
        }

        else if (!falling && (c1 == null || TileManager.Instance.IsUnobstructed(below))) {
            falling = true;
            fallStartTime = Time.time;
            fallStartHeight = transform.position.y;
        }

        // a = -9.8,     v0 = 0,     s0 = (x, y)

        // => s = 1/2 a t^2 + v0t + s0 
        //      = -4.9t^2 + y0

        if (falling) {
            float t = Time.time - fallStartTime;
            Vector2 newPos = new(transform.position.x, fallStartHeight - GRAVITY * t * t);
            Vector2 translation = newPos - (Vector2) transform.position;
            transform.Translate(translation);
        }
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
