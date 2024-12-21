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

    public abstract String GetName();

    public abstract String GetDescription();

    public abstract InfoBranch GetInfoTree(object obj = null);
}

public enum TaskPriority {
    Critical,
    Urgent,
    Important,
    Normal,
    Nonessential
}
