using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerBehaviour : MonoBehaviour, TaskAgent {

    [SerializeField]
    State Idle, Hive, Forage;
    Animator animator;

    WorkerTask task;

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
        if (task is WorkerTask workerTask) {
            this.task = workerTask;
            return true;
        } else return false;
    }

    public void SetTask(Task task) {
        if (task == null) {
            this.task = null;
            return;
        }

        if (task is WorkerTask workerTask) {
            this.task = workerTask;
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

        switch (task.category) {
            case WorkerTaskType.Hive: {
                stateMachine.SetChildState(Hive);
                return;
            }
        }

        stateMachine.SetChildState(Idle);
    }
}
