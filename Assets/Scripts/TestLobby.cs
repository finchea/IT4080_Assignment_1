using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    public LobbyUi lobbyUi;

    // Start is called before the first frame update
    void Start()
    {
        CreateTestCards();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateTestCards()
    {
        PlayerCard pc = lobbyUi.playerCards.AddCard("test player 1");
        pc.color = Color.grey;
        pc.ready = true;
        pc.ShowKick(false);
        pc.UpdateDisplay();
    }
}
