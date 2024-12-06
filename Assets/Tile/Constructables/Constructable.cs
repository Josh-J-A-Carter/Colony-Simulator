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
    protected String infoName, infoDescription;

    [SerializeField]
    protected Sprite previewSprite;

    [SerializeField]
    protected List<ResourceRequirement> requiredResources;
    public ReadOnlyCollection<(Item, uint)> requiredResourcesReadOnly;

    public int RowCount() {
        return gridData.Length;
    }

    public GridRow GetRow(int index) {
        if (index < 0 || index >= RowCount()) throw new Exception($"No row with index {index} in this Constructable, {this}.");

        return gridData[index];
    }

    public void SetData(GridRow[] gridData) {
        this.gridData = gridData;
    }

    /// <summary>
    /// Calculate all the points exterior to this constructable,
    /// in relation to the bottom-left corner point of this constructable (NOT the world origin)
    /// </summary>
    public List<Vector2Int> CalculateExteriorPoints() {
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

        return exterior.ToList();
    }


    public String GetName() {
        return infoName;
    }

    public String GetDescription() {
        return infoDescription;
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

        InfoLeaf nameProperty = new InfoLeaf("Name", value: GetName());
        genericCategory.AddChild(nameProperty);

        InfoLeaf typeProperty = new InfoLeaf("Type", value: "Structure");
        genericCategory.AddChild(typeProperty);

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

    public ReadOnlyCollection<(Item, uint)> GetRequiredResources() {
        if (requiredResourcesReadOnly == null) {
            requiredResourcesReadOnly = requiredResources.Select((resource) => (resource.item, resource.quantity)).ToList().AsReadOnly();
        }

        return requiredResourcesReadOnly;
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
    public bool obstructive;
}


[Serializable]
public struct ResourceRequirement {
    public Item item;
    public uint quantity;
}