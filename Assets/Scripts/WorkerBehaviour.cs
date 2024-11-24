using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerBehaviour : MonoBehaviour {

    [SerializeField]
    State Idle, House, Forage;

    [SerializeField]
    Animator animator;

    StateMachine stateMachine;

    void Start() {
        stateMachine = new StateMachine();

        State[] states = GetComponentsInChildren<State>();
        foreach (State state in states) {
            state.Setup(gameObject, animator, null);
        }
    }

    // Update is called once per frame
    void Update() {
        if (stateMachine.currentState == null) DecideState();
    }

    void DecideState() {
        stateMachine.SetState(Idle);
    }
}
