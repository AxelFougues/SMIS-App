using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static ConnexionStatus;

public class FeatureButton : MonoBehaviour{

    public GameObject feature;
    public GameObject allowedAlert;
    public GameObject deniedAlert;
    public GameObject comingSoonAlert;
    public List<Device> prefered = new List<Device>();
    public List<Device> allowed = new List<Device>();

    string availability = "denied";

    ConnexionStatus connexionStatus;
    MenuBehaviour menu;

    private void Start() {
        connexionStatus = FindObjectOfType(typeof(ConnexionStatus)) as ConnexionStatus;
        menu = FindObjectOfType(typeof(MenuBehaviour)) as MenuBehaviour;
        Events.current.onConnexionStatusChanged += onConnexionStatusChanged;
        Events.current.onThemeChanged += onThemeChanged;
        updateAvailability();
    }

    public void onClick() {
        switch (availability) {
            case "prefered":
                feature.SetActive(true);
                break;
            case "allowed":
                feature.SetActive(true);
                displayAllowedPopup();
                break;
            case "denied":
                displayDeniedPopup();
                break;
            case "comingSoon":
                displayCommingSoonAlert();
                break;
        }
        menu.menuOut();
    }

    private void onConnexionStatusChanged() {
        updateAvailability();
    }

    private void updateAvailability() {
        if (feature != null) {
            if (!Global.current.settings.deviceCompatibilityWarning && prefered.Contains(connexionStatus.connexion)) {
                availability = "prefered";
            } else if (allowed.Contains(connexionStatus.connexion)) {
                availability = "allowed";
            }
            else availability = "denied";
        } else {
            availability = "comingSoon";
        }
        onThemeChanged();
    }

    private void onThemeChanged() {
        switch (availability) {
            case "prefered":
                GetComponentInChildren<TMP_Text>().color = Global.current.theme.text;
                break;
            case "allowed":
                GetComponentInChildren<TMP_Text>().color = Global.current.theme.grayed;
                break;
            case "denied":
                GetComponentInChildren<TMP_Text>().color = Global.current.theme.denied;
                break;
            case "comingSoon":
                GetComponentInChildren<TMP_Text>().color = Global.current.theme.denied;
                break;
        }
        GetComponent<Image>().color = Global.current.theme.foreground;
    }

    public void displayAllowedPopup() {
        allowedAlert.SetActive(true);
        string preferedString = "";
        foreach (Device device in prefered) preferedString += device.ToString() + " ";
        allowedAlert.GetComponent<Alert>().variableText.text = preferedString;
    }

    public void displayDeniedPopup() {
        deniedAlert.SetActive(true);
        string preferedString = "";
        foreach (Device device in prefered) preferedString += device.ToString() + " ";
        deniedAlert.GetComponent<Alert>().variableText.text = preferedString;
    }

    public void displayCommingSoonAlert() {
        comingSoonAlert.SetActive(true);
        string preferedString = "";
        foreach (Device device in prefered) preferedString += device.ToString() + " ";
        comingSoonAlert.GetComponent<Alert>().variableText.text = preferedString;
    }

}
