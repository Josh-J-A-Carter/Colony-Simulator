using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item/Generic Item Component")]
public class ItemComponent : ScriptableObject {
    
    [field: SerializeField]
    public ItemTag ComponentType { get; private set; }

}
