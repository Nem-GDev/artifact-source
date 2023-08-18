using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkScale : NetworkBehaviour
{
    [Networked(OnChanged = nameof(UpdateScale))] public Vector3 scale { get; set; } = new Vector3(1,1,1);

    public static void UpdateScale(Changed<NetworkScale> changed)
    {
        changed.Behaviour.UpdateScale();
    }
    private void UpdateScale()
    {
        transform.localScale = scale;
    }
}
