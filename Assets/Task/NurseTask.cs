using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class NurseTask : Task, ILocative, IConsumer {
    
    BroodComb broodComb;
    Vector2Int startPos;

    ReadOnlyCollection<Vector2Int> exteriorPoints;
    ReadOnlyCollection<Vector2Int> interiorPoints;


    ReadOnlyCollection<(Resource, uint)> requiredResources;

    InventoryManager allocator;
    List<(Item, uint)> allocation;

    bool hasAllocation = false;

    public NurseTask(TaskPriority priority, Vector2Int startPos, BroodComb broodComb, List<(Resource, uint)> requiredResources) {
        this.priority = priority;
        this.startPos = startPos;
        
        this.broodComb = broodComb;
        this.requiredResources = requiredResources.AsReadOnly();

        creationTime = Time.time;
    }

    public override void OnCompletion() {
        Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(startPos);

    #if UNITY_EDITOR
        Debug.Assert(allocation.Count == 1);
    #endif

        (Item item, uint quantity) = allocation[0];

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

    public bool CanCoexist() {
        return true;
    }


    public ReadOnlyCollection<(Resource, uint)> GetRequiredResources() {
        return requiredResources;
    }

    public bool HasAllocation() {
        return hasAllocation;
    }

    public void Allocate(InventoryManager allocator, List<(Item, uint)> allocation) {
        this.allocator = allocator;
        this.allocation = allocation;
        hasAllocation = true;
    }

    public (InventoryManager, List<(Item, uint)>) Deallocate() {
        allocation = null;
        allocator = null;
        hasAllocation = false;
        return (allocator, allocation);
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

        int percentProgress = (int) (100 * (float) progress / MAX_PROGRESS);
        InfoLeaf progressProperty = new InfoLeaf("Progress", percentProgress + "%");
        root.AddChild(progressProperty);

        return root;
    }
}
