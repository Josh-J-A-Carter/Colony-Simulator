using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public interface IConsumer {

    public abstract ReadOnlyCollection<(Resource, uint)> GetRequiredResources();

    public abstract bool HasAllocation();

    public abstract void Allocate(InventoryManager inventory, List<(Item, uint)> allocation);

    public abstract (InventoryManager, List<(Item, uint)>) Deallocate();

    /// <summary>
    /// Where should the items be left if the task is cancelled, and the items need to be left somewhere
    /// other than the allocator (e.g. the allocator has been destroyed).
    /// </summary>
    /// <returns></returns>
    public abstract Vector2Int GetDefaultDeallocationPosition();
}
