using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILiving {

    public bool IsDead { get; }

    public HealthComponent HealthComponent { get; }
}
