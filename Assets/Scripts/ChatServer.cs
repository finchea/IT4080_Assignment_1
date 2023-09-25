using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatServer : NetworkBehaviour
{
    public ChatUi chatUi;
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];

    // Start is called before the first frame update
    void Start()
    {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnect;
            if (IsHost)
            {
                DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.LocalClientId}");
            } else
            {
                DisplayMessageLocally(SYSTEM_ID, "You are the server");
            }
            
        } else
        {
            DisplayMessageLocally(SYSTEM_ID, $"You are a client {NetworkManager.LocalClientId}");
        }
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        ServerSendDirectMessage($"I ({NetworkManager.LocalClientId}) see you ({clientId}) have connected to the server, well done", NetworkManager.LocalClientId, clientId);
        SendChatMessageServerRpc($"clientId {clientId} has connected to the server");
    }

    private void ServerOnClientDisconnect(ulong clientId)
    {
        SendChatMessageServerRpc($"clientId {clientId} has disconnected from the server");
    }

    private void DisplayMessageLocally(ulong from, string message)
    {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;

        if (from == NetworkManager.LocalClientId)
        {
            fromStr = "you";
            textColor = Color.magenta;
        } else if (from == SYSTEM_ID)
        {
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
        if (message.StartsWith("@"))
        {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            ulong toClientId = ulong.Parse(clientIdStr);

            int num = 0;

            var clientIds = NetworkManager.ConnectedClients.Keys;
            foreach (ulong id in clientIds)
            {
                if (toClientId == id) 
                {
                    ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);
                    num++;
                }
            }
            if (num == 0) 
            {
                ServerSendDirectMessage("The message could not be sent.", NetworkManager.LocalClientId, serverRpcParams.Receive.SenderClientId);
            }
        } else
        {
            RecieveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
        
    }

    [ClientRpc]
    public void RecieveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default)
    {
        DisplayMessageLocally(from, message);
    }

    private void ServerSendDirectMessage(string message, ulong from, ulong to)
    {
        dmClientIds[0] = from;
        dmClientIds[1] = to;
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;
        
        RecieveChatMessageClientRpc(message, from, rpcParams);
    }
}
