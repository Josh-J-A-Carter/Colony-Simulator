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

    protected override String GetTaskCategory() {
        return "Foraging";
    }

    protected override String GetTaskType() {
        return "Foraging plant materials";
    }


    public override bool IsRuleGenerated() {
        return true;
    }
}
