using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cinemachine;
using TMPro;
using System;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [Networked] public string weapon { get; set; }
    [Networked] public string element { get; set; }
    public List<AbilityTemplate> abilities = new();
    public Animator animator;
    public float speed = 3f;
    public GameObject playerMesh;
    public NetworkPrefabRef spawnVFX;
    AbilityCaster caster;
    CharacterController controller;
    Camera cam;
    Vector3 currentVelocity;
    UnitStats stats;
    EnemySpawner enemySpawner;
    float lastUpgrade = 0f;
    TMP_Text upsCounter, up1Text, up2Text, qCD, eCD;
    GameObject restartButton;
    bool firstUISetup = true;
    AudioSource aC;

    Queue<Upgrade> upgrades = new();
    Upgrade currentUp1, currentUp2;
    GameObject up1r1, up1r2, up1r3, up2r1, up2r2, up2r3;

    int indexAttack, indexAb1, indexAb2;

    public AudioClip upN, upCh;

    private void Awake()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    public override void Spawned()
    {
        base.Spawned();

        caster = GetComponent<AbilityCaster>();
        enemySpawner = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();
        aC = GetComponent<AudioSource>();
        //Debug.Log(GetComponent<UnitStats>().unitType);
        if (Object.HasInputAuthority)
        {
            SetupCamera();
            SetupUI();
        }
        cam = Camera.main;
        stats = GetComponent<UnitStats>();

        //? Setup abilities
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_BowSnipe));
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_BowFocusFire));
        //abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_BowBasicArrow));
        //abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_BowRadiationArrow));
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_BowFireArrow));
        //abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_BowChainArrow));
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_ShieldBash));
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_ShieldExplode));
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_ShieldVortex));
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_MolotovBasicThrow));
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_MolotovDOT));
        abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_MolotovRoll));

        // Partially deprecated code
        switch (weapon)
        {
            case "Bow":
                //abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_BowBasicArrow));
                break;

            case "Shield":
                //abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_ShieldBash));
                break;

            case "Molotov":
                //abilities.Add(AbilityDataSet.GetAbilityByName(AbilityDataSet.ABILITY_MolotovBasicThrow));
                break;

            default:
                Debug.LogError("Unrecognized or undefined weapon name on player");
                break;
        }
        FindAbilities();
    }
    private void SetupCamera()
    {
        //? SETUP VCAM SETTINGS DYNAMICALLY HERE
        CinemachineVirtualCamera vcam = GameObject.FindWithTag("Camera").
            GetComponent<CinemachineVirtualCamera>();

        if (vcam == null)
            Debug.LogWarning("No vcam found");
        vcam.Follow = gameObject.transform;
        CinemachineTransposer fol = vcam.AddCinemachineComponent<CinemachineTransposer>();

        fol.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        fol.m_FollowOffset = new Vector3(0, 10, -5);
        fol.m_YawDamping = 1.5f;
        fol.m_RollDamping = 1.5f;
        fol.m_PitchDamping = 1.5f;
        fol.m_AngularDamping = 1.5f;
    }

    public override void FixedUpdateNetwork()
    {
        //!implement custom controller here
        //! note that previous player prefabs wont work with custom controller
        //? controller movement
        if (Object.HasInputAuthority)
        {
            UpdateUICD();
            if (firstUISetup)
            {
                SetupUpgradeUI();
                SetupWeaponUI();
            }
        }
        if (!stats.isAlive)
        {
            gameObject.SetActive(false);
            restartButton.SetActive(true);
            return;
        }
        ApplyInput();
        for (int i = 0; i < abilities.Count; i++)
        {
            abilities[i].currentcd -= Runner.DeltaTime;
        }

        CheckForUpgrade();

        int upCount = 0;
        upCount += upgrades.Count / 2;
        if (currentUp1 != null)
            upCount++;

        if (upsCounter != null)
        {
            upsCounter.text = upCount.ToString();
        }
    }

    private void ApplyInput()
    {
        if (GetInput(out NetworkInputData input))
        {
            // ? Movement logic & animations
            Vector3 mov = new Vector3(speed * Runner.DeltaTime * input.dir.x, -10f * Runner.DeltaTime,
                                        speed * Runner.DeltaTime * input.dir.z);
            transform.LookAt(new Vector3(input.aim.x, transform.position.y, input.aim.z));
            controller.Move(mov);
            currentVelocity = input.dir;
            //Debug.Log($"Speed: {speed} | Delta: {Runner.DeltaTime} | MOV: {mov}");
            if (animator != null)
            {
                float angle = Vector3.Angle(currentVelocity, transform.forward);
                animator.SetFloat("rZ", Mathf.Cos(angle * Mathf.Deg2Rad) * currentVelocity.magnitude * 2f, 0.1f, Runner.DeltaTime);
                animator.SetFloat("rX", Mathf.Sin(angle * Mathf.Deg2Rad) * currentVelocity.magnitude * 2f, 0.1f, Runner.DeltaTime);
            }

            // ? Attack logic

            if (input.primary && abilities[indexAttack].currentcd <= 0f)
            {
                if (caster.CastAbility(abilities[indexAttack], new Vector3(input.aim.x, transform.position.y, input.aim.z)))
                    abilities[indexAttack].currentcd = abilities[indexAttack].cd;
            }
            else if (input.ability1 && abilities[indexAb1].currentcd <= 0f)
            {
                if (caster.CastAbility(abilities[indexAb1], new Vector3(input.aim.x, transform.position.y, input.aim.z)))
                    abilities[indexAb1].currentcd = abilities[indexAb1].cd;
            }
            else if (input.ability2 && abilities[indexAb2].currentcd <= 0f)
            {
                if (caster.CastAbility(abilities[indexAb2], new Vector3(input.aim.x, transform.position.y, input.aim.z)))
                    abilities[indexAb2].currentcd = abilities[indexAb2].cd;
            }

            if (currentUp1 != null)
            {
                if (input.upgrade1)
                {
                    //Debug.Log("Up1 picked");
                    aC.PlayOneShot(upCh, 0.5f);
                    RPC_Upgrade(currentUp1.rarity, currentUp1.type, currentUp1.value);
                    currentUp1 = null;
                    currentUp2 = null;
                    up1Text.text = "";
                    up2Text.text = "";
                    if (upgrades.Count >= 2)
                    {
                        currentUp1 = upgrades.Dequeue();
                        currentUp2 = upgrades.Dequeue();
                        up1Text.text = "1: " + currentUp1.type + "";
                        up2Text.text = "2: " + currentUp2.type + "";
                    }
                    SetupUpgradeUI();
                }
                else if (input.upgrade2)
                {
                    //Debug.Log("Up2 picked");
                    aC.PlayOneShot(upCh, 0.5f);
                    RPC_Upgrade(currentUp2.rarity, currentUp2.type, currentUp2.value);
                    currentUp1 = null;
                    currentUp2 = null;
                    up1Text.text = "";
                    up2Text.text = "";
                    if (upgrades.Count >= 2)
                    {
                        currentUp1 = upgrades.Dequeue();
                        currentUp2 = upgrades.Dequeue();
                        up1Text.text = "1: " + currentUp1.type + "";
                        up2Text.text = "2: " + currentUp2.type + "";
                    }
                    SetupUpgradeUI();
                }
            }
        }
    }

    public void FindAbilities()
    {
        //? Find corresponding attack and abilities
        indexAttack = -1;
        indexAb1 = -1;
        indexAb2 = -1;
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].sourceWeapon == weapon)
            {
                if (abilities[i].types.Contains(AbilityDataSet.TYPE_BasicAttack))
                    indexAttack = i;
                else if (abilities[i].types.Contains(AbilityDataSet.TYPE_Ability1))
                    indexAb1 = i;
                else if (abilities[i].types.Contains(AbilityDataSet.TYPE_Ability2))
                    indexAb2 = i;
            }
        }
        if (indexAttack == -1)
            Debug.LogError($"Basic attack not found for weapon {weapon} on this player.");
        if (indexAb1 == -1)
            Debug.LogError($"Ability1 not found for weapon {weapon} on this player.");
        if (indexAb2 == -1)
            Debug.LogError($"Ability2 not found for weapon {weapon} on this player.");
    }

    private void UpdateUICD()
    {
        if (!Object.HasInputAuthority)
            return;

            
        if (qCD != null)
        {
            if (abilities[indexAb1].currentcd > 0)
                qCD.text = abilities[indexAb1].currentcd.ToString("0.0");
            else
                qCD.text = " ";

            if (abilities[indexAb2].currentcd > 0)
                eCD.text = abilities[indexAb2].currentcd.ToString("0.0");
            else 
                eCD.text = " ";
        }
    }
    public void CheckForUpgrade()
    {
        //? Locally ran only on input authority client
        if (Object.HasInputAuthority)
        {
            if (enemySpawner.score > lastUpgrade + 1000f)
            {
                lastUpgrade = enemySpawner.score;
                GenerateUpgrades();
            }
        }
    }
    public void GenerateUpgrades()
    {
        //? Upgrade list is generated and saved locally
        //? Once an upgrade is chosen, info is sent to be applied by server(in input section)
        //? UI counter setup
        SetupUpgradeUI();
        aC.PlayOneShot(upN, 1f);

        Upgrade up1 = Upgrade.GetRandom();
        Upgrade up2 = Upgrade.GetRandom();
        //Debug.Log("Generated upgrades:");
        //Debug.Log($"Up1: {up1.type} | {up1.rarity}");
        //Debug.Log($"Up2: {up2.type} | {up2.rarity}");

        if (currentUp1 == null)
        {
            currentUp1 = up1;
            currentUp2 = up2;

            up1Text.text = "1: " + currentUp1.type + "";
            up2Text.text = "2: " + currentUp2.type + "";
            SetupUpgradeUI();
        }
        else
        {
            upgrades.Enqueue(up1);
            upgrades.Enqueue(up2);
        }

    }

    public void SetupWeaponUI()
    {
        if (!Object.HasInputAuthority)
            return;

        GameObject shieldUI = GameObject.Find("ShieldUI");
        GameObject bowUI = GameObject.Find("BowUI");
        GameObject molotovUI = GameObject.Find("MolotovUI");
        restartButton = GameObject.Find("RestartB");

        if (shieldUI == null)
            return;
        else
            firstUISetup = false;

        qCD = GameObject.Find("QCD").GetComponent<TMP_Text>();
        eCD = GameObject.Find("ECD").GetComponent<TMP_Text>();

        if(restartButton != null)
            restartButton.SetActive(false);

        foreach (Image pic in shieldUI.GetComponentsInChildren<Image>())
        {
            pic.enabled = false;
        }
        foreach (Image pic in bowUI.GetComponentsInChildren<Image>())
        {
            pic.enabled = false;
        }
        foreach (Image pic in molotovUI.GetComponentsInChildren<Image>())
        {
            pic.enabled = false;
        }

        switch (weapon)
        {
            case "Bow":
                foreach (Image pic in bowUI.GetComponentsInChildren<Image>())
                {
                    pic.enabled = true;
                }
                break;

            case "Shield":
                foreach (Image pic in shieldUI.GetComponentsInChildren<Image>())
                {
                    pic.enabled = true;
                }
                break;

            case "Molotov":
                foreach (Image pic in molotovUI.GetComponentsInChildren<Image>())
                {
                    pic.enabled = true;
                }
                break;
        }
    }

    private void SetupUpgradeUI()
    {
        if (!Object.HasInputAuthority)
            return;

        try
        {
            if (upsCounter == null)
                upsCounter = GameObject.Find("AvailableUpgrades").GetComponent<TMP_Text>();
        }
        catch
        {
            return;
        }


        if (up1Text == null || up2Text == null)
        {
            up1Text = GameObject.Find("Upgrade1").GetComponent<TMP_Text>();
            up2Text = GameObject.Find("Upgrade2").GetComponent<TMP_Text>();
        }
        if (up1r1 == null)
        {
            up1r1 = GameObject.Find("up1rar1");
            up1r2 = GameObject.Find("up1rar2");
            up1r3 = GameObject.Find("up1rar3");

            up2r1 = GameObject.Find("up2rar1");
            up2r2 = GameObject.Find("up2rar2");
            up2r3 = GameObject.Find("up2rar3");
        }

        up1r1.SetActive(false);
        up1r2.SetActive(false);
        up1r3.SetActive(false);

        up2r1.SetActive(false);
        up2r2.SetActive(false);
        up2r3.SetActive(false);

        if (currentUp1 == null)
            return;

        switch (currentUp1.rarity)
        {
            case "Common":
                up1r1.SetActive(true);
                break;
            case "Uncommon":
                up1r2.SetActive(true);
                break;
            case "Rare":
                up1r3.SetActive(true);
                break;
        }
        switch (currentUp2.rarity)
        {
            case "Common":
                up2r1.SetActive(true);
                break;
            case "Uncommon":
                up2r2.SetActive(true);
                break;
            case "Rare":
                up2r3.SetActive(true);
                break;
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_Upgrade(string rarity, string type, float value)
    {
        //! Implement application of all upgrade types
        switch (type)
        {
            case Upgrade.TYPES_maxHP:
                stats.AddMaxHealth(value);
                break;

            case Upgrade.TYPES_regenHP:
                stats.AddRegen(value);
                break;

            case Upgrade.TYPES_armour:
                stats.AddArmourPercent(value);
                break;

            case Upgrade.TYPES_moveSpeed:
                stats.AddSpeedPercent(value);
                break;

            case Upgrade.TYPES_attackSpeed:
                foreach (AbilityTemplate t in abilities)
                {
                    if (t.types.Contains(AbilityDataSet.TYPE_BasicAttack))
                        t.ModifyCD(-value);
                }
                break;

            case Upgrade.TYPES_aoe:
                foreach (AbilityTemplate t in abilities)
                {
                    t.ModifyRadiusAOE(+value);
                }
                break;

            case Upgrade.TYPES_cd:
                foreach (AbilityTemplate t in abilities)
                {
                    if (t.types.Contains(AbilityDataSet.TYPE_Ability1) ||
                        t.types.Contains(AbilityDataSet.TYPE_Ability2))
                        t.ModifyCD(-value);
                }
                break;

            case Upgrade.TYPES_damageMulti:
                foreach (AbilityTemplate t in abilities)
                {
                    t.ModifyDamage(+value);
                }
                break;

            case Upgrade.TYPES_lifesteal:
                caster.onHits[AbilityDataSet.ONHIT_lifesteal] += (int)value;
                break;

            case Upgrade.TYPES_crit:
                caster.onHits[AbilityDataSet.ONHIT_crit] += (int)value;
                break;

            case Upgrade.TYPES_stun:
                caster.onHits[AbilityDataSet.ONHIT_stun] += (int)value;
                break;

            case Upgrade.TYPES_slow:
                caster.onHits[AbilityDataSet.ONHIT_slow] += (int)value;
                break;

            case Upgrade.TYPES_poison:
                caster.onHits[AbilityDataSet.ONHIT_poison] += (int)value;
                break;
        }
    }

    private void SetupUI()
    {
        //? Setup ability UI as well as score/misc.
    }
}
