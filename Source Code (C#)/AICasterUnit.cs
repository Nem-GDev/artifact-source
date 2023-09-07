using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

public class AICasterUnit : NetworkBehaviour
{
    public float aiPollRate = 0.2f;
    public float playerDetectionRadius = 30f;
    public Animator animator;
    public List<string> abilityNames = new();
    public List<AbilityTemplate> abilities = new();
    public bool isPaused = false;

    TickTimer pollTimer;
    List<int> notOnCDIndices = new List<int>();
    int nextAbilityIndex;
    bool isCasting = false;
    NavMeshAgent agent;
    GameObject currentTarget;
    float currentDistanceToTarget, smallestAbilityRange;
    AbilityCaster caster;

    public override void Spawned()
    {
        base.Spawned();
        if (!Runner.IsServer)
            GetComponent<NetworkTransform>().InterpolationDataSource = InterpolationDataSources.Auto;


        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("No NavMeshAgent");

        foreach (string s in abilityNames)
            abilities.Add(AbilityDataSet.GetAbilityByName(s));

        if (abilities == null || abilities.Count == 0)
            Debug.LogError("No abilities added for this unit");

        smallestAbilityRange = abilities[0].castRange;
        foreach (AbilityTemplate ab in abilities)
        {
            if (ab.castRange < smallestAbilityRange)
                smallestAbilityRange = ab.castRange;
        }
        agent.stoppingDistance = smallestAbilityRange - 1f;
        caster = GetComponent<AbilityCaster>();
        //Debug.Log(abilities[0].abilityName);
        gameObject.GetComponent<UnitStats>().unitType = "Enemy";


        //Debug.Log("Spawned");
        agent.enabled = false;
    }
    public override void FixedUpdateNetwork()
    {
        //? Only run ai logic on server
        if (!Runner.IsServer)
            return;

        agent.enabled = true;

        if (isPaused)
        {
            agent.isStopped = true;
            animator.SetFloat("velocity", 0f);
            return;
        }
        else
            agent.isStopped = false;
        //? Update animator properties
        animator.SetFloat("velocity", agent.velocity.magnitude);

        //? Currently insta rotating to target permanently
        if (currentTarget != null)
            RotateTowardsTarget();
        //Debug.Log("Has target ");
        //? Further code only runs on the AIPollrate frequency;
        if (pollTimer.ExpiredOrNotRunning(Runner))
            pollTimer = TickTimer.CreateFromSeconds(Runner, aiPollRate);
        else
            return;

        if (isCasting)
            return;

        //! 1st; Update current target, path, distance etc. (if no target, dont continue)
        if (UpdatePathingTarget() == false)
            return;

        //! 2nd; Update ability cds, stats etc., order by next cast desirability
        UpdateAbilityPool();

        //! 3rd; Check whether in range of next cast, if there is an available next cast, cast it
        if (nextAbilityIndex != -1)
            if (currentDistanceToTarget < abilities[nextAbilityIndex].castRange)
                CastNextAbility();
    }

    private void CastNextAbility()
    {
        animator.SetTrigger("attack");
        abilities[nextAbilityIndex].currentcd = abilities[nextAbilityIndex].cd;
        notOnCDIndices.Remove(nextAbilityIndex);
        caster.CastAbility(abilities[nextAbilityIndex], currentTarget.transform.position);
    }
    private void RotateTowardsTarget()
    {
        var turnTo = currentTarget.transform.position;
        Vector3 direction = (turnTo - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = lookRotation;
    }
    private void UpdateAbilityPool()
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            //? reduce cooldowns
            abilities[i].currentcd -= aiPollRate;
            //? add abilities not on cooldown to list if not on list already
            if (abilities[i].currentcd < 0 && !notOnCDIndices.Contains(i))
                notOnCDIndices.Add(i);
        }

        //? find next highest range ability off cd
        nextAbilityIndex = -1;
        float highestRange = 0;
        for (int i = 0; i < notOnCDIndices.Count; i++)
        {
            if (abilities[notOnCDIndices[i]].castRange > highestRange)
            {
                nextAbilityIndex = notOnCDIndices[i];
                highestRange = abilities[notOnCDIndices[i]].castRange;
            }
        }
    }

    private bool UpdatePathingTarget()
    {
        Collider[] nearbyPlayers = Physics.OverlapSphere(transform.position,
                                    playerDetectionRadius, LayerMask.GetMask("Player"));

        if (nearbyPlayers.Length == 0 || nearbyPlayers == null)
            return false;

        currentTarget = nearbyPlayers[0].gameObject;
        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        //? find closest player
        foreach (Collider pl in nearbyPlayers)
        {
            float newDist = Vector3.Distance(transform.position, pl.transform.position);
            if (newDist < dist)
            {
                currentTarget = pl.gameObject;
                dist = newDist;
            }
        }
        currentDistanceToTarget = dist;
        agent.SetDestination(currentTarget.transform.position);
        return true;
    }
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(currentTarget.transform.position + new Vector3(0, 3, 0), 0.3f);
    }
}
