using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayTask : QueenTask {
    Vector2Int location;

    public LayTask(TaskPriority priority, Vector2Int location) {
        this.priority = priority;
        this.location = location;

        creationTime = Time.time;
    }

    public Vector2Int GetLocation() {
        return location;
    }

    public override void OnCompletion() {
        (_, Constructable constructable) = TileManager.Instance.GetConstructableAt(location);
        BroodComb comb = constructable as BroodComb;

        if (comb == null) return;
        comb.TryLayEgg(location, comb.toFertilise);
    }

    public override String GetName() {
        return "Foraging";
    }

    public override String GetDescription() {
        return "Foraging for nectar, pollen, sap, and other resources essential to the survival of the bee nest";
    }

    public override InfoBranch GetInfoTree(object obj = null) {
        InfoBranch root = new InfoBranch(String.Empty);
        
        InfoLeaf nameProperty = new InfoLeaf("Type", "Foraging");
        root.AddChild(nameProperty);

        int percentProgress = (int) (100 * (float) progress / MAX_PROGRESS);
        InfoLeaf progressProperty = new InfoLeaf("Progress", percentProgress + "%");
        root.AddChild(progressProperty);

        return root;
    }
}
