using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class ForageTask : WorkerTask, ILocative, IReward {
    
    IProducer forageStructure;

    ReadOnlyCollection<Vector2Int> interiorPoints, exteriorPoints;

    Vector2Int startPos;

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

    public List<(Item, uint)> GetRewardItems() {
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

        InfoLeaf progressProperty = new InfoLeaf("Progress", (float) progress / MAX_PROGRESS + "%");
        root.AddChild(progressProperty);

        return root;
    }

    public override WorkerTaskType GetCategory() {
        return WorkerTaskType.Forage;
    }
}
