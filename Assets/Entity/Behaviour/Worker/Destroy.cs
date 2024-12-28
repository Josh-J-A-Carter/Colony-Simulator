using UnityEngine;

public class Destroy : State {
    [SerializeField]
    State pathfind, destroy;
    
    ITaskAgent taskAgent;
    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
    }
    
    public override void OnEntry() {
        stateMachine.SetChildState(pathfind);
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (!success) {
            taskAgent.CancelAssignment();
            CompleteState(false);
            return;
        }

        if (exitingChild == pathfind) stateMachine.SetChildState(destroy);
        else CompleteState();    
    }
}
