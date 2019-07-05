using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift : MonoBehaviour
{
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        Player player = other.GetComponent<Player>();

        if (player)
        {
            if (player.hasKey)
            {
                GameController.instance.StartLeaving();
                anim.SetBool("Open", false);
                Invoke("Leave", 2f);
            }
        }
    }

    void Leave()
    {
        GameObject.Find("Player").transform.parent = transform;
        anim.SetTrigger("LiftUp");
    }
}
