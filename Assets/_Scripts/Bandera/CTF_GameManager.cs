using TMPro;
using UnityEngine;

public class CTF_GameManager : MonoBehaviour
{
    public static CTF_GameManager Instance;

    private int teamAScore;
    private int teamBScore;

    public TextMeshProUGUI playerTeamText;
    public TextMeshProUGUI iaTeamText;


    private void Awake()
    {
        Instance = this;

        teamAScore = 0;
        teamBScore = 0;
    }

    public void AddScore(Team team)
    {
        if (team == Team.Player)
            teamAScore++;
        else
            teamBScore++;

        playerTeamText.text = teamAScore.ToString();
        iaTeamText.text = teamBScore.ToString();
    }
}
