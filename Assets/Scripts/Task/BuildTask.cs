using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildTask : WorkerTask {
    
    Constructable constructable;
    Vector2Int location;

    public BuildTask(TaskPriority priority, Vector2Int location, Constructable constructable) {
        this.priority = priority;
        this.location = location;
        this.constructable = constructable;

        creationTime = Time.time;
        category = WorkerTaskType.Hive;
    }

    public override void OnCreation() {
        TileManager.Instance.SetTaskPreview(location, constructable);
    }

    public override void OnCompletion() {
        TileManager.Instance.RemoveTaskPreview(location);

        Dictionary<String, object> data;
        TileManager.Instance.Construct(location, constructable, out data);

        data[Comb.STORAGE_TYPE] = Comb.StorageType.Brood;
        Dictionary<String, object> broodData = new Dictionary<String, object>();
        broodData[Comb.BROOD_TIME_LEFT] = 20;
        data[Comb.BROOD_DATA] = broodData;
    }

    public List<Vector2Int> CalculateExteriorPoints() {
        List<Vector2Int> exterior = constructable.CalculateExteriorPoints();

        for (int i = 0 ; i < exterior.Count ; i += 1) {
            exterior[i] += location;
        }

        return exterior;
    }

}
