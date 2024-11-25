using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerBehaviour : MonoBehaviour {

    [SerializeField]
    State Idle, House, Forage;
    Animator animator;

    StateMachine stateMachine;

    void Start() {
        stateMachine = new StateMachine();

        animator = GetComponent<Animator>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, animator, null);
        }
    }

    // Update is called once per frame
    void Update() {
        if (stateMachine.EmptyState()) DecideState();

        stateMachine.Run();
    }

    void FixedUpdate() {
        stateMachine.FixedRun();
    }

    void DecideState() {
        stateMachine.SetState(Idle);
    }
}
