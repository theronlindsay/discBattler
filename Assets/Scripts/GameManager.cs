using UnityEngine;
using Unity.Netcode;

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

    public static GameManager Instance;

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

    public void PlayerScored(NetworkObjectReference scoringPlayer)
    {
        if (!IsServer) return;

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
    private void ResetPlayersClientRpc()
    {
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            Transform spawnPoint = GetSpawnPoint(player.OwnerClientId);
            player.ResetStateServerRpc(spawnPoint.position, spawnPoint.rotation);
        }
    }

    private Transform GetSpawnPoint(ulong clientId)
    {
        // Assign spawn points based on client ID
        return clientId == 0 ? spawnPointPlayer1 : spawnPointPlayer2;
    }
}
