using UnityEngine;
using Random = UnityEngine.Random;

public class Queen_Sting : State {

    [SerializeField]
    AnimationClip anim;

    QueenBehaviour queen;
    ITargetable target => (queen.GetTask() as AttackTask)?.GetTarget();
    const float DISTANCE_EPSILON = 0.75f;
    const int DMG_AMOUNT = 10, DMG_RAND_MIN = 0, DMG_RAND_MAX = 3;

    Path path;
    int stepSpeed = 10;

    public override void OnSetup() {
        queen = entity.GetComponent<QueenBehaviour>();
    }

    public override void OnEntry() {
        animator.Play(anim.name);

        CalculatePath();
    }

    public override void FixedRun() {
        if (target == null || target.CanTarget() == false) {
            CompleteState();
            return;
        }

        bool success = path.Increment();

        bool withinDistance = Vector2.Distance(entity.transform.position, target.GetPosition()) < DISTANCE_EPSILON;

        if (withinDistance) {
            target.Damage((uint) (DMG_AMOUNT + Random.Range(DMG_RAND_MIN, DMG_RAND_MAX)));
            CompleteState();
            queen.InitiateStingCoolOff();
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
        path = Pathfind.FindPath(startPos, target.GetPosition(), oneTagFrom: new[]{ ConstructableTag.BeeTraversable });
    
        if (path == null) {
            CompleteState(false);
            return;
        }

        path.Initialise(entity, stepSpeed);
    }
}
