using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBehaviour : MonoBehaviour{

    Animator animator;
    public Button retractButton;

    private void Start() {
        animator = GetComponent<Animator>();
        retractButton.gameObject.SetActive(false);
    }

    public void menuIn() {
        animator.SetTrigger("menuIn");
        retractButton.gameObject.SetActive(true);
    }

    public void menuOut() {
        animator.SetTrigger("menuOut");
        retractButton.gameObject.SetActive(false);
    }
    
}
