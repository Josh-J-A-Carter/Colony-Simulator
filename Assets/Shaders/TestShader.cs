using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeTest : MonoBehaviour {

    Material material;

    void Start() {
        material = GetComponent<Material>();

        bool[] data = new bool[] { false, true, true, false };

        ComputeBuffer discovered = new ComputeBuffer(data.Length, sizeof(bool));
        discovered.SetData(data);

        material.SetBuffer("_MaxAmplitude", discovered);

        // material.SetInt("_ArraySize", data.Length);
    }
}
