using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBehaviour : MonoBehaviour, IInformative, IEntity {
    
    [SerializeField]
    State idle;
    protected Animator animator;
    protected StateMachine stateMachine;
    // State currentState => stateMachine.childState;

    String nameInfo;

    public GameObject GetGameObject() {
        return gameObject;
    }

    public void Start() {
        stateMachine = new StateMachine();

        animator = GetComponent<Animator>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>().Setup(gameObject, animator, stateMachine);
        }
    }

    public void Update() {
        if (stateMachine.EmptyState()) DecideState();

        stateMachine.Run();
    }

    public void FixedUpdate() {
        stateMachine.FixedRun();
    }

    void DecideState() {
        stateMachine.SetChildState(idle);
    }

    public string GetName() {
        return nameInfo;
    }

    public void SetName(String name) {
        nameInfo = name;
    }

    public string GetDescription() {
        throw new System.NotImplementedException();
    }

    public InfoBranch GetInfoTree(object obj = null) {
        InfoBranch root = new InfoBranch(String.Empty);

        // Generic
        InfoBranch genericCategory = new InfoBranch("Generic Properties");
        root.AddChild(genericCategory);

        InfoLeaf typeProperty = new InfoLeaf("Type", "Drone Honey Bee (Entity)");
        genericCategory.AddChild(typeProperty);

        InfoLeaf nameProperty = new InfoLeaf("Name", nameInfo);
        genericCategory.AddChild(nameProperty);

        
        // Task


        return root;
    }
}
