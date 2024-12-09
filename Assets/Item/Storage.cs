using System;
using System.Collections.Generic;
using UnityEngine;

public interface Storage {
    public bool Take(Dictionary<String, object> instance, Item item, uint quantity);

    public void Give(Vector2Int defaultLocation, Dictionary<String, object> instance, Item item, uint quantity);

    public uint CountItem(Dictionary<String, object> instance, Item item);

    public bool IsAvailableStorage(Dictionary<String, object> instance);

    public uint RemainingCapacity(Dictionary<String, object> instance);

}
