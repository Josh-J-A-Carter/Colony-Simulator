using System;
using System.Linq;
using UnityEngine;

public abstract class State : MonoBehaviour {

    protected StateMachine parent;
    public StateMachine stateMachine;
    protected State child => stateMachine.childState;

    protected Animator animator;
    protected GameObject entity;

    public void Setup(GameObject entity, Animator animator, StateMachine parent) {
        stateMachine = new StateMachine(this);

        this.entity = entity;
        this.animator = animator;
        this.parent = parent;

        // Recursively set up child states, if present
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(entity, animator, stateMachine);
        }

        OnSetup();
    }

    public virtual void OnSetup() {}

    public virtual void OnEntry() {}


    public void OnExitRecursive() {
        OnExit();

        stateMachine.ResetChildState();
    }

    public virtual void OnExit() {}

    public virtual void OnChildExit(State exitingChild, bool success = true) {}


    public void CompleteState(bool success = true) {
        parent.ResetChildState(success: success);

        parent.state?.OnChildExit(this, success);
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
