using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBehaviour : MonoBehaviour{

    public Animator retractionButtonAnimator;
    Animator animator;
    public Image retractButton;

    private void Start() {
        animator = GetComponent<Animator>();
        retractButton.raycastTarget = false;
    }

    public void menuIn() {
        retractButton.raycastTarget = true;
        retractionButtonAnimator.SetTrigger("FadeIn");
        animator.SetTrigger("menuIn");
    }

    public void menuOut() {
        retractButton.raycastTarget = false;
        retractionButtonAnimator.SetTrigger("FadeOut");
        animator.SetTrigger("menuOut");
    }

    public void closeAll() {
        Events.current.featureSelected();
    }
    
}
