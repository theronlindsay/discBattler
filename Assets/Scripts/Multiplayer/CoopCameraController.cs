using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoopCameraController : MonoBehaviour
{

    private int NumberOfPlayers;
    private GameObject Player1;
    private GameObject Player2;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (NumberOfPlayers){
            case 1:
                transform.position = new Vector3(Player1.transform.position.x, Player1.transform.position.y, -10);
                break;
            case 2:
                transform.position = new Vector3((Player1.transform.position.x + Player2.transform.position.x) / 2, (Player1.transform.position.y + Player2.transform.position.y) / 2, -10);
            
                
                break;
        }
    }

    public void AddPlayer(GameObject player){
        if (NumberOfPlayers == 0){
            Player1 = player;
        } else if (NumberOfPlayers == 1){
            Player2 = player;
        } else {
            Debug.Log("Error: More than 2 players");
        }

        NumberOfPlayers++;
    }
}
