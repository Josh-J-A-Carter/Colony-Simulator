using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task {
    public const int MAX_PROGRESS = 250;
    public int progress { get; protected set; } = 0;

    bool complete = false;

    bool pending = true;

    public TaskPriority priority { get; protected set; }

    public float creationTime { get; protected set; }

    public int assignment { get; protected set; }

    public void IncrementProgress() {
        if (complete) return;
        progress += 1;

        if (progress >= MAX_PROGRESS) {
            TaskManager.Instance.MarkComplete(this);
            complete = true;
        }
    }

    public bool IsPending() {
        return pending;
    }

    public void Confirm() {
        pending = false;
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

    public virtual void OnCreation() {}

    public virtual void OnCompletion() {}

    public virtual void OnCancellation() {}

}

public enum TaskPriority {
    Critical,
    Urgent,
    Important,
    Normal,
    Nonessential
}
