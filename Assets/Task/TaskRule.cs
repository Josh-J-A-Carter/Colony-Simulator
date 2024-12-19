using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that takes some parameters to determine at regular intervals if new tasks should be created, until the rule is destroyed.
/// </summary>
public abstract class TaskRule {

    public TaskPriority priority { get; protected set; }

    ///<summary> Tell the <c>TaskRule</c> to check for any updates, and create or destroy tasks as needed. </summary>
    public abstract void Refresh();

    public virtual void OnDestruction() {}

}
