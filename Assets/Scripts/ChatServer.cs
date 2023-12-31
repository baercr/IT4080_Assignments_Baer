using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;



public class ChatServer : NetworkBehaviour
{
    public ChatUi chatUi;
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];

    void Start() {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            if (IsHost) 
            {
                DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.LocalClientId}");
            } else {
                DisplayMessageLocally(SYSTEM_ID, "You are the server");
            }
            } else {
                DisplayMessageLocally(SYSTEM_ID, $"You are client {NetworkManager.LocalClientId}");
        }

        NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        // Changed the greeting message from the host so it is not a whisper from the host,
        // but just a message sent directly to the connected client.
        ServerSendWelcomeMessage(
            $"I see you: ({clientId}), have connected to the server, well done.",
            SYSTEM_ID,
            clientId);


        // Send a message to all clients when a client connects.
        ReceiveChatMessageClientRpc(
            $"Player {clientId} has connected to the server.",
            NetworkManager.LocalClientId);
    }


    // Send a message to all clients when a client disconnects.
    private void ServerOnClientDisconnected(ulong clientId) 
    {
        ReceiveChatMessageClientRpc(
            $"Player {clientId} has disconnected from the server.",
            NetworkManager.LocalClientId);
    }

    private void DisplayMessageLocally(ulong from, string message) {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;

        if (from == NetworkManager.LocalClientId)
        {
            fromStr = "you";
            textColor = Color.magenta;
        } else if(from == SYSTEM_ID)  {
            fromStr = "SYS";
            textColor = Color.green;
        }

        chatUi.addEntry(fromStr, message, textColor);
    }

    private void OnChatUiMessageEntered(string message)
    {
        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default) 
    {
        if (message.StartsWith("@")) {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            ulong toClientId = ulong.Parse(clientIdStr);

            ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);   
        } else {
            ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }

    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default) 
    {
        DisplayMessageLocally(from, message);
    }


    private void ServerSendDirectMessage(string message, ulong from, ulong to) {
        dmClientIds[0] = from;
        dmClientIds[1] = to;

        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;

        /*clientIds[0] = from;
        ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
        clientIds[0] = to;*/

        // if (true) { };

        ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
    }

    private void ServerSendWelcomeMessage(string message, ulong from, ulong to)
    {
        dmClientIds[0] = from;
        dmClientIds[1] = to;

        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;

        ReceiveChatMessageClientRpc($"{message}", from, rpcParams);
    }

}
