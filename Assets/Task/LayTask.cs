using System;
using UnityEngine;

public class LayTask : Task {
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

    protected override String GetTaskCategory() {
        return "Queen duties";
    }

    protected override String GetTaskType() {
        return "Laying brood";
    }


    public override bool IsWorkerTask() {
        return false;
    }

    public override bool IsQueenTask() {
        return true;
    }
}
