using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsFeature : MonoBehaviour{

    public GameObject DeviceCompatibilityWarning;

    public GameObject maxFrequency;
    public GameObject minFrequency;
    public GameObject EQSteps;

    private void OnEnable() {
        refreshVisuals();
    }

    public void refreshVisuals() {
        DeviceCompatibilityWarning.GetComponentInChildren<Toggle>().isOn = Global.current.settings.bypassDeviceCompatibilityWarning;
        maxFrequency.GetComponentInChildren<TMP_InputField>().text = Global.current.settings.maxFrequency.ToString();
        minFrequency.GetComponentInChildren<TMP_InputField>().text = Global.current.settings.minFrequency.ToString();
        EQSteps.GetComponentInChildren<TMP_InputField>().text = Global.current.settings.EQSteps.ToString();
    }

    public void updateSettings() {
        Global.current.settings.bypassDeviceCompatibilityWarning = DeviceCompatibilityWarning.GetComponentInChildren<Toggle>().isOn;

        string value = maxFrequency.GetComponentInChildren<TMP_InputField>().text;
        float frequency = 0;
        if (float.TryParse(value, out frequency) && isBetween(frequency, Global.current.settings.minFrequency, 20000f)) {
            Global.current.settings.maxFrequency = frequency;
        }else maxFrequency.GetComponentInChildren<TMP_InputField>().text = Global.current.settings.maxFrequency.ToString();

        value = minFrequency.GetComponentInChildren<TMP_InputField>().text;
        frequency = 0;
        if (float.TryParse(value, out frequency) && isBetween(frequency, 0, Global.current.settings.maxFrequency)) {
            Global.current.settings.minFrequency = frequency;
        } else minFrequency.GetComponentInChildren<TMP_InputField>().text = Global.current.settings.minFrequency.ToString();

        value = EQSteps.GetComponentInChildren<TMP_InputField>().text;
        int steps = 0;
        if(int.TryParse(value, out steps) && isBetween(steps, 2, 10)) Global.current.settings.EQSteps = steps;
        else EQSteps.GetComponentInChildren<TMP_InputField>().text = Global.current.settings.EQSteps.ToString();

        Events.current.settingsChanged();
    }




    bool isBetween(float testValue, float bound1, float bound2) {
        if (bound1 > bound2)
            return testValue >= bound2 && testValue <= bound1;
        return testValue >= bound1 && testValue <= bound2;
    }

}
