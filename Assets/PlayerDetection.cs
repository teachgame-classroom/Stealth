using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{

    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Player")
        {
            foreach(Enemy e in Enemy.enemies)
            {
                e.SpotPlayer(collider.transform.position);
            }
        }
    }
}
