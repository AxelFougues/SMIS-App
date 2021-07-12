using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Feature : MonoBehaviour{

    

    void Start(){
        Events.current.onThemeChanged += onThemeChanged;
        Events.current.onFeatureSelected += onFeatureSelected;
    }


    private void onThemeChanged() {
        GetComponent<Image>().color = Global.current.theme.background;
    }

    private void onFeatureSelected() {
        gameObject.SetActive(false);
    }

}
