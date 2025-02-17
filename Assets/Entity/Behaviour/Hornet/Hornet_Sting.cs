using UnityEngine;
using Random = UnityEngine.Random;

public class Hornet_Sting : State {

    [SerializeField]
    AnimationClip anim;

    HornetBehaviour hornet;

    ITargetable target => hornet.CurrentTarget;
    const float DISTANCE_EPSILON = 0.75f;
    const int DMG_AMOUNT = 8, DMG_RAND_MIN = 0, DMG_RAND_MAX = 3;

    Path path;
    int stepSpeed = 10;

    public override void OnSetup() {
        hornet = entity.GetComponent<HornetBehaviour>();
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        CalculatePath();
    }

    public override void FixedRun() {
        if (target == null || hornet.ReadyToSting() == false) {
            CompleteState();
            return;
        }

        bool success = path.Increment();

        bool withinDistance = Vector2.Distance(entity.transform.position, target.GetPosition()) < DISTANCE_EPSILON;

        if (withinDistance) {
            target.Damage((uint) (DMG_AMOUNT + Random.Range(DMG_RAND_MIN, DMG_RAND_MAX)), hornet);
            CompleteState();
            hornet.InitiateStingCoolOff();
            return;
        }

        if (path.IsComplete() && withinDistance) {
            CompleteState();
            return;
        }

        if (!success || path.IsComplete()) {
            CalculatePath();
        }
    }

    void CalculatePath() {
        Vector2 startPos = entity.transform.position;
        path = Pathfind.FindPath(startPos, target.GetPosition());
    
        if (path == null) {
            CompleteState(false);
            return;
        }

        path.Initialise(entity, stepSpeed);
    }
}
