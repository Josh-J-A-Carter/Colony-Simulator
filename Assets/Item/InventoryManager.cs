using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
    Inventory inventory;

    [SerializeField]
    uint maxCapacity;

    [SerializeField]
    List<PassiveProduce> passivelyProducedItems;

    const int FIXED_FRAMES_PER_SECOND = 50;    

    int step = 0;

    public void Awake() {
        inventory = new Inventory(maxCapacity);

        for (int i = 0 ; i < passivelyProducedItems.Count ; i += 1) {
            passivelyProducedItems[i].Reset();
        }
    }

    public void FixedUpdate() {
        step += 1;

        if (step >= FIXED_FRAMES_PER_SECOND) {

            for (int i = 0 ; i < passivelyProducedItems.Count ; i += 1) {
                PassiveProduce produce = passivelyProducedItems[i];

                produce.Decrement();
                if (produce.delay <= 0) {
                    Give(produce.item, produce.quantity);
                    produce.Reset();
                }
            }

            step = 0;
        }
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

    public void Give(Item item, uint quantity) {
        uint space = RemainingCapacity();

        if (space >= quantity) {
            inventory.AddAtomic(item, quantity);
            return;
        }

        inventory.AddAtomic(item, space);
        uint remaining = quantity - space;
        EntityManager.Instance.InstantiateItemEntity((Vector2) transform.position, item, remaining);
    }

    public bool Take(Item item, uint quantity) {
        return inventory.RemoveAtomic(item, quantity);
    }

    public InfoBranch GetInfoTree() {
        return inventory.GetInfoTree();
    }
}

[Serializable]
class PassiveProduce {

    public Item item;

    public int minQuantity, maxQuantity;

    public int minDelaySeconds, maxDelaySeconds;

    public int delay { get; private set; }
    public uint quantity { get; private set; }

    public void Reset() {
        delay = Random.Range(minDelaySeconds, maxDelaySeconds + 1);
        quantity = (uint) Random.Range(minQuantity, maxQuantity + 1);
    }

    public void Decrement() {
        delay -= 1;
    }
}
