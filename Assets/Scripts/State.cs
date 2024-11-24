using UnityEngine;

public abstract class State : MonoBehaviour {

    public State parent;
    public StateMachine stateMachine;

    public Animator animator;
    public GameObject entity;

    public void Setup(GameObject entity, Animator animator, State parent) {
        stateMachine = new StateMachine();

        this.entity = entity;
        this.animator = animator;
        this.parent = parent;

        // Set up children recursively (if they exist)
        State[] states = GetComponentsInChildren<State>();
        foreach (State state in states) {
            state.Setup(gameObject, animator, this);
        }
    }

    public virtual void OnEnter() {}


    public void OnExitRecursive() {
        OnExit();

        this.stateMachine.ResetState();
    }

    public virtual void OnExit() {}

    public virtual void OnChildExit(State exitingChild) {}


    public void CompleteState() {
        this.parent.stateMachine.ResetState();
    }

    public void RunRecursive() {
        this.Run();
        stateMachine?.Run();
    }

    public virtual void Run() {}

    public void FixedRunRecursive() {
        this.FixedRun();
        stateMachine?.FixedRun();
    }

    public virtual void FixedRun() {}
}
