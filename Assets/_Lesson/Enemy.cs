using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public static List<Enemy> enemies = new List<Enemy>();

    public static bool isAlarm;
    public bool isShooting;
    public AudioClip shotClip;

    private LineRenderer lineRenderer;

    private bool doingDamage;

    public float alarmDuration = 10f;
    private float alarmTimer = 0;

    public float lookRange = 10;
    public float lookAngle = 45;
    public float hearDistance;

    private Vector3 playerDirection;
    private float playerDistance;
    private Vector3 lastPlayerPos;

    public PathManager pathManager;
    private Transform[] waypoints;
    private int currentWaypoint = -1;

    public AnimationCurve ikWeightCurve;

    [Range(0,1)]
    public float IKWeight;
    public Transform ikTarget;
    private Animator anim;
    private float speed;

    private Transform playerTrans;
    private AudioSource playerSound;

    private Vector3 aimIKPos;
    private Quaternion aimIKRot;

    public Transform rightHandTrans;

    private NavMeshAgent agent;

    private Vector3 navMeshHitPoint;

    // Start is called before the first frame update
    void Start()
    {
        enemies.Add(this);

        waypoints = pathManager.GetWayPoints();

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        playerTrans = GameObject.Find("Player").transform;
        playerSound = playerTrans.GetComponent<AudioSource>();
        lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckPlayerDirection();

        if(!isAlarm)
        {
            if(CanSpotPlayer())
            {
                isAlarm = true;
                alarmTimer = alarmDuration;
            }

            Patrol();
        }
        else
        {
            alarmTimer -= Time.deltaTime;

            if(alarmTimer < 0)
            {
                isAlarm = false;
            }

            if(!isShooting)
            {
                ChasePlayer();
            }
            else
            {
                if(CanSpotPlayer())
                {
                    alarmTimer = alarmDuration;

                    if(CanSeePlayer())
                    {
                        agent.isStopped = true;
                    }
                }
                else
                {
                    isShooting = false;
                    agent.isStopped = false;
                }
            }
        }

        Shoot();
        SyncNavAnimRotation();
    }

    void Patrol()
    {
        speed = 0.5f;

        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < 0.1f)
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

    void ChasePlayer()
    {
        agent.SetDestination(lastPlayerPos);

        if(Vector3.Distance(transform.position, lastPlayerPos) < 0.1f)
        {
            speed = 0;
        }
        else
        {
            if (CanSeePlayer())
            {
                speed = 0;
                isShooting = true;
            }
            else
            {
                speed = 1;
            }
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

        anim.SetFloat("Speed", speed, 0.2f, Time.deltaTime);
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
        anim.SetBool("Shoot", isAlarm && isShooting);

        float shot = anim.GetFloat("Shot");

        if(shot > 0.5f && !doingDamage)
        {
            doingDamage = true;
            playerTrans.GetComponent<Player>().TakeDamage(60);
            PlayEffect();
        }

        if(shot < 0.5f)
        {
            doingDamage = false;
        }

    }

    void PlayEffect()
    {
        lineRenderer.SetPosition(0, lineRenderer.transform.position);
        lineRenderer.SetPosition(1, playerTrans.position + Vector3.up * 1.5f);
        lineRenderer.GetComponent<Light>().enabled = true;

        lineRenderer.gameObject.SetActive(true);

        AudioSource.PlayClipAtPoint(shotClip, transform.position);
    }

    void CheckPlayerDirection()
    {
        playerDirection = playerTrans.position - transform.position;
        playerDistance = playerDirection.magnitude;
        playerDirection.Normalize();
        aimIKPos = transform.position + Vector3.up * 1.5f + playerDirection;
    }

    public void SpotPlayer(Vector3 lastPos)
    {
        alarmTimer = alarmDuration;
        lastPlayerPos = lastPos;
        isAlarm = true;
    }

    bool CanSpotPlayer()
    {
        bool result = playerTrans.GetComponent<Player>().Health > 0 && (CanSeePlayer() || CanHearPlayer());

        if(result)
        {
            lastPlayerPos = playerTrans.position;
        }

        return result;
    }

    bool CanSeePlayer()
    {
        bool playerInViewAngle = Vector3.Angle(transform.forward, playerDirection) < lookAngle;

        if (playerInViewAngle)
        {
            bool playerInRange = playerDistance < lookRange;

            if(playerInRange)
            {
                RaycastHit hit;

                if(Physics.Raycast(transform.position + transform.up, playerDirection, out hit))
                {
                    if(hit.transform.tag == "Player")
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    bool CanHearPlayer()
    {
        bool playerInRange = playerDistance < hearDistance;

        if(playerInRange)
        {
            if (playerSound)
            {
                return playerSound.isPlaying;
            }
        }

        return false;
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
        IKWeight = anim.GetFloat("AimWeight");
        
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

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), string.Format("Alarm:{0}, Timer:{1}", isAlarm, alarmTimer));
    }
}
