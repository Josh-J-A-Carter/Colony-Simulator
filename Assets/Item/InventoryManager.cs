using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
    Inventory inventory;

    [SerializeField]
    uint maxCapacity;

    public void Awake() {
        inventory = new Inventory(maxCapacity);
    }

    public uint RemainingCapacity() {
        return inventory.MaxCapacity() - inventory.Carrying();
    }

    public bool Has(Item item, uint quantity = 1) {
        return inventory.CountItem(item) >= quantity;
    }

    public void Give(List<(Item, uint)> items) {

        List<(Item, uint)> remaining;
        if (inventory.TryAdd(items, out remaining)) return;

        foreach ((Item item, uint quantity) in remaining) {
            EntityManager.Instance.InstantiateItemEntity((Vector2) transform.position, item, quantity);
        }
    }

    public bool Take(Item item, uint quantity) {
        return inventory.RemoveAtomic(item, quantity);
    }

    public InfoBranch GetInfoTree() {
        return inventory.GetInfoTree();
    }
}
