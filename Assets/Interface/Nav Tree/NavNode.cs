using System;
using UnityEngine;

public abstract class NavNode : ScriptableObject {
    
    [SerializeField]
    String categoryName;

    [SerializeField]
    Sprite preview;

    NavNode parentNode;

    public NavNode GetParentNode() {
        return parentNode;
    }

    public String GetCategoryName() {
        return categoryName;
    }

    public Sprite GetPreview() {
        return preview;
    }

}
