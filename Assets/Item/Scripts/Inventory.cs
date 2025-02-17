using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Inventory {

    uint maxCapacity;
    List<(Item, uint)> contents = new List<(Item, uint)>();

    uint carrying = 0;


    public Inventory(uint maxCapacity) {
        this.maxCapacity = maxCapacity;
    }

    public uint Carrying() {
        return carrying;
    }

    public uint MaxCapacity() {
        return maxCapacity;
    }

    public ReadOnlyCollection<(Item, uint)> GetContents() {
        return contents.AsReadOnly();
    }

    public uint CountResource(Resource res) {
        if (res.ResourceType == ResourceType.Item) {
            return CountItem(res.Item);
        } else if (res.ResourceType == ResourceType.Tag) {
            return CountItemTag(res.ItemTag);
        } 

    #if UNITY_EDITOR
        throw new System.Exception("Unknown ResourceType variant");
    #endif
    }

    public uint CountItem(Item item) {
        foreach ((Item item_star, uint quantity) in contents) if (item == item_star) return quantity;

        return 0;
    }

    public uint CountItemTag(ItemTag tag) {
        uint count = 0;

        foreach ((Item item, uint quantity) in contents) {
            if (item.HasItemTag(tag)) count += quantity;
        }

        return count;
    }

    public bool Contains(Item item) {
        int index = contents.FindIndex((val) => val.Item1 == item);

        if (index == -1) return false;

        return true;
    }

    public bool HasResources(List<(Resource, uint)> resources) {
        foreach ((Resource res, uint quantity) in resources) {
            if (CountResource(res) < quantity) return false;
        }

        return true;
    }

    /// <summary>
    /// Find all those resource requirements which are currently unsatisfied by the inventory
    /// </summary>
    public List<(Resource, uint)> FindRemainder(List<(Resource, uint)> toSatisfy) {
        List<(Resource, uint)> remaining = new();

        foreach ((Resource res, uint quantity) in toSatisfy) {
            uint existing = CountResource(res);

            if (existing < quantity) remaining.Add((res, quantity - existing));
        }

        return remaining;
    }


    /// <summary>
    /// Try remove a collection of resources (i.e. items and/or tags) from the inventory, returning all those <b>items</b> that were taken
    /// </summary>
    public List<(Item, uint)> TakeResources(List<(Resource, uint)> resources) {
        List<(Item, uint)> taken = new();

        foreach ((Resource res, uint quantity) in resources) taken.AddRange(RemoveResource(res, quantity));

        return taken;
    }

    /// <summary>
    /// Try remove a single resource (i.e. item or tag) from the inventory, returning all those <b>items</b> that were taken
    /// </summary>
    public List<(Item, uint)> RemoveResource(Resource resource, uint quantity) {
        // Item type is easier to deal with, since it's all in one slot
        if (resource.ResourceType == ResourceType.Item) {
            uint existing = CountItem(resource.Item);
            uint toTake = quantity <= existing ? quantity : existing;
            RemoveAtomic(resource.Item, toTake);
            return new() { (resource.Item, toTake) };
        }
    #if UNITY_EDITOR
        else if (resource.ResourceType != ResourceType.Tag) {
            throw new System.Exception("Unknown ResourceType");
        }
    #endif

        List<(Item, uint)> taken = new();

        uint target = quantity;

        for (int i = 0 ; i < contents.Count ; i += 1) {
            (Item current, uint existing) = contents[i];
            if (!current.HasItemTag(resource.ItemTag)) continue;

            // Take either as much of contents[i] as possible, or the reaminder of target
            uint toTake = target < existing ? target : existing;
            RemoveAtomic(current, toTake);
            taken.Add((current, toTake));
            target -= toTake;
            i -= 1;

            if (target <= 0) return taken;
        }

        return taken;
    }


    /// <summary>
    /// Attempt to remove all of the items in the list from this <c>inventory</c>. All successful removals are committed,
    /// even if a given quantity of an item can't be removed because it does not exist (in sufficient quantity).
    /// In such a case, the existing quantity of said item is removed from the <c>inventory</c>, and the remainder is
    /// stored in the output parameter.
    /// </summary>
    /// <returns>True if every item in the list is fully removed, false otherwise.</returns>
    public bool TryRemove(List<(Item, uint)> itemsIn, out List<(Item, uint)> itemsRemaining) {
        bool success = true;
        itemsRemaining = new List<(Item, uint)>();

        for (int index = 0 ; index < itemsIn.Count ; index += 1) {
            (Item item, uint quantity) = itemsIn[index];

            uint existing = CountItem(item);
            if (existing < quantity) {
                itemsRemaining.Add((item, quantity - existing));
                RemoveAtomic(item, existing);
                success = false;
            }

            else RemoveAtomic(item, quantity);
        }

        if (itemsRemaining.Count == 0) itemsRemaining = null;
        return success;
    }

    public List<(Item, uint)> RemoveN(uint toRemoveTotal) {
        List<(Item, uint)> removed = new();

        for (int i = 0 ; i < contents.Count ; i += 1) {
            (Item item, uint quantity) = contents[i];

            uint toRemove = toRemoveTotal < quantity ? toRemoveTotal : quantity;
            RemoveAtomic(item, toRemove);
            removed.Add((item, toRemove));
            toRemoveTotal -= toRemove;
            i -= 1;

            if (toRemoveTotal <= 0) return removed;
        }
    #if UNITY_EDITOR
        throw new System.Exception("Unable to remove requested total from inventory");
    #endif
    }

    /// <summary>
    /// <para>
    /// Remove <c>quantity</c> of <c>item</c> to the inventory in an atomic way;
    /// i.e. either <i>all</i> of the items are removed, or <i>none</i> are.
    /// </para>
    /// Also updates the <c>carrying</c> field.
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public bool RemoveAtomic(Item item, uint quantity) {
        // See if the item is in the inventory
        int itemIndex = contents.FindIndex(0, (tuple) => tuple.Item1 == item);

        // Item does not even exist
        if (itemIndex == -1) return false;

        (_, uint existing) = contents[itemIndex];
        if (existing < quantity) return false;

        carrying -= quantity;

        // Removing this would result in 0 of the item left
        if (existing == quantity) {
            contents.RemoveAt(itemIndex);
            return true;
        }

        contents[itemIndex] = (item, existing - quantity);
        return true;
    }

    /// <summary>
    /// Attempt to add all of the items in the list to this <c>inventory</c>. If capacity runs out
    /// part-way through the list, the partial additions are still committed, and the remaining items
    /// are given as an output parameter.
    /// </summary>
    /// <returns>True if capacity does not run out partway through the operation, false otherwise.</returns>
    public bool TryAdd(List<(Item, uint)> itemsIn, out List<(Item, uint)> itemsRemaining) {
        for (int index = 0 ; index < itemsIn.Count ; index += 1) {
            (Item item, uint quantity) = itemsIn[index];

            if (quantity + carrying <= maxCapacity) {
                AddAtomic(item, quantity);
            }

            else {
                // We have: quantity + carrying > MAX_CAPACITY
                //       => quantity + carrying - MAX_CAPACITY > 0
                // Thus, the inventory can take on: MAX_CAPACITY - carrying
                uint remaining = quantity + carrying - maxCapacity;
                AddAtomic(item, maxCapacity - carrying);

                // Add the remainder of this item
                itemsRemaining = new List<(Item, uint)>() { (item, remaining) };
                // Add rest of the list
                for (int i = index + 1 ; i < itemsIn.Count ; i += 1) itemsRemaining.Add(itemsIn[i]);
                return false;
            }
        }

        itemsRemaining = null;
        return true;
    }

    /// <summary>
    /// <para>
    /// Add <c>quantity</c> of <c>item</c> to the inventory in an atomic way;
    /// i.e. either <i>all</i> of the items are added, or <i>none</i> are added.
    /// </para>
    /// Also updates the <c>carrying</c> field.
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public bool AddAtomic(Item item, uint quantity) {
        // We cannot add all of these items atomically
        if (quantity + carrying > maxCapacity) return false;

        // It must be that quantity == 0, so we don't need to do any work
        if (quantity == 0) return true;

        // See if the item is already in the inventory
        int index = contents.FindIndex(0, (tuple) => tuple.Item1 == item);

        if (index == -1) {
            contents.Add((item, quantity));
        }

        else {
            (_, uint existing) = contents[index];
            contents[index] = (item, existing + quantity);
        }

        carrying += quantity;
        return true;
    }


    public InfoBranch GetInfoTree() {
        InfoBranch root = new InfoBranch("Inventory");

        InfoLeaf maxCapacityProperty = new InfoLeaf("Max Capacity", value: maxCapacity + " item(s)");
        root.AddChild(maxCapacityProperty);

        InfoLeaf carryingProperty = new InfoLeaf("Carrying", value: carrying + " item(s)");
        root.AddChild(carryingProperty);

        // Only try display contents if there are any to show
        if (contents.Count == 0) return root;
        
        InfoBranch contentsCategory = new InfoBranch("Contents");
        root.AddChild(contentsCategory);

        foreach ((Item item, uint quantity) in contents) {
            InfoLeaf itemProperty = new InfoLeaf(item.GetName(), value: quantity + " unit(s)");
            contentsCategory.AddChild(itemProperty);
        }

        return root;
    }
}
