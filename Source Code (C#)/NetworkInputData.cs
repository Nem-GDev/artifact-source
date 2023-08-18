using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector3 dir;
    public Vector3 aim;
    public bool primary;
    public bool ability1, ability2;
    public bool upgrade1, upgrade2;

}
