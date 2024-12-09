using System.Collections.Generic;
using System;
using UnityEngine;

public class TileEntityStore {

    public const int TICK_RATE = 25;

    public int tick { get; private set; } = 0;
    
    List<(Vector2Int, TileEntity, Dictionary<String, object>)> tileEntities;

    List<(Vector2Int, TileEntity, Dictionary<String, object>)> tileEntitiesToAdd;
    List<Vector2Int> tileEntitiesToRemove;

    public TileEntityStore() {
        tileEntities = new List<(Vector2Int, TileEntity, Dictionary<string, object>)>();
        tileEntitiesToAdd = new List<(Vector2Int, TileEntity, Dictionary<String, object>)>();
        tileEntitiesToRemove = new List<Vector2Int>();
    }

    public void Tick() {
        tick += 1;

        //
        // Instead of directly adding or removing tile entities when AddTileEntity or RemoveTileEntity are called,
        // we add the tile entities to a list to avoid concurrency issues; we are iterating tileEntities, so
        // if one of those tries to alter the list in some way during the loop (e.g. tries to delete itself),
        // we're probably gonna have a bad time!
        // 
        RemovePending();
        AddPending();

        // Call the tick function for each tile entity
        if (tick >= TICK_RATE) {
            tick = 0;

            foreach ((Vector2Int position, TileEntity tileEntity, Dictionary<String, object> data) in tileEntities) {
                tileEntity.TickInstance(position, data);
            }
        }
    }

    public List<(Vector2Int, T, Dictionary<String, object>)> Query<T>() {
        List<(Vector2Int, T, Dictionary<String, object>)> queryResult = new List<(Vector2Int, T, Dictionary<string, object>)>();

        foreach ((Vector2Int position, TileEntity tileEntity, Dictionary<String, object> data) in tileEntities) {
            if (tileEntity is T instanceOfT) {
                queryResult.Add((position, instanceOfT, data));
            }
        }

        return queryResult;
    }

    public Dictionary<String, object> AddTileEntity(Vector2Int position, TileEntity tileEntity) {
        Dictionary<String, object> data = tileEntity.GenerateDefaultData();

        tileEntitiesToAdd.Add((position, tileEntity, data));
        return data;
    }

    public Dictionary<String, object> GetTileEntityData(Vector2Int position) {
        foreach ((Vector2Int pos, _, Dictionary<String, object> data) in tileEntities) {
            if (pos == position) return data;
        }

        return null;
    }

    public void RemoveTileEntity(Vector2Int position) {
        tileEntitiesToRemove.Add(position);
    }

    void RemovePending() {
        for (int i = 0 ; i < tileEntitiesToRemove.Count ; i += 1) {
            Vector2Int position = tileEntitiesToRemove[i];

            for (int j = 0 ; j < tileEntities.Count ; j += 1) {
                if (tileEntities[j].Item1 == position) {
                    (Vector2Int pos, TileEntity tileEntity, Dictionary<String, object> data) = tileEntities[j];
                    // tileEntity.Destroy(pos, tileEntities[j].Item3);
                    tileEntities.RemoveAt(j);
                    break;
                }
            }
        }

        tileEntitiesToRemove.Clear();
    }

    void AddPending() {
        for (int i = 0 ; i < tileEntitiesToAdd.Count ; i += 1) {
            (Vector2Int pos, TileEntity tileEntity, Dictionary<String, object> data) = tileEntitiesToAdd[i];
            tileEntities.Add((pos, tileEntity, data));
            // tileEntity.Create(pos, data);
        }

        tileEntitiesToAdd.Clear();
    }

}
