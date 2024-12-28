using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Idle__Pathfind : State {

    [SerializeField]
    /// Maximum/minimum distances to meander, from the current position
    const int MIN_RANGE_X = 3, MAX_RANGE_X = 8, MIN_RANGE_Y = 0, MAX_RANGE_Y = 3;

    [SerializeField]
    AnimationClip anim;

    const int TARGET_ATTEMPTS = 20;

    Path path;

    static readonly int stepSpeed = 15;

    public override void OnEntry() {
        animator.Play(anim.name);

        ChooseTarget();
    }

    public override void FixedRun() {
        bool success = path.Increment();

        if (path.IsComplete()) {
            CompleteState();
            return;
        }

        if (success == false) CompleteState(false);
    }

    /// <summary>
    /// Choose a random destination (within a given radius) & calculate the path to it
    /// </summary>
    void ChooseTarget() {

        int currentX = (int) Math.Floor(entity.transform.position.x);
        int currentY = (int) Math.Floor(entity.transform.position.y);

        Vector2Int current = new Vector2Int(currentX, currentY);

        for (int attempt = 0 ; attempt < TARGET_ATTEMPTS ; attempt += 1) {
            int displacementX = Utilities.RandSign() * Random.Range(MIN_RANGE_X, MAX_RANGE_X);
            int targetX = currentX + displacementX;

            int displacementY = Utilities.RandSign() * Random.Range(MIN_RANGE_Y, MAX_RANGE_Y);
            int targetY = currentY + displacementY;

            Vector2Int target = new Vector2Int(targetX, targetY);

            Path path = Pathfind.FindPath(current, target, oneTagFrom: new[]{ ConstructableTag.BeeTraversable });
            
            if (path != null) {
                this.path = path;
                path.Initialise(entity, stepSpeed);
                return;
            }
        }

        CompleteState();
    }

}
