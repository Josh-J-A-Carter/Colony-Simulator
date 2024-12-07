using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BuildTask : WorkerTask, Locative, Consumer {
    
    Constructable constructable;
    Vector2Int startPos;

    ReadOnlyCollection<Vector2Int> exteriorPoints;
    ReadOnlyCollection<Vector2Int> interiorPoints;


    InventoryManager allocator;

    bool hasAllocation = false;

    public BuildTask(TaskPriority priority, Vector2Int startPos, Constructable constructable) {
        this.priority = priority;
        this.startPos = startPos;
        this.constructable = constructable;

        creationTime = Time.time;
        category = WorkerTaskType.Hive;
    }

    public override bool MustAbort() {
        // If there is already something in the world map at any of the necessary positions, abort
        foreach (Vector2Int pos in GetInteriorPoints()) {
            (_, Constructable existingConstructable) =  TileManager.Instance.GetConstructableAt(pos);
            if (existingConstructable != null) return true;
        }

        return false;
    }

    public override void OnCancellation() {
        TileManager.Instance.RemoveTaskPreview(startPos);
    }

    public override void OnConfirmation() {
        TileManager.Instance.SetTaskPreview(startPos, constructable);
    }

    public override void OnCompletion() {
        TileManager.Instance.RemoveTaskPreview(startPos);

        TileManager.Instance.Construct(startPos, constructable);

        TaskManager.Instance.CreateTask(new LayTask(TaskPriority.Normal, startPos));
    }

    public ReadOnlyCollection<Vector2Int> GetExteriorPoints() {
        if (exteriorPoints == null) {
            List<Vector2Int> exteriorTemp = new List<Vector2Int>();
            foreach (Vector2Int pos in constructable.GetExteriorPoints()) exteriorTemp.Add(pos + startPos);
            exteriorPoints = exteriorTemp.AsReadOnly();
        }

        return exteriorPoints;
    }

    public ReadOnlyCollection<Vector2Int> GetInteriorPoints() {
        if (interiorPoints == null) {
            List<Vector2Int> interiorTemp = new List<Vector2Int>();
            foreach (Vector2Int pos in constructable.GetInteriorPoints()) interiorTemp.Add(pos + startPos);
            interiorPoints = interiorTemp.AsReadOnly();
        }

        return interiorPoints;
    }

    public Vector2Int GetStartPosition() {
        return startPos;
    }


    public ReadOnlyCollection<(Item, uint)> GetRequiredResources() {
        return constructable.GetRequiredResources();
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
}
