using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State {

    [SerializeField]
    State Stall, Meander;

    Task task => taskAgent.GetTask();

    public override void OnEntry() {
        stateMachine.SetChildState(Meander);
    }

    public override void OnChildExit(State exitingChild, bool _) {
        if (task != null) CompleteState();

        if (exitingChild == Stall) {
            stateMachine.SetChildState(Meander);
        } else if (exitingChild == Meander) {
            stateMachine.SetChildState(Stall);
        }
    }
}
