using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftTrigger : MonoBehaviour
{
    public GameObject lift;
    private AudioSource audioSource;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = lift.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        Player player = collider.GetComponent<Player>();

        if (player)
        {
            if(player.hasKey)
            {
                anim.SetBool("Open", true);
            }
            else
            {
                audioSource.Play();
            }
        }
    }
}
