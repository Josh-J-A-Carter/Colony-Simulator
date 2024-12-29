using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public abstract class Task : IInformative {
    public const int MAX_PROGRESS = 250;
    public int progress { get; protected set; } = 0;

    bool complete = false;

    bool pendingConfirmation = true;

    public TaskPriority priority { get; protected set; }

    public float creationTime { get; protected set; }

    public int assignment { get; protected set; }

    public void IncrementProgress() {
        if (complete) return;
        progress += 1;

        if (progress >= MAX_PROGRESS) {
            // Only allow for completion once resources have been allocated (if applicable)
            if (this is IConsumer consumer && !consumer.HasAllocation()) return;
            TaskManager.Instance.MarkComplete(this);
            complete = true;
        }
    }

    /// <summary>
    /// Give control to the Task implementation so that it can decide whether to end early
    /// </summary>
    public virtual bool EarlyCompletion() {
        return false;
    }

    /// <summary>
    /// Before the task is confirmed, this method is called to decide whether the task should be
    /// aborted, or if it can be continued with.
    /// </summary>
    public virtual bool MustAbort() {
        return false;
    }

    public bool IsPendingConfirmation() {
        return pendingConfirmation;
    }

    public void Confirm() {
        pendingConfirmation = false;

        OnConfirmation();
    }

    public void SetPriority(TaskPriority priority) {
        this.priority = priority;
    }

    public void IncrementAssignment() {
        assignment += 1;
    }

    public void DecrementAssignment() {
        assignment -= 1;
    }

    public bool IsComplete() {
        return complete;
    }

    public virtual void OnConfirmation() {}

    public virtual void OnCompletion() {}

    public virtual void OnCancellation() {}

    public String GetName() {
        throw new NotImplementedException();
    }

    public String GetDescription() {
        throw new NotImplementedException();
    }

    public InfoBranch GetGenericInfoTree() {
        InfoBranch genericCategory = new("Generic task info");

        InfoLeaf typeProperty = new("Task", GetTaskType());
        genericCategory.AddChild(typeProperty);

        InfoLeaf categoryProperty = new("Category", GetTaskCategory());
        genericCategory.AddChild(categoryProperty);

        InfoLeaf priorityProperty = new("Priority", Enum.GetName(typeof(TaskPriority), priority));
        genericCategory.AddChild(priorityProperty);

        InfoLeaf progressProperty = new("Progress", GetProgress());
        genericCategory.AddChild(progressProperty);

        InfoLeaf allocProperty = new("Assigned workers", assignment + " unit(s)");
        genericCategory.AddChild(allocProperty);

        return genericCategory;
    }

    InfoBranch GetConsumerInfo() {
        IConsumer con = this as IConsumer;
        if (con == null) return null;

        InfoBranch consumerCategory = new("Resource requirement info");

        InfoLeaf allocProperty = new("Resources allocated", con.HasAllocation().ToString());
        consumerCategory.AddChild(allocProperty);

        foreach ((Resource res, uint quantity) in con.GetRequiredResources()) {
            String name;
            if (res.ResourceType == ResourceType.Item) name = res.Item.GetName();
            else name = Utilities.GetDescription(res.ItemTag);
            InfoLeaf itemProperty = new InfoLeaf(name, value: quantity + " unit(s)");
            consumerCategory.AddChild(itemProperty);
        }

        return consumerCategory;
    }

    protected virtual InfoBranch GetOtherTaskInfo() {
        return null;
    }

    public InfoBranch GetInfoTree(object obj = null) {
        InfoBranch root = new($"Information for task \"{GetTaskType()}\"");
        
        root.AddChild(GetGenericInfoTree());

        InfoBranch consumerInfo = GetConsumerInfo();
        if (consumerInfo != null) root.AddChild(consumerInfo);

        InfoBranch otherInfo = GetOtherTaskInfo();
        if (otherInfo != null) root.AddChild(otherInfo);

        return root;
    }

    protected abstract String GetTaskCategory();

    protected abstract String GetTaskType();

    protected virtual String GetProgress() {
        int percentProgress = (int) (100 * (float) progress / MAX_PROGRESS);
        return percentProgress + "%";
    }

    public virtual bool IsWorkerTask() {
        return true;
    }

    public virtual bool IsQueenTask() {
        return false;
    }

    public virtual bool IsRuleGenerated() {
        return false;
    }
}

public enum TaskPriority {
    Critical,
    Urgent,
    Important,
    Normal,
    Nonessential
}
