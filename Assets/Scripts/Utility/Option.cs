using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Option<T> {
    
    bool hasValue = false;
    T value;

    public static Option<T> Some(T value) {
        Option<T> option = new Option<T>();
        option.value = value;
        option.hasValue = true;
        return option;
    }

    public static Option<T> None() {
        return new Option<T>();
    }

    public bool ValueOrDefault(T defaultValue, out T value) {
        if (hasValue) {
            value = this.value;
            return true;
        }

        value = defaultValue;
        return false;
    }

}
