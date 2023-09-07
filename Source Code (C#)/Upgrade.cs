using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Upgrade
{
    public const string TYPES_maxHP = "Max HP",
                        TYPES_regenHP = "Regen HP",
                        TYPES_armour = "Armour",
                        TYPES_moveSpeed = "Move Speed",
                        TYPES_attackSpeed = "Attack Speed",
                        TYPES_aoe = "Aoe",
                        TYPES_cd = "CD",
                        TYPES_damageMulti = "Damage Multi",
                        TYPES_lifesteal = "Lifesteal",
                        TYPES_crit = "Crit",
                        TYPES_stun = "Stun",
                        TYPES_slow = "Slow",
                        TYPES_poison = "Poison";

    public static readonly string[] typeList = {
        TYPES_maxHP,
        TYPES_regenHP,
        TYPES_armour,
        TYPES_moveSpeed,
        TYPES_attackSpeed,
        TYPES_aoe,
        TYPES_cd,
        TYPES_damageMulti,
        TYPES_lifesteal,
        TYPES_crit,
        TYPES_stun,
        TYPES_slow,
        TYPES_poison
    };

    public string rarity;
    public string type;
    public float value;

    public Upgrade(string rarity, string type, float value)
    {
        this.rarity = rarity;
        this.type = type;
        this.value = value;
    }

    public static Upgrade GetRandom()
    {
        Upgrade r = new Upgrade("", typeList[Random.Range(0, typeList.Length)], 0f);

        int rar = Random.Range(1, 101);
        if (rar <= 15)
            r.rarity = "Rare";
        else if (rar <= 40)
            r.rarity = "Uncommon";
        else
            r.rarity = "Common";

        switch (r.type)
        {
            case TYPES_maxHP:
                if (r.rarity == "Common")
                    r.value = 150f;
                else if (r.rarity == "Uncommon")
                    r.value = 300f;
                else if (r.rarity == "Rare")
                    r.value = 500f;
                break;

            case TYPES_regenHP:
                if (r.rarity == "Common")
                    r.value = 10f;
                else if (r.rarity == "Uncommon")
                    r.value = 25f;
                else if (r.rarity == "Rare")
                    r.value = 40f;
                break;

            case TYPES_armour:
            if (r.rarity == "Common")
                    r.value = 3f;
                else if (r.rarity == "Uncommon")
                    r.value = 6f;
                else if (r.rarity == "Rare")
                    r.value = 10f;
                break;

            case TYPES_moveSpeed:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;

            case TYPES_attackSpeed:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;

            case TYPES_aoe:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;

            case TYPES_cd:
            if (r.rarity == "Common")
                    r.value = 4f;
                else if (r.rarity == "Uncommon")
                    r.value = 8f;
                else if (r.rarity == "Rare")
                    r.value = 12f;
                break;

            case TYPES_damageMulti:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;

            case TYPES_lifesteal:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;

            case TYPES_crit:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;

            case TYPES_stun:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;

            case TYPES_slow:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;

            case TYPES_poison:
            if (r.rarity == "Common")
                    r.value = 5f;
                else if (r.rarity == "Uncommon")
                    r.value = 10f;
                else if (r.rarity == "Rare")
                    r.value = 15f;
                break;
        }

        return r;
    }
}