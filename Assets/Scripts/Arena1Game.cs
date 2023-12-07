using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Arena1Game : NetworkBehaviour
{
    public Player playerPrefab;
    public Player hostPrefab;
    public Camera arenaCamera;

    private NetworkedPlayers networkedPlayers;

    private int positionIndex = 0;
    private Vector3[] startPositions = new Vector3[] {
        new Vector3(40, 2, 0),
        new Vector3(-40, 2, 0),
        new Vector3(0, 2, 40),
        new Vector3(0, 2, -40)
    };

    private int WrapInt(int curValue, int increment, int max)
    {
        int toReturn = curValue + increment;
        if(toReturn > max)
        {
            toReturn = 0;
        }

        return toReturn;
    }

    // Start is called before the first frame update
    void Start() {
        arenaCamera.enabled = !IsClient;
        arenaCamera.GetComponent<AudioListener>().enabled = !IsClient;

        networkedPlayers = GameObject.Find("NetworkedPlayers").GetComponent<NetworkedPlayers>();
        NetworkHelper.Log($"Players = {networkedPlayers.allNetPlayers.Count}");

        if (IsServer)
        {
            SpawnPlayers();
        }

    }

   

    private Vector3 NextPosition()
    {
        Vector3 pos = startPositions[positionIndex];
        positionIndex += 1;
        if (positionIndex > startPositions.Length - 1)
        {
            positionIndex = 0;
        }

        return pos;
    }

    private void SpawnPlayers() {
        foreach (NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            Player prefab = playerPrefab;

            Player playerSpawn = Instantiate(
                prefab,
                NextPosition(),
                Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(info.clientId);
            //playerSpawn.PlayerColor.Value = NextColor();
        }
    }



}
