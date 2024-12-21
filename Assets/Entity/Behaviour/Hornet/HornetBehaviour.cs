using UnityEngine;

public class HornetBehaviour : MonoBehaviour {

    public Vector2Int Home { get; private set; }

    StateMachine stateMachine;

    State state => stateMachine.childState;

    Renderer render;

    Animator animator;

    [SerializeField]
    State patrol, nest;

    [SerializeField]
    HornetNest nestConst;


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

    public void Update() {
        if (state == nest) return;

        if (state == null) stateMachine.SetChildState(patrol);
    }

    public void FixedUpdate() {
        stateMachine.FixedRun();
    }

    public void OnNestEntry() {
        stateMachine.SetChildState(nest);

        render.enabled = false;
    }

    public void OnNestExit() {
        stateMachine.ResetChildState();

        render.enabled = true;
    }
}
