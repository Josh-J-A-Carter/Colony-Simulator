using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Resource {
    
    [field: SerializeField]
    public ResourceType ResourceType { get; private set; }

    [field: SerializeField]
    public Item Item { get; private set; }

    [field: SerializeField]
    public ItemTag ItemTag { get; private set; }

    public Resource(Item Item) {
        this.Item = Item;

        // Unused nonsense
        ItemTag = 0;

        ResourceType = ResourceType.Item;
    }

    public Resource(ItemTag ItemTag) {
        this.ItemTag = ItemTag;

        // Unused nonsense
        Item = null;

        ResourceType = ResourceType.Tag;
    }
}


public enum ResourceType {
    Item,
    Tag
}