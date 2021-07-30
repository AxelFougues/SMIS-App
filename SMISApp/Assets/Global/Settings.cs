using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Settings")]
public class Settings : ScriptableObject{

    public bool bypassDeviceCompatibilityWarning = true;

    public float maxFrequency = 400f;

    public float minFrequency = 40f;

    public int EQSteps = 10;

}
