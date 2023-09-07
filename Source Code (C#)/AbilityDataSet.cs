using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDataSet : MonoBehaviour
{
    //! New abilities;
    //! 1) Add name 2) Add template 3) Add copy template

    public const string TYPE_PHYSICAL = "Physical",
                TYPE_MAGICAL = "Magical",
                TYPE_RANGED = "Ranged",
                TYPE_MELEE = "Melee",
                TYPE_Electricity = "Electricity",
                TYPE_Radiation = "Radiation",
                TYPE_Fire = "Fire",
                TYPE_BasicAttack = "BasicAttack",
                TYPE_Ability1 = "Ability1",
                TYPE_Ability2 = "Ability2";

    public const string ONHIT_lifesteal = "lifesteal",
                        ONHIT_crit = "crit",
                        ONHIT_stun = "stun",
                        ONHIT_slow = "slow",
                        ONHIT_aoeIgnite = "aoeIgnite",
                        ONHIT_poison = "poison",
                        ONHIT_explode = "explode";

    public const string LOGIC_INSTANT_BOX = "InstantBox",
                 LOGIC_INSTANT_SPHERE = "InstantSphere",
                 LOGIC_INSTANT_SPHERE_SLOW = "InstantSphereSlow",
                 LOGIC_INSTANT_SPHERE_VORTEX = "InstantSphereVortex",
                 LOGIC_PROJECTILE = "Projectile",
                 LOGIC_PROJECTILE_ROLL_TIMED_AOE = "ProjectileRollTimedAOE",
                 LOGIC_PROJECTILE_TARGET = "ProjectileTarget",
                 LOGIC_PROJECTILE_TARGET_COUNT = "ProjectileTargetCount",
                 LOGIC_PROJECTILE_AOE_DOT = "ProjectileAOEDOT",

                 LOGIC_PROJECTILE_AOE = "ProjectileAOE";

    //! Note: Make sure ability VFX prefabs have same name as abilities themselves +suffix.
    //* New ability definition; define name + stats, setup VFX link/finder in awake()
    public const string ABILITY_Cleave = "Cleave",
                ABILITY_MagicArrow = "MagicArrow",
                ABILITY_BowBasicArrow = "BowBasicArrow",
                ABILITY_BowRadiationArrow = "BowRadiationArrow",
                ABILITY_BowFireArrow = "BowFireArrow",
                ABILITY_BowChainArrow = "BowChainArrow",
                ABILITY_ShieldBash = "ShieldBash",
                ABILITY_ShieldExplode = "ShieldExplode",
                ABILITY_ShieldVortex = "ShieldVortex",
                ABILITY_MolotovBasicThrow = "MolotovBasicThrow",
                ABILITY_MolotovDOT = "MolotovDOT",
                ABILITY_MolotovRoll = "MolotovRoll",
                ABILITY_EnemyMolotovRoll = "EnemyMolotovRoll",
                ABILITY_BowFocusFire = "BowFocusFire",
                ABILITY_EnemyBowFocusFire = "EnemyBowFocusFire",
                ABILITY_BowSnipe = "BowSnipe";


    public static AbilityTemplate GetAbilityByName(string n)
    {
        // // Warning: This method currently returns a static template,
        // // TODO Implement an instantiantiation of a copy of specified template
        switch (n)
        {
            case ABILITY_Cleave:
                return AbilityTemplate.CopyTemplate(Cleave);

            case ABILITY_MagicArrow:
                return AbilityTemplate.CopyTemplate(MagicArrow);

            case ABILITY_BowBasicArrow:
                return AbilityTemplate.CopyTemplate(BowBasicArrow);

            case ABILITY_ShieldBash:
                return AbilityTemplate.CopyTemplate(ShieldBash);

            case ABILITY_MolotovBasicThrow:
                return AbilityTemplate.CopyTemplate(MolotovBasicThrow);

            case ABILITY_BowRadiationArrow:
                return AbilityTemplate.CopyTemplate(BowRadiationArrow);

            case ABILITY_BowFireArrow:
                return AbilityTemplate.CopyTemplate(BowFireArrow);

            case ABILITY_BowChainArrow:
                return AbilityTemplate.CopyTemplate(BowChainArrow);

            case ABILITY_BowFocusFire:
                return AbilityTemplate.CopyTemplate(BowFocusFire);

            case ABILITY_EnemyBowFocusFire:
                return AbilityTemplate.CopyTemplate(EnemyBowFocusFire);

            case ABILITY_BowSnipe:
                return AbilityTemplate.CopyTemplate(BowSnipe);

            case ABILITY_ShieldExplode:
                return AbilityTemplate.CopyTemplate(ShieldExplode);

            case ABILITY_ShieldVortex:
                return AbilityTemplate.CopyTemplate(ShieldVortex);

            case ABILITY_MolotovDOT:
                return AbilityTemplate.CopyTemplate(MolotovDOT);

            case ABILITY_MolotovRoll:
                return AbilityTemplate.CopyTemplate(MolotovRoll);

            case ABILITY_EnemyMolotovRoll:
                return AbilityTemplate.CopyTemplate(EnemyMolotovRoll);

            default:
                Debug.LogError("Invalid ability to copy");
                return null;
        }
    }
    public static AbilityTemplate Cleave = new()
    {
        abilityName = ABILITY_Cleave,
        logicType = LOGIC_INSTANT_BOX,
        damage = 300f,
        types = new() { TYPE_MELEE, TYPE_PHYSICAL },
        castRange = 2.5f,
        cd = 2f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f
    };

    public static AbilityTemplate MagicArrow = new()
    {
        abilityName = ABILITY_MagicArrow,
        logicType = LOGIC_PROJECTILE,
        damage = 200f,
        types = new() { TYPE_RANGED, TYPE_MAGICAL },
        castRange = 15f,
        maxRange = 20f,
        cd = 3f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 0
    };

    public static AbilityTemplate BowBasicArrow = new()
    {
        abilityName = ABILITY_BowBasicArrow,
        logicType = LOGIC_PROJECTILE,
        damage = 250f,
        radiusAOE = 5f,
        sourceWeapon = "Bow",
        types = new() { TYPE_RANGED, TYPE_PHYSICAL, TYPE_BasicAttack },
        maxRange = 20f,
        cd = 1.5f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 1
    };

    public static AbilityTemplate BowSnipe = new()
    {
        abilityName = ABILITY_BowSnipe,
        logicType = LOGIC_PROJECTILE_TARGET,
        damage = 700f,
        sourceWeapon = "Bow",
        types = new() { TYPE_RANGED, TYPE_PHYSICAL, TYPE_Ability1 },
        maxRange = 20f,
        cd = 6f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 1
    };
    public static AbilityTemplate BowFocusFire = new()
    {
        abilityName = ABILITY_BowFocusFire,
        logicType = LOGIC_PROJECTILE_TARGET_COUNT,
        damage = 150f,
        count = 10,
        countDelay = 0.1f,
        sourceWeapon = "Bow",
        types = new() { TYPE_RANGED, TYPE_PHYSICAL, TYPE_Ability2 },
        maxRange = 20f,
        cd = 12f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 1
    };

    public static AbilityTemplate EnemyBowFocusFire = new()
    {
        abilityName = ABILITY_EnemyBowFocusFire,
        logicType = LOGIC_PROJECTILE_TARGET_COUNT,
        damage = 80f,
        count = 4,
        countDelay = 0.1f,
        sourceWeapon = "Bow",
        types = new() { TYPE_RANGED, TYPE_PHYSICAL, TYPE_Ability2 },
        castRange = 15f,
        maxRange = 20f,
        cd = 5f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 1
    };

    public static AbilityTemplate BowRadiationArrow = new()
    {
        abilityName = ABILITY_BowRadiationArrow,
        logicType = LOGIC_PROJECTILE_AOE,
        damage = 250f,
        radiusAOE = 5f,
        sourceWeapon = "Bow",
        types = new() { TYPE_Radiation, TYPE_BasicAttack },
        maxRange = 50f,
        cd = 1f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        mod_aoeDamage = .3f,
        punchThrough = 1
    };
    public static AbilityTemplate BowFireArrow = new()
    {
        abilityName = ABILITY_BowFireArrow,
        logicType = LOGIC_PROJECTILE,
        damage = 250f,
        radiusAOE = 5f,
        sourceWeapon = "Bow",
        types = new() { TYPE_Fire, TYPE_BasicAttack },
        maxRange = 20f,
        cd = 1.5f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 1
    };
    public static AbilityTemplate BowChainArrow = new()
    {
        abilityName = ABILITY_BowChainArrow,
        logicType = LOGIC_PROJECTILE,
        damage = 250f,
        radiusAOE = 5f,
        sourceWeapon = "Bow",
        types = new() { TYPE_Electricity, TYPE_BasicAttack },
        maxRange = 20f,
        cd = 1f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 4
    };

    public static AbilityTemplate ShieldBash = new()
    {
        abilityName = ABILITY_ShieldBash,
        logicType = LOGIC_INSTANT_SPHERE,
        damage = 100f,
        sourceWeapon = "Shield",
        types = new() { TYPE_MELEE, TYPE_PHYSICAL, TYPE_BasicAttack },
        radiusAOE = 5f,
        cd = 1.5f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 1
    };

    public static AbilityTemplate ShieldExplode = new()
    {
        abilityName = ABILITY_ShieldExplode,
        logicType = LOGIC_INSTANT_SPHERE_SLOW,
        damage = 260f,
        sourceWeapon = "Shield",
        types = new() { TYPE_MELEE, TYPE_PHYSICAL, TYPE_Ability1 },
        radiusAOE = 6f,
        cd = 6f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 1
    };
    public static AbilityTemplate ShieldVortex = new()
    {
        abilityName = ABILITY_ShieldVortex,
        logicType = LOGIC_INSTANT_SPHERE_VORTEX,
        damage = 400f,
        sourceWeapon = "Shield",
        types = new() { TYPE_MELEE, TYPE_PHYSICAL, TYPE_Ability2 },
        radiusAOE = 7f,
        cd = 12f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        punchThrough = 1
    };

    public static AbilityTemplate MolotovBasicThrow = new()
    {
        abilityName = ABILITY_MolotovBasicThrow,
        logicType = LOGIC_PROJECTILE_AOE,
        damage = 150f,
        sourceWeapon = "Molotov",
        types = new() { TYPE_RANGED, TYPE_MAGICAL, TYPE_BasicAttack },
        maxRange = 15f,
        cd = 1.5f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        mod_aoeDamage = 1f,
        radiusAOE = 4f,
        punchThrough = 1
    };
    public static AbilityTemplate MolotovDOT = new()
    {
        abilityName = ABILITY_MolotovDOT,
        logicType = LOGIC_PROJECTILE_AOE_DOT,
        damage = 400f,
        sourceWeapon = "Molotov",
        types = new() { TYPE_RANGED, TYPE_MAGICAL, TYPE_Ability1 },
        maxRange = 15f,
        cd = 6f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        mod_aoeDamage = 1f,
        radiusAOE = 8f,
        punchThrough = 1
    };
    public static AbilityTemplate MolotovRoll = new()
    {
        abilityName = ABILITY_MolotovRoll,
        logicType = LOGIC_PROJECTILE_ROLL_TIMED_AOE,
        damage = 480f,
        sourceWeapon = "Molotov",
        types = new() { TYPE_RANGED, TYPE_MAGICAL, TYPE_Ability2 },
        castRange = 10f,
        maxRange = 15f,
        cd = 12f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        mod_aoeDamage = 1f,
        radiusAOE = 8f,
        punchThrough = 1
    };

    public static AbilityTemplate EnemyMolotovRoll = new()
    {
        abilityName = ABILITY_EnemyMolotovRoll,
        logicType = LOGIC_PROJECTILE_ROLL_TIMED_AOE,
        damage = 300f,
        sourceWeapon = "Molotov",
        types = new() { TYPE_RANGED, TYPE_MAGICAL, TYPE_Ability2 },
        castRange = 8f,
        maxRange = 30f,
        cd = 5f,
        currentcd = 0f,
        modifier_Scale = 1f,
        mod_projectileSpeed = 1f,
        mod_aoeDamage = 1f,
        radiusAOE = 6f,
        punchThrough = 1
    };
}
