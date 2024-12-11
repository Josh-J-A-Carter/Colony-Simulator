using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoCheckbox : InfoNode {

    String[] path;

    bool value;

    bool modifiable;

    public InfoCheckbox(String categoryName, String[] path, bool value, bool modifiable) {
        this.categoryName = categoryName;
        this.path = path;
        this.value = value;
        this.modifiable = modifiable;
    }

    public bool GetValue() {
        return value;
    }

    public String[] GetPath() {
        return path;
    }

    public bool IsModifiable() {
        return modifiable;
    }
}
