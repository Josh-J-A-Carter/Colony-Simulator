using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State {

    [SerializeField]
    State Stall, Meander;

    public override void OnEntry() {
        stateMachine.SetState(Stall);
    }

    public override void OnChildExit(State exitingChild) {
        if (exitingChild == Stall) {
            stateMachine.SetState(Meander);
        } else if (exitingChild == Meander) {
            stateMachine.SetState(Stall);
        }
    }
}
