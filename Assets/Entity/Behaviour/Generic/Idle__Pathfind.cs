using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Idle__Pathfind : State {

    [SerializeField]
    /// Maximum/minimum distances to meander, from the current position
    static int minRange = 3, maxRange = 8;

    [SerializeField]
    AnimationClip anim;

    static readonly int maxTargetAttempts = 20;

    Path path;

    static readonly int stepSpeed = 15;

    int stepsMax;

    int step;

    public override void OnEntry() {
        animator.Play(anim.name);

        ChooseTarget();
    }

    public override void OnExit() {}

    public override void FixedRun() {
        bool success = Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        step += 1;

        if (step > stepsMax) {
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

        bool targetFound = false;
        for (int attempt = 0 ; attempt < maxTargetAttempts ; attempt += 1) {
            int signX = (int) Math.Pow(-1, Random.Range(0, 2));
            int displacementX = signX * Random.Range(minRange, maxRange);
            int targetX = currentX + displacementX;

            int signY = (int) Math.Pow(-1, Random.Range(0, 2));
            int displacementY = signY * Random.Range(minRange, maxRange);
            int targetY = currentY + displacementY;

            Vector2Int target = new Vector2Int(targetX, targetY);

            Path path = Pathfind.FindPath(current, target);
            
            if (path != null) {
                targetFound = true;
                this.path = path;
                break;
            }
        }

        if (!targetFound) {
            CompleteState();
            return;
        }

        step = 0;
        stepsMax = path.Count * stepSpeed;
    }

}
