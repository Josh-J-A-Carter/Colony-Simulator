using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Patrol : State {

    [SerializeField]
    State patrolWander, patrolReturn;

    HornetBehaviour hornet;

    Vector2Int home => hornet.Home;

    const int DISTANCE_BEFORE_RETURN = 20;

    public override void OnSetup() {
        hornet = entity.GetComponent<HornetBehaviour>();
    }

    public override void OnEntry() {
        if (Vector2.Distance(transform.position, home) >= DISTANCE_BEFORE_RETURN) {
            stateMachine.SetChildState(patrolReturn);
        }

        else stateMachine.SetChildState(patrolWander);
    }

    public override void OnChildExit(State exitingChild, bool success) {
        if (success == false) {
            CompleteState(false);
            return;
        }

        if (exitingChild == patrolWander) {
            stateMachine.SetChildState(patrolReturn);
        }

        // We must be at the nest
        // So randomly choose to either enter nest or continue patrolling
        else if (Utilities.RandBool()) {
            stateMachine.SetChildState(patrolWander);
        }

        else {
            Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(home);
            (Vector2Int pos, Constructable constructable) = TileManager.Instance.GetConstructableAt(home);
            HornetNest nest = constructable as HornetNest;
            if (data == null || nest == null) {
                CompleteState(false);
                return;
            }

            if (nest.TryAddToNest(pos, data, hornet) == false) CompleteState(false);
        }
    }
}
