using System.Collections.Generic;
using UnityEngine;

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

    private float nextSpawnTime;
    public List<Bird> Birds { get; private set; } = new();

    public Transform Target { get; private set; }


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
