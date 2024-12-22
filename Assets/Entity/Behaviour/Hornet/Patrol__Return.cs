using UnityEngine;

public class Patrol__Return : State {

    HornetBehaviour hornet;

    Vector2Int home => hornet.Home;

    Path path;

    static readonly int stepSpeed = 13;


    public override void OnSetup() {
        hornet = entity.GetComponent<HornetBehaviour>();
    }

    public override void OnEntry() {
        path = Pathfind.FindPath(transform.position, home);
        
        if (path != null) {
            path.Initialise(entity, stepSpeed);
            return;
        }

        CompleteState(false);
    }

    public override void FixedRun() {
        bool success = path.Increment();

        if (path.IsComplete()) {
            CompleteState();
            return;
        }

        if (success == false) CompleteState(false);
    }

}
