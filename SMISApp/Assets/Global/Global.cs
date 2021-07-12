using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour{
    public static Global current;

    public Theme theme;
    public Settings settings;

    void Awake() {
        current = this;
    }
}
