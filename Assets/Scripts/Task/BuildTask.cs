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

        List<Vector2Int> exterior = CalculateExteriorPoints();

        // Debug.Log("exterior:");
        // foreach (Vector2Int v in exterior) Debug.Log(v);
    }

    public override void OnCompletion() {
        TileManager.Instance.RemoveTaskPreview(location);

        TileManager.Instance.Construct(location, constructable);
    }

    public List<Vector2Int> CalculateExteriorPoints() {
        List<Vector2Int> exterior = constructable.CalculateExteriorPoints();

        for (int i = 0 ; i < exterior.Count ; i += 1) {
            exterior[i] += location;
        }

        return exterior;
    }

}
