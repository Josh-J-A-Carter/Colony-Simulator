using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task {
    public const int MAX_PROGRESS = 250;
    public int progress { get; protected set; } = 0;

    bool complete = false;

    public TaskPriority priority { get; protected set; }

    public float creationTime { get; protected set; }

    public int assignment { get; protected set; }

    public void IncrementProgress() {
        if (complete) return;
        progress += 1;

        TaskManager.Instance.MarkComplete(this);
        complete = true;
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

}

public enum TaskPriority {
    Emergency,
    Critical,
    Urgent,
    Important,
    Nonessential,
    Insignificant
}
