using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Tidy : State {

    [SerializeField]
    State Collect, Store;

    public override void OnEntry() {
        DecideState();
    }

    public override void OnChildExit(State _exitingChild, bool success){
        if (!success) {
            CompleteState();
            return;
        }

        DecideState();
    }

    void DecideState() {
        ReadOnlyCollection<ItemEntity> itemEntities = EntityManager.Instance.GetItemEntities();

        if (inventory.RemainingCapacity() == 0 || itemEntities.Count == 0) {
            stateMachine.SetChildState(Store);
        } else if (itemEntities.Count > 1) {
            stateMachine.SetChildState(Collect);
        } else {
            CompleteState();
        }
    }
    
}
