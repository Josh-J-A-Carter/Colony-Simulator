using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStorage {
    public bool Take(Dictionary<String, object> instance, Item item, uint quantity);

    public void Give(Vector2Int defaultLocation, Dictionary<String, object> instance, Item item, uint quantity);

    public uint CountItem(Dictionary<String, object> instance, Item item);

    public uint CountResource(Dictionary<String, object> instance, Resource res);

    public bool HasResources(Dictionary<String, object> instance, List<(Resource, uint)> resources);


    public bool IsAvailableStorage(Dictionary<String, object> instance);

    public uint RemainingCapacity(Dictionary<String, object> instance);

}
