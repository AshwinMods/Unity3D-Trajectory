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

    // Ut + .5 ATT
    public float Distance_AtVel_DueToAcc_InTime(float u, float a, float t)
    {
        return u * t + 0.5f * a * t * t;
    }

    public void Calculate_Trajectory()
    {
        if (pathVertList == null)
        {
            pathVertList = new List<Vector3>();
        }

        pathVertList.Clear();
        float dt = 0;
        Vector3 d;
        for (int i = 0; i < splits; i++)
        {
            dt = (time / (splits - 1)) * i;
            d.x = Distance_AtVel_DueToAcc_InTime(velocity.x, acceleration.x, dt);
            d.y = Distance_AtVel_DueToAcc_InTime(velocity.y, acceleration.y, dt);
            d.z = Distance_AtVel_DueToAcc_InTime(velocity.z, acceleration.z, dt);
            pathVertList.Add(d);
        }
    }

    [Header("Ref")]
    [SerializeField] LineRenderer lineRendere = null;
    [SerializeField] Rigidbody projectile;

    [Header("Editor Setting")]
    [SerializeField] bool calc_Trajectory = false;
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

        if (fire)
        {
            fire = false;
            projectile.transform.position = transform.position;
            projectile.velocity = velocity + unityAccuracyFix;
        }
    }
}
