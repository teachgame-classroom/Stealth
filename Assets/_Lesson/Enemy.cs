using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public PathManager pathManager;
    private Transform[] waypoints;
    private int currentWaypoint = -1;

    public AnimationCurve ikWeightCurve;

    [Range(0,1)]
    public float IKWeight;
    public Transform ikTarget;
    private Animator anim;

    private Transform playerTrans;

    private Vector3 aimIKPos;
    private Quaternion aimIKRot;

    public Transform rightHandTrans;

    private NavMeshAgent agent;

    private Vector3 navMeshHitPoint;

    // Start is called before the first frame update
    void Start()
    {
        waypoints = pathManager.GetWayPoints();

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        playerTrans = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Patrol();
        SyncNavAnimRotation();
    }

    void Patrol()
    {
        if (agent.remainingDistance < 0.01f)
        {
            currentWaypoint++;
            if (currentWaypoint > waypoints.Length - 1)
            {
                currentWaypoint = 0;
            }
        }

        Vector3 waypointPos = waypoints[currentWaypoint].position;

        NavMeshHit hit;

        if(NavMesh.SamplePosition(waypointPos, out hit, 1, NavMesh.AllAreas))
        {
            navMeshHitPoint = hit.position;
            agent.SetDestination(hit.position);            
        }
    }

    void SyncNavAnimRotation()
    {
        agent.speed = anim.velocity.magnitude;

        agent.updatePosition = false;
        agent.updateRotation = false;

        Vector3 moveDirection = (agent.nextPosition - transform.position).normalized;
        Vector3 forward = transform.forward;

        //求出角色应该向左还是向右转
        //朝向和移动方向作叉乘，如果叉乘得到的向量directionCross和角色正上方transform.up方向相同，说明移动方向在右边，否则说明在左边
        Vector3 directionCross = Vector3.Cross(transform.forward, moveDirection);

        //transform.up和directionCross是否同向，根据点乘结果来判断
        float dot = Vector3.Dot(directionCross, transform.up);

        anim.SetFloat("Speed", 0.5f, 0.2f, Time.deltaTime);
        anim.SetFloat("Angle", dot, 0.2f, Time.deltaTime);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 5);
        }
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
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(navMeshHitPoint, 0.25f);
    }

    void OnAnimatorMove()
    {
        transform.position = agent.nextPosition;
    }
}
