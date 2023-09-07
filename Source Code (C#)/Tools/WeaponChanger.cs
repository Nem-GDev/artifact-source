using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponChanger : NetworkBehaviour
{
    public string weapon;
    public string element;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!Runner.IsServer)
            return;

        if (other.gameObject.GetComponent<UnitStats>().unitType == "Player")
        {
            switch (weapon)
            {
                case "Bow":
                    other.gameObject.GetComponent<PlayerController>().weapon = "Bow";
                    other.gameObject.GetComponent<UnitStats>().armourPercent = 0f;
                    break;
                case "Shield":
                    other.gameObject.GetComponent<PlayerController>().weapon = "Shield";
                    other.gameObject.GetComponent<UnitStats>().armourPercent = 30f;
                    break;
                case "Molotov":
                    other.gameObject.GetComponent<PlayerController>().weapon = "Molotov";
                    other.gameObject.GetComponent<UnitStats>().armourPercent = 0f;
                    break;
            }
            other.gameObject.GetComponent<PlayerController>().FindAbilities();
            other.gameObject.GetComponent<PlayerController>().SetupWeaponUI();
        }
    }
}
