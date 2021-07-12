using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events : MonoBehaviour{

    public static Events current;

    void Awake() {
        current = this;
    }

    public event Action onConnexionStatusChanged;
    public void connexionStatusChanged() {
        if (onConnexionStatusChanged != null) onConnexionStatusChanged();
    }

    public event Action onFeatureSelected;
    public void featureSelected() {
        if (onFeatureSelected != null) onFeatureSelected();
    }

    public event Action onThemeChanged;
    public void themeChanged() {
        if (onThemeChanged != null) onThemeChanged();
    }

    public event Action onSettingsChanged;
    public void settingsChanged() {
        if (onSettingsChanged != null) onSettingsChanged();
    }
}
