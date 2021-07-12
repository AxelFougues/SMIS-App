using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Alert : MonoBehaviour{
    public TMP_Text variableText;

    private void OnDisable() {
        variableText.text = "";
    }
}
