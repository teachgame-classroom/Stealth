using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    private Transform[] waypoints;

    // Start is called before the first frame update
    void Start()
    {
        FindWaypoints();
    }

    private void FindWaypoints()
    {
        waypoints = new Transform[transform.childCount + 1];
        waypoints[0] = transform;

        for (int i = 1; i < waypoints.Length; i++)
        {
            waypoints[i] = transform.GetChild(i - 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform[] GetWayPoints()
    {
        if(waypoints == null)
        {
            FindWaypoints();
        }

        return waypoints;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);

        for (int i = 0; i < transform.childCount; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.GetChild(i).position, Vector3.one * 0.07f);

            Vector3 lineStartPos = transform.position;
            Vector3 lineEndPos = transform.GetChild(i).position;

            if (i > 0)
            {
                lineStartPos = transform.GetChild(i - 1).position;
            }
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lineStartPos, lineEndPos);
        }
    }
}
