using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using System.Text.RegularExpressions;

public class NetworkedPlayers : NetworkBehaviour
{
    public NetworkList<NetworkPlayerInfo> allNetPlayers;

    private int colorIndex = 0;
    private Color[] playerColors = new Color[] {
        Color.green,
        Color.yellow,
        Color.red,
        Color.black,
        Color.gray,
        Color.cyan,
        Color.blue,
    };

    private void Awake()
    {
        allNetPlayers = new NetworkList<NetworkPlayerInfo>();
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (IsServer)
        {
            ServerStart();
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisonnected;
        }
        NetworkHelper.Log($"player count = {allNetPlayers.Count}");
    }

    void ServerStart()
    {
        NetworkPlayerInfo host = new NetworkPlayerInfo(NetworkManager.LocalClientId);
        host.ready = true;
        host.color = NextColor();
        host.playerName = "The Host";
        allNetPlayers.Add(host);
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        NetworkPlayerInfo client = new NetworkPlayerInfo(clientId);
        client.ready = false;
        client.color = NextColor();
        client.playerName = $"Player {clientId}";
        allNetPlayers.Add(client);
    }

    private void ServerOnClientDisonnected(ulong clientId)
    {
        var idx = FindPlayerIndex(clientId);
        if (idx != -1)
        {
            allNetPlayers.RemoveAt(idx);
        }
    }

    private Color NextColor()
    {
        Color newColor = playerColors[colorIndex];
        colorIndex += 1;
        if (colorIndex > playerColors.Length - 1)
        {
            colorIndex = 0;
        }
        return newColor;
    }

    public int FindPlayerIndex(ulong clientId)
    {
        var idx = 0;
        var found = false;

        while (idx < allNetPlayers.Count && !found)
        {
            if (allNetPlayers[idx].clientId == clientId)
            {
                found = true;
            }
            else
            {
                idx += 1;
            }
        }

        if (!found)
        {
            idx = -1;
        }

        return idx;
    }

    public void UpdateReady(ulong clientId, bool ready)
    {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1)
        {
            return;
        }

        NetworkPlayerInfo info = allNetPlayers[idx];
        info.ready = ready;
        allNetPlayers[idx] = info;
    }

    public void UpdatePlayerName(ulong clientId, string playerName)
    {
        int idx = FindPlayerIndex(clientId);
        if (idx == -1)
        {
            return;
        }

        NetworkPlayerInfo info = allNetPlayers[idx];
        if (playerName.Length <= 15 && !playerName.Equals("invalid") && !playerName.Equals("bob"))
        {
            info.playerName = playerName;
        } else if (info.clientId == NetworkManager.LocalClientId)
        {
            info.playerName = "The Host";
        } else
        {
            info.playerName = $"Player {clientId}";
        }
        allNetPlayers[idx] = info;
    }

    public NetworkPlayerInfo GetMyPlayerInfo()
    {
        NetworkPlayerInfo toReturn = new NetworkPlayerInfo(ulong.MaxValue);
        int idx = FindPlayerIndex(NetworkManager.LocalClientId);
        if(idx != -1)
        {
            toReturn = allNetPlayers[idx];
        }
        return toReturn;
    }

    public bool AllPlayersReady()
    {
        bool theyAre = true;
        int idx = 0;
        while(theyAre && idx < allNetPlayers.Count)
        {
            theyAre = allNetPlayers[idx].ready;
            idx += 1;
        }
        return theyAre;
    }
}
