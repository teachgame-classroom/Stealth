using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    public float followSpeed = 10;

    // Start is called before the first frame update
    void Start()
    {
        if(player == null)
        {
            player = GameObject.Find("Player");
        }
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        if(player)
        {
            transform.position = Vector3.Lerp(transform.position, player.transform.position, followSpeed * Time.deltaTime);
        }
    }
}
