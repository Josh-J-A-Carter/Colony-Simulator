using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour {

    public static TileManager Instance { get; private set; }

    [SerializeField]
    Tilemap worldMap, previewMap, taskPreviewMap;

    Graph obstacles;

    ConstructableGraph constructableGraph, constructablePreviewGraph, constructableTaskPreviewGraph;

    WorldLoader worldLoader;

    const int WORLD_WIDTH = 256;
    const int WORLD_HEIGHT = 48;
    const int MIN_X = - WORLD_WIDTH / 2;
    const int MIN_Y = - WORLD_HEIGHT / 3;

    BoundsInt bounds;


    [SerializeField]
    TileBase dirt, grass;

    [SerializeField]
    Constructable flower, flower2, flower3;

    TileEntityStore tileEntityStore;

    public const int TICK_RATE = 25;
    public const int FIXED_FRAME_RATE = 50;
    public const float TICKS_TO_SECONDS = (float) TICK_RATE / FIXED_FRAME_RATE;

    public int GetTileEntityTick() { return tileEntityStore.tick; }

    public void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        bounds = new(new(MIN_X, MIN_Y), new(WORLD_WIDTH, WORLD_HEIGHT));

        obstacles = new Graph();
        obstacles.CreateGraph(MIN_X, MIN_Y, MIN_X + WORLD_WIDTH, MIN_Y + WORLD_HEIGHT);

        constructableGraph = new ConstructableGraph(worldMap, this);
        constructablePreviewGraph = new ConstructableGraph(previewMap, this);
        constructableTaskPreviewGraph = new ConstructableGraph(taskPreviewMap, this);

        tileEntityStore = new TileEntityStore();

        worldLoader = new WorldLoader();

        worldLoader.LoadOrGenerateWorld(worldMap, obstacles, MIN_X, MIN_Y, WORLD_WIDTH, WORLD_HEIGHT, dirt, grass);


        Construct(new Vector2Int(-10, 1), flower);
        Construct(new Vector2Int(0, 1), flower);
        Construct(new Vector2Int(10, 1), flower2);
        Construct(new Vector2Int(11, 1), flower2);

        Construct(new Vector2Int(15, 1), flower3);
        Construct(new Vector2Int(14, 1), flower3);
        Construct(new Vector2Int(13, 1), flower3);
    }

    public void FixedUpdate() {
        tileEntityStore.Tick();
    }


    public bool IsInBounds(int x, int y) {
        return obstacles.IsInBounds(x, y);
    }

    public BoundsInt GetBounds() {
        return bounds;
    }

    public bool IsObstructed(Vector2Int p, ConstructableTag[] oneTagFrom = null) {
        return !IsUnobstructed(p.x, p.y, oneTagFrom);
    }

    public bool IsObstructed(int x, int y, ConstructableTag[] oneTagFrom = null) {
        return !IsUnobstructed(x, y, oneTagFrom);
    }

    public bool IsUnobstructed(Vector2Int pos, ConstructableTag[] oneTagFrom = null) {
        return IsUnobstructed(pos.x, pos.y, oneTagFrom);
    }

    public bool IsUnobstructed(int x, int y, ConstructableTag[] oneTagFrom = null) {
        if (oneTagFrom != null) {
            (_, Constructable c) = GetConstructableAt(new(x, y));
            if (c) foreach (ConstructableTag tag in oneTagFrom) if (c.HasTag(tag)) return true;
        }

        return !obstacles.IsObstructed(new Vector2Int(x, y));
    }

    public Dictionary<String, object> GetTileEntityData(Vector2Int position) {
        return tileEntityStore.GetTileEntityData(position);
    }

    public List<(Vector2Int, T, Dictionary<String, object>)> QueryTileEntities<T>(Func<(Vector2Int, T, Dictionary<String, object>), bool> filter = null) {
        if (filter == null) filter = _ => true;

        return tileEntityStore.Query<T>().Where(filter).ToList();
    }


    public bool FindResourceInStorage(Resource res, uint quantity, out List<Vector2Int> result) {
        result = new();
        int target = (int) quantity;
        
        foreach ((Vector2Int pos, IStorage storage, Dictionary<String, object> data) in QueryTileEntities<IStorage>()) {
            uint contribution = storage.CountResource(data, res);
            
            if (contribution == 0) continue;

            result.Add(pos);
            target -= (int) contribution;

            // Return early if we already reach the target
            if (target <= 0) return true;
        }

        return false;
    }

    public List<(Vector2Int, IStorage, Dictionary<String, object>)> FindAvailableStorage() {
        return tileEntityStore
                        .Query<IStorage>()
                        .Where(tuple => tuple.Item2.IsAvailableStorage(tuple.Item3))
                        .OrderBy(tuple => tuple.Item2.RemainingCapacity(tuple.Item3))
                        .ToList();
    }

    /// <summary>
    /// Build a <c>Constructable</c> into the world tilemap, without obtaining a reference to any tile entity data that is created there.
    /// </summary>
    /// <returns>True if the construction takes place, false otherwise; e.g. it is obstructed.</returns>
    public bool Construct(Vector2Int startPosition, Constructable constructable, Dictionary<String, object> dataTemplate = null) {
        return Construct(startPosition, constructable, out _, dataTemplate);
    }

    /// <summary>
    /// Build a <c>Constructable</c> into the world tilemap, and obtain a reference to any tile entity data that is created there
    /// through the <c>data</c> parameter.
    /// </summary>
    /// <returns>True if the construction takes place, false otherwise; e.g. it is obstructed.</returns>
    public bool Construct(Vector2Int startPosition, Constructable constructable, out Dictionary<String, object> data, Dictionary<String, object> dataTemplate = null) {
        // First, check if the desired area is completely clear.
        foreach (Vector2Int pos in constructable.GetInteriorPoints()) {
            if (worldMap.HasTile((Vector3Int) (startPosition + pos))) {
                data = null;
                return false;
            }
        }

        // If the constructable is a tile entity, make sure to add this to the list of tile entities
        if (constructable is TileEntity tileEntity) {
            data = tileEntityStore.AddTileEntity(startPosition, tileEntity, dataTemplate);
        } else data = null;

        // The area is clear, so we may continue with construction
        bool obstructive = constructable.IsObstructive();
        foreach (Vector2Int pos in constructable.GetInteriorPoints()) {
            SetTile(startPosition + pos, constructable.GetTileAt(pos), obstructive);
            constructableGraph.SetConstructable(startPosition + pos, (startPosition, constructable));
        }

        return true;
    }

    /// <summary>
    /// This method redraws a constructable to use its variant. Fails if the constructable at this position is not
    /// equal to the supplied parameter, <c>constructable</c>, or if the structure has a different starting position. Otherwise, succeeds.
    /// </summary>
    /// <param name="variantGenerator">Generate the corresponding tile variant for a given point, 
    /// given in <c>constructable</c>'s interior coordinate space
    /// </param>
    public bool DrawVariant(Vector2Int startPosition, Constructable constructable, Func<Vector2Int, TileBase> variantGenerator) {
        (Vector2Int existingStart, Constructable existingConstructable) = GetConstructableAt(startPosition);
        if (existingConstructable != constructable || existingStart != startPosition) return false;
        
        bool obstructive = constructable.IsObstructive();
        foreach (Vector2Int pos in constructable.GetInteriorPoints()) SetTile(startPosition + pos, variantGenerator(pos), obstructive);

        return true;
    }

    public bool Destroy(Vector2Int position) {

        // Find the beginning of the constructable which covers the desired position
        (Vector2Int startPosition, Constructable constructable) = GetConstructableAt(position);

        if (constructable == null) return false;

        Dictionary<String, object> data = null;

        // If the constructable is a tile entity, make sure to remove this - also get data to pass to OnDestruction
        if (constructable is TileEntity) {
            data = tileEntityStore.GetTileEntityData(startPosition);
            tileEntityStore.RemoveTileEntity(startPosition);
        }

        // Tell constructable to clean up
        constructable.OnDestruction(startPosition, data);

        // Remove from tilemap & constructable storage map
        foreach (Vector2Int pos in constructable.GetInteriorPoints()) {
            SetTile(startPosition + pos, null, false);
            constructableGraph.RemoveConstructable(startPosition + pos);
        }

        return true;
    }

    public bool SetTaskPreview(Vector2Int startPosition, Constructable constructable) {
        foreach (Vector2Int pos in constructable.GetInteriorPoints()) {
            SetTaskPreviewTile(startPosition + pos, constructable.GetPreviewTileAt(pos));
            constructableTaskPreviewGraph.SetConstructable(startPosition + pos, (startPosition, constructable));
        }

        return true;
    }

    public bool RemoveTaskPreview(Vector2Int position) {

        (Vector2Int startPosition, Constructable constructable) = GetConstructableTaskPreviewAt(position);

        if (constructable == null) return false;

        foreach (Vector2Int pos in constructable.GetInteriorPoints()) {
            SetTaskPreviewTile(startPosition + pos, null);
            constructableTaskPreviewGraph.RemoveConstructable(startPosition + pos);
        }

        return true;
    }



    public bool SetPreview(Vector2Int startPosition, Constructable constructable) {
        // First, check if the desired area is completely clear.
        foreach (Vector2Int pos in constructable.GetInteriorPoints()) if (previewMap.HasTile((Vector3Int) (startPosition + pos))) return false;

        // The area is clear, so we may continue with construction
        foreach (Vector2Int pos in constructable.GetInteriorPoints()) {
            SetPreviewTile(startPosition + pos, constructable.GetPreviewTileAt(pos));
            constructablePreviewGraph.SetConstructable(startPosition + pos, (startPosition, constructable));
        }

        return true;
    }

    public bool RemovePreview(Vector2Int position) {

        (Vector2Int startPosition, Constructable constructable) = GetConstructablePreviewAt(position);

        if (constructable == null) return false;

        foreach (Vector2Int pos in constructable.GetInteriorPoints()) {
            SetPreviewTile(startPosition + pos, null);
            constructablePreviewGraph.RemoveConstructable(startPosition + pos);
        }

        return true;
    }

    public (Vector2Int, Constructable) GetConstructableAt(Vector2Int position) {
        return constructableGraph.GetConstructable(position);
    }

    public (Vector2Int, Constructable) GetConstructablePreviewAt(Vector2Int position) {
        return constructablePreviewGraph.GetConstructable(position);
    }

    public (Vector2Int, Constructable) GetConstructableTaskPreviewAt(Vector2Int position) {
        return constructableTaskPreviewGraph.GetConstructable(position);
    }

    void SetPreviewTile(Vector2Int pos, TileBase t) {
        if (!obstacles.IsInBounds(pos.x, pos.y)) return;

        previewMap.SetTile((Vector3Int) pos, t);
    }

    void SetTaskPreviewTile(Vector2Int pos, TileBase t) {
        if (!obstacles.IsInBounds(pos.x, pos.y)) return;

        taskPreviewMap.SetTile((Vector3Int) pos, t);
    }

    void SetTile(Vector2Int pos, TileBase t, bool obstructive) {
        if (!obstacles.IsInBounds(pos.x, pos.y)) return;

        worldMap.SetTile((Vector3Int) pos, t);

        obstacles.SetObstructed(pos, obstructive);
    }
}
