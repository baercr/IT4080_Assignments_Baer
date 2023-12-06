using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class NetworkPlayerScore : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text playerName;
    [SerializeField]
    TMPro.TMP_Text scoreUI;
    [SerializeField]
    TMPro.TMP_Text playerNum;

    public void TrackPlayer(GameObject player)
    {
        player.GetComponent<NetworkPlayerData>().playerName.OnValueChanged += OnNameChanged;
        player.GetComponent<NetworkPlayerData>().score.OnValueChanged += OnScoreChanged;
        player.GetComponent<NetworkPlayerData>().playerNumber.OnValueChanged += OnplayerNumChanged;
        OnScoreChanged(0, player.GetComponent<NetworkPlayerData>().score.Value);
        OnNameChanged("", player.GetComponent<NetworkPlayerData>().playerName.Value);
        OnplayerNumChanged(0, player.GetComponent<NetworkPlayerData>().playerNumber.Value);
    }

    public void OnplayerNumChanged(ulong previousValue, ulong newValue)
    {
        playerNum.text = newValue.ToString();
    }

    public void OnNameChanged(FixedString128Bytes previousValue, FixedString128Bytes newValue)
    {
        playerName.text = newValue.ToString();
    }

    public void OnScoreChanged(int previousValue, int newValue)
    {
        scoreUI.text = newValue.ToString();
    }
}
