using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildState : State {
    WorkerTask task => (WorkerTask) taskAgent.GetTask();

    public override void OnEntry() {
        Debug.Log("Enter build");
    }

    public override void Run(){
        task.IncrementProgress();
        if (task.IsComplete()) {
            Debug.Log("Complete build");
            CompleteState();
        }
    }

}
