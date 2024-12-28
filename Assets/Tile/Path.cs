using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Path {

    TileManager tm => TileManager.Instance;

    List<Vector2Int> points;
    ConstructableTag[] oneTagFrom;

    public int Count => points.Count;

    const float DIAGONAL_DISTANCE = 1.414f;
    const float CARDINAL_DISTANCE = 1.0f;
    static Vector2 TRANSLATION = new Vector2(0.5f, 0.5f);

    float[] linearSegments;
    float linearMax;

    // State for path traversal
    GameObject entity;
    int step, stepsMax;
    int initialStep, initialStepsMax;
    Vector2 initialDelta;


    public Path(List<Vector2Int> points, ConstructableTag[] oneTagFrom) {
        this.points = points;
        this.oneTagFrom = oneTagFrom;

        CalculateLinearSegments();
    }

    public Vector2Int NextGridPos() {
        // Map 'step' from [0, stepsTotal] to [0, linearMax]
        float normalisedStep = linearMax * step / stepsMax;

        // Find the indices i, j of points in the path, such that linearSegments[i] < normalisedStep < linearSegments[j].
        int index = 0;
        for (int i = 0 ; i < linearSegments.Length - 1; i += 1) {
            if (linearSegments[i] <= normalisedStep && normalisedStep <= linearSegments[i + 1]) break;

            index += 1;
        }

        if (index >= linearSegments.Length - 1) return points.Last();

        return points[index];
    }

    public void Initialise(GameObject entity, int slowness) {
        this.entity = entity;

        step = 0;
        stepsMax = slowness * Count;

        // We need to deal with the initial displacement of entity.transform.position from points[0]; it could be up to 0.5f!
        Vector2 initialPosition = entity.transform.position;
        
        initialStep = 0;
        float initialDistance = Vector2.Distance(entity.transform.position, points[0] + TRANSLATION);
        initialStepsMax = 1 + (int) (initialDistance * slowness);

        initialDelta = (points[0] + TRANSLATION - initialPosition) / initialStepsMax;

        int sign = Math.Sign(initialDelta.x);
        if (sign != 0) entity.transform.localScale = new Vector3(sign, 1, 1);
    }

    /// <summary>
    /// Move <c>entity</c> along <c>path</c> using linear interpolation.
    /// </summary>
    /// <returns><c>true</c> if the path is still valid, false otherwise.</returns>
    public bool Increment() {
        if (step >= stepsMax) return false;

        int currentX = (int) Math.Floor(entity.transform.position.x);
        int currentY = (int) Math.Floor(entity.transform.position.y);

        Vector2Int current = new(currentX, currentY);

        if (IsValidFrom(current)) {
            // Initial interpolation; Path uses a grid but the entity may not be perfectly aligned at the start
            if (initialStep < initialStepsMax) {
                entity.transform.Translate(initialDelta);
                initialStep += 1;
            }

            else {
                // Linearly interpolate to next point
                Vector2 nextPoint = LinearlyInterpolate(step, stepsMax);
                Vector2 translation = nextPoint - (Vector2) entity.transform.position;
                entity.transform.Translate(translation);

                /// Remember to flip the character's sprite as needed
                int sign = Math.Sign(translation.x);
                if (sign != 0) entity.transform.localScale = new Vector3(sign, 1, 1);

                step += 1;
            }

            return true;
        }

        return false;
    }

    public bool IsComplete() {
        return step >= stepsMax;
    }

    public override string ToString(){
        String acc = $"[ {points[0]}";
        for (int i = 1 ; i < points.Count ; i += 1) acc = acc + points[i];
        return acc + "]";
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

        if (index == -1) {
            // The position may be slightly off due to rounding issues
            // so check if this is close to diagonals in the path
            // Obviously, this tile needs to be unobstructed too
            if (tm.IsObstructed(start, oneTagFrom)) return false;

            bool foundInDiagonal = false;
            for (int i = 0 ; i < points.Count - 1 ; i += 1) {
                if (Adjacent(points[i], points[i + 1])) continue;

                // So reaching this code, point[i + 1] is diagonal to points[i + 1]
                // Thus, if start is adjacent to these points, it's *basically* on the path
                // and we don't need to say the path is invalid
                if (Adjacent(points[i], start) && Adjacent(points[i + 1], start)) {
                    foundInDiagonal = true;
                    index = i;              // need to validate the rest of the path from here!
                    break;
                }
            }

            if (!foundInDiagonal) return false;
        }

        for (int i = index ; i < points.Count ; i += 1) {

            // If this is the final point, there are no points after it,
            // so the path will be valid as long as this point is unobstructed
            if (i == points.Count - 1) {
                if (!tm.IsObstructed(points[i], oneTagFrom)) continue;
                return false;
            }

            // If this point and the next are cardinally adjacent, it is sufficient that
            // they both be unobstructed (only check points[i] since points[i + 1] will be checked next iteration)
            if (Adjacent(points[i], points[i + 1])) {
                if (!tm.IsObstructed(points[i], oneTagFrom)) continue;
                return false;
            }

            // If this point (p1) and the next (p2) are diagonally adjacent, we need there to be another point (p3) such that
            // p1 and p3 are cardinally adjacent, and p2 and p3 are cardinally adjacent, with p3 unobstructed
            // (basically, diagonals need to be able to be approximated with a corner of cardinals)
            if (Diagonal(points[i], points[i + 1])) {
                if (tm.IsObstructed(points[i], oneTagFrom)) return false;
                if (tm.IsObstructed(points[i + 1], oneTagFrom)) return false;

                // Here, we know points[i] and points[i + 1] are diagonal, and both unobstructed
                // Therefore, look for an unobstructed corner point
                Vector2Int p = new Vector2Int(points[i].x + 1, points[i].y);
                if (Adjacent(p, points[i]) && Adjacent(p, points[i + 1]) && !tm.IsObstructed(p, oneTagFrom)) continue;
                p = new Vector2Int(points[i].x - 1, points[i].y);
                if (Adjacent(p, points[i]) && Adjacent(p, points[i + 1]) && !tm.IsObstructed(p, oneTagFrom)) continue;
                p = new Vector2Int(points[i].x, points[i].y + 1);
                if (Adjacent(p, points[i]) && Adjacent(p, points[i + 1]) && !tm.IsObstructed(p, oneTagFrom)) continue;
                p = new Vector2Int(points[i].x, points[i].y - 1);
                if (Adjacent(p, points[i]) && Adjacent(p, points[i + 1]) && !tm.IsObstructed(p, oneTagFrom)) continue;

                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Calculate the linear segments in the path.
    /// 
    /// This takes into account the fact that diagonal tiles are further away than cardinal neighbour tiles, i.e.
    /// distance from (x, y) to (x + 1, y) is 1.0, but distance from (x, y) to (x + 1, y + 1) is sqrt(2).
    /// </summary>
    void CalculateLinearSegments() {
        linearSegments = new float[points.Count];

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

        if (Adjacent(p1, p2)) return CARDINAL_DISTANCE;

        return DIAGONAL_DISTANCE;
    }

    bool Adjacent(Vector2Int p1, Vector2Int p2) {
        if (p1.x + 1 == p2.x && p1.y == p2.y) return true;
        if (p1.x - 1 == p2.x && p1.y == p2.y) return true;
        if (p1.x == p2.x && p1.y + 1 == p2.y) return true;
        if (p1.x == p2.x && p1.y - 1 == p2.y) return true;

        return false;
    }

    bool Diagonal(Vector2Int p1, Vector2Int p2) {
        if (Adjacent(p1, p2)) return false;

        if (Math.Abs(p1.x - p2.x) > 1) return false;
        if (Math.Abs(p1.y - p2.y) > 1) return false;

        return true;
    }

    /// <summary>
    /// Linearly interpolate along the path, from <c>0</c> to <c>stepsTotal</c>,
    /// returning the position in the path that should be taken after <c>step</c> steps.
    /// </summary>
    Vector2 LinearlyInterpolate(int step, int stepsTotal) {
        
        // Map 'step' from [0, stepsTotal] to [0, linearMax]
        float normalisedStep = linearMax * step / stepsTotal;

        // Find the indices i, j of points in the path, such that linearSegments[i] < normalisedStep < linearSegments[j].
        int index = 0;
        for (int i = 0 ; i < linearSegments.Length - 1; i += 1) {
            if (linearSegments[i] <= normalisedStep && normalisedStep <= linearSegments[i + 1]) break;

            index += 1;
        }

        if (index >= linearSegments.Length - 1) return points.Last() + TRANSLATION;

        // So now we know between which two points in the path to linearly interpolate
        // Thus, calculate how far through this sub-path we are (i.e. the path between points[index] and points[index + 1])
        float segmentProgress = normalisedStep - linearSegments[index];
        float segmentMax = linearSegments[index + 1] - linearSegments[index];

        float t = segmentProgress / segmentMax; // t is in [0, 1]

        // All that remains is to linearly interpolate
        Vector2 segmentStart = points.ElementAt(index) + TRANSLATION;
        Vector2 segmentEnd = points.ElementAt(index + 1) + TRANSLATION;

        Vector2 interpolatedPosition = segmentStart + t * (segmentEnd - segmentStart);

        return interpolatedPosition;
    }
}
