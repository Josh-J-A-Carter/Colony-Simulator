using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Collections.ObjectModel;

public class InventoryManager : MonoBehaviour {
    Inventory inventory;

    [SerializeField]
    uint maxCapacity;

    [SerializeField]
    List<PassiveProduce> passivelyProducedItems;

    bool passiveProductionEnabled = true;

    const int FIXED_FRAMES_PER_SECOND = 50;

    int step = 0;

    public void Awake() {
        inventory = new Inventory(maxCapacity);

        for (int i = 0 ; i < passivelyProducedItems.Count ; i += 1) {
            passivelyProducedItems[i].Reset();
        }
    }

    public void FixedUpdate() {
        if (passiveProductionEnabled == false) return;

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

    public ReadOnlyCollection<(Item, uint)> GetContents() {
        return inventory.GetContents();
    }

    public uint RemainingCapacity() {
        return inventory.MaxCapacity() - inventory.Carrying();
    }

    public uint MaxCapacity() {
        return inventory.MaxCapacity();
    }

    public uint Carrying() {
        return inventory.Carrying();
    }

    public bool Has(Item item, uint quantity = 1) {
        return inventory.CountItem(item) >= quantity;
    }

    public bool HasResources(List<(Resource, uint)> resources) {
        return inventory.HasResources(resources);
    }

    public List<(Resource, uint)> FindRemainder(List<(Resource, uint)> resources) {
        return inventory.FindRemainder(resources);
    }

    public uint CountItem(Item item) {
        return inventory.CountItem(item);
    }

    public uint CountResource(Resource res) {
        return inventory.CountResource(res);
    }

    public void Give(List<(Item, uint)> items) {

        if (items == null) return;

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

    public List<(Item, uint)> TakeResources(List<(Resource, uint)> resources) {
        return inventory.TakeResources(resources);
    }

    public List<(Item, uint)> RemoveN(uint toRemoveTotal) {
        return inventory.RemoveN(toRemoveTotal);
    }


    public InfoBranch GetInfoTree() {
        return inventory.GetInfoTree();
    }

    public void EmptyInventory() {
        foreach ((Item item, uint quantity) in inventory.GetContents()) {
            int signX = (int) Math.Pow(-1, Random.Range(0, 2));
            float displacementX = signX * Random.Range(-0.5f, 0.5f);

            int signY = (int) Math.Pow(-1, Random.Range(0, 2));
            float displacementY = signY * Random.Range(0f, 1f);
            
            Vector2 destination = ((Vector2) transform.position) + new Vector2(displacementX, displacementY);

            EntityManager.Instance.InstantiateItemEntity(destination, item, quantity);
        }
    }

    public void EnablePassiveProduction() {
        passiveProductionEnabled = true;
    }

    public void DisablePassiveProduction() {
        passiveProductionEnabled = false;
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
