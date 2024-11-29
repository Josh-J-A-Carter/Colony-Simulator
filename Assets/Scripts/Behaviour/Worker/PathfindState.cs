using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindState : State {
    public override void OnEntry() {
        Debug.Log("Enter pathfind");
    }

    public override void FixedRun(){
        if (stateMachine.activeFor > 5) {
            CompleteState();
        }
    }
}
