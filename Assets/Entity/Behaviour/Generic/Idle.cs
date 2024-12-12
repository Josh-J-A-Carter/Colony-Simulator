using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : State {

    [SerializeField]
    State stall, pathfind;

    Task task => taskAgent?.GetTask();

    public override void OnEntry() {
        stateMachine.SetChildState(pathfind);
    }

    public override void OnChildExit(State exitingChild, bool _) {
        if (task != null) CompleteState();

        if (exitingChild == stall) {
            // A good idea to leave this state every now and then
            CompleteState();
        } else if (exitingChild == pathfind) {
            stateMachine.SetChildState(stall);
        }
    }
}
