using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldLoader {

    int worldWidth, worldHeight, minX, minY;


    TileBase dirt, grass;

    Tilemap gameWorld;

    public bool LoadOrGenerateWorld(Tilemap gameWorld, TileBase dirt, TileBase grass, int minX, int minY, int worldWidth, int worldHeight) {

        this.gameWorld = gameWorld;
        this.dirt = dirt;
        this.grass = grass;

        this.minX = minX;
        this.minY = minY;
        this.worldWidth = worldWidth;
        this.worldHeight = worldHeight;

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
        positions = new Vector3Int[worldWidth * worldHeight];
        tiles = new TileBase[worldWidth * worldHeight];

        int surface_level = 0;

        for (int index = 0 ; index < worldWidth * worldHeight ; index += 1) {

            int x = index % worldWidth + minX;
            int y = index / worldWidth + minY;

            positions[index] = new Vector3Int(x, y, 0);

            if (y > surface_level) {
                continue;
            } else if (y == surface_level || y == surface_level - 1) {
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
