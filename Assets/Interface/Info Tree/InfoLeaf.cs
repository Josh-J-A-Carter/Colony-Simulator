using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoLeaf : InfoNode {

    String value, description;

    bool hasWarning;

    public InfoLeaf(String categoryName, String value = null, String description = null, bool hasWarning = false) {
        this.categoryName = categoryName;
        this.value = value;
        this.description = description;
        this.hasWarning = hasWarning;
    }

    public String GetValue() {
        return value;
    }

    public String GetDescription() {
        return description;
    }

    /// <summary>
    /// <para>Does this property have any warnings associated with it?</para>
    /// For example, a property value may be of concern and thus require immediate user attention,
    /// so it should be flagged with a visual warning indicator.
    /// </summary>
    public bool HasWarning() {
        return hasWarning;
    }
}
