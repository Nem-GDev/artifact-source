using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class UnitStats : NetworkBehaviour
{
    [Networked] public string unitType { get; set; }
    [Networked] public float maxHealth { get; set; }
    [Networked] public float currentHealth { get; set; }
    [Networked] public bool isAlive { get; set; } = true;
    [Networked] public float armourCapPercent { get; set; } = 70f;

    [Networked] private float currentDOTPool { get; set; }
    [Networked] private float currentDOTAmount { get; set; }
    [Networked] private bool isStunned { get; set; } = false;
    [Networked] private bool isSlowedTimed { get; set; } = false;
    [Networked] public float armourPercent { get; set; } = 0f;
    [Networked] private float healthRegenPerSec { get; set; } = 0f;
    [Networked] private float moveSpeedMulti { get; set; } = 1f;

    public NetworkPrefabRef deathEffect;
    TickTimer dotTimer, stunTimer, slowTimer, regenTimer;
    float baseSpeed, speedBeforeSlowTimed;


    public override void Spawned()
    {
        base.Spawned();
        if (unitType == "Enemy")
        {
            baseSpeed = GetComponent<NavMeshAgent>().speed;
        }
        else if (unitType == "Player")
        {
            baseSpeed = GetComponent<PlayerController>().speed;
        }
    }

    public void Heal(float amount)
    {
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        if (amount + currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += amount;
        }
    }
    public void TakeDamage(float dam, UnitStats source, Dictionary<string, int> onHits)
    {
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        //* On hits -----------------
        if (onHits[AbilityDataSet.ONHIT_aoeIgnite] > 0)
        {
            if (UnityEngine.Random.Range(0, 101) <= onHits[AbilityDataSet.ONHIT_aoeIgnite])
            {
                //TODO: 
            }
        }
        if (onHits[AbilityDataSet.ONHIT_explode] > 0)
        {
            if (UnityEngine.Random.Range(0, 101) <= onHits[AbilityDataSet.ONHIT_explode])
            {
                //TODO: 
            }
        }
        if (onHits[AbilityDataSet.ONHIT_crit] > 0)
        {
            if (UnityEngine.Random.Range(0, 101) <= onHits[AbilityDataSet.ONHIT_crit] ||
                onHits[AbilityDataSet.ONHIT_crit] >= 100)
            {
                currentHealth -= (dam * (1 - (armourPercent / 100)));
            }
        }
        if (onHits[AbilityDataSet.ONHIT_lifesteal] > 0)
        {
            source.Heal(0.5f * onHits[AbilityDataSet.ONHIT_lifesteal]/100 * (dam * (1 - (armourPercent / 100))));
        }
        if (onHits[AbilityDataSet.ONHIT_poison] > 0 )
        {
            if (UnityEngine.Random.Range(0, 101) <= onHits[AbilityDataSet.ONHIT_poison] ||
                onHits[AbilityDataSet.ONHIT_poison] >= 100)
            {
                TakeTotalDot(3f * (dam * (1 - (armourPercent / 100))));
            }
        }
        if (onHits[AbilityDataSet.ONHIT_slow] > 0)
        {
            if (UnityEngine.Random.Range(0, 101) <= onHits[AbilityDataSet.ONHIT_slow] ||
                onHits[AbilityDataSet.ONHIT_slow] >= 100)
            {
                TakeSlowTimed(40f, 1.5f);
            }
        }
        if (onHits[AbilityDataSet.ONHIT_stun] > 0)
        {
            if (UnityEngine.Random.Range(0, 101) <= onHits[AbilityDataSet.ONHIT_stun] ||
                onHits[AbilityDataSet.ONHIT_stun] >= 100)
            {
                TakeStun(1f);
            }
        }

        // * Damage ----------------
        currentHealth -= (dam * (1 - (armourPercent / 100)));
        CheckDeath();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        //? Applying dot
        if (dotTimer.ExpiredOrNotRunning(Runner) && currentDOTPool > 0 && currentDOTAmount > 0)
        {
            // ! Dots always last 4sec with 8 intervals and can stack but always end within 4sec
            dotTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
            currentHealth -= currentDOTAmount;
            currentDOTPool -= currentDOTAmount;
        }

        //? Applying regen
        if (regenTimer.ExpiredOrNotRunning(Runner))
        {
            regenTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
            if (currentHealth < maxHealth)
            {
                currentHealth += healthRegenPerSec / 2;
            }
        }

        //? updating stun
        if (stunTimer.ExpiredOrNotRunning(Runner) && isStunned)
            if (unitType == "Enemy")
            {
                isStunned = false;
                GetComponent<AICasterUnit>().isPaused = false;
            }
            else if (unitType == "Player")
            {

            }

        // ? updating slow
        if (slowTimer.ExpiredOrNotRunning(Runner) && isSlowedTimed)
        {
            isSlowedTimed = false;
            moveSpeedMulti = speedBeforeSlowTimed;
            UpdateSpeed();
        }

        //! MAke sure o check death at the end of tick to avoid destroying obj & null ref.
        CheckDeath();
    }
    public void TakeTotalDot(float dot)
    {
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        currentDOTPool += dot;
        currentDOTAmount = currentDOTPool / 8;
    }

    public void TakeStun(float duration)
    {
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        if (unitType == "Enemy")
        {
            if (stunTimer.ExpiredOrNotRunning(Runner))
            {
                isStunned = true;
                GetComponent<AICasterUnit>().isPaused = true;
                stunTimer = TickTimer.CreateFromSeconds(Runner, duration);
            }
        }
        else if (unitType == "Player")
        {
            // TODO: Implement
        }
    }

    public void TakeSlowTimed(float percent, float duration)
    {
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        if (slowTimer.ExpiredOrNotRunning(Runner))
        {
            isSlowedTimed = true;
            speedBeforeSlowTimed = moveSpeedMulti;
            moveSpeedMulti -= (percent / 100);
            slowTimer = TickTimer.CreateFromSeconds(Runner, duration);
            UpdateSpeed();
        }
    }

    public void AddMaxHealth(float value)
    {
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        maxHealth += value;
        currentHealth += value;
    }

    public void AddRegen(float perSec)
    {
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        healthRegenPerSec += perSec;
    }

    public void AddArmourPercent(float percent)
    {
        if (Object == null)
            return;
        if (!Runner.IsServer)
            return;

        if ((armourPercent + percent) < armourCapPercent)
            armourPercent += percent;
    }

    public void AddSpeedPercent(float percent)
    {
        moveSpeedMulti += (percent / 100f);
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        if (unitType == "Enemy")
            GetComponent<NavMeshAgent>().speed = baseSpeed * moveSpeedMulti;

        else if (unitType == "Player")
            GetComponent<PlayerController>().speed = baseSpeed * moveSpeedMulti;
    }

    public void Init(float hp, string unitT)
    {
        isAlive = true;
        maxHealth = hp;
        currentHealth = hp;
        unitType = unitT;
    }

    public void CheckDeath()
    {
        if (currentHealth < 0)
        {
            Runner.Spawn(deathEffect, new Vector3(transform.position.x,
                        transform.position.y + 1f, transform.position.z));
            if (unitType == "Enemy")
            {
                isAlive = false;
                Runner.Despawn(Object);
            }
            else if (unitType == "Player")
            {
                isAlive = false;
            }
        }
    }
}
