using UnityEngine;
using Random = UnityEngine.Random;

public class Worker_Sting : State {

    [SerializeField]
    AnimationClip anim;

    WorkerBehaviour worker;
    ITargetable target => (worker.GetTask() as AttackTask)?.GetTarget();
    const float DISTANCE_EPSILON = 0.75f;
    const int DMG_AMOUNT = 7, DMG_RAND_MIN = -1, DMG_RAND_MAX = 2;

    Path path;
    int stepSpeed = 10;

    public override void OnSetup() {
        worker = entity.GetComponent<WorkerBehaviour>();
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
            worker.InitiateStingCoolOff();
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
