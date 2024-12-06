using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Constructable")]
public class Constructable : ScriptableObject, Informative {

    [SerializeField]
    protected GridRow[] gridData;

    [SerializeField]
    protected bool obstructive;

    [SerializeField]
    protected String nameInfo, descriptionInfo;

    [SerializeField]
    protected Sprite previewSprite;

    [SerializeField]
    protected List<ResourceRequirement> requiredResources;

    ReadOnlyCollection<(Item, uint)> requiredResourcesReadOnly;
    ReadOnlyCollection<Vector2Int> exteriorPoints;
    ReadOnlyCollection<Vector2Int> interiorPoints;

    public bool IsObstructive() {
        return obstructive;
    }

    public TileBase GetTileAt(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return gridData[row].gridEntries[col].worldTile;
    }

    public TileBase GetPreviewTileAt(Vector2Int pos) {
        int col = pos.x;
        int row = pos.y;

        return gridData[row].gridEntries[col].previewTile;
    }

    public void SetData(GridRow[] gridData, bool obstructive) {
        this.gridData = gridData;
        this.obstructive = obstructive;
    }

    /// <summary>
    /// Calculate all the points exterior to this constructable,
    /// in relation to the bottom-left corner point of this constructable (NOT the world origin)
    /// </summary>
    void CalculateExteriorPoints() {
        // We need to consider all the y-levels present in the constructable, plus the one underneath,
        // and the one above it
        int lengthY = gridData.Length + 2;
        // Find the longest row. The x-ordinates to check are all those in the constructable, plus at most one on each side.
        int longestRowLength = gridData.Aggregate(0, (acc, row) => acc > row.gridEntries.Length ? acc : row.gridEntries.Length);
        int lengthX = longestRowLength + 2;

        // Construct a 2D array of booleans, where 'true' signifies that the coordinate is present in the constructable
        bool[,] present = new bool[lengthX, lengthY];
        int xOffset = 1, yOffset = 1;       // the 2D array begins at (-1, -1), while gridData begins at (0, 0)
        for (int y = 0 ; y < gridData.Length ; y += 1) {
            GridRow row = gridData[y];
            for (int x = 0 ; x < row.gridEntries.Length ; x += 1) {
                GridEntry grid = row.gridEntries[x];
                if (grid.worldTile == null) continue;

                present[x + xOffset, y + yOffset] = true;
            }
        }

        // Now, iterate through all the points that are not marked as present, but are adjacent to such a point
        // and add them to the hash set
        HashSet<Vector2Int> exterior = new HashSet<Vector2Int>();

        for (int x = 0 ; x < lengthX ; x += 1) {
            for (int y = 0 ; y < lengthY ; y += 1) {
                if (present[x, y]) continue;

                // (x, y + 1)
                if (y + 1 < lengthY && present[x, y + 1]) exterior.Add(new Vector2Int(x - xOffset, y - yOffset));
                // (x, y - 1)
                else if (y - 1 >= 0 && present[x, y - 1]) exterior.Add(new Vector2Int(x - xOffset, y - yOffset));
                // (x + 1, y)
                else if (x + 1 < lengthX && present[x + 1, y]) exterior.Add(new Vector2Int(x - xOffset, y - yOffset));
                // (x - 1, y)
                else if (x - 1 >= 0 && present[x - 1, y]) exterior.Add(new Vector2Int(x - xOffset, y - yOffset));
            }
        }

        exteriorPoints = exterior.ToList().AsReadOnly();
    }

    public ReadOnlyCollection<Vector2Int> GetExteriorPoints() {
        if (exteriorPoints == null) CalculateExteriorPoints();

        return exteriorPoints;
    }

    public ReadOnlyCollection<Vector2Int> GetInteriorPoints() {
        if (interiorPoints == null) {
            List<Vector2Int> interiorTemp = new List<Vector2Int>();

            for (int row = 0; row < gridData.Length ; row += 1) {
                GridRow rowData = gridData[row];

                for (int col = 0; col < rowData.gridEntries.Length; col += 1) {
                    GridEntry entry = rowData.gridEntries[col];
                    // Ignore empty entries
                    if (entry.worldTile == null) continue;

                    interiorTemp.Add(new Vector2Int(col, row));
                }
            }

            interiorPoints = interiorTemp.AsReadOnly();
        }

        return interiorPoints;
    }

    public ReadOnlyCollection<(Item, uint)> GetRequiredResources() {
        if (requiredResourcesReadOnly == null) {
            requiredResourcesReadOnly = requiredResources.Select((resource) => (resource.item, resource.quantity)).ToList().AsReadOnly();
        }

        return requiredResourcesReadOnly;
    }


    public String GetName() {
        return nameInfo;
    }

    public String GetDescription() {
        return descriptionInfo;
    }

    public Sprite GetPreviewSprite() {
        return previewSprite;
    }

    public virtual InfoType GetInfoType() {
        return InfoType.Structure;
    }

    public InfoBranch GetInfoTree(object instance = null) {
        // Dummy root, since we need a tree structure - this node is thrown away
        InfoBranch root = new InfoBranch(String.Empty);

        // Generic info
        InfoBranch genericCategory = new InfoBranch("Generic Properties");
        root.AddChild(genericCategory);

        InfoLeaf typeProperty = new InfoLeaf("Type", value: "Structure");
        genericCategory.AddChild(typeProperty);

        InfoLeaf nameProperty = new InfoLeaf("Name", value: nameInfo);
        genericCategory.AddChild(nameProperty);


        // Tile Entity info (if applicable)
        // Note that even if this is a tile entity, the root node may have no children
        // and thus no information will be added
        if (this is TileEntity tileEntity) {
            Dictionary<String, object> data = (Dictionary<String, object>) instance;
            InfoBranch tileEntityInfoRoot = tileEntity.GetTileEntityInfoTree(data);

            List<InfoNode> children = tileEntityInfoRoot.GetChildren();
            foreach (InfoNode child in children) root.AddChild(child);
        }

        return root;
    }
}

[Serializable]
public struct GridRow {
    public GridEntry[] gridEntries;
}

[Serializable]
public struct GridEntry {
    public TileBase worldTile;
    public TileBase previewTile;
}


[Serializable]
public struct ResourceRequirement {
    public Item item;
    public uint quantity;
}