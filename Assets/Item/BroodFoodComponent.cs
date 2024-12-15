using System;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "ScriptableObjects/Item/Royal Jelly Component")]
public class RoyalJellyComponent : ItemComponent {

    [field: SerializeField]
    public uint NutritionalValue { get; private set; }
}
