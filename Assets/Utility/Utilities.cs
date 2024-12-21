using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Random = UnityEngine.Random;


public static class Utilities {
    public static Dictionary<String, object> RecursiveDataCopy(Dictionary<String, object> data) {
        Dictionary<String, object> copy = new Dictionary<string, object>(data.Count);

        foreach (KeyValuePair<String, object> pair in data) {
            if (pair.Value is Dictionary<String, object> childData) copy.Add(pair.Key, RecursiveDataCopy(childData));

            // Base case here: value is not a dictionary, so assume it is pass by value.
            else copy.Add(pair.Key, pair.Value);

        }

        return copy;
    }


    /// <summary>
    /// Get human-readable description for an Enum variant; Each variant must be tagged with '[Description("...")]'
    /// </summary>
    public static String GetDescription(this Enum value) {
        Type type = value.GetType();
        String name = Enum.GetName(type, value);
        if (name == null) return null;

        FieldInfo field = type.GetField(name);
        if (field == null) return null;
        DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

        if (attr == null) return null;
        return attr.Description;
    }

    public static bool RandBool() {
        return Random.Range(0, 2) == 0;
    }

    public static int RandSign() {
        return (int) Math.Pow(-1, Random.Range(0, 2));
    }
}