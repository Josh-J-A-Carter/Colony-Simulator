using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nurse : State {

    [SerializeField]
    State pathfind, administer;

    public override void OnEntry() {
        stateMachine.SetChildState(pathfind);
    }

    public override void OnChildExit(State exitingChild, bool success = true) {
        if (!success) {
            taskAgent.CancelAssignment();
            CompleteState(false);
            return;
        }

        if (exitingChild == pathfind) stateMachine.SetChildState(administer);
        else CompleteState();
    }
}
