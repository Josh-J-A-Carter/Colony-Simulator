using System;
using System.Collections.Generic;
using UnityEngine;

public interface IConfigurable {
    
    public abstract InfoBranch GetConfigTree(Dictionary<String, object> instance);

}