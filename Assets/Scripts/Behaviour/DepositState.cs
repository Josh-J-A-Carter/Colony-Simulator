using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepositState : State {
    LayTask task => (LayTask) taskAgent.GetTask();

    public override void FixedRun(){
        task.IncrementProgress();
        if (task.IsComplete()) {
            CompleteState();
        }
    }
}
