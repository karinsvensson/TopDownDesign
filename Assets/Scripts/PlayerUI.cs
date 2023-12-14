using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : Singleton<PlayerUI>
{
    [SerializeField] Slider healthbar;

    public void UpdateHealthbar(float healthPercentage)
    {
        healthbar.value = healthPercentage;
    }
}
