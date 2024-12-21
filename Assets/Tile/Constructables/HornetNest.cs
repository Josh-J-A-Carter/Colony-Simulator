using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Constructable/Hornet Nest")]
public class HornetNest : TileEntity {

    /// <summary> Path to the <c>List</c> containing <c>HornetBehaviour</c>, for those entities currently inside the nest </summary>
    const String HORNETS_CONTAINED = "hornetsContained";

    [SerializeField]
    GridRow[] oneVariant, twoVariant, threeVariant;

    public override Dictionary<String, object> GenerateDefaultData() {
        Dictionary<String, object> data = new();

        data[HORNETS_CONTAINED] = new List<HornetBehaviour>();
        return data;
    }

    public bool TryAddToNest(Vector2Int startPos, Dictionary<String, object> instance, HornetBehaviour hornet) {
        List<HornetBehaviour> existing = (List<HornetBehaviour>) instance[HORNETS_CONTAINED];

        if (existing.Count == 3) return false;

        existing.Add(hornet);
        hornet.OnNestEntry();

        if (existing.Count == 1) DrawVariant(startPos, pos => oneVariant[pos.y].gridEntries[pos.x].worldTile);
        else if (existing.Count == 2) DrawVariant(startPos, pos => twoVariant[pos.y].gridEntries[pos.x].worldTile);
        if (existing.Count == 3) DrawVariant(startPos, pos => threeVariant[pos.y].gridEntries[pos.x].worldTile);

        return true;
    }

    public bool TryRemoveFromNest(Vector2Int startPos, Dictionary<String, object> instance, HornetBehaviour hornet) {
        List<HornetBehaviour> existing = (List<HornetBehaviour>) instance[HORNETS_CONTAINED];

        if (existing.Remove(hornet) == false) return false;

        hornet.OnNestExit();

        if (existing.Count == 0) DrawVariant(startPos, GetTileAt);
        else if (existing.Count == 1) DrawVariant(startPos, pos => oneVariant[pos.y].gridEntries[pos.x].worldTile);
        if (existing.Count == 2) DrawVariant(startPos, pos => twoVariant[pos.y].gridEntries[pos.x].worldTile);

        return true;
    }

    public override void TickInstance(Vector2Int position, Dictionary<String, object> instance) {

    }
}
