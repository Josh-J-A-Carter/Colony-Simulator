using System;
using UnityEngine;

public interface Informative {
    
    public abstract Sprite GetPreviewSprite();

    public abstract String GetName();

    public abstract String GetDescription();

    public abstract InfoBranch GetInfoTree(object obj);

    public abstract InfoType GetInfoType();
}

public enum InfoType {
    Structure,
    Entity,
    Entity_Structure
}