using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftTrigger : MonoBehaviour
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

    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Player")
        {
            anim.SetBool("Open", true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            anim.SetBool("Open", false);
            Invoke("Leave", 2f);
        }
    }

    void Leave()
    {
        GameObject.Find("Player").transform.parent = transform;
        anim.SetTrigger("LiftUp");
    }
}
