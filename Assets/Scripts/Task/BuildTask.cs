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

        TileManager.Instance.Construct(location, constructable);
    }

}
