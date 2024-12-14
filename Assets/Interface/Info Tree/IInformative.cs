using System;
using UnityEngine;

public interface IInformative {
    
    public abstract String GetName();

    public abstract String GetDescription();

    public abstract InfoBranch GetInfoTree(object obj = null);
}
