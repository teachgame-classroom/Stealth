using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableByTime : MonoBehaviour
{
    void OnEnable()
    {
        Invoke("DisableDelay", 0.1f);
    }

    void DisableDelay()
    {
        gameObject.SetActive(false);
    }
}
