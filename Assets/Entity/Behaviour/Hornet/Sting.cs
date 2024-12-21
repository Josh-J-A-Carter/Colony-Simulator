using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sting : State {

    [SerializeField]
    AnimationClip anim;

    HornetBehaviour hornet;

    ITargetable target => hornet.CurrentTarget;
    const float DISTANCE_EPSILON = 0.5f;
    const int DMG_AMOUNT = 10;

    Path path;
    int pathPulse, step, stepsMax, stepSpeed = 10;
    const int PATH_PULSE_RATE = 25;

    public override void OnSetup() {
        hornet = entity.GetComponent<HornetBehaviour>();
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        CalculatePath();
    }

    public override void FixedRun() {
        if (target == null) {
            CompleteState(false);
            return;
        }

        Pathfind.MoveAlongPath(entity, path, step, stepsMax);

        if (Vector2.Distance(entity.transform.position, target.GetPosition()) < DISTANCE_EPSILON) {
            target.Damage(DMG_AMOUNT);
            CompleteState();
            return;
        }

        step += 1;

        pathPulse += 1;

        if (pathPulse >= PATH_PULSE_RATE) {
            pathPulse = 0;

            CalculatePath();
        }
    }

    void CalculatePath() {
        path = Pathfind.FindPath(entity.transform.position, target.GetPosition());
    
        if (path == null) {
            CompleteState(false);
            return;
        }

        step = 0;
        stepsMax = path.Count * stepSpeed;
    }
}
