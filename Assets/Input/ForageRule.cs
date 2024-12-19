using System;
using System.Collections.Generic;
using UnityEngine;

public class ForageRule : TaskRule {

    public enum Type {
        Nectar,
        Pollen,
        Sap
    }

    Type type;
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

    public void SetTags(List<ItemTag> newTags) {
        if (tags.Equals(newTags)) return;

        tags = newTags;

        updatedRule = true;
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
                
                // Is there already a task at that location?
                foreach (ILocative task in childTasks) if (task.GetStartPosition() == pos) return false;

                // Does it contain an item with all the required tags?
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

            foreach (ItemTag tag in tags) if (item.HasItemTag(tag) == false) continue;

            return true;
        }

        return false;
    }

    public override void OnDestruction() {
        foreach (Task task in childTasks) {
            TaskManager.Instance.CancelTask(task);
        }
    }
}
