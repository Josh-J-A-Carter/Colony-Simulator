using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveState : State {

    [SerializeField]
    State Pathfind, Build;
    BuildTask task => (BuildTask) taskAgent.GetTask();

    const float MAX_IDLE = 2.0f;
    
    public override void OnEntry() {
        Debug.Log($"current child: {child}");

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
            stateMachine.SetChildState(Pathfind);
        }
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (exitingChild == Pathfind && success) {
            stateMachine.SetChildState(Build);
        } else if (exitingChild == Build && success) {
            CompleteState();
        } else if (!success) {
            taskAgent.CancelAssignment();
            CompleteState();
        }
    }
}
