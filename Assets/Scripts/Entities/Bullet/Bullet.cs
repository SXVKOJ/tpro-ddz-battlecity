using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 25;
    
    private Vector2 direction;
    private bool isEnemyBullet;
    private float spawnTime;
    
    public bool IsEnemyBullet => isEnemyBullet;
    public int Damage => damage;
    
    public void Initialize(Vector2 dir, bool enemyBullet)
    {
        direction = dir;
        isEnemyBullet = enemyBullet;
        spawnTime = Time.time;

        // Визуальное отличие пуль врага и игрока
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = enemyBullet ? Color.red : Color.yellow;
        }
    }
    
    private void Update()
    {
        // Движение пули
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
        // Проверка границ через GameManager
        CheckMapBoundaries();
        
        // Автоуничтожение через время
        if (Time.time - spawnTime >= lifetime)
        {
            DestroyBullet();
        }
    }

    private void CheckMapBoundaries()
    {
        if (GameManager.Instance != null && GameManager.Instance.UseMapBoundaries)
        {
            Vector2 currentPosition = transform.position;
            Vector2 minBounds = GameManager.Instance.MapMinBounds;
            Vector2 maxBounds = GameManager.Instance.MapMaxBounds;
            
            // Проверяем выходит ли пуля за границы
            if (currentPosition.x < minBounds.x || currentPosition.x > maxBounds.x ||
                currentPosition.y < minBounds.y || currentPosition.y > maxBounds.y)
            {
                DestroyBullet();
                Debug.Log("🚫 Bullet hit map boundary!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }
    
    private void HandleCollision(Collider2D other)
    {
        string otherTag = other.gameObject.tag;
        
        Debug.Log($"💥 Bullet hit: {otherTag} - {other.gameObject.name}");
        
        // Игнорируем столкновения с объектами того же типа
        if ((isEnemyBullet && otherTag == "Enemy") || (!isEnemyBullet && otherTag == "Player"))
            return;
            
        switch (otherTag)
        {
            case "Wall":
                HandleWallCollision(other);
                break;
            case "SteelWall":
                HandleSteelWallCollision();
                break;
            case "Player":
            case "Enemy":
                HandleTankCollision(other);
                break;
            case "Bullet":
                HandleBulletCollision();
                break;
            case "Base":
                HandleBaseCollision(other);
                break;
        }
    }
    
    private void HandleWallCollision(Collider2D wall)
    {
        // Разрушаем кирпичную стену
        Destroy(wall.gameObject);
        DestroyBullet();
        
        Debug.Log("🧱 Brick wall destroyed!");
    }
    
    private void HandleSteelWallCollision()
    {
        // Стальная стена не разрушается
        DestroyBullet();
        Debug.Log("🔩 Steel wall - bullet destroyed");
    }
    
    private void HandleTankCollision(Collider2D tank)
    {
        // Урон наносится в TankCollisionHandler
        DestroyBullet();
        Debug.Log("🎯 Bullet hit tank");
    }
    
    private void HandleBulletCollision()
    {
        // Пули уничтожают друг друга
        DestroyBullet();
        Debug.Log("💥 Bullet hit another bullet");
    }
    
    private void HandleBaseCollision(Collider2D baseObject)
    {
        // Уничтожение базы
        Destroy(baseObject.gameObject);
        DestroyBullet();
        Debug.Log("🏠 Base destroyed!");
    }
    
    private void DestroyBullet()
    {
        // Возвращаем пулю в пул или уничтожаем
        if (BulletManager.Instance != null)
        {
            BulletManager.Instance.ReturnBulletToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}