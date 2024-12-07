using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject, Informative {

    [SerializeField]
    String infoName, infoDescription;

    [SerializeField]
    Sprite previewSprite;

    public string GetDescription() {
        return infoDescription;
    }
    public InfoType GetInfoType() {
        return InfoType.Item;
    }

    public string GetName() {
        return infoName;
    }

    public Sprite GetPreviewSprite() {
        return previewSprite;
    }

    public InfoBranch GetInfoTree(object _ = null) {
        throw new NotImplementedException();
    }
}