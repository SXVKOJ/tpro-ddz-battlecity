using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Debug View")]
    [SerializeField] private bool showSpawnPoints = true;
    
    private List<Transform> spawnPoints = new List<Transform>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private int enemiesSpawned = 0;
    private float nextSpawnTime;
    
    // Публичные свойства
    public int EnemiesRemaining => GameManager.Instance.EnemiesPerLevel - enemiesSpawned + activeEnemies.Count;
    public int ActiveEnemiesCount => activeEnemies.Count;
    public int TotalEnemiesSpawned => enemiesSpawned;
    
    private void Start()
    {
        InitializeFromGameManager();
    }
    
    private void InitializeFromGameManager()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager instance not found!");
            return;
        }
        
        nextSpawnTime = Time.time + GameManager.Instance.EnemySpawnInterval;
        Debug.Log($"🎯 EnemySpawner initialized. Total enemies: {GameManager.Instance.EnemiesPerLevel}");
    }
    
    private void Update()
    {
        if (!GameManager.Instance.SpawnEnemies) return;
        if (GameManager.Instance.EnemyPrefab == null) return;
        
        if (Time.time >= nextSpawnTime && enemiesSpawned < GameManager.Instance.EnemiesPerLevel)
        {
            TrySpawnEnemy();
            nextSpawnTime = Time.time + GameManager.Instance.EnemySpawnInterval;
        }
        
        // Очистка уничтоженных врагов из списка
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
    
    private void TrySpawnEnemy()
    {
        if (activeEnemies.Count >= GameManager.Instance.MaxActiveEnemies)
        {
            return;
        }
        
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("⚠️ No spawn points assigned, creating default ones");
            CreateDefaultSpawnPoints();
        }
        
        SpawnEnemy();
    }
    
    private void SpawnEnemy()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        GameObject enemy = Instantiate(GameManager.Instance.EnemyPrefab, spawnPoint.position, Quaternion.identity);
        enemy.name = $"EnemyTank_{enemiesSpawned + 1}";
        
        // Настраиваем врага через код
        SetupEnemyComponents(enemy);
        
        activeEnemies.Add(enemy);
        enemiesSpawned++;
        
        // Подписываемся на событие смерти врага
        TankController tankController = enemy.GetComponent<TankController>();
        if (tankController != null)
        {
            tankController.OnTankDestroyed += () => OnEnemyDestroyed(enemy);
        }
        
        Debug.Log($"🤖 Enemy spawned at {spawnPoint.position}. Total: {enemiesSpawned}/{GameManager.Instance.EnemiesPerLevel}");
        
        // Проверка завершения уровня
        CheckLevelCompletion();
    }
    
    private void SetupEnemyComponents(GameObject enemy)
    {
        // Настраиваем TankController для врага
        TankController tankController = enemy.GetComponent<TankController>();
        if (tankController != null)
        {
            // Можно настроить специфические параметры для врага
            tankController.SetMovement(true);
        }
        
        // Добавляем AI если его нет
        if (enemy.GetComponent<EnemyAI>() == null)
        {
            enemy.AddComponent<EnemyAI>();
        }
    }
    
    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) 
        {
            // Создаем точку спавна по умолчанию
            GameObject defaultPoint = new GameObject("DefaultSpawnPoint");
            defaultPoint.transform.position = new Vector2(0, 8f);
            defaultPoint.transform.SetParent(transform);
            spawnPoints.Add(defaultPoint.transform);
        }
        
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
    
    private void CreateDefaultSpawnPoints()
    {
        // Создаем 3 точки спавна по умолчанию
        AddSpawnPoint(new Vector2(-5f, 8f));
        AddSpawnPoint(new Vector2(0f, 8f));
        AddSpawnPoint(new Vector2(5f, 8f));
    }
    
    private void OnEnemyDestroyed(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        
        // Добавляем очки за уничтожение врага
        GameManager.Instance?.AddScore(100);
        
        Debug.Log($"💀 Enemy destroyed. Active enemies: {activeEnemies.Count}");
        
        // Проверка завершения уровня
        CheckLevelCompletion();
    }
    
    private void CheckLevelCompletion()
    {
        if (enemiesSpawned >= GameManager.Instance.EnemiesPerLevel && activeEnemies.Count == 0)
        {
            LevelComplete();
        }
    }
    
    private void LevelComplete()
    {
        Debug.Log("🎉 Level completed! All enemies destroyed!");
        GameManager.Instance?.LevelComplete();
    }
    
    // Публичные методы для управления спавнером
    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (!spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Add(spawnPoint);
            spawnPoint.SetParent(transform);
        }
    }
    
    public void AddSpawnPoint(Vector2 position)
    {
        GameObject spawnPoint = new GameObject($"SpawnPoint_{spawnPoints.Count + 1}");
        spawnPoint.transform.position = position;
        spawnPoint.transform.SetParent(transform);
        spawnPoints.Add(spawnPoint.transform);
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        activeEnemies.Clear();
    }
    
    public void SetTotalEnemies(int count)
    {
        // Это значение теперь берется из GameManager
        Debug.Log($"Enemies per level updated to: {count}");
    }
    
    public void SetMaxEnemies(int count)
    {
        // Это значение теперь берется из GameManager
        Debug.Log($"Max active enemies updated to: {count}");
    }
    
    // Визуализация точек спавна в редакторе
    private void OnDrawGizmos()
    {
        if (!showSpawnPoints) return;
        
        Gizmos.color = Color.red;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireCube(spawnPoint.position, new Vector3(1f, 1f, 0f));
                Gizmos.DrawIcon(spawnPoint.position, "enemy_spawn.png", true);
            }
        }
    }
}