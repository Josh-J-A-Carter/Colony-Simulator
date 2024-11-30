using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildState : State {
    BuildTask task => (BuildTask) taskAgent.GetTask();

    public override void FixedRun(){
        task.IncrementProgress();
        if (task.IsComplete()) {
            CompleteState();
        }
    }
}
