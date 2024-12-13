using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldLoader {

    const int WORLD_WIDTH = 10;

    const int WORLD_HEIGHT = 10;

    TileBase dirt, grass;

    Tilemap gameWorld;

    public bool LoadOrGenerateWorld(Tilemap gameWorld, TileBase dirt, TileBase grass) {

        this.gameWorld = gameWorld;
        this.dirt = dirt;
        this.grass = grass;

        // Try read world data from file
        // If it does exist, pass to load
        Vector3Int[] positions;
        TileBase[] tiles;
        if (TryLoadFromFile(out positions, out tiles) == false) {
            // If it doesn't exist, generate it
            bool success = GenerateWorld(out positions, out tiles);

            if (success == false) return false;
        }

        // Set the tile map to use this data
        LoadWorld(tiles, positions);


        return true;
    }

    bool TryLoadFromFile(out Vector3Int[] positions, out TileBase[] tiles) {
        positions = null;
        tiles = null;
        return false;
    }

    bool GenerateWorld(out Vector3Int[] positions, out TileBase[] tiles) {
        positions = new Vector3Int[WORLD_WIDTH * WORLD_HEIGHT];
        tiles = new TileBase[WORLD_WIDTH * WORLD_HEIGHT];

        for (int index = 0 ; index < WORLD_WIDTH * WORLD_HEIGHT ; index += 1) {

            int x = index % WORLD_WIDTH;
            int y = index / WORLD_WIDTH;

            positions[index] = new Vector3Int(x, y, 0);

            if (y > WORLD_HEIGHT - 2) {
                continue;
            } else if (y == WORLD_HEIGHT - 2) {
                tiles[index] = grass;
            } else {
                tiles[index] = dirt;
            }
        }

        return true;
    }


    bool LoadWorld(TileBase[] tiles, Vector3Int[] positions) {
        if (gameWorld == null || tiles == null) return false;

        gameWorld.SetTiles(positions, tiles);

        return true;
    }



}
