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

        Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(location);

        data[Comb.STORAGE_TYPE] = Comb.StorageType.Brood;
        Dictionary<String, object> broodData = new Dictionary<String, object>();
        broodData[Comb.BROOD_TIME_LEFT] = 20;
        data[Comb.BROOD_DATA] = broodData;
    }
}
