using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityData {
    
    (int, int)[] attributes;

    public TileEntityData((int, int)[] attributes) {
        this.attributes = attributes;
    }

    public void SetAttribute(int attribute, int value) {
        for (int i = 0 ; i < attributes.Length ; i += 1) {
            if (attributes[i].Item1 == attribute) {
                attributes[i].Item2 = value;
                return;
            }
        }
    }

    public bool TryGetAttribute(int attribute, out int value) {
        for (int i = 0 ; i < attributes.Length ; i += 1) {
            if (attributes[i].Item1 == attribute) {
                value = attributes[i].Item2;
                return true;
            }
        }

        value = -1;
        return false;
    }
}