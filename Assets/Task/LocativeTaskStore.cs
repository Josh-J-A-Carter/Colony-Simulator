using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocativeTaskStore {

    Dictionary<Vector2Int, ILocative> taskMap = new Dictionary<Vector2Int, ILocative>();

    public void SetTask(ILocative task) {
        foreach (Vector2Int pos in task.GetInteriorPoints()) taskMap[pos] = task;
    }

    public List<Task> GetConflictingTasks(ILocative task) {
        List<Task> conflictingTasks = new List<Task>();

        foreach (Vector2Int pos in task.GetInteriorPoints()) {
            ILocative possibleConflict;
            bool exists = taskMap.TryGetValue(pos, out possibleConflict);

            if (!exists) continue;

            conflictingTasks.Add(possibleConflict as Task);
        }

        return conflictingTasks;
    }

    public void UnsetTask(ILocative task) {
        foreach (Vector2Int pos in task.GetInteriorPoints()) taskMap.Remove(pos);
    }

}
