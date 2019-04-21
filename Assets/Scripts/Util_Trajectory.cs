using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util_Trajectory : MonoBehaviour
{
    [Header("Continious Path")]
    [SerializeField] List<Vector3> pathVertList = new List<Vector3>();
    [SerializeField] float time = 1;
    [SerializeField] Vector3 velocity = Vector3.right;
    [SerializeField] Vector3 acceleration = Vector3.down;
    [SerializeField] Vector3 unityAccuracyFix = Vector3.zero;
    [SerializeField] int splits = 3;

    [Header("Dotted Path")]
    [SerializeField] bool timeBasedDot = false;
    [SerializeField] List<Vector3> dotList = new List<Vector3>();
    [SerializeField] float dotTimeSpace = 0.1f;
    [Space]
    [SerializeField] int dotCount = 10;
    [SerializeField] float dotDistSpace = 0.25f;
    [SerializeField] float dotCalcTimeStep = 0.1f;
    [SerializeField] bool usePathforDots = false;

    [Space]
    [SerializeField] float pathLength = 0;
    // Ut + .5 ATT
    public float Distance_AtVel_DueToAcc_InTime(float u, float a, float t)
    {
        return u * t + 0.5f * a * t * t;
    }
    public float Vel_ForDistance_DueToAcc_InTime(float d, float a, float t)
    {
        return (d - Distance_AtVel_DueToAcc_InTime(0, a, t)) / time;
    }

    public void Calculate_Trajectory()
    {
        pathLength = 0;

        if (pathVertList == null)
        {
            pathVertList = new List<Vector3>();
        }

        if (pathVertList.Count > splits)
        {
            pathVertList.RemoveRange(splits, (pathVertList.Count - splits));
        }
        else if(pathVertList.Count < splits)
        {
            pathVertList.AddRange(new Vector3[splits - pathVertList.Count]);
        }
        float dt = 0;
        Vector3 d;
        for (int i = 0; i < splits; i++)
        {
            dt = (time / (splits - 1)) * i;
            d.x = Distance_AtVel_DueToAcc_InTime(velocity.x, acceleration.x, dt);
            d.y = Distance_AtVel_DueToAcc_InTime(velocity.y, acceleration.y, dt);
            d.z = Distance_AtVel_DueToAcc_InTime(velocity.z, acceleration.z, dt);
            pathVertList[i] = d;
            if (i > 0)
            {
                pathLength += Vector3.Distance(pathVertList[i], pathVertList[i - 1]);
            }
        }
    }
    public void Calculate_Velocity(Vector3 targetPos)
    {
        Vector3 d = (targetPos - transform.position);
        velocity.x = Vel_ForDistance_DueToAcc_InTime(d.x, acceleration.x, time);
        velocity.y = Vel_ForDistance_DueToAcc_InTime(d.y, acceleration.y, time);
        velocity.z = Vel_ForDistance_DueToAcc_InTime(d.z, acceleration.z, time);
    }
    public void Calculate_Dots()
    {
        pathLength = 0;
        int dotNum = (timeBasedDot ? (int)(time / dotTimeSpace + 0.5f) : dotCount);
        
        if (dotList == null)
        {
            dotList = new List<Vector3>();
        }

        if (dotList.Count > dotNum)
        {
            dotList.RemoveRange(dotNum, (dotList.Count - dotNum));
        }
        else if(dotList.Count < dotNum)
        {
            dotList.AddRange(new Vector3[dotNum - dotList.Count]);
        }

        float dt = 0;
        Vector3 dot = Vector3.zero;
        Vector3 dir = Vector3.one;
        if (timeBasedDot)
        {
            for (int i = 0; i < dotNum; i++)
            {
                dt = (i + 1) * dotTimeSpace;
                dot.x = Distance_AtVel_DueToAcc_InTime(velocity.x, acceleration.x, dt);
                dot.y = Distance_AtVel_DueToAcc_InTime(velocity.y, acceleration.y, dt);
                dot.z = Distance_AtVel_DueToAcc_InTime(velocity.z, acceleration.z, dt);
                dotList[i] = dot;
                if (i > 0)
                {
                    pathLength += Vector3.Distance(dotList[i], dotList[i - 1]);
                }
            }
        }
        else
        {
            bool usingPath = usePathforDots;
            int plIndx = 0;
            dotList[0] = Vector3.zero;
            for (int i = 1; i < dotNum; i++)
            {
                while ( Vector3.Distance(dotList[i-1] , dot) < dotDistSpace)
                {
                    if (usingPath)
                    {
                        if (plIndx < pathVertList.Count)
                        {
                            dot = pathVertList[plIndx++];
                        }
                        else
                        {
                            usingPath = false;
                            dt = time;
                        }
                    }
                    else
                    {
                        dt += dotCalcTimeStep;
                        dot.x = Distance_AtVel_DueToAcc_InTime(velocity.x, acceleration.x, dt);
                        dot.y = Distance_AtVel_DueToAcc_InTime(velocity.y, acceleration.y, dt);
                        dot.z = Distance_AtVel_DueToAcc_InTime(velocity.z, acceleration.z, dt);
                    }
                }
                dir = (dot - dotList[i - 1]).normalized;
                dotList[i] = dotList[i - 1] + dir * dotDistSpace;

                if (i > 0)
                {
                    pathLength += Vector3.Distance(dotList[i], dotList[i - 1]);
                }
            }
        }

    }

    [Header("Ref")]
    [SerializeField] LineRenderer lineRendere = null;
    [SerializeField] Rigidbody projectile;
    [SerializeField] Transform target;

    [Header("Editor Setting")]
    [SerializeField] bool calc_Trajectory = false;
    [SerializeField] bool calc_Velocity = false;
    [SerializeField] bool calc_Dots = false;
    [SerializeField] bool auto_calc = false;

    [Space]
    [SerializeField] bool fire = false;
    private void OnDrawGizmosSelected()
    {
        time = Mathf.Max(0.001f, time);
        dotTimeSpace = Mathf.Max(0.001f, dotTimeSpace);
        dotDistSpace = Mathf.Max(0.001f, dotDistSpace);
        dotCalcTimeStep = Mathf.Max(0.001f, dotCalcTimeStep);
        splits = Mathf.Max(2, splits);

        if (calc_Trajectory || auto_calc)
        {
            calc_Trajectory = false;
            Calculate_Trajectory();
            lineRendere.positionCount = splits;
            lineRendere.SetPositions(pathVertList.ToArray());
        }

        if (calc_Velocity || auto_calc)
        {
            calc_Velocity = false;
            Calculate_Velocity(target.transform.position);
        }

        if (calc_Dots || auto_calc)
        {
            calc_Dots = false;
            Calculate_Dots();
        }

        if (fire)
        {
            fire = false;
            projectile.transform.position = transform.position;
            projectile.velocity = velocity + unityAccuracyFix;
        }
    }

    private void OnDrawGizmos()
    {
        if (auto_calc)
        {
            OnDrawGizmosSelected();
        }

        if (dotList != null)
        {
            for (int i = 0; i < dotList.Count; i++)
            {
                Gizmos.DrawWireSphere(dotList[i], 0.05f);
            }
        }
    }
}
