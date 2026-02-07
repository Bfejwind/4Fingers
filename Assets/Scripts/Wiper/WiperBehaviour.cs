using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WiperBehaviour : MonoBehaviour
{
    public float screenDirtiness;
    public Image SandOverlay;
    public WiperTargetL leftWiped;
    public WiperTargetR rightWiped;

    void Start()
    {
        screenDirtiness = 0;
    }

    void SandEffect()
    {
        screenDirtiness = Mathf.Clamp01(screenDirtiness);
        Color imageColor = Color.white;
        imageColor.a = screenDirtiness;
        SandOverlay.color = imageColor;
    }
    public void GettingDirty()
    {
        screenDirtiness += .1f;
    }
    public void GettingClean()
    {
        screenDirtiness -= .1f;
    }
    void Update()
    {
        if (leftWiped.wipedOn && rightWiped.wipedOn)
        {
            GettingClean();
            leftWiped.wipedOn = false;
            rightWiped.wipedOn = false;
        }
        SandEffect();
    }
}
