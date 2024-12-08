using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public TextMeshProUGUI roundText;

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            player1ScoreText.text = $"Player 1: {GameManager.Instance.player1Score.Value}";
            player2ScoreText.text = $"Player 2: {GameManager.Instance.player2Score.Value}";
            roundText.text = $"Round {GameManager.Instance.currentRound.Value}";
        }
    }
}
