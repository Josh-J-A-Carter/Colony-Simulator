using UnityEngine;

public class HornetBehaviour : MonoBehaviour {

    public Vector2Int Home { get; private set; }

    StateMachine stateMachine;

    Animator animator;

    [SerializeField]
    State patrol;

    public void Start() {
        stateMachine = new();

        animator = GetComponent<Animator>();

        // Recursively set up the states
        foreach (Transform child in gameObject.transform) {
            child.GetComponent<State>()?.Setup(gameObject, animator, stateMachine);
        }

        Home = new(-20, 4);
    }

    public void Update() {
        if (stateMachine.EmptyState()) stateMachine.SetChildState(patrol);
    }

    public void FixedUpdate() {
        stateMachine.FixedRun();
    }
}
