using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEditor;

public class GameManager : NetworkBehaviour
{
    [Header("Game Settings")]
    public int maxRounds = 3;

    [Header("Game State")]
    public NetworkVariable<int> player1Score = new NetworkVariable<int>();
    public NetworkVariable<int> player2Score = new NetworkVariable<int>();
    public NetworkVariable<int> currentRound = new NetworkVariable<int>(1);

    [Header("Spawn Points")]
    public Transform spawnPointPlayer1;
    public Transform spawnPointPlayer2;

    public GameObject player1;
    public GameObject player2;

    public GameObject disc; // The disc prefab

    public static GameManager Instance;
    [Rpc(SendTo.Everyone)]
    public void PlayerScoredServerRpc(NetworkObjectReference scoringPlayer)
    {

        // Award point to the scoring player
        if (scoringPlayer.TryGet(out NetworkObject scoringObject))
        {
            if (scoringObject.OwnerClientId == 0) // Assuming Player 1
                player1Score.Value++;
            else if (scoringObject.OwnerClientId == 1) // Assuming Player 2
                player2Score.Value++;
        }

        Debug.Log($"Player 1: {player1Score.Value} | Player 2: {player2Score.Value}");

        // Check if the game should end
        if (currentRound.Value >= maxRounds || Mathf.Max(player1Score.Value, player2Score.Value) > maxRounds / 2)
        {
            EndGame();
        }
        else
        {
            NextRound();
        }
    }

    [Rpc(SendTo.Server)]
    public void SpawnDiscRpc(Vector3 position, Quaternion rotation, NetworkObjectReference player)
    {
        //Instantiate the disc for everyone
        GameObject discObject = Instantiate(disc, position, rotation);
        // Set the player that threw the disc
        discObject.GetComponent<bullet>().player = player;
        // Set the starting position of the disc
        discObject.GetComponent<bullet>().startingPosition = position;
        // Get the player that threw the disc
        player.TryGet(out NetworkObject playerObject);
        // Set the disc reference for the player
        playerObject.GetComponentInChildren<gun>().discReference = discObject.GetComponent<NetworkObject>();
        // Define the disc as a network object
        NetworkObject networkObject = discObject.GetComponent<NetworkObject>();
        // Set the disc reference for the player
        playerObject.GetComponentInChildren<gun>().discReference = networkObject;
        // Spawn the disc for everyone
        networkObject.Spawn();
        networkObject.GetComponent<bullet>().Shoot(networkObject.transform.forward, 30);
        
    }

    [Rpc(SendTo.Server)]
    public void ReturnDiscRpc(NetworkObjectReference disc)
    {
        // Get the disc that needs to be returned
        disc.TryGet(out NetworkObject discObject);

        // Call the return disc function on the child of the player
        discObject.GetComponent<bullet>().Recall();
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        if (IsServer)
        {
            player1Score.Value = 0;
            player2Score.Value = 0;
            currentRound.Value = 1;
        }
    }

    private void NextRound()
    {
        currentRound.Value++;
        Debug.Log($"Starting Round {currentRound.Value}");

        // Reset all players to their spawn points
        ResetPlayers();
    }

    private void EndGame()
    {
        Debug.Log("Game Over!");
        string winner = player1Score.Value > player2Score.Value ? "Player 1" : "Player 2";
        Debug.Log($"{winner} Wins!");
        // Add logic to reset or exit to main menu
    }

    private void ResetPlayers()
    {
        if (IsServer)
        {
            Debug.Log("Server: Resetting all players for the new round.");
            ResetPlayersClientRpc();
        }
    }

    [ClientRpc]
    public void ResetPlayersClientRpc()
    {
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            Transform spawnPoint = GetSpawnPoint(player.OwnerClientId);
            player.ResetStateServerRpc(spawnPoint.position, spawnPoint.rotation);
        }
    }



    public Transform GetSpawnPoint(ulong clientId)
    {
        // Assign spawn points based on client ID
        if (clientId == 0)
            return spawnPointPlayer1;
        else if (clientId == 1)
            return spawnPointPlayer2;
        else
            throw new KeyNotFoundException($"No spawn point found for client ID {clientId}");
    }

    public void PlayerScored(NetworkObjectReference player){
        PlayerScoredServerRpc(player);
    }   

    public void SpawnDisc(GameObject disc, Vector3 position, Quaternion rotation, NetworkObjectReference player){
        SpawnDiscRpc(position, rotation, player);
    }
}
