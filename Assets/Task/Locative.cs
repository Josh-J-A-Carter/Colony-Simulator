using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public interface Locative {

    public abstract ReadOnlyCollection<Vector2Int> GetInteriorPoints();

    public abstract ReadOnlyCollection<Vector2Int> GetExteriorPoints();

    public abstract Vector2Int GetStartPosition();

}
