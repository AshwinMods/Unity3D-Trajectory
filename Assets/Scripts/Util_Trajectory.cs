using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util_Trajectory : MonoBehaviour
{
    [SerializeField] List<Vector3> pathVertList = new List<Vector3>();
    [SerializeField] float time = 1;
    [SerializeField] Vector3 velocity = Vector3.right;
    [SerializeField] Vector3 acceleration = Vector3.down;
    [SerializeField] Vector3 unityAccuracyFix = Vector3.zero;
    [SerializeField] int splits = 3;
    [Space]
    [SerializeField] Vector3 targetPos = Vector3.one;
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
        }
    }

    public void Calculate_Velocity()
    {
        Vector3 d = (targetPos - transform.position);
        velocity.x = Vel_ForDistance_DueToAcc_InTime(d.x, acceleration.x, time);
        velocity.y = Vel_ForDistance_DueToAcc_InTime(d.y, acceleration.y, time);
        velocity.z = Vel_ForDistance_DueToAcc_InTime(d.z, acceleration.z, time);
    }

    [Header("Ref")]
    [SerializeField] LineRenderer lineRendere = null;
    [SerializeField] Rigidbody projectile;
    [SerializeField] Transform target;

    [Header("Editor Setting")]
    [SerializeField] bool calc_Trajectory = false;
    [SerializeField] bool calc_Velocity = false;
    [SerializeField] bool auto_calc = false;

    [Space]
    [SerializeField] bool fire = false;
    private void OnDrawGizmosSelected()
    {
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
            targetPos = target.transform.position;
            Calculate_Velocity();
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
    }
}
