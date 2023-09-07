using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class UnitUI : NetworkBehaviour
{
    [SerializeField] UnitStats stats;
    [SerializeField] Slider hpSlider;
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        hpSlider.maxValue = stats.maxHealth;
        hpSlider.value = stats.currentHealth;
    }
}
