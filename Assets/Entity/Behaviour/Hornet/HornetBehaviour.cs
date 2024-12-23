using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HornetBehaviour : MonoBehaviour, IEntity, ITargetable {

    public Vector2Int Home { get; private set; }

    StateMachine stateMachine;

    State state => stateMachine.childState;

    Renderer render;
    Animator animator;
    HealthComponent healthComponent;
    GravityComponent gravity;

    bool isDead;

    [SerializeField]
    State patrol, nest, sting, die;

    [SerializeField]
    HornetNest nestConst;

    const int MAX_TARGET_DISTANCE = 15, MAX_FOLLOW_DISTANCE = 35, TARGET_PULSE_RATE = 25, STING_COOL_OFF = 5;

    public ITargetable CurrentTarget { get; private set; }
    float beganStingCoolOff;
    int targetPulse;

    Material material;
    float beganHurtTime;
    bool hurt;
    const float MAX_HURT_TIME = 0.75f;
    const String RED_TINT = "_REDTINT_ON";


    public GameObject GetGameObject() {
        return gameObject;
    }

    public void Start() {
        stateMachine = new();

        animator = GetComponent<Animator>();
        render = GetComponent<Renderer>();
        healthComponent = GetComponent<HealthComponent>();
        gravity = GetComponent<GravityComponent>();
        material = GetComponent<Renderer>().material;

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>()?.Setup(gameObject, animator, stateMachine);
        }

        Home = new(-20, 1);
        TileManager.Instance.Construct(Home, nestConst);
    }

    public void Update() {
        HideHurt();
    }

    public void FixedUpdate() {
        stateMachine.FixedRun();

        if (isDead) return;

        if (healthComponent.IsDead) {
            OnDeath();
            return;
        }

        if (stateMachine.EmptyState()) DecideState();

        UpdateTargets();
    }

    void DecideState() {
        if (ReadyToSting()) {
            stateMachine.SetChildState(sting);
            return;
        }

        stateMachine.SetChildState(patrol);
    }

    void OnDeath() {
        stateMachine.SetChildState(die);
        isDead = true;

        gravity.Enable();
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

    public int Friendliness() {
        return -1;
    }

    public void Damage(uint amount, ITargetable attacker = null) {
        healthComponent.Damage(amount);
        ShowHurt();
    }
    
    void ShowHurt() {
        // Visual hurt indication
        hurt = true;
        beganHurtTime = Time.time;
        material.EnableKeyword(RED_TINT);
    }

    void HideHurt() {
        if (hurt == false || beganHurtTime + MAX_HURT_TIME > Time.time) return;

        material.DisableKeyword(RED_TINT);
        hurt = false;
    }

    public Vector2 GetPosition() {
        return transform.position;
    }

    public bool IsDead() {
        return healthComponent.IsDead;
    }
}
