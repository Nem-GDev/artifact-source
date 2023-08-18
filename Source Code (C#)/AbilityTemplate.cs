using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTemplate
{
    public string abilityName;
    public string logicType;
    public string sourceWeapon;
    public List<string> types = new();
    public float damage;
    public float castRange, maxRange;
    public float castTime;
    public float cd, currentcd;
    public float radiusAOE;
    public int punchThrough = 1;
    public int count;
    public float countDelay;
    // ! ---------------------- Mods
    public float modifier_Scale = 1f, mod_projectileSpeed = 1f, mod_aoeDamage = 1f;
    public float mod_cd = 1f, mod_radiusAOE = 1f, mod_maxRange = 1f, mod_damage = 1f;
    // ! ---------------------- Base Value caches
    private float base_cd, base_radiusAOE, base_maxRange, base_damage;

    public static AbilityTemplate CopyTemplate(AbilityTemplate t)
    {
        return new AbilityTemplate()
        {
            abilityName = t.abilityName,
            logicType = t.logicType,
            sourceWeapon = t.sourceWeapon,
            types = t.types,
            damage = t.damage,
            castRange = t.castRange,
            maxRange = t.maxRange,
            castTime = t.castTime,
            cd = t.cd,
            currentcd = t.currentcd,
            radiusAOE = t.radiusAOE,
            punchThrough = t.punchThrough,
            count = t.count,
            countDelay = t.countDelay,

            // ! ---------------------- Mods
            modifier_Scale = t.modifier_Scale,
            mod_projectileSpeed = t.mod_projectileSpeed,
            mod_aoeDamage = t.mod_aoeDamage,

            mod_cd = 1f,
            mod_radiusAOE = 1f,
            mod_maxRange = 1f,
            mod_damage = 1f,
            // ! ---------------------- Base Value caches
            base_cd = t.cd,
            base_radiusAOE = t.radiusAOE,
            base_maxRange = t.maxRange,
            base_damage = t.damage
        };
    }

    public bool ModifyCD(float percent)
    {
        if ((mod_cd + (percent / 100)) < 0.1f){
            mod_cd = 0.1f;
            cd = base_cd * mod_cd;
            return true;
        }

        mod_cd += (percent / 100);
        cd = base_cd * mod_cd;
        return true;
    }
    public bool ModifyRadiusAOE(float percent)
    {
        mod_radiusAOE += (percent / 100);
        radiusAOE = base_radiusAOE * mod_radiusAOE;
        return true;
    }
    public bool ModifyMaxRange(float percent)
    {
        mod_maxRange += (percent / 100);
        maxRange = base_maxRange * mod_maxRange;
        return true;
    }
    public bool ModifyDamage(float percent)
    {
        mod_damage += (percent / 100);
        damage = base_damage * mod_damage;
        return true;
    }
}
