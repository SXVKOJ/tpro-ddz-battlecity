using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int maxEnemies = 4;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int enemiesPerLevel = 20;
    
    private List<Vector2> spawnPoints = new List<Vector2>();
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    
    public void AddSpawnPoint(Vector2 point)
    {
        spawnPoints.Add(point);
    }
    
    private void Start()
    {
        InvokeRepeating(nameof(TrySpawnEnemy), spawnInterval, spawnInterval);
    }
    
    private void TrySpawnEnemy()
    {
        if (enemiesSpawned < enemiesPerLevel && enemiesAlive < maxEnemies && spawnPoints.Count > 0)
        {
            SpawnEnemy();
        }
    }
    
    private void SpawnEnemy()
    {
        Vector2 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
        enemiesSpawned++;
        enemiesAlive++;
        
        // Подписываемся на событие смерти врага
        enemy.GetComponent<TankController>().OnTankDestroyed += OnEnemyDied;
    }
    
    private void OnEnemyDied()
    {
        enemiesAlive--;
        GameManager.Instance.AddScore(100);
        
        if (enemiesSpawned >= enemiesPerLevel && enemiesAlive == 0)
        {
            GameManager.Instance.LevelComplete();
        }
    }
}