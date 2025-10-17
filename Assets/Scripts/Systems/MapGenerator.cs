using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Tile References")]
    [SerializeField] private TileBase brickTile;
    [SerializeField] private TileBase steelTile;
    [SerializeField] private TileBase waterTile;
    [SerializeField] private TileBase grassTile;
    
    [Header("Level Design")]
    [SerializeField] private int mapWidth = 26;
    [SerializeField] private int mapHeight = 26;
    [SerializeField] private TextAsset[] levelMaps;
    
    private Tilemap wallTilemap;
    private Tilemap environmentTilemap;
    
    private void Start()
    {
        wallTilemap = GameObject.Find("WallTilemap").GetComponent<Tilemap>();
        environmentTilemap = GameObject.Find("EnvironmentTilemap").GetComponent<Tilemap>();
        
        GenerateLevel(0); // Генерация первого уровня
    }
    
    public void GenerateLevel(int levelIndex)
    {
        ClearMap();
        
        if (levelIndex < levelMaps.Length)
        {
            LoadLevelFromText(levelMaps[levelIndex]);
        }
        else
        {
            GenerateRandomLevel();
        }
    }
    
    private void LoadLevelFromText(TextAsset levelData)
    {
        string[] lines = levelData.text.Split('\n');
        
        for (int y = 0; y < lines.Length && y < mapHeight; y++)
        {
            string line = lines[y].Trim();
            for (int x = 0; x < line.Length && x < mapWidth; x++)
            {
                Vector3Int position = new Vector3Int(x, mapHeight - y - 1, 0);
                
                switch (line[x])
                {
                    case 'B': // Кирпич
                        wallTilemap.SetTile(position, brickTile);
                        break;
                    case 'S': // Сталь
                        wallTilemap.SetTile(position, steelTile);
                        break;
                    case 'W': // Вода
                        environmentTilemap.SetTile(position, waterTile);
                        break;
                    case 'G': // Трава
                        environmentTilemap.SetTile(position, grassTile);
                        break;
                    case 'P': // Спавн игрока
                        SetPlayerSpawn(x, mapHeight - y - 1);
                        break;
                    case 'E': // Спавн врагов
                        AddEnemySpawnPoint(x, mapHeight - y - 1);
                        break;
                }
            }
        }
    }
    
    private void GenerateRandomLevel()
    {
        // Генерация границ карты
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), steelTile);
                }
            }
        }
        
        // Случайные стены
        for (int i = 0; i < 50; i++)
        {
            int x = Random.Range(2, mapWidth - 2);
            int y = Random.Range(2, mapHeight - 2);
            wallTilemap.SetTile(new Vector3Int(x, y, 0), brickTile);
        }
    }
    
    private void SetPlayerSpawn(int x, int y)
    {
        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            spawnPoint.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        }
    }
    
    private void AddEnemySpawnPoint(int x, int y)
    {
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.AddSpawnPoint(new Vector2(x + 0.5f, y + 0.5f));
        }
    }
    
    private void ClearMap()
    {
        wallTilemap.ClearAllTiles();
        environmentTilemap.ClearAllTiles();
    }
}