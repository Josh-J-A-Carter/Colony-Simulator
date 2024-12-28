using UnityEngine;

public class Harvest : State {

    [SerializeField]
    State pathfind, collect;

    ITaskAgent taskAgent;
    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();

        (pathfind as Locative__Pathfind).TargetInteriorPoints();
    }

    public override void OnEntry() {
        stateMachine.SetChildState(pathfind);
    }

    public override void OnChildExit(State exitingChild, bool success = true) {
        if (!success) {
            taskAgent.CancelAssignment();
            CompleteState(false);
            return;
        }

        if (exitingChild == pathfind) stateMachine.SetChildState(collect);
        else CompleteState();
    }
}
