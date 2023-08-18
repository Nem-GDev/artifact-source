using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SelfDestructor : NetworkBehaviour
{
    // Start is called before the first frame update
    public float timer;
    TickTimer t;
    public override void Spawned()
    {
        base.Spawned();
        if (Runner.IsServer)
            t = TickTimer.CreateFromSeconds(Runner, timer);
    }

    // Update is called once per frame
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Runner.IsServer)
            if (t.Expired(Runner))
                Runner.Despawn(Object);
    }
}
