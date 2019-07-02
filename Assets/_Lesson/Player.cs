using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AnimationCurve aimCurve;

    [Range(0, 1)]
    public float AimIKWeight;
    public Transform AimIKTarget;

    [Range(0, 1)]
    public float RightFootIKWeight;

    [Range(0, 1)]
    public float LeftFootIKWeight;
    private float AnimFootIKWeight;

    public Transform hipTrans;

    private Vector3 leftFootIKPos;
    private Vector3 rightFootIKPos;

    private Quaternion leftFootIKRot;
    private Quaternion rightFootIKRot;

    [Range(0, 720)]
    public float rotateSpeed = 180;
    private Transform camTrans;
    private Animator anim;

    private bool isWalking;
    private bool isSneaking;

    public Transform gunTrans;
    public Transform gunHolderTrans;
    public Transform gunAimPosTrans;

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

        AnimFootIKWeight = anim.GetFloat("FootIKWeight");

        if(AnimFootIKWeight > 0)
        {
            leftFootIKPos = GetFootIKPos(transform, transform.up + transform.right * -0.2f, out leftFootIKRot, out LeftFootIKWeight);
            rightFootIKPos = GetFootIKPos(transform, transform.up + transform.right * 0.2f, out rightFootIKRot, out RightFootIKWeight);
            RightFootIKWeight *= AnimFootIKWeight;
            LeftFootIKWeight *= AnimFootIKWeight;
        }
        else
        {
            RightFootIKWeight = 0;
            LeftFootIKWeight = 0;
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartAim();
        }

        if (Input.GetMouseButtonUp(1))
        {
            EndAim();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            isSneaking = !isSneaking;
            anim.SetBool("Sneak", isSneaking);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            anim.SetTrigger("Shout");
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

    Vector3 GetFootIKPos(Transform hipTrans, Vector3 offset, out Quaternion IKRot, out float IKWeight)
    {
        RaycastHit hit;
        if(Physics.Raycast(hipTrans.position + offset, Vector3.down, out hit, 1.3f))
        {
            IKRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            IKWeight = 1;
            return hit.point + hit.normal * 0.12f;
        }
        IKRot = transform.rotation;
        IKWeight = 0;
        return hit.point + hit.normal * 0.12f;
    }

    void StartAim()
    {
        StartCoroutine(AimCoroutine());
    }

    void EndAim()
    {
        StartCoroutine(EndAimCoroutine());
    }

    IEnumerator AimCoroutine()
    {
        float time = 0;

        while(time < 1)
        {
            time += Time.deltaTime * 4;
            AimIKWeight = aimCurve.Evaluate(time);
            yield return null;
        }

        time = 0;

        while(time < 1)
        {
            time += Time.deltaTime * 4;

            Vector3 gunPos = Vector3.Lerp(gunHolderTrans.position, gunAimPosTrans.position, aimCurve.Evaluate(time));
            Quaternion gunRot = Quaternion.Lerp(gunHolderTrans.rotation, gunAimPosTrans.rotation, aimCurve.Evaluate(time));

            gunTrans.position = gunPos;
            gunTrans.rotation = gunRot;
            yield return null;
        }
    }

    IEnumerator EndAimCoroutine()
    {
        float time = 1;

        while (time > 0)
        {
            time -= Time.deltaTime * 4;

            Vector3 gunPos = Vector3.Lerp(gunHolderTrans.position, gunAimPosTrans.position, aimCurve.Evaluate(time));
            Quaternion gunRot = Quaternion.Lerp(gunHolderTrans.rotation, gunAimPosTrans.rotation, aimCurve.Evaluate(time));

            gunTrans.position = gunPos;
            gunTrans.rotation = gunRot;
            yield return null;
        }

        time = 1;

        while (time > 0)
        {
            time -= Time.deltaTime * 4;
            AimIKWeight = aimCurve.Evaluate(time);
            yield return null;
        }
    }


    void OnAnimatorIK(int layerIndex)
    {
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, AimIKWeight);
        anim.SetIKPosition(AvatarIKGoal.RightHand, AimIKTarget.position);

        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, AimIKWeight);
        anim.SetIKRotation(AvatarIKGoal.RightHand, AimIKTarget.rotation);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, LeftFootIKWeight);
        anim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKPos);

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, RightFootIKWeight);
        anim.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKPos);

        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, LeftFootIKWeight);
        anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootIKRot);

        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, RightFootIKWeight);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, rightFootIKRot);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(leftFootIKPos, 0.1f);
        Gizmos.DrawWireSphere(rightFootIKPos, 0.1f);
    }

}
