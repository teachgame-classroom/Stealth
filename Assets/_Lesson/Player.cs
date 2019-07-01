using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Range(0, 720)]
    public float rotateSpeed = 180;
    private Transform camTrans;
    private Animator anim;

    private bool isWalking;
    private bool isSneaking;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        camTrans = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        isWalking = Input.GetKey(KeyCode.LeftShift);
        
        if(Input.GetKeyDown(KeyCode.C))
        {
            isSneaking = !isSneaking;
            anim.SetBool("Sneak", isSneaking);
        }

        Vector3 camTransForwardProj = camTrans.forward;
        camTransForwardProj.Set(camTransForwardProj.x, 0, camTransForwardProj.z);
        camTransForwardProj.Normalize();

        Vector3 direction = camTrans.right * h + camTransForwardProj * v;

        float directionMag = isWalking ? Mathf.Clamp(direction.magnitude, 0, 0.5f) : direction.magnitude;

        if(directionMag > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        //anim.SetFloat("Speed", v);
        anim.SetFloat("Speed", directionMag, 0.25f, Time.deltaTime);
    }
}
