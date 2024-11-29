using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface TaskAgent {

    /// <summary>
    /// Offer a task to the agent, which they may either accept or decline.
    /// <br></br>
    /// If accepted, the agent goes on to set its current task.
    /// </summary>
    /// <returns>Returns <c>true</c> if accepted, <c>false</c> otherwise.</returns>
    public abstract bool OfferTask(Task task);

    public void CancelAssignment() {
        TaskManager.Instance.UnassignAgent(this);
    }

    /// <summary>
    /// Forcibly set the task of the agent. This is always accepted by the agent,
    //  as opposed to <c>OfferTask</c> which can be declined.
    /// </summary>
    public abstract void SetTask(Task task);

    public abstract Task GetTask();

}
