using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocativeTaskStore {

    Dictionary<Vector2Int, Locative> taskMap = new Dictionary<Vector2Int, Locative>();

    public void SetTask(Locative task) {
        foreach (Vector2Int pos in task.GetInteriorPoints()) taskMap[pos] = task;
    }

    public List<Task> GetConflictingTasks(Locative task) {
        List<Task> conflictingTasks = new List<Task>();

        foreach (Vector2Int pos in task.GetInteriorPoints()) {
            Locative possibleConflict;
            bool exists = taskMap.TryGetValue(pos, out possibleConflict);

            if (!exists) continue;

            conflictingTasks.Add(possibleConflict as Task);
        }

        return conflictingTasks;
    }

    public void UnsetTask(Locative task) {
        foreach (Vector2Int pos in task.GetInteriorPoints()) taskMap.Remove(pos);
    }

}
