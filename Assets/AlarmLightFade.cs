using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmLightFade : MonoBehaviour
{
    public AnimationCurve intensityCurve;
    private Light light;

    private float t;

    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameController.isAlarm)
        {
            t += Time.deltaTime / 2;
            t = Mathf.Repeat(t, 1);
        }
        else
        {
            t -= Time.deltaTime / 2;
            t = Mathf.Max(0, t);
        }

        float intensity = intensityCurve.Evaluate(t);
        light.intensity = intensity;
    }


}
