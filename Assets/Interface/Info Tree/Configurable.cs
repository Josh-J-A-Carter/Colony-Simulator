using System;
using System.Collections.Generic;
using UnityEngine;

public interface Configurable {
    
    public abstract InfoBranch GetConfigTree(Dictionary<String, object> instance);

}