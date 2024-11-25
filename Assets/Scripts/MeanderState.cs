using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeanderState : State {

    [SerializeField]
    /// Maximum/minimum distances to meander, from the current position
    static int minRange = 3, maxRange = 8;

    [SerializeField]
    AnimationClip animation;

    Path path;

    static readonly int stepSpeed = 25;

    int stepsMax;

    int step;

    public override void OnEnter() {
        this.animator.Play(animation.name);

        ChooseTarget();
    }

    public override void OnExit() {}

    public override void FixedRun() {
        int currentX = (int) Math.Floor(entity.transform.position.x);
        int currentY = (int) Math.Floor(entity.transform.position.y);

        Vector2Int current = new Vector2Int(currentX, currentY);

        if (path.IsValidFrom(current)) {
            
            // Linearly interpolate to next point
            Vector2 nextPoint = path.LinearlyInterpolate(step, stepsMax);
            entity.transform.position = nextPoint;


            /// Remember to flip the character's sprite as needed
            /// 
            /// 
            /// 
            /// 
            /// 

            step += 1;

            // Once we reach the end of the path, the state is completed
            if (step > stepsMax) CompleteState();

        } else ChooseTarget();
    }

    /// <summary>
    /// Choose a random destination (within a given radius) & calculate the path to it
    /// </summary>
    void ChooseTarget() {
        int currentX = (int) Math.Floor(entity.transform.position.x);
        int currentY = (int) Math.Floor(entity.transform.position.y);

        Vector2Int current = new Vector2Int(currentX, currentY);

        bool targetFound = false;
        while (!targetFound) {
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
            }
        }

        step = 0;
        stepsMax = path.Count * stepSpeed;
    }

}
