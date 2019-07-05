using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFad : MonoBehaviour
{
    public AnimationCurve fadeCurve;
    private Image image;
    private float t;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameController.isLeaving)
        {
            t += Time.deltaTime / 5;
            float fade = fadeCurve.Evaluate(t);
            image.color = Color.Lerp(Color.clear, Color.black, fade);
        }
    }
}
