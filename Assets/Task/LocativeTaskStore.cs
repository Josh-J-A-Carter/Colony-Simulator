using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocativeTaskStore {

    Dictionary<Vector2Int, Task> taskMap = new Dictionary<Vector2Int, Task>();

    public void SetTask<T>(T task) where T: Task, Locative {
        foreach (Vector2Int pos in task.GetInteriorPoints()) taskMap[pos] = task;
    }

    public List<Task> GetConflictingTasks<T>(T task) where T: Task, Locative {
        List<Task> conflictingTasks = new List<Task>();

        foreach (Vector2Int pos in task.GetInteriorPoints()) {
            Task possibleConflict;
            bool exists = taskMap.TryGetValue(pos, out possibleConflict);

            if (!exists) continue;

            conflictingTasks.Add(possibleConflict);
        }

        return conflictingTasks;
    }

    public void UnsetTask<T>(T task) where T: Task, Locative {
        foreach (Vector2Int pos in task.GetInteriorPoints()) taskMap.Remove(pos);
    }

}
