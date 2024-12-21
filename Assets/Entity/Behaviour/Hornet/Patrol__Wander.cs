using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Patrol__Wander : State {

    HornetBehaviour hornet;

    Vector2Int home => hornet.Home;

    Path path;

    const int MIN_DISTANCE_X = 25, MAX_DISTANCE_X = 40;
    const int MIN_DISTANCE_Y = 2, MAX_DIStANCE_Y = 7;

    const int MAX_PATHFIND_ATTEMPTS = 10;

    static readonly int stepSpeed = 13;

    int stepsMax, step;


    public override void OnSetup() {
        hornet = entity.GetComponent<HornetBehaviour>();
    }

    public override void OnEntry() {
        for (int attempt = 0 ; attempt < MAX_PATHFIND_ATTEMPTS ; attempt += 1) {
            Vector2Int dst = new(Utilities.RandSign() * Random.Range(MIN_DISTANCE_X, MAX_DISTANCE_X + 1),
                                 Utilities.RandSign() * Random.Range(MIN_DISTANCE_Y, MAX_DIStANCE_Y + 1));

            path = Pathfind.FindPath(transform.position, dst);
            if (path != null) {
                step = 0;
                stepsMax = stepSpeed * path.Count;
                return;
            }
        }

        CompleteState(false);
    }

    public override void FixedRun() {
        bool success = Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        step += 1;

        if (step >= stepsMax) {
            CompleteState();
            return;
        }

        if (success == false) CompleteState(false);
    }

}
