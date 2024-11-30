using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayState : State {

    [SerializeField]
    State FindComb, Deposit;
    LayTask task => (LayTask) taskAgent.GetTask();

    const float MAX_IDLE = 2.0f;
    
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
            stateMachine.SetChildState(FindComb);
        }
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (exitingChild == FindComb && success) {
            stateMachine.SetChildState(Deposit);
        } else if (exitingChild == Deposit && success) {
            CompleteState();
        } else if (!success) {
            taskAgent.CancelAssignment();
            CompleteState();
        }
    }
}
