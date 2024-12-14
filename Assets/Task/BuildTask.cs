using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BuildTask : WorkerTask, ILocative, Consumer {
    
    Constructable constructable;
    Vector2Int startPos;

    Dictionary<String, object> configDataTemplate;

    ReadOnlyCollection<Vector2Int> exteriorPoints;
    ReadOnlyCollection<Vector2Int> interiorPoints;


    InventoryManager allocator;

    bool hasAllocation = false;

    public BuildTask(TaskPriority priority, Vector2Int startPos, Constructable constructable, Dictionary<String, object> configDataTemplate = null) {
        this.priority = priority;
        this.startPos = startPos;
        this.constructable = constructable;
        this.configDataTemplate = configDataTemplate;

        creationTime = Time.time;
    }

    public override WorkerTaskType GetCategory() {
        return WorkerTaskType.House;
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

        TileManager.Instance.Construct(startPos, constructable, configDataTemplate);

        if (constructable is BroodComb broodComb) {
            TaskManager.Instance.CreateTask(new LayTask(TaskPriority.Normal, startPos));
        }   
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


    public override String GetName() {
        return "Construction";
    }

    public override String GetDescription() {
        return "Constructing little bee structures out of beeswax and other naturally found materials";
    }

    public override InfoBranch GetInfoTree(object obj = null) {
        InfoBranch root = new InfoBranch(String.Empty);
        
        InfoLeaf nameProperty = new InfoLeaf("Type", "Construction");
        root.AddChild(nameProperty);

        InfoLeaf progressProperty = new InfoLeaf("Progress", (float) progress / MAX_PROGRESS + "%");
        root.AddChild(progressProperty);

        return root;
    }
}
