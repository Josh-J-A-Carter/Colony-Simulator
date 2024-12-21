using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Nest : State {

    HornetBehaviour hornet;

    Vector2Int home => hornet.Home;

    const int MIN_WAIT_TIME = 10, MAX_WAIT_TIME = 20;

    int chosenTime;

    public override void OnEntry() {
        chosenTime = Random.Range(MIN_WAIT_TIME, MAX_WAIT_TIME);

        hornet = entity.GetComponent<HornetBehaviour>();
    }

    public override void FixedRun() {
        if (parent.activeFor >= chosenTime) {
            Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(home);
            (Vector2Int pos, Constructable constructable) = TileManager.Instance.GetConstructableAt(home);
            HornetNest nest = constructable as HornetNest;

            nest.TryRemoveFromNest(pos, data, hornet);
            CompleteState();
        }
    }

}
