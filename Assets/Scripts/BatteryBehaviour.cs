using UnityEngine;

public class BatteryBehaviour : MonoBehaviour
{
    public float batteryCharge = 1f;
    public float maxLiquidHeight = 1f;
    public Transform liquid;

    // Update is called once per frame
    void Update()
    {
        UpdateBattery();
    }

    public void UpdateBattery()
    {
        //Clamp the charge
        batteryCharge = Mathf.Clamp01(batteryCharge);
        //Change liquid height depending on charge
        Vector3 scale = liquid.localScale;
        scale.y = batteryCharge;
        liquid.localScale = scale;
        //Keep the liquid at the bottom of the container
        liquid.localPosition = new Vector3(
            liquid.localPosition.x,
            scale.y*maxLiquidHeight/2f,
            liquid.localPosition.z
        );
    }
    public void DrainBattery()
    {
        batteryCharge -= .1f;
    }
}
