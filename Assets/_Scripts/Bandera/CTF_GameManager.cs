using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CTF_GameManager : MonoBehaviour
{
    public static CTF_GameManager Instance;

    [Header("Score")]
    private int teamAScore;
    private int teamBScore;
    public TextMeshProUGUI playerTeamText;
    public TextMeshProUGUI iaTeamText;

    [Header("Condiciones de fin")]
    [SerializeField] private int scoreToWin = 5;        
    [SerializeField] private float timeLimit = 120f;    
    private float timer;
    private bool gameEnded = false;

    [Header("UI de fin")]
    [SerializeField] private GameObject victoryPanel;   
    [SerializeField] private GameObject defeatPanel;    
    public TextMeshProUGUI timerText;

    [Header("Banderas")]
    [SerializeField] private Flag playerFlag;
    [SerializeField] private Flag aiFlag;


    public Flag GetOwnFlag(Team team) => team == Team.Player ? playerFlag : aiFlag;


    public Flag GetEnemyFlag(Team team) => team == Team.Player ? aiFlag : playerFlag;
    private void Awake()
    {
        Instance = this;
        teamAScore = 0;
        teamBScore = 0;
        timer = timeLimit;
    }

    private void Update()
    {
        if (gameEnded) return;

        
        timer -= Time.deltaTime;

        
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timer);
            timerText.text = seconds.ToString();
        }

        
        if (timer <= 0f)
        {
            Defeat();
        }
    }

    public void AddScore(Team team)
    {
        if (gameEnded) return;   

        if (team == Team.Player)
            teamAScore++;
        else
            teamBScore++;

        playerTeamText.text = teamAScore.ToString();
        iaTeamText.text = teamBScore.ToString();


        if (teamAScore >= scoreToWin)
            Victory();
        else if (teamBScore >= scoreToWin)
            Defeat();
    }

    private void Victory()
    {
        gameEnded = true;
        
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;   
    }

    private void Defeat()
    {
        gameEnded = true;
        
        if (defeatPanel != null) defeatPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;   
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}