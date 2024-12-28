using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class DestroyTask : Task, ILocative {
    Vector2Int startPos;
    Constructable preview;
    Constructable constructable;

    ReadOnlyCollection<Vector2Int> exteriorPoints;
    ReadOnlyCollection<Vector2Int> interiorPoints;

    public DestroyTask(TaskPriority priority, Vector2Int startPos, Constructable preview, Constructable constructable) {
        this.priority = priority;

        this.startPos = startPos;
        this.preview = preview;
        this.constructable = constructable;

        creationTime = Time.time;
    }

    public bool IsConstructableTagPresent(ConstructableTag tag) {
        return constructable.HasTag(tag);
    }

    public override void OnCancellation() {
        TileManager.Instance.RemoveTaskPreview(startPos);
    }

    public override void OnConfirmation() {
        TileManager.Instance.SetTaskPreview(startPos, preview);
    }

    public override void OnCompletion() {
        TileManager.Instance.RemoveTaskPreview(startPos);

        TileManager.Instance.Destroy(startPos);
    }

    public ReadOnlyCollection<Vector2Int> GetExteriorPoints() {
        if (exteriorPoints == null) {
            List<Vector2Int> exteriorTemp = new();
            foreach (Vector2Int pos in constructable.GetExteriorPoints()) exteriorTemp.Add(pos + startPos);
            exteriorPoints = exteriorTemp.AsReadOnly();
        }

        return exteriorPoints;
    }

    public ReadOnlyCollection<Vector2Int> GetInteriorPoints() {
        if (interiorPoints == null) {
            List<Vector2Int> interiorTemp = new();
            foreach (Vector2Int pos in constructable.GetInteriorPoints()) interiorTemp.Add(pos + startPos);
            interiorPoints = interiorTemp.AsReadOnly();
        }

        return interiorPoints;
    }

    public Vector2Int GetStartPosition() {
        return startPos;
    }

    public bool CanCoexist() {
        return false;
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

        int percentProgress = (int) (100 * (float) progress / MAX_PROGRESS);
        InfoLeaf progressProperty = new InfoLeaf("Progress", percentProgress + "%");
        root.AddChild(progressProperty);

        return root;
    }

}
