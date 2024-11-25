using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State {

    [SerializeField]
    State Stall, Meander;

    public override void OnEnter() {
        stateMachine.SetState(Stall);

        Debug.Log("Set stall");
    }

    public override void OnChildExit(State exitingChild) {
        Debug.Log("Exit stall");

        if (exitingChild == Stall) {
            stateMachine.SetState(Meander);
        } else if (exitingChild == Meander) {
            stateMachine.SetState(Stall);
        }
    }
}
