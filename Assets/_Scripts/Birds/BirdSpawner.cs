using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BirdSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private Bird birdPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Targets")]
    [SerializeField] private Transform[] targetPoints;

    [Header("Bird Amount")]
    [SerializeField] private int minBirds = 5;
    [SerializeField] private int maxBirds = 10;
    [Header("Spawn Spread")]
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float flightHeight = 20f;
    [SerializeField] private float spawnDelay = 3f;

    [SerializeField] private Image cooldownImage;
    [SerializeField] private TMPro.TextMeshProUGUI cooldownText;

    private float nextSpawnTime;
    public List<Bird> Birds { get; private set; } = new();

    public Transform Target { get; private set; }

    private void Start()
    {
        cooldownImage.fillAmount = 0;
        cooldownText.gameObject.SetActive(false);
    }

    public void SpawnFlock()
    {
        Birds.Clear();

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Target = targetPoints[Random.Range(0, targetPoints.Length)];

        int amount = Random.Range(minBirds, maxBirds + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;

            Vector3 spawnPosition = new Vector3(
                spawn.position.x + randomOffset.x,
                flightHeight,
                spawn.position.z + randomOffset.y
            );

            Bird bird = Instantiate(
                birdPrefab,
                spawnPosition,
                Quaternion.identity);

            bird.flock = this;

            Birds.Add(bird);
        }
    }

    private void Update()
    {
        float remaining = Mathf.Max(0, nextSpawnTime - Time.time);

        if (remaining > 0)
        {
            cooldownImage.fillAmount = remaining / spawnDelay;

            cooldownText.gameObject.SetActive(true);
            cooldownText.text = Mathf.CeilToInt(remaining).ToString();
        }
        else
        {
            cooldownImage.fillAmount = 0;
            cooldownText.gameObject.SetActive(false);
        }

        if (Input.GetMouseButtonDown(1) && Time.time >= nextSpawnTime)
        {
            SpawnFlock();
            nextSpawnTime = Time.time + spawnDelay;
        }
        if (Birds.Count == 0)
            return;

        bool reached = true;

        foreach (Bird bird in Birds)
        {
            if (bird == null)
                continue;

            if (Vector3.Distance(bird.transform.position, Target.position) > 4f)
            {
                reached = false;
                break;
            }
        }

        if (reached)
        {
            foreach (Bird bird in Birds)
            {
                if (bird != null)
                    Destroy(bird.gameObject);
            }

            Birds.Clear();
        }
    }
}
