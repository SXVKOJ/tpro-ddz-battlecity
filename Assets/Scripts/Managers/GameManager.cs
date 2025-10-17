using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Settings")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int playerScore = 0;
    [SerializeField] private int currentLevel = 1;
    
    [Header("Map Boundaries")]
    [SerializeField] private SpriteRenderer mapSprite;
    [SerializeField] private Vector2 mapMinBounds = new Vector2(-10, -10);
    [SerializeField] private Vector2 mapMaxBounds = new Vector2(10, 10);
    [SerializeField] private bool useMapBoundaries = true;
    
    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    
    private GameObject currentPlayer;
    
    // Публичные свойства для доступа к границам
    public Vector2 MapMinBounds => mapMinBounds;
    public Vector2 MapMaxBounds => mapMaxBounds;
    public bool UseMapBoundaries => useMapBoundaries;
    
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
        UpdateUI();
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
    
    // Метод для принудительного обновления границ (при смене уровня)
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
        Debug.Log($"Lives: {playerLives}, Score: {playerScore}");
    }
    
    private void GameOver()
    {
        Debug.Log("Game Over!");
        // Показать экран Game Over
    }
}