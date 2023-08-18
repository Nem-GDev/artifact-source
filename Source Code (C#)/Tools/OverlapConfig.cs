using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapConfig : MonoBehaviour
{
    public bool show = true;
    public Transform centerOffset;
    public string type;
    public float radius;
    public Vector3 dimensions = new();
    private void OnDrawGizmos()
    {
        if (show)
        {
            Gizmos.color = Color.red;
            if (type == "Box")
            {
                Gizmos.DrawWireCube(centerOffset.position, dimensions);
            }
            else if (type == "Sphere")
            {
                Gizmos.DrawWireSphere(centerOffset.position, radius);
            }
        }
    }

    public Vector3 GetBoxConfig()
    {
        return dimensions;
    }
    public float GetSphereConfig()
    {
        return radius;
    }
}
