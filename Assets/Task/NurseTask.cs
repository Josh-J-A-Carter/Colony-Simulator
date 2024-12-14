using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class NurseTask : WorkerTask, ILocative, Consumer {
    
    BroodComb broodComb;
    Vector2Int startPos;

    ReadOnlyCollection<Vector2Int> exteriorPoints;
    ReadOnlyCollection<Vector2Int> interiorPoints;


    ReadOnlyCollection<(Item, uint)> requiredResources;
    Item item;
    uint quantity;
    InventoryManager allocator;

    bool hasAllocation = false;

    public NurseTask(TaskPriority priority, Vector2Int startPos, BroodComb broodComb, Item item, uint quantity) {
        this.priority = priority;
        this.startPos = startPos;
        
        this.broodComb = broodComb;
        this.item = item;
        this.quantity = quantity;

        requiredResources = new List<(Item, uint)>() { (item, quantity) }.AsReadOnly();

        creationTime = Time.time;
    }

    public override WorkerTaskType GetCategory() {
        return WorkerTaskType.Nurse;
    }

    public override void OnCompletion() {
        Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(startPos);

        broodComb.GiveBrood(startPos, data, item, quantity);
    }

    public ReadOnlyCollection<Vector2Int> GetExteriorPoints() {
        if (exteriorPoints == null) {
            List<Vector2Int> exteriorTemp = new List<Vector2Int>();
            foreach (Vector2Int pos in broodComb.GetExteriorPoints()) exteriorTemp.Add(pos + startPos);
            exteriorPoints = exteriorTemp.AsReadOnly();
        }

        return exteriorPoints;
    }

    public ReadOnlyCollection<Vector2Int> GetInteriorPoints() {
        if (interiorPoints == null) {
            List<Vector2Int> interiorTemp = new List<Vector2Int>();
            foreach (Vector2Int pos in broodComb.GetInteriorPoints()) interiorTemp.Add(pos + startPos);
            interiorPoints = interiorTemp.AsReadOnly();
        }

        return interiorPoints;
    }

    public Vector2Int GetStartPosition() {
        return startPos;
    }


    public ReadOnlyCollection<(Item, uint)> GetRequiredResources() {
        return requiredResources;
    }

    public bool HasAllocation() {
        return hasAllocation;
    }

    public void Allocate(InventoryManager allocator) {
        this.allocator = allocator;
        hasAllocation = true;
    }

    public InventoryManager GetAllocator() {
        return allocator;
    }

    public Vector2Int GetDefaultDeallocationPosition() {
        return startPos;
    }

    public override String GetName() {
        return "Nursing";
    }

    public override String GetDescription() {
        return "Feeding newly hatched eggs and capping larvae before they become pupae";
    }

    public override InfoBranch GetInfoTree(object obj = null) {
        InfoBranch root = new InfoBranch(String.Empty);
        
        InfoLeaf nameProperty = new InfoLeaf("Type", "Nursing");
        root.AddChild(nameProperty);

        InfoLeaf progressProperty = new InfoLeaf("Progress", (float) progress / MAX_PROGRESS + "%");
        root.AddChild(progressProperty);

        return root;
    }
}
