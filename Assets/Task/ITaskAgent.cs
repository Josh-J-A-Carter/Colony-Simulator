
public interface ITaskAgent {

    /// <summary>
    /// Offer a task to the agent, which they may either accept or decline.
    /// <br></br>
    /// If accepted, the agent goes on to set its current task.
    /// </summary>
    /// <returns>Returns <c>true</c> if accepted, <c>false</c> otherwise.</returns>
    public abstract bool OfferTask(Task task);

    /// <summary>
    /// Calling this function allows a task agent to discontinue its current task prematurely.
    /// This is particularly useful if the agent finds that it has become impossible to fulfill the task.
    /// </summary>
    public void CancelAssignment() {
        TaskManager.Instance.UnassignAgent(this);
    }

    public virtual void OnTaskCancellation() {}

    /// <summary>
    /// Forcibly set the task of the agent. This is always accepted by the agent,
    //  as opposed to <c>OfferTask</c> which can be declined.
    /// </summary>
    public abstract void SetTask(Task task);

    public abstract Task GetTask();
}
