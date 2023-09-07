using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ProjectileLogic : NetworkBehaviour
{
    public bool isExplosive = false;
    public bool isChain = false;
    public bool isDOT = false;
    public bool isAOEDOT = false;
    public bool isTargeted = false;
    public bool isRolltimed = false;
    public GameObject targetedObject = null;
    public float radius;
    public float aoeMod = 1f;
    public string targetType;
    public string friendly;
    public float damage;
    public int punchThrough;
    public float range;
    private float timer;
    private TickTimer life;
    public NetworkPrefabRef hitEffect;

    public UnitStats stats;
    public Dictionary<string, int> onHits;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("HIT!");
        if (Object != null)
            if (!Runner.IsServer)
                return;

        if (isTargeted)
        {
            if (other.gameObject == targetedObject)
            {
                other.gameObject.GetComponent<UnitStats>().TakeDamage(damage, stats, onHits);
                Runner.Spawn(hitEffect, transform.position, transform.rotation);
                Runner.Despawn(Object);
            }
        }
        else if (isRolltimed)
        {
            return;
        }
        else if (isAOEDOT)
        {
            if (LayerMask.LayerToName(other.gameObject.layer) != friendly)
            {
                Collider[] hitTargets = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask(targetType));
                void AdjustExplosion(NetworkRunner runner, NetworkObject obj)
                { obj.gameObject.GetComponent<NetworkScale>().scale *= radius; }
                if (hitEffect != null)
                {
                    NetworkObject ex = Runner.Spawn(hitEffect, transform.position, transform.rotation *
                                                    Quaternion.Euler(-90, 0, 0),
                                                    inputAuthority: null, AdjustExplosion, predictionKey: null);
                }
                foreach (Collider c in hitTargets)
                {
                    //Debug.Log("Hit a player collider");
                    c.GetComponent<UnitStats>().TakeTotalDot(damage);
                }
                Runner.Despawn(Object);
            }
        }
        else if (isExplosive)
        {
            //Debug.Log("in explosive");
            //! Explosive logic
            if (LayerMask.LayerToName(other.gameObject.layer) != friendly)
            {
                Collider[] hitTargets = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask(targetType));
                void AdjustExplosion(NetworkRunner runner, NetworkObject obj)
                { obj.gameObject.GetComponent<NetworkScale>().scale *= radius; }
                if (hitEffect != null)
                {
                    NetworkObject ex = Runner.Spawn(hitEffect, transform.position, transform.rotation *
                                                    Quaternion.Euler(-90, 0, 0),
                                                    inputAuthority: null, AdjustExplosion, predictionKey: null);
                }
                foreach (Collider c in hitTargets)
                {
                    //Debug.Log("Hit a player collider");
                    if (c.gameObject == other.gameObject)
                        c.gameObject.GetComponent<UnitStats>().TakeDamage((damage * aoeMod) + damage, stats, onHits);
                    else
                        c.gameObject.GetComponent<UnitStats>().TakeDamage(damage * aoeMod, stats, onHits);
                }
                Runner.Despawn(Object);
            }
        }
        else if (isChain)
        {
            //! Chain logic
            if (LayerMask.LayerToName(other.gameObject.layer) == targetType)
            {
                //Debug.Log("HIT!");
                other.gameObject.GetComponent<UnitStats>().TakeDamage(damage, stats, onHits);
                Runner.Spawn(hitEffect, transform.position, transform.rotation);
                punchThrough--;
                if (punchThrough <= 0)
                    Runner.Despawn(Object);
            }
        }
        else if (isDOT)
        {
            //! DOT logic
            if (LayerMask.LayerToName(other.gameObject.layer) == targetType)
            {
                //Debug.Log("HIT!");
                other.gameObject.GetComponent<UnitStats>().TakeTotalDot(damage);
                other.gameObject.GetComponent<UnitStats>().TakeDamage(damage, stats, onHits);
                Runner.Spawn(hitEffect, transform.position, transform.rotation);
                Runner.Despawn(Object);
            }
        }
        else
        {
            //! Basic logic
            if (LayerMask.LayerToName(other.gameObject.layer) == targetType)
            {
                //Debug.Log("HIT!");
                other.gameObject.GetComponent<UnitStats>().TakeDamage(damage, stats, onHits);
                Runner.Spawn(hitEffect, transform.position, transform.rotation);
                Runner.Despawn(Object);
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (life.Expired(Runner))
        {
            if (isRolltimed)
            {
                Collider[] hitTargets = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask(targetType));
                void AdjustExplosion(NetworkRunner runner, NetworkObject obj)
                { obj.gameObject.GetComponent<NetworkScale>().scale *= radius; }
                if (hitEffect != null)
                {
                    NetworkObject ex = Runner.Spawn(hitEffect, transform.position, transform.rotation *
                                                    Quaternion.Euler(-90, 0, 0),
                                                    inputAuthority: null, AdjustExplosion, predictionKey: null);
                }
                foreach (Collider c in hitTargets)
                {
                    //Debug.Log("Hit a player collider");
                    c.GetComponent<UnitStats>().TakeDamage(damage, stats, onHits);
                }
            }
            Runner.Despawn(Object);
        }
    }

    public void SetUp(bool isExplosive, float radius, float aoeMod, string target, string friendly,
                         float damage, float range, int punchThrough)
    {
        this.isExplosive = isExplosive;
        this.radius = radius;
        this.aoeMod = aoeMod;
        this.friendly = friendly;
        this.targetType = target;
        this.damage = damage;
        this.range = range;
        this.punchThrough = punchThrough;
        life = TickTimer.CreateFromSeconds(Runner, this.range / 10f);
    }
    public void SetUp(string target, float damage, float range, int punchThrough)
    {
        this.targetType = target;
        this.damage = damage;
        this.range = range;
        this.punchThrough = punchThrough;
        life = TickTimer.CreateFromSeconds(Runner, this.range / 10f);
    }

}
