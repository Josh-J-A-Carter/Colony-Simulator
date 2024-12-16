using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Tidy : State {

    [SerializeField]
    State Collect, Store;

    int tidyCycles;

    const int MAX_TIDY_CYCLES = 3;

    InventoryManager inventory;

    public override void OnSetup() {
        inventory = entity.GetComponent<InventoryManager>();
    }

    public override void OnEntry() {
        tidyCycles = 0;

        DecideState();
    }

    public override void OnChildExit(State _exitingChild, bool success){
        if (!success) {
            CompleteState();
            return;
        }

        tidyCycles += 1;

        DecideState();
    }

    void DecideState() {
        ReadOnlyCollection<ItemEntity> itemEntities = EntityManager.Instance.GetItemEntities();

        if (tidyCycles >= MAX_TIDY_CYCLES) {
            CompleteState();
        } else if (inventory.RemainingCapacity() == 0 || itemEntities.Count == 0) {
            stateMachine.SetChildState(Store);
        } else if (inventory.RemainingCapacity() > 0 && itemEntities.Count > 0) {
            stateMachine.SetChildState(Collect);
        } else {
            CompleteState();
        }
    }
    
}
