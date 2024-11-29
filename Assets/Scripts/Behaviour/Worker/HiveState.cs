using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveState : State {

    [SerializeField]
    State Pathfind, Build;

    WorkerTask task => (WorkerTask) taskAgent.GetTask();

    const float MAX_IDLE = 2.0f;
    
    public override void OnEntry() {
        Debug.Log("Enter hive state");

        if (task == null) {
            CompleteState();
            Debug.Log("Exit hive state");
            return;
        }

        // Wrong type of task, exit the state
        if (this.task.category != WorkerTaskType.Hive) {
            CompleteState();
                        Debug.Log("Exit hive state");
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

    public override void OnChildExit(State exitingChild) {
        if (exitingChild == Pathfind) {
            Debug.Log("Exit pathfind");
            stateMachine.SetChildState(Build);
        } else if (exitingChild == Build) {
            Debug.Log("Exit hive state");
            CompleteState();
        }
    }
}
