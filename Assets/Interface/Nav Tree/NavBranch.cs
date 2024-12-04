using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Nav Branch")]
public class NavBranch : NavNode {

    [SerializeField]
    NavNode[] subcategories;

    public NavNode[] GetChildren() {
        return subcategories;
    }

}
