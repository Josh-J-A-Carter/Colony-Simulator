using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class ForageTask : Task, ILocative, IReward {
    
    IProducer forageStructure;

    ReadOnlyCollection<Vector2Int> interiorPoints, exteriorPoints;

    Vector2Int startPos;

    bool allocatedReward = false;

    public ForageTask(Vector2Int startPos, IProducer forageStructure) {
        this.startPos = startPos;
        this.forageStructure = forageStructure;
    }

    public ReadOnlyCollection<Vector2Int> GetExteriorPoints() {
        if (exteriorPoints == null) {
            List<Vector2Int> exteriorTemp = new List<Vector2Int>();

            Constructable constructable = forageStructure as Constructable;
            foreach (Vector2Int pos in constructable.GetExteriorPoints()) exteriorTemp.Add(pos + startPos);

            exteriorPoints = exteriorTemp.AsReadOnly();
        }

        return exteriorPoints;
    }

    public ReadOnlyCollection<Vector2Int> GetInteriorPoints() {
        if (interiorPoints == null) {
            List<Vector2Int> interiorTemp = new List<Vector2Int>();

            Constructable constructable = forageStructure as Constructable;
            foreach (Vector2Int pos in constructable.GetInteriorPoints()) interiorTemp.Add(pos + startPos);

            interiorPoints = interiorTemp.AsReadOnly();
        }

        return interiorPoints;
    }

    public Vector2Int GetStartPosition() {
        return startPos;
    }

    public bool CanCoexist() {
        return false;
    }

    public List<Item> GetAvailableRewardItems() {
        Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(startPos);
        return forageStructure.AvailableProductionItemTypes(data);
    }

    public List<(Item, uint)> CollectRewardItems() {
        if (allocatedReward) return new();

        allocatedReward = true;
        Dictionary<String, object> data = TileManager.Instance.GetTileEntityData(startPos);
        return forageStructure.CollectAll(data);
    }

    public override String GetName() {
        return "Foraging";
    }

    public override String GetDescription() {
        return "Foraging for nectar, pollen, sap, and other resources essential to the survival of the bee nest";
    }

    public override InfoBranch GetInfoTree(object obj = null) {
        InfoBranch root = new InfoBranch(String.Empty);
        
        InfoLeaf nameProperty = new InfoLeaf("Type", "Foraging");
        root.AddChild(nameProperty);

        int percentProgress = (int) (100 * (float) progress / MAX_PROGRESS);
        InfoLeaf progressProperty = new InfoLeaf("Progress", percentProgress + "%");
        root.AddChild(progressProperty);

        return root;
    }

    public override bool IsRuleGenerated() {
        return true;
    }
}
