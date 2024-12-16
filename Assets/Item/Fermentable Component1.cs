using System;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "ScriptableObjects/Item/Fermentable Component")]
public class FermentableComponent : ItemComponent {

    [SerializeField]
    public Item fermentationItem;
}
