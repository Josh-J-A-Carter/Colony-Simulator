using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lay : State {

    [SerializeField]
    State pathfind, deposit;
    LayTask task => (LayTask) taskAgent.GetTask();

    const float MAX_IDLE = 2.0f;

    public ITaskAgent taskAgent;
    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
    }
    
    public override void OnEntry() {
        if (task == null) {
            CompleteState();
            return;
        }
    }

    public override void FixedRun(){
        // If we haven't yet received a task, exit the state
        if (task == null) {
            if (parent.activeFor > MAX_IDLE) CompleteState();
            return;
        }

        if (child == null) {
            stateMachine.SetChildState(pathfind);
        }
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (exitingChild == pathfind && success) {
            stateMachine.SetChildState(deposit);
        } else if (exitingChild == deposit && success) {
            CompleteState();
        } else if (!success) {
            taskAgent.CancelAssignment();
            CompleteState();
        }
    }
}
