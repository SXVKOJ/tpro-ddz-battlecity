using UnityEngine;

public enum PowerUpType
{
    Health,
    Shield,
    Speed,
    FireRate,
    PowerBullet
}

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerUpType type;
    [SerializeField] private float duration = 10f;
    [SerializeField] private int value = 25;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TankController tank = other.GetComponent<TankController>();
            if (tank != null)
            {
                ApplyPowerUp(tank);
                Destroy(gameObject);
            }
        }
    }
    
    public void ApplyPowerUp(TankController tank)
    {
        switch (type)
        {
            case PowerUpType.Health:
                tank.TakeDamage(-value); // Восстановление здоровья
                break;
            case PowerUpType.Shield:
                tank.ActivateShield(duration);
                break;
            case PowerUpType.Speed:
                tank.BoostSpeed(duration, 1.5f);
                break;
            case PowerUpType.FireRate:
                tank.BoostFireRate(duration, 0.5f);
                break;
            case PowerUpType.PowerBullet:
                tank.ActivatePowerBullet(duration);
                break;
        }
        
        AudioManager.Instance.PlaySound("PowerUp");
    }
}