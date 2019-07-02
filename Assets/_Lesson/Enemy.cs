using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public AnimationCurve ikWeightCurve;

    [Range(0,1)]
    public float IKWeight;
    public Transform ikTarget;
    private Animator anim;

    private Transform playerTrans;

    private Vector3 aimIKPos;
    private Quaternion aimIKRot;

    public Transform rightHandTrans;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        playerTrans = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            Shoot();
            //UseSwitch();
        }

        Vector3 aimDirection = playerTrans.position - transform.position;

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(aimDirection), Time.deltaTime);

        aimIKPos = transform.position + aimDirection.normalized + Vector3.up * 1.4f;
        IKWeight = anim.GetFloat("AimWeight");

    }

    void UseSwitch()
    {
        StartCoroutine(IKCoroutine());
    }

    void Shoot()
    {
        anim.SetBool("Shoot", true);
    }

    IEnumerator IKCoroutine()
    {
        float time = 0;

        while(time <= 1)
        {
            time += Time.deltaTime * 0.5f;
            float weight = ikWeightCurve.Evaluate(time);
            IKWeight = weight;
            yield return null;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, IKWeight);
        anim.SetIKPosition(AvatarIKGoal.RightHand, aimIKPos);

        //anim.SetIKRotationWeight(AvatarIKGoal.RightHand, IKWeight);
        //anim.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.Euler(0,0,0));
    }
}
