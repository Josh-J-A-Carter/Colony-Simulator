using System;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "ScriptableObjects/Item/Food Component")]
public class FoodComponent : ItemComponent {

    [field: SerializeField]
    public uint NutritionalValue { get; private set; }
}
