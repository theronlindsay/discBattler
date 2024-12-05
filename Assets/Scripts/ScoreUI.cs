using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text roundText;

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
