using System;
using System.ComponentModel;

[Serializable]
public enum ItemTag {
    [Description("Food")]
    Food,
    [Description("Royal Jelly")]
    RoyalJelly,
    [Description("Nectar")]
    Nectar,
    [Description("Pollen")]
    Pollen,
    [Description("Sap")]
    Sap,
    [Description("Fermentable(s)")]
    Fermentable,
    [Description("Beeswax")]
    Beeswax,
    [Description("Standard Quality")]
    Standard,
    [Description("Superior Quality")]
    Superior,
    [Description("Perfect Quality")]
    Perfect
}
