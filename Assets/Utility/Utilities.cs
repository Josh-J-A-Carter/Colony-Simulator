using System;
using System.Collections.Generic;


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
}