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
}
