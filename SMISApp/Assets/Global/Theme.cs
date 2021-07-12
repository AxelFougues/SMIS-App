using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Theme")]
public class Theme : ScriptableObject{
    public Color background;
    public Color foreground;
    public Color text;
    public Color negativeText;
    public Color grayed;
    public Color lightGrayed;
    public Color denied;
}
