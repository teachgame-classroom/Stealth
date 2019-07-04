using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int Health = 100;

    public float lookAngle = 75f;
    public float lookRange = 10f;

    public AnimationCurve aimCurve;

    public Transform lookTargetTrans;
    private Vector3 lookTargetPos;

    [Range(0, 1)]
    public float lookIKWeight;
    private float lookIKWeight_Current;

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

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        camTrans = Camera.main.transform;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        isWalking = Input.GetKey(KeyCode.LeftShift);

        AnimFootIKWeight = anim.GetFloat("FootIKWeight");

        if(AnimFootIKWeight > 0.1f)
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

        FindLookInterestPoint();

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
            audioSource.Play();
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

    void FindLookInterestPoint()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, lookRange);

        for(int i = 0; i < cols.Length; i++)
        {
            Vector3 direction = (cols[i].transform.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, direction) < lookAngle)
            {
                LookInterestPointMarker lookInterestPointMarker = cols[i].GetComponentInChildren<LookInterestPointMarker>();

                if (lookInterestPointMarker)
                {
                    lookTargetTrans = lookInterestPointMarker.transform;
                    lookTargetPos = lookTargetTrans.position;
                    lookIKWeight = 1;
                    return;
                }
            }
        }

        lookIKWeight = 0;
        //lookTargetTrans = null;
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

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if(Health <= 0)
        {
            GetComponent<Collider>().enabled = false;
            anim.SetTrigger("Die");
        }
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

        Debug.Log(lookIKWeight_Current);

        if(true)
        {
            lookIKWeight_Current = Mathf.MoveTowards(lookIKWeight_Current, lookIKWeight, 2 * Time.deltaTime);
            anim.SetLookAtWeight(lookIKWeight_Current);
            anim.SetLookAtPosition(lookTargetPos);
        }
        else
        {
            lookIKWeight_Current = Mathf.MoveTowards(lookIKWeight_Current, lookIKWeight, 2 * Time.deltaTime);
            anim.SetLookAtWeight(lookIKWeight_Current);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(leftFootIKPos, 0.1f);
        Gizmos.DrawWireSphere(rightFootIKPos, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lookRange);

        Gizmos.color = Color.red;
        Vector3 leftSightBorder = Quaternion.Euler(0, lookAngle, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftSightBorder * lookRange);

        Vector3 rightSightBorder = Quaternion.Euler(0, -lookAngle, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + rightSightBorder * lookRange);

    }

}
