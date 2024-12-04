using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildState : State {
    BuildTask task => (BuildTask) taskAgent.GetTask();

    public override void FixedRun(){
        if (task == null) return;

        task.IncrementProgress();
        if (task.IsComplete()) {
            CompleteState();
        }
    }
}
