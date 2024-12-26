using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class ForageRule : TaskRule {

    public enum Type {
        Nectar,
        Pollen,
        Sap
    }

    public Type type { get; protected set; }
    ItemTag mainTag;

    List<ItemTag> tags;
    List<ForageTask> childTasks;

    bool updatedRule;

    public ForageRule(Type type, List<ItemTag> tags, TaskPriority priority) {
        this.priority = priority;
        this.type = type;
        this.tags = tags;

        mainTag = type switch {
            Type.Nectar => ItemTag.Nectar,
            Type.Pollen => ItemTag.Pollen,
            Type.Sap => ItemTag.Sap,
            _ => throw new Exception($"Unknown ForageRule type, {type}")
        };

        childTasks = new();

        updatedRule = false;
    }

    public void SetType(Type newType) {
        if (type == newType) return;

        type = newType;

        mainTag = type switch {
            Type.Nectar => ItemTag.Nectar,
            Type.Pollen => ItemTag.Pollen,
            Type.Sap => ItemTag.Sap,
            _ => throw new Exception($"Unknown ForageRule type, {type}")
        };

        updatedRule = true;
    }

    public void SetPriority(TaskPriority newPriority) {
        if (priority == newPriority) return;

        priority = newPriority;

        updatedRule = true;
    }

    public void AddTag(ItemTag tag) {
        if (tags.Contains(tag)) return;

        tags.Add(tag);

        updatedRule = true;
    }

    public void RemoveTag(ItemTag tag) {
        if (tags.Remove(tag) == false)  return;

        updatedRule = true;
    }

    public bool HasTag(ItemTag tag) {
        return tags.Contains(tag);
    }

    public override void Refresh() {
        if (updatedRule) {
            CancelInvalidatedTasks();
            updatedRule = false;
        }

        RemoveOldTaskReferences();

        TryCreateNewTasks();
    }

    void CancelInvalidatedTasks() {
        for (int index = 0 ; index < childTasks.Count ; index += 1) {
            ForageTask task = childTasks[index];
            if (task.priority != priority || !SatisfyTags(task.GetAvailableRewardItems())) {
                TaskManager.Instance.CancelTask(task);
                childTasks.RemoveAt(index);
                index -= 1;
            }
        }
    }

    void RemoveOldTaskReferences() {
        for (int i = 0 ; i < childTasks.Count ; i += 1 ) {
            if (childTasks[i].IsComplete()) {
                childTasks.RemoveAt(i);
                i -= 1;
            }
        }
    }

    void TryCreateNewTasks() {
        List<(Vector2Int, IProducer, Dictionary<String, object>)> storage = TileManager.Instance.QueryTileEntities<IProducer>(
            val => {
                (Vector2Int pos, IProducer type, Dictionary<String, object> instance) = val;
                
                // Is there already a foraging task at that location?
                ReadOnlyCollection<Task> existingTasks = TaskManager.Instance.GetTasksAt(pos);
                foreach (Task task in existingTasks) if (task is ForageTask) return false;

                // Does it contain an item with all the required tags
                return SatisfyTags(type.AvailableProductionItemTypes(instance));
            }
        );

        foreach ((Vector2Int pos, IProducer type, _) in storage) {
            ForageTask task = new(pos, type);

            childTasks.Add(task);
            TaskManager.Instance.CreateTask(task);
        }
    }

    bool SatisfyTags(List<Item> items) {
        foreach (Item item in items) {
            if (item.HasItemTag(mainTag) == false) continue;

            bool found = true;
            foreach (ItemTag tag in tags) {
                if (item.HasItemTag(tag) == false) {
                    found = false;
                    break;
                }
            }

            if (found) return true;
        }

        return false;
    }

    public override void OnDestruction() {
        foreach (Task task in childTasks) {
            TaskManager.Instance.CancelTask(task);
        }
    }

    public bool Equals(ForageRule rule) {
        if (rule.mainTag != mainTag) return false;

        foreach (ItemTag t in tags) if (rule.tags.Contains(t) == false) return false;
        foreach (ItemTag t in rule.tags) if (tags.Contains(t) == false) return false;

        return true;
    }
}
