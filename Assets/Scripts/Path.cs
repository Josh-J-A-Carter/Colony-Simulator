using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Path {

    TileManager tm => TileManager.Instance;

    List<Vector2Int> points;

    public int Count => points.Count;

    static readonly float DIAGONAL_DISTANCE = 1.414f;
    static readonly float CARDINAL_DISTANCE = 1.0f;

    float[] linearSegments;
    float linearMax;

    public Path(List<Vector2Int> points) {
        this.points = points;

        CalculateLinearSegments();
    }

    public override string ToString(){
        return points.Aggregate<Vector2Int, String>("", (acc, p) => acc + " | " + p);
    }

    /// <summary>
    /// Is this path still valid / unobstructed, starting from a given point?
    /// 
    /// Note - the point <c>start</c> must part of the path.
    /// </summary>
    public bool IsValidFrom(Vector2Int start) {
        // Loop through the path to see if any of it has become obstructed
        // BUT we can ignore any points that have already been travelled
        int index = points.FindIndex(p => p == start);

        if (index == -1) return false;

        for (int i = index ; i < points.Count ; i += 1) if (!tm.IsUnobstructed(points[i])) return false;

        return true;
    }

    /// <summary>
    /// Is this path still valid / unobstructed, in its entirety?
    /// </summary>
    public bool IsValid() {
        // Loop through the path to see if any of it has become obstructed
        foreach (Vector2Int point in points) if (!tm.IsUnobstructed(point)) return false;

        return true;
    }

    /// <summary>
    /// Calculate the linear segments in the path.
    /// 
    /// This takes into account the fact that diagonal tiles are further away than cardinal neighbour tiles, i.e.
    /// distance from (x, y) to (x + 1, y) is 1.0, but distance from (x, y) to (x + 1, y + 1) is sqrt(2).
    /// </summary>
    void CalculateLinearSegments() {
        linearSegments = new float[this.points.Count];

        float cumulativeDistance = 0;

        for (int i = 0 ; i < linearSegments.Length ; i += 1) {
            linearSegments[i] = cumulativeDistance;

            if (i >= points.Count - 1) continue;

            cumulativeDistance += Distance(points[i], points[i + 1]);
        }

        linearMax = linearSegments.Last();
    }

    float Distance(Vector2Int p1, Vector2Int p2) {
        if (p1 == p2) return 0;

        if (p1.x + 1 == p2.x && p1.y == p2.y) return CARDINAL_DISTANCE;
        if (p1.x - 1 == p2.x && p1.y == p2.y) return CARDINAL_DISTANCE;
        if (p1.x == p2.x && p1.y + 1 == p2.y) return CARDINAL_DISTANCE;
        if (p1.x == p2.x && p1.y - 1 == p2.y) return CARDINAL_DISTANCE;

        return DIAGONAL_DISTANCE;
    }

    /// <summary>
    /// Linearly interpolate along the path, from <c>0</c> to <c>stepsTotal</c>,
    /// returning the position in the path that should be taken after <c>step</c> steps.
    /// </summary>
    public Vector2 LinearlyInterpolate(int step, int stepsTotal) {

        if (step > stepsTotal) throw new System.Exception("step > stepsTotal; has a timer for path interpolation gone too far?");

        // Map 'step' from [0, stepsTotal] to [0, linearMax]
        float normalisedStep = linearMax * step / stepsTotal;

        // Find the indices i, j of points in the path, such that linearSegments[i] < normalisedStep < linearSegments[j].
        int index = 0;
        for (int i = 0 ; i < linearSegments.Length - 1; i += 1) {
            if (linearSegments[i] <= normalisedStep && normalisedStep <= linearSegments[i + 1]) break;

            index += 1;
        }

        // So now we know between which two points in the path to linearly interpolate
        // Thus, calculate how far through this sub-path we are (i.e. the path between points[index] and points[index + 1])
        float segmentProgress = normalisedStep - linearSegments[index];
        float segmentMax = linearSegments[index + 1] - linearSegments[index];

        float t = segmentProgress / segmentMax; // t is in [0, 1]

        // All that remains is to linearly interpolate
        Vector2 translation = new Vector2(0.5f, 0.5f);
        Vector2 segmentStart = points.ElementAt(index) + translation;
        Vector2 segmentEnd = points.ElementAt(index + 1) + translation;

        Vector2 interpolatedPosition = segmentStart + t * segmentEnd;

        return interpolatedPosition;
    }
}
