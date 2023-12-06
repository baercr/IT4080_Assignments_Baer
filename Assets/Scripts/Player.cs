using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);

    public NetworkVariable<int> ScoreNetVar = new NetworkVariable<int>(0);

    public NetworkVariable<int> playerHP = new NetworkVariable<int>();

    public BulletSpawner bulletSpawner;

    public float movementSpeed = 50f;
    public float rotationSpeed = 130f;
    
    private Camera playerCamera;
    private GameObject playerBody;
    // private GameObject playerHead;

    private void NetworkInit() {
        playerBody = transform.Find("PlayerBody").gameObject;

        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;

        ApplyPlayerColor();
        PlayerColor.OnValueChanged += OnPlayerColorChanged;

        if (IsClient) {
            ScoreNetVar.OnValueChanged += ClientOnScoreValueChanged;
        }
    }

    private void Awake()
    {
        NetworkHelper.Log(this, "Awake");
    }

    void Start() {
        NetworkHelper.Log(this, "Start");
    }

    public override void OnNetworkSpawn()
    {
        NetworkHelper.Log(this, "OnNetworkSpawn");
        NetworkInit();
        base.OnNetworkSpawn();
    }

    private void ClientOnScoreValueChanged(int old, int current) {
        NetworkHelper.Log(this, $"My score is {ScoreNetVar.Value}");
    }

    private void OnCollisionEnter(Collision collision) {
        if (IsServer) {
            ServerHandleCollision(collision);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(!IsServer)
        {
            return;
        }
        if (other.GetComponent<BulletScript>())
        {
            Debug.Log("Player dmg " + other.GetComponent<NetworkObject>().OwnerClientId);

            //NetworkManager.Singleton.ConnectedClients[other.GetComponent<NetworkObject>().OwnerClientId].PlayerObject.GetComponent<NetworkPlayerData>().score.Value += 1;
                playerHP.Value -= 10;
        }
        else if (other.CompareTag("power_up"))
        {
            other.GetComponent<BasePowerUp>().ServerPickup(this);
        }

        /*if (IsServer) {
            if (other.CompareTag("power_up"))
            {
                other.GetComponent<BasePowerUp>().ServerPickup(this);
            }
        } */
    }

    private void ServerHandleCollision(Collision collision) {
        if (collision.gameObject.CompareTag("bullet")) {
            ulong ownerId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            NetworkHelper.Log(this,
                $"Hit by {collision.gameObject.name} " +
                $"owned by {ownerId}");
            Player other = NetworkManager.Singleton.ConnectedClients[ownerId].PlayerObject.GetComponent<Player>();
            other.ScoreNetVar.Value += 1;
            Destroy(collision.gameObject);
        }
    }

    private void Update() {
        if (IsOwner) {
            OwnerHandleInput();

            if (Input.GetButtonDown("Fire1")) {
                NetworkHelper.Log("Requesting Fire");
                bulletSpawner.FireServerRpc();
            }
        }
    }

    private void OwnerHandleInput() {
        Vector3 movement = CalcMovement();
        Vector3 rotation = CalcRotation();
        if (movement != Vector3.zero || rotation != Vector3.zero)
        {
            MoveServerRpc(CalcMovement(), CalcRotation());
        }
    }

    private void ApplyPlayerColor() {
        NetworkHelper.Log(this, $"Applying color {PlayerColor.Value}");
        playerBody.GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
    }

    private void OnPlayerColorChanged(Color previous, Color current) 
    {
        ApplyPlayerColor();
    }

    [ServerRpc(RequireOwnership = true)]
    private void MoveServerRpc(Vector3 movement, Vector3 rotation) {        
        transform.Translate(movement);
        transform.Rotate(rotation);
    }

    // Rotate around the y axis when shift is not pressed
    private Vector3 CalcRotation() {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Vector3 rotVect = Vector3.zero;
        if (!isShiftKeyDown)
        {
            rotVect = new Vector3(0, Input.GetAxis("Horizontal"), 0);
            rotVect *= rotationSpeed * Time.deltaTime;
        }
        return rotVect;
    }


    // Move up and back, and strafe when shift is pressed
    private Vector3 CalcMovement() {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown)
        {
            x_move = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed * Time.deltaTime;

        return moveVect;
    }

}