using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : State {

    [SerializeField]
    State stall, pathfind;

    Task task => taskAgent.GetTask();

    public ITaskAgent taskAgent;
    public override void OnSetup() {
        taskAgent = entity.GetComponent<ITaskAgent>();
    }

    public override void OnEntry() {
        stateMachine.SetChildState(pathfind);
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (task != null || success == false) {
            CompleteState();
            return;
        }

        if (exitingChild == stall) {
            // A good idea to leave this state every now and then
            CompleteState();
        } else if (exitingChild == pathfind) {
            stateMachine.SetChildState(stall);
        }
    }
}
