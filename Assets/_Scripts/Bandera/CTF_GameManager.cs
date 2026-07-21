using System.Collections.Generic;
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

    [Header("Bases")]
    [SerializeField] private Transform playerBase;
    [SerializeField] private Transform aiBase;

    [Header("Coordinación IA")]
    [SerializeField] private float roleCheckInterval = 1f;
    private readonly List<EnemyController> attackerCandidates = new();
    private EnemyController currentAttacker;
    private float roleTimer;

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

        roleTimer += Time.deltaTime;
        if (roleTimer >= roleCheckInterval)
        {
            UpdateAttackerRole();
            roleTimer = 0f;
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

    public Vector3 GetBasePosition(Team team) => team == Team.Player ? playerBase.position : aiBase.position;

    // Los melees se anotan solos al arrancar
    public void RegisterAttackerCandidate(EnemyController enemy)
    {
        if (!attackerCandidates.Contains(enemy))
            attackerCandidates.Add(enemy);
    }

    
    public bool IsAttacker(EnemyController enemy) => currentAttacker == enemy;

    
    private void UpdateAttackerRole()
    {
        Flag targetFlag = GetEnemyFlag(Team.AI);   // la bandera que la IA quiere robar
        if (targetFlag == null) return;

        // Si un aliado ya la está llevando, nadie más tiene que ir
        if (targetFlag.State == FlagState.Carried)
        {
            currentAttacker = null;
            return;
        }

        // Si el atacante actual sigue siendo válido lo mantengo,
        // así el rol no salta entre dos enemigos que están a distancias parecidas
        if (currentAttacker != null && currentAttacker.IsAlive() && !currentAttacker.HasFlag)
            return;

        // Elijo el candidato vivo más cercano a la bandera
        EnemyController closest = null;
        float closestDist = Mathf.Infinity;
        Vector3 flagPos = targetFlag.transform.position;

        foreach (var candidate in attackerCandidates)
        {
            if (candidate == null || !candidate.IsAlive() || candidate.HasFlag) continue;

            float d = Vector3.Distance(candidate.transform.position, flagPos);
            if (d < closestDist)
            {
                closestDist = d;
                closest = candidate;
            }
        }

        currentAttacker = closest;
    }
}