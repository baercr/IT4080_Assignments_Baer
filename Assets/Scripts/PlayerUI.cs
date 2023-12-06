using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    public Slider healthUIWS;

    public Slider healthUISP;

    //public GameObject scoreCard;

    private void OnEnable()
    {
        GetComponent <Player>().playerHP.OnValueChanged += HealthChanged;
    }

    private void OnDisable()
    {
        GetComponent<Player>().playerHP.OnValueChanged -= HealthChanged;
    }

    private void HealthChanged(int previousValue, int newValue)
    {
        if (newValue / 100f > 1)
        {
            healthUIWS.value = 1;
            healthUISP.value = 1;
        }
        else
        {
            healthUIWS.value = newValue / 100f;
            healthUISP.value = newValue / 100f;
        }
    }
}
