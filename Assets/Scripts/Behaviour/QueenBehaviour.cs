using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBehaviour : MonoBehaviour, TaskAgent {
    
    [SerializeField]
    State Idle, Lay;
    Animator animator;
    QueenTask task;
    StateMachine stateMachine;
    State currentState => stateMachine.childState;

    void Start() {
        stateMachine = new StateMachine();

        animator = GetComponent<Animator>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, this, animator, stateMachine);
        }

        TaskManager.Instance.RegisterAgent(this);
    }

    public bool OfferTask(Task task) {
        if (task is QueenTask queenTask) {
            this.task = queenTask;
            return true;
        } else return false;
    }

    public void SetTask(Task task) {
        if (task == null) {
            this.task = null;
            return;
        }

        if (task is QueenTask queenTask) {
            this.task = queenTask;
        }
    }

    public Task GetTask() {
        return task;
    }

    void Update() {
        if (stateMachine.EmptyState()) DecideState();

        stateMachine.Run();
    }

    void FixedUpdate() {
        stateMachine.FixedRun();
    }

    void DecideState() {
        if (task == null) {
            stateMachine.SetChildState(Idle);
            return;
        }

        if (task is LayTask layTask) {
            stateMachine.SetChildState(Lay);
            return;
        }

        stateMachine.SetChildState(Idle);
    }
}
