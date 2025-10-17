using UnityEngine;
using System.Collections.Generic;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance;
    
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int maxBullets = 20;
    
    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePool()
    {
        for (int i = 0; i < maxBullets; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }
    
    public void SpawnBullet(Vector2 position, Vector2 direction, bool isPlayerBullet)
    {
        Debug.Log($"Spawning bullet at {position}, direction: {direction}, isPlayer: {isPlayerBullet}");
        
        if (bulletPrefab == null)
        {
            Debug.LogError("BulletPrefab is null!");
            return;
        }
        
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.transform.position = position;
            bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            bullet.SetActive(true);
            
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(direction, isPlayerBullet);
                Debug.Log("Bullet spawned successfully");
            }
            else
            {
                Debug.LogError("Bullet component not found!");
            }
        }
        else
        {
            Debug.LogWarning("No bullets available in pool");
        }
    }
    
    public void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}