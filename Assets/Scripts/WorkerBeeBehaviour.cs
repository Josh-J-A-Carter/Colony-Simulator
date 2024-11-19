using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorkerBeeBehaviour : MonoBehaviour
{

    [SerializeField]
    Animator animator;

    [SerializeField]
    PathfindingGraph graph;

    enum State {
        Idle,
        Fly
    };

    List<Vector2Int> flyPath;
    Vector2Int flyGoal;

    State currentState = State.Idle;

    int timer = timerMax;
    static readonly int timerMax = 250;

    
    void Start() {
        Random.InitState(DateTime.Now.Millisecond);
    }

    void FixedUpdate() {

        if (currentState == State.Idle) {
            timer += 1;

            if (timer >= timerMax) {
                timer = 0;

                // ChangeState(State.Fly);
                if (flyPath != null) graph.DevisualisePath(flyPath);

                bool foundGoal = false;
                while (!foundGoal) {
                    int x = Random.Range(graph.minX, graph.maxX);
                    int y = Random.Range(graph.minY + 1, graph.maxY);

                    if (graph.IsUnobstructed(x, y)) {
                        flyPath = graph.FindPath(transform.position, new Vector2Int(x, y));
                        graph.VisualisePath(flyPath);
                        break;
                    }
                }
            }
        }
    }

    void ChangeState(State newState) {
        // Don't want to restart animations
        if (currentState == newState) return;

        // Want the benefits of type safety - so using an enum, not a static class.
        // But this may have a performance impact since it uses reflection...
        String newStateName = Enum.GetName(typeof(State), newState);

        animator.Play(newStateName);
        currentState = newState;
    }

}
