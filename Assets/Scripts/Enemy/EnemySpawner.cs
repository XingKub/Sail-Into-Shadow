using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private float minSpawnDistance = 5f;

    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 10f;

    [Header("Map Boundaries")]
    [SerializeField] private Vector2 mapMin;
    [SerializeField] private Vector2 mapMax;

    private Transform playerTransform;
    private float nextSpawnTime;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnEnemy()
    {
        for (int i = 0; i < 10; i++)
        {
            float randomX = Random.Range(mapMin.x, mapMax.x);
            float randomY = Random.Range(mapMin.y, mapMax.y);
            Vector2 rayOrigin = new Vector2(randomX, randomY);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);

            if (hit.collider != null)
            {
                Vector2 spawnPosition = hit.point;

                //check distance from player
                if (playerTransform != null)
                {
                    float distanceToPlayer = Vector2.Distance(spawnPosition, playerTransform.position);
                    if (distanceToPlayer < minSpawnDistance) continue;

                    Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                    return;
                }
            }
        }
    }
}