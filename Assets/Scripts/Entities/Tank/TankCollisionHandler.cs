using UnityEngine;

public class TankCollisionHandler : MonoBehaviour
{
    private TankController tankController;
    
    private void Awake()
    {
        tankController = GetComponent<TankController>();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string collisionTag = collision.gameObject.tag;
        
        switch (collisionTag)
        {
            case "Wall":
                HandleWallCollision(collision);
                break;
            case "Enemy":
                HandleEnemyCollision(collision);
                break;
            case "Water":
                HandleWaterCollision();
                break;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        string triggerTag = other.gameObject.tag;
        
        switch (triggerTag)
        {
            case "PowerUp":
                HandlePowerUpCollision(other);
                break;
            case "Base":
                HandleBaseCollision();
                break;
        }
    }
    
    private void HandleWallCollision(Collision2D collision)
    {
        // Останавливаем танк при столкновении со стеной
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Воспроизвести звук столкновения
        AudioManager.Instance.PlaySound("TankHit");
    }
    
    private void HandleEnemyCollision(Collision2D collision)
    {
        // Отталкивание от врага
        Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
        GetComponent<Rigidbody2D>().AddForce(pushDirection * 5f, ForceMode2D.Impulse);
        
        // Нанести урон
        tankController.TakeDamage(10);
    }
    
    private void HandlePowerUpCollision(Collider2D powerUp)
    {
        PowerUp powerUpComponent = powerUp.GetComponent<PowerUp>();
        if (powerUpComponent != null)
        {
            powerUpComponent.ApplyPowerUp(tankController);
            Destroy(powerUp.gameObject);
        }
    }
    
    private void HandleWaterCollision()
    {
        // Тонет в воде
        tankController.TakeDamage(100);
    }
    
    private void HandleBaseCollision()
    {
        // Защита базы
        Debug.Log("Protect the base!");
    }
}