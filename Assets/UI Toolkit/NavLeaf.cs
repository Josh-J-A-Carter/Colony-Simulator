using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Nav Leaf")]
public class NavLeaf : NavNode {

    [SerializeField]
    Constructable[] constructables;

    public Constructable[] GetChildren() {
        return constructables;
    }

}
