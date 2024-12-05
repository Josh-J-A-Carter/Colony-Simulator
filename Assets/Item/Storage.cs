using System;
using System.Collections.Generic;

public interface Storage {
    public abstract Inventory GetInventory(Dictionary<String, object> instance);
}
