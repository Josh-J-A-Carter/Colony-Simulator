using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Patrol : State {

    HornetBehaviour wasp;

    Vector2Int home => wasp.Home;

    Path path;


    const int DISTANCE_BEFORE_RETURN = 40;

    const int MIN_DISTANCE_X = 50, MAX_DISTANCE_X = 60;
    const int MIN_DISTANCE_Y = 2, MAX_DIStANCE_Y = 7;

    const int MAX_PATHFIND_ATTEMPTS = 10;

    static readonly int stepSpeed = 18;

    int stepsMax, step;


    public override void OnSetup() {
        wasp = entity.GetComponent<HornetBehaviour>();
    }

    public override void OnEntry() {
        if (Vector2.Distance(transform.position, home) >= DISTANCE_BEFORE_RETURN) {
            path = Pathfind.FindPath(transform.position, home);
            
            if (path != null) {
                step = 0;
                stepsMax = stepSpeed * path.Count;
                return;
            }
        }

        for (int attempt = 0 ; attempt < MAX_PATHFIND_ATTEMPTS ; attempt += 1) {
            int signX = (int) Math.Pow(-1, Random.Range(0, 2));
            int signY = (int) Math.Pow(-1, Random.Range(0, 2));
            Vector2Int dst = new(signX * Random.Range(MIN_DISTANCE_X, MAX_DISTANCE_X + 1), signY * Random.Range(MIN_DISTANCE_Y, MAX_DIStANCE_Y + 1));

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
