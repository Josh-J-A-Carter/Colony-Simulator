using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HornetBehaviour : MonoBehaviour {

    public Vector2Int Home { get; private set; }

    StateMachine stateMachine;

    State state => stateMachine.childState;

    Renderer render;

    Animator animator;

    [SerializeField]
    State patrol, nest, sting;

    [SerializeField]
    HornetNest nestConst;

    const int MAX_TARGET_DISTANCE = 15, MAX_FOLLOW_DISTANCE = 35, TARGET_PULSE_RATE = 25, STING_COOL_OFF = 5;

    public ITargetable CurrentTarget { get; private set; }
    float beganStingCoolOff;
    int targetPulse;


    public void Start() {
        stateMachine = new();

        animator = GetComponent<Animator>();
        render = GetComponent<Renderer>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>()?.Setup(gameObject, animator, stateMachine);
        }

        Home = new(-20, 1);
        TileManager.Instance.Construct(Home, nestConst);
    }

    public void FixedUpdate() {
        if (state == null) DecideState();

        stateMachine.FixedRun();

        UpdateTargets();
    }

    void DecideState() {
        if (ReadyToSting()) {
            stateMachine.SetChildState(sting);
            return;
        }

        stateMachine.SetChildState(patrol);
    }

    public void OnNestEntry() {
        stateMachine.SetChildState(nest);

        render.enabled = false;
    }

    public void OnNestExit() {
        stateMachine.ResetChildState();

        render.enabled = true;
    }

    void UpdateTargets() {
        targetPulse += 1;

        if (targetPulse < TARGET_PULSE_RATE) return;
        targetPulse = 0;

        // Target still exists & is reachable?
        bool needNewTarget = false;

        if (CurrentTarget == null) needNewTarget = true;

        else if (Vector2.Distance(transform.position, CurrentTarget.GetPosition()) > MAX_FOLLOW_DISTANCE) needNewTarget = true;

        else {
            Path p = Pathfind.FindPath(transform.position, CurrentTarget.GetPosition());
            needNewTarget = p == null;
        }

        // Yes, quit now
        if (needNewTarget == false) return;

        // No, try find another target
        CurrentTarget = null;

        List<ITargetable> potentialTargets = EntityManager.Instance.QueryEntities<ITargetable>(t => {
            if (t.Friendliness() <= 0) return false;
            if (t.IsDead()) return false;
            if (Vector2.Distance(transform.position, t.GetPosition()) > MAX_TARGET_DISTANCE) return false;

            Path p = Pathfind.FindPath(transform.position, t.GetPosition());
            return p != null;
        });

        if (potentialTargets == null || potentialTargets.Count == 0) return;

        potentialTargets.OrderBy(t => Vector2.Distance(transform.position, t.GetPosition()));

        CurrentTarget = potentialTargets[0];
    }

    public bool ReadyToSting() {
        return CurrentTarget != null && beganStingCoolOff + STING_COOL_OFF < Time.time;
    }

    public void InitiateStingCoolOff() {
        beganStingCoolOff = Time.time;
        CurrentTarget = null;
    }
}
