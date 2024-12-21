using UnityEngine;

public class Patrol__Return : State {

    HornetBehaviour wasp;

    Vector2Int home => wasp.Home;

    Path path;

    static readonly int stepSpeed = 13;

    int stepsMax, step;


    public override void OnSetup() {
        wasp = entity.GetComponent<HornetBehaviour>();
    }

    public override void OnEntry() {
        path = Pathfind.FindPath(transform.position, home);
        
        if (path != null) {
            step = 0;
            stepsMax = stepSpeed * path.Count;
            return;
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
