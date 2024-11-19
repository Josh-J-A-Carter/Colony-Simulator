using System;
using System.Collections.Generic;
using System.Linq;
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
    // Vector2Int flyGoal;
    float flyTime;
    float flyTimeMin = 0;
    float flyTimeMax;

    float totalFlyTime;

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

                bool foundGoal = false;
                while (!foundGoal) {
                    int x = Random.Range(graph.minX, graph.maxX);
                    int y = Random.Range(graph.minY + 1, graph.maxY);

                    if (graph.IsUnobstructed(x, y)) {
                        flyPath = graph.FindPath(transform.position, new Vector2Int(x, y));
                        graph.VisualisePath(flyPath);
                        
                        ChangeState(State.Fly);
                        flyTime = flyTimeMin;
                        flyTimeMax = flyPath.Count - 1; // -1 because flyPath includes the starting position
                        totalFlyTime = flyTimeMax * 10; // the number of FixedUpdate calls it should take to complete the path

                        break;
                    }
                }
            }
        }

        else if (currentState == State.Fly) {
            timer += 1;
            
            flyTime += flyTimeMax / totalFlyTime;
            // After timerMax iterations, we will have flyTime = flyTimeMax

            if (flyTime >= flyTimeMax) {
                timer = 0;
                ChangeState(State.Idle);
                graph.DevisualisePath(flyPath);
            }

            else {
                // Check point tile, and next tile in the path
                Vector2 translation = new Vector2(0.5f, 0.5f);
                Vector2 incrementalStart = flyPath.ElementAt((int) flyTime) + translation;
                Vector2 incrementalEnd = flyPath.ElementAt(1 + (int) flyTime) + translation;

                // Move towards the next tile
                float incrementalFlyTime = (int) flyTime - flyTime;
                Vector2 desiredPosition = incrementalStart + incrementalFlyTime * (incrementalStart - incrementalEnd);
                Vector2 delta = desiredPosition - (Vector2) transform.position;
                transform.Translate(delta);
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
