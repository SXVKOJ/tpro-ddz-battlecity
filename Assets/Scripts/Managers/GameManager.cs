using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Settings")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int playerScore = 0;
    [SerializeField] private int currentLevel = 1;
    
    [Header("Enemy Settings")]
    [SerializeField] private bool spawnEnemies = true;
    [SerializeField] private int enemiesPerLevel = 20;
    [SerializeField] private int maxActiveEnemies = 4;
    [SerializeField] private float enemySpawnInterval = 5f;
    [SerializeField] private GameObject enemyPrefab;
    
    [Header("Map Boundaries")]
    [SerializeField] private SpriteRenderer mapSprite;
    [SerializeField] private Vector2 mapMinBounds = new Vector2(-10, -10);
    [SerializeField] private Vector2 mapMaxBounds = new Vector2(10, 10);
    [SerializeField] private bool useMapBoundaries = true;
    
    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    
    private GameObject currentPlayer;
    private EnemySpawner enemySpawner;
    
    // Публичные свойства для доступа к настройкам
    public Vector2 MapMinBounds => mapMinBounds;
    public Vector2 MapMaxBounds => mapMaxBounds;
    public bool UseMapBoundaries => useMapBoundaries;
    public bool SpawnEnemies => spawnEnemies;
    public int EnemiesPerLevel => enemiesPerLevel;
    public int MaxActiveEnemies => maxActiveEnemies;
    public float EnemySpawnInterval => enemySpawnInterval;
    public GameObject EnemyPrefab => enemyPrefab;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CalculateMapBounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
        UpdateUI();
    }
    
    private void InitializeGame()
    {
        // Создаем спавнер врагов если нужно
        if (spawnEnemies && enemyPrefab != null)
        {
            CreateEnemySpawner();
        }
        else if (spawnEnemies && enemyPrefab == null)
        {
            Debug.LogWarning("⚠️ Spawn enemies is enabled but enemy prefab is not assigned!");
        }
    }
    
    private void CreateEnemySpawner()
    {
        // Создаем объект спавнера через код
        GameObject spawnerObject = new GameObject("EnemySpawner");
        enemySpawner = spawnerObject.AddComponent<EnemySpawner>();
        
        // Настраиваем спавн точки автоматически
        CreateSpawnPoints();
        
        Debug.Log("🎯 Enemy spawner created successfully");
    }
    
    private void CreateSpawnPoints()
    {
        if (enemySpawner == null || mapSprite == null) return;
        
        // Создаем точки спавна в верхней части карты
        Bounds mapBounds = mapSprite.bounds;
        float spawnY = mapBounds.max.y - 1f; // Отступ от верхнего края
        
        // Создаем несколько точек спавна по ширине карты
        int spawnPointCount = 3;
        float step = mapBounds.size.x / (spawnPointCount + 1);
        
        for (int i = 0; i < spawnPointCount; i++)
        {
            float spawnX = mapBounds.min.x + step * (i + 1);
            Vector2 spawnPosition = new Vector2(spawnX, spawnY);
            enemySpawner.AddSpawnPoint(spawnPosition);
        }
        
        Debug.Log($"📍 Created {spawnPointCount} spawn points automatically");
    }
    
    private void CalculateMapBounds()
    {
        if (mapSprite != null)
        {
            Bounds mapBounds = mapSprite.bounds;
            mapMinBounds = mapBounds.min;
            mapMaxBounds = mapBounds.max;
            
            Debug.Log($"🗺️ GameManager: Map bounds calculated: {mapMinBounds} to {mapMaxBounds}");
        }
        else
        {
            // Автопоиск карты
            GameObject mapObject = GameObject.FindGameObjectWithTag("Map");
            if (mapObject != null)
            {
                mapSprite = mapObject.GetComponent<SpriteRenderer>();
                if (mapSprite != null)
                {
                    CalculateMapBounds();
                    return;
                }
            }
            
            Debug.LogWarning("⚠️ GameManager: Map sprite not found, using default bounds");
        }
    }

    // Где-то в вашем коде
    public void SpawnSingleEnemy()
    {
        if (SpawnEnemies && EnemyPrefab != null)
        {
            Vector2 spawnPosition = new Vector2(Random.Range(-MapMinBounds.x, MapMaxBounds.x), 8f);
            GameObject enemy = Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("🤖 Single enemy spawned programmatically");
        }
    }
    
    // Методы для динамического изменения настроек
    public void SetEnemySpawning(bool enable)
    {
        spawnEnemies = enable;
        if (enemySpawner != null)
        {
            enemySpawner.gameObject.SetActive(enable);
        }
    }
    
    public void SetEnemiesPerLevel(int count)
    {
        enemiesPerLevel = count;
        if (enemySpawner != null)
        {
            enemySpawner.SetTotalEnemies(count);
        }
    }
    
    public void SetMaxActiveEnemies(int count)
    {
        maxActiveEnemies = count;
        if (enemySpawner != null)
        {
            enemySpawner.SetMaxEnemies(count);
        }
    }
    
    public void UpdateMapBounds(SpriteRenderer newMapSprite)
    {
        mapSprite = newMapSprite;
        CalculateMapBounds();
    }
    
    public void SpawnPlayer()
    {
        if (playerSpawnPoint != null && playerPrefab != null)
        {
            // Уничтожаем старого игрока если есть
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
            }
            
            currentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            Debug.Log($"🎮 Player spawned at {playerSpawnPoint.position}");
        }
    }
    
    public void PlayerDied()
    {
        playerLives--;
        UpdateUI();
        
        if (playerLives > 0)
        {
            Invoke(nameof(SpawnPlayer), 2f);
        }
        else
        {
            GameOver();
        }
    }
    
    public void AddScore(int points)
    {
        playerScore += points;
        UpdateUI();
    }
    
    public void LevelComplete()
    {
        currentLevel++;
        LoadLevel(currentLevel);
    }
    
    public void LoadLevel(int level)
    {
        SceneManager.LoadScene($"Level_{level}");
    }
    
    private void UpdateUI()
    {
        // Здесь будет обновление UI
        int enemiesRemaining = enemySpawner != null ? enemySpawner.EnemiesRemaining : 0;
        Debug.Log($"Lives: {playerLives}, Score: {playerScore}, Enemies: {enemiesRemaining}");
    }
    
    private void GameOver()
    {
        Debug.Log("Game Over!");
        // Показать экран Game Over
    }
}