using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HealthBehaviour : MonoBehaviour
{
    //Assign and Access each HP Bar
    [SerializeField]
    GameObject HPBar1;
    [SerializeField]
    GameObject HPBar2;
    [SerializeField]
    GameObject HPBar3;
    [SerializeField]
    GameObject HPBar4;
    //Reference material to change to when damaged/repaired
    public Material HPWeakRed;
    public Material HPSolidGreen;

    [SerializeField]
    int timesDamaged;
    void Start()
    {
        timesDamaged = 0;
    }

    public void OnDamaged()
    {
        timesDamaged++;
        switch (timesDamaged)
        {
            case 1:
            if (HPBar1 != null)
                {
                    Renderer HPBarRenderer = HPBar1.GetComponent<Renderer>();
                    HPBarRenderer.material = HPWeakRed;
                }
            break;
            case 2:
            if (HPBar2 != null)
                {
                    Renderer HPBarRenderer = HPBar2.GetComponent<Renderer>();
                    HPBarRenderer.material = HPWeakRed;
                }
            break;
            case 3:
            if (HPBar3 != null)
                {
                    Renderer HPBarRenderer = HPBar3.GetComponent<Renderer>();
                    HPBarRenderer.material = HPWeakRed;
                }
            break;
            case 4:
            if (HPBar1 != null)
                {
                    Renderer HPBarRenderer = HPBar4.GetComponent<Renderer>();
                    HPBarRenderer.material = HPWeakRed;
                }
            break;

            default:
            break;
        }
    }
    public void OnRepair()
    {
        timesDamaged--;
        switch (timesDamaged)
        {
            case 0:
            if (HPBar1 != null)
                {
                    Renderer HPBarRenderer = HPBar1.GetComponent<Renderer>();
                    HPBarRenderer.material = HPSolidGreen;
                }
            break;
            case 1:
            if (HPBar2 != null)
                {
                    Renderer HPBarRenderer = HPBar2.GetComponent<Renderer>();
                    HPBarRenderer.material = HPSolidGreen;
                }
            break;
            case 2:
            if (HPBar3 != null)
                {
                    Renderer HPBarRenderer = HPBar3.GetComponent<Renderer>();
                    HPBarRenderer.material = HPSolidGreen;
                }
            break;
            case 3:
            if (HPBar1 != null)
                {
                    Renderer HPBarRenderer = HPBar4.GetComponent<Renderer>();
                    HPBarRenderer.material = HPSolidGreen;
                }
            break;

            default:
            break;
        }
    }


    
}
