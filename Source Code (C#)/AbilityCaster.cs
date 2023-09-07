using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class AbilityCaster : NetworkBehaviour
{
    [SerializeField] List<string> orderedAbilityNames;
    [SerializeField] List<NetworkPrefabRef> orderedAbilityPrefabs;
    string targetTypeOpponent;
    string targetTypeFriendly;
    bool isCasting;
    public Dictionary<string, int> onHits = new(){
        {AbilityDataSet.ONHIT_aoeIgnite, 0}, //!ni
        {AbilityDataSet.ONHIT_crit, 0},
        {AbilityDataSet.ONHIT_explode, 0},  //!ni
        {AbilityDataSet.ONHIT_lifesteal, 0},
        {AbilityDataSet.ONHIT_poison, 0},
        {AbilityDataSet.ONHIT_slow, 0},
        {AbilityDataSet.ONHIT_stun, 0}
    };
    public UnitStats stats;

    Vector3 mouse;
    AudioSource tmpSource;
    public AudioClip defClip;

    public override void Spawned()
    {
        base.Spawned();
        if (GetComponent<UnitStats>().unitType == "Enemy")
        {
            targetTypeFriendly = "Enemy";
            targetTypeOpponent = "Player";
        }
        else
        {
            targetTypeFriendly = "Player";
            targetTypeOpponent = "Enemy";
        }
        stats = GetComponent<UnitStats>();
    }
    public bool CastAbility(AbilityTemplate ability, Vector3 target)
    {
        if (!isCasting)
        {
            if (ability.logicType == AbilityDataSet.LOGIC_INSTANT_BOX)
            {
                CastInstantBox(ability);
                return true;
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_INSTANT_SPHERE)
            {
                CastInstantSphere(ability);
                return true;
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_PROJECTILE)
            {
                CastProjectile(ability, target);
                return true;
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_PROJECTILE_AOE)
            {
                CastProjectileAOE(ability, target);
                return true;
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_PROJECTILE_TARGET)
            {
                return CastProjectileTarget(ability, target);
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_PROJECTILE_TARGET_COUNT)
            {
                return CastProjectileTargetCount(ability, target);
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_INSTANT_SPHERE_SLOW)
            {
                CastInstantSphereSlow(ability);
                return true;
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_INSTANT_SPHERE_VORTEX)
            {
                CastInstantSphereVortex(ability);
                return true;
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_PROJECTILE_AOE_DOT)
            {
                CastProjectileAOEDOT(ability, target);
                return true;
            }
            else if (ability.logicType == AbilityDataSet.LOGIC_PROJECTILE_ROLL_TIMED_AOE)
            {
                CastProjectileRolltimedAOE(ability, target);
                return true;
            }


            else
                return false;
        }
        else
            return false;

    }

    private void CastInstantBox(AbilityTemplate ability)
    {
        //! All logic is ran only on server, except for vfx and networked param changes
        string vfxName = ability.abilityName + "_Casted";
        Vector3 boxCenterOffset = transform.Find(vfxName).GetComponent<OverlapConfig>().centerOffset.position;
        Vector3 boxSize = transform.Find(vfxName).GetComponent<OverlapConfig>().GetBoxConfig();

        RPC_PlayVFX(vfxName);
        Collider[] hitTargets = Physics.OverlapBox(boxCenterOffset,
                                    boxSize / 2, transform.rotation, LayerMask.GetMask(targetTypeOpponent));
        foreach (Collider c in hitTargets)
        {
            //Debug.Log("Hit a player collider");
            c.gameObject.GetComponent<UnitStats>().TakeDamage(ability.damage, stats, onHits);
        }
    }
    private void CastInstantSphere(AbilityTemplate ability)
    {
        //? find prefab corresponding to ability
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef abi = orderedAbilityPrefabs[index];

        //? Find init data 
        string vfxName = ability.abilityName + "_Casted";
        Vector3 sphereCenterOffset = transform.Find(vfxName).GetComponent<OverlapConfig>().centerOffset.position;
        Quaternion rot = transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);

        // ? spawn effect
        void AdjustAOE(NetworkRunner runner, NetworkObject obj)
        { obj.transform.Find(vfxName).GetComponent<NetworkScale>().scale *= (ability.radiusAOE * 2); }
        NetworkObject ob = Runner.Spawn(abi, sphereCenterOffset, rot,
                                        inputAuthority: null, AdjustAOE, predictionKey: null);

        // ? hit logic
        Collider[] hitTargets = Physics.OverlapSphere(sphereCenterOffset, ability.radiusAOE, LayerMask.GetMask(targetTypeOpponent));
        foreach (Collider c in hitTargets)
        {
            //Debug.Log("Hit a player collider");
            c.gameObject.GetComponent<UnitStats>().TakeDamage(ability.damage, stats, onHits);
        }

    }
    private void CastInstantSphereSlow(AbilityTemplate ability)
    {
        //? find prefab corresponding to ability
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef abi = orderedAbilityPrefabs[index];

        //? Find init data 
        string vfxName = ability.abilityName + "_Casted";
        Vector3 sphereCenterOffset = transform.Find(vfxName).GetComponent<OverlapConfig>().centerOffset.position;
        Quaternion rot = transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);

        // ? spawn effect
        void AdjustAOE(NetworkRunner runner, NetworkObject obj)
        { obj.transform.Find(vfxName).GetComponent<NetworkScale>().scale *= (ability.radiusAOE * 2); }
        NetworkObject ob = Runner.Spawn(abi, sphereCenterOffset, rot,
                                        inputAuthority: null, AdjustAOE, predictionKey: null);

        // ? hit logic
        Collider[] hitTargets = Physics.OverlapSphere(sphereCenterOffset, ability.radiusAOE, LayerMask.GetMask(targetTypeOpponent));
        foreach (Collider c in hitTargets)
        {
            //Debug.Log("Hit a player collider");
            c.gameObject.GetComponent<UnitStats>().TakeDamage(ability.damage, stats, onHits);
            c.gameObject.GetComponent<UnitStats>().TakeSlowTimed(30f, 3f);
        }

    }

    private void CastInstantSphereVortex(AbilityTemplate ability)
    {
        //? find prefab corresponding to ability
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef abi = orderedAbilityPrefabs[index];

        //? Find init data 
        string vfxName = ability.abilityName + "_Casted";
        Vector3 sphereCenterOffset = transform.Find(vfxName).GetComponent<OverlapConfig>().centerOffset.position;
        Quaternion rot = transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);

        // ? spawn effect
        void AdjustAOE(NetworkRunner runner, NetworkObject obj)
        { obj.transform.Find(vfxName).GetComponent<NetworkScale>().scale *= (ability.radiusAOE * 2); }
        NetworkObject ob = Runner.Spawn(abi, sphereCenterOffset, rot,
                                        inputAuthority: null, AdjustAOE, predictionKey: null);

        // ? hit logic
        Collider[] hitTargets = Physics.OverlapSphere(sphereCenterOffset, ability.radiusAOE, LayerMask.GetMask(targetTypeOpponent));
        foreach (Collider c in hitTargets)
        {
            //Debug.Log("Hit a player collider");
            c.gameObject.GetComponent<UnitStats>().TakeDamage(ability.damage, stats, onHits);
            c.gameObject.GetComponent<UnitStats>().TakeStun(2.5f);
            float xR = UnityEngine.Random.Range(-0.5f, 0.5f);
            float zR = UnityEngine.Random.Range(-0.5f, 0.5f);
            c.transform.position = new Vector3(transform.position.x + xR, c.transform.position.y, transform.position.z + zR);
        }

    }

    private void CastProjectile(AbilityTemplate ability, Vector3 target)
    {
        //? find prefab corresponding to projectile
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef proj = orderedAbilityPrefabs[index];
        //? spawn and shoot logic
        Vector3 dir = new Vector3(target.x - transform.position.x, 0f, target.z - transform.position.z).normalized;
        Vector3 source = new Vector3(transform.position.x, 1f, transform.position.z);
        NetworkObject pr = Runner.Spawn(proj, source, transform.rotation);
        ProjectileLogic pl = pr.GetComponent<ProjectileLogic>();

        if (ability.types.Contains(AbilityDataSet.TYPE_Electricity))
            pl.isChain = true;
        if (ability.types.Contains(AbilityDataSet.TYPE_Fire))
            pl.isDOT = true;
        if (ability.types.Contains(AbilityDataSet.TYPE_Radiation))
        {
            pl.isExplosive = true;
            pl.radius = ability.radiusAOE;
        }

        pl.SetUp(targetTypeOpponent, ability.damage, ability.maxRange, ability.punchThrough);
        pl.stats = stats;
        pl.onHits = onHits;
        pr.GetComponent<Rigidbody>().AddForce(dir * ability.maxRange * 100f);

    }
    private void CastProjectileRolltimedAOE(AbilityTemplate ability, Vector3 target)
    {

        //? find prefab corresponding to projectile
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef proj = orderedAbilityPrefabs[index];
        //? spawn and shoot logic
        Vector3 dir = new Vector3(target.x - transform.position.x, 0f - transform.position.y,
                                 target.z - transform.position.z).normalized;
        Vector3 source = transform.Find("ProjectileAOESource").GetComponent<OverlapConfig>().centerOffset.position;

        if (!Runner.IsServer)
            return;
        NetworkObject pr = Runner.Spawn(proj, source, transform.rotation);
        ProjectileLogic pl = pr.GetComponent<ProjectileLogic>();
        pl.SetUp(targetTypeOpponent, ability.damage, ability.maxRange, ability.punchThrough);
        pl.isRolltimed = true;
        pl.radius = ability.radiusAOE;
        pl.stats = stats;
        pl.onHits = onHits;
        pr.GetComponent<Rigidbody>().AddForce(dir * ability.maxRange * 10f);
    }
    private void CastProjectileAOE(AbilityTemplate ability, Vector3 target)
    {

        //? find prefab corresponding to projectile
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef proj = orderedAbilityPrefabs[index];
        //? spawn and shoot logic
        Vector3 dir = new Vector3(target.x - transform.position.x, 0f - transform.position.y,
                                 target.z - transform.position.z).normalized;
        Vector3 source = transform.Find("ProjectileAOESource").GetComponent<OverlapConfig>().centerOffset.position;

        if (!Runner.IsServer)
            return;
        NetworkObject pr = Runner.Spawn(proj, source, transform.rotation);
        pr.GetComponent<ProjectileLogic>().SetUp(true, ability.radiusAOE, ability.mod_aoeDamage, targetTypeOpponent, targetTypeFriendly,
                                             ability.damage, ability.maxRange, ability.punchThrough);
        pr.GetComponent<ProjectileLogic>().stats = stats;
        pr.GetComponent<ProjectileLogic>().onHits = onHits;
        pr.GetComponent<Rigidbody>().AddForce(dir * ability.maxRange * 40f);
    }

    private void CastProjectileAOEDOT(AbilityTemplate ability, Vector3 target)
    {

        //? find prefab corresponding to projectile
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef proj = orderedAbilityPrefabs[index];
        //? spawn and shoot logic
        Vector3 dir = new Vector3(target.x - transform.position.x, 0f - transform.position.y,
                                 target.z - transform.position.z).normalized;
        Vector3 source = transform.Find("ProjectileAOESource").GetComponent<OverlapConfig>().centerOffset.position;

        if (!Runner.IsServer)
            return;
        NetworkObject pr = Runner.Spawn(proj, source, transform.rotation);
        ProjectileLogic pl = pr.GetComponent<ProjectileLogic>();
        pl.SetUp(targetTypeOpponent, ability.damage, ability.maxRange, ability.punchThrough);
        pl.isAOEDOT = true;
        pl.radius = ability.radiusAOE;
        pl.stats = stats;
        pl.onHits = onHits;
        pr.GetComponent<Rigidbody>().AddForce(dir * ability.maxRange * 40f);
    }

    private bool CastProjectileTarget(AbilityTemplate ability, Vector3 target)
    {
        //? find prefab corresponding to projectile
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef proj = orderedAbilityPrefabs[index];

        GameObject currentMouseTarget = GetClosestEnemy(target);
        if (currentMouseTarget == null)
            return false;

        //? spawn and shoot logic
        Vector3 dir = new Vector3(currentMouseTarget.transform.position.x - transform.position.x,
                                 0f, currentMouseTarget.transform.position.z - transform.position.z).normalized;
        Vector3 source = new Vector3(transform.position.x, 1f, transform.position.z);
        NetworkObject pr = Runner.Spawn(proj, source, transform.rotation);
        ProjectileLogic pl = pr.GetComponent<ProjectileLogic>();
        pl.isTargeted = true;
        pl.targetedObject = currentMouseTarget;

        pl.SetUp(targetTypeOpponent, ability.damage, ability.maxRange, ability.punchThrough);
        pl.stats = stats;
        pl.onHits = onHits;
        pr.GetComponent<Rigidbody>().AddForce(dir * ability.maxRange * 70f);
        return true;
    }
    private bool CastProjectileTargetCount(AbilityTemplate ability, Vector3 target)
    {
        //? find prefab corresponding to projectile
        int index = orderedAbilityNames.IndexOf(ability.abilityName);
        NetworkPrefabRef proj = orderedAbilityPrefabs[index];

        GameObject currentMouseTarget = GetClosestEnemy(target);
        if (currentMouseTarget == null)
            return false;

        isCasting = true;
        int maxCount = ability.count;
        int count = 0;
        StartCoroutine(IterateProjectiles());
        return true;

        IEnumerator IterateProjectiles()
        {
            while (count < maxCount)
            {
                if (currentMouseTarget == null)
                    break;
                //? spawn and shoot logic
                Vector3 dir = new Vector3(currentMouseTarget.transform.position.x - transform.position.x,
                                         0f, currentMouseTarget.transform.position.z - transform.position.z).normalized;
                Vector3 source = new Vector3(transform.position.x, 1f, transform.position.z);
                NetworkObject pr = Runner.Spawn(proj, source, transform.rotation);
                ProjectileLogic pl = pr.GetComponent<ProjectileLogic>();
                pl.isTargeted = true;
                pl.targetedObject = currentMouseTarget;

                pl.SetUp(targetTypeOpponent, ability.damage, ability.maxRange, ability.punchThrough);
                pl.stats = stats;
                pl.onHits = onHits;
                pr.GetComponent<Rigidbody>().AddForce(dir * ability.maxRange * 70f);
                count++;
                yield return new WaitForSeconds(ability.countDelay);
            }
            isCasting = false;
        }
    }

    private GameObject GetClosestEnemy(Vector3 pos)
    {
        mouse = pos;
        GameObject ob = null;

        Collider[] candidates = Physics.OverlapSphere(pos, 2f, LayerMask.GetMask(targetTypeOpponent));
        int closestIndex = 0;
        float dist = 100f;
        for (int i = 0; i < candidates.Length; i++)
        {
            if (Vector3.Distance(pos, candidates[i].transform.position) < dist)
            {
                closestIndex = i;
                dist = Vector3.Distance(pos, candidates[i].transform.position);
            }
        }
        if (dist == 100f)
            return null;
        else
        {
            ob = candidates[closestIndex].gameObject;
            return ob;
        }

    }

    [Rpc]
    public void RPC_PlayVFX(string vfxName)
    {
        transform.Find(vfxName).GetComponent<ParticleSystem>().Play();
        if (tmpSource == null)
        {
            gameObject.AddComponent<AudioSource>();
            tmpSource = GetComponent<AudioSource>();
            tmpSource.volume = 0.3f;
        }
        tmpSource.PlayOneShot(defClip);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
    }
    
    //! Cast projectile notes
    //? Consists of spawning a corresponding projectile and giving it specific force in direction
    //? Projectile prefab will have a projectile script to handle collision
    //? No separate VFX as the projectile prefab is the vfx itself
    //? Options; can either spawn server side with physics, and sync with clients
    //? or can rpc call to spawn on each client but only apply collision on server
    //? in order to avoid using resources.load which is very slow, we can preload projectiles on prefab ref.s
    //* We can setup a list/dictionary etc for referencing in editor
}
