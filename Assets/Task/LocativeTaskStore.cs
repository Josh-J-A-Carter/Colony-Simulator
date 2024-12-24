using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class LocativeTaskStore {

    Dictionary<Vector2Int, List<ILocative>> taskMap = new Dictionary<Vector2Int, List<ILocative>>();

    public void AddTask(ILocative task) {
        foreach (Vector2Int pos in task.GetInteriorPoints()) {
            GetListAt(pos).Add(task);
        }
    }

    List<ILocative> GetListAt(Vector2Int pos) {
        List<ILocative> list;

        bool exists = taskMap.TryGetValue(pos, out list);
        if (exists == false) {
            list = new();
            taskMap[pos] = list;
        }

        return list;
    }

    public List<Task> GetConflictingTasks(ILocative task) {
        List<Task> conflictingTasks = new List<Task>();

        foreach (Vector2Int pos in task.GetInteriorPoints()) {
            List<ILocative> possibleConflicts;
            bool exists = taskMap.TryGetValue(pos, out possibleConflicts);
            if (!exists) continue;

            foreach (ILocative possibleConflict in possibleConflicts) {
                if (possibleConflict.CanCoexist()) continue;

                conflictingTasks.Add(possibleConflict as Task);
            }
        }

        return conflictingTasks;
    }

    public void RemoveTask(ILocative task) {
        foreach (Vector2Int pos in task.GetInteriorPoints()) GetListAt(pos).Remove(task);
    }

    public ReadOnlyCollection<Task> GetTasksAt(Vector2Int pos) {
        return GetListAt(pos).Select(loc => loc as Task).ToList().AsReadOnly();
    }
}
