using UnityEngine;

public class TankController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private Transform firePoint;

    [Header("Tank Properties")]
    [SerializeField] private int health = 100;
    [SerializeField] private bool isPlayer = true;

    public bool IsPlayer => isPlayer;

    private Rigidbody2D rb;
    private Vector2 currentDirection = Vector2.up;
    private float nextFireTime;
    private bool canMove = true;
    private bool isMoving = false;

    // События для UI и других систем
    public System.Action<int> OnHealthChanged;
    public System.Action OnTankDestroyed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!canMove) return;

        if (isPlayer)
        {
            HandlePlayerInput();
        }

        HandleShooting();
    }

    private void FixedUpdate()
    {
        // Движение обрабатывается в FixedUpdate для плавности
        if (canMove && isMoving)
        {
            MoveTank();
        }
        
        // Ограничиваем позицию в пределах карты через GameManager
        if (GameManager.Instance != null && GameManager.Instance.UseMapBoundaries)
        {
            ClampPositionToMapBounds();
        }
    }

    private void ClampPositionToMapBounds()
    {
        if (GameManager.Instance == null) return;
        
        Vector2 currentPosition = transform.position;
        Vector2 minBounds = GameManager.Instance.MapMinBounds;
        Vector2 maxBounds = GameManager.Instance.MapMaxBounds;
        
        // Учитываем размер танка
        Collider2D tankCollider = GetComponent<Collider2D>();
        float tankRadius = tankCollider != null ? tankCollider.bounds.extents.x : 0.5f;
        
        Vector2 clampedPosition = new Vector2(
            Mathf.Clamp(currentPosition.x, minBounds.x + tankRadius, maxBounds.x - tankRadius),
            Mathf.Clamp(currentPosition.y, minBounds.y + tankRadius, maxBounds.y - tankRadius)
        );
        
        if (clampedPosition != currentPosition)
        {
            transform.position = clampedPosition;
            
            // Останавливаем движение если уперлись в границу
            if (IsMovingTowardBoundary(currentPosition, clampedPosition))
            {
                StopMovement();
            }
        }
    }
    
    private bool IsMovingTowardBoundary(Vector2 originalPos, Vector2 clampedPos)
    {
        // Проверяем движение по X
        if (Mathf.Abs(clampedPos.x - originalPos.x) > 0.01f)
        {
            if (currentDirection.x > 0 && clampedPos.x < originalPos.x) return true;
            if (currentDirection.x < 0 && clampedPos.x > originalPos.x) return true;
        }
        
        // Проверяем движение по Y
        if (Mathf.Abs(clampedPos.y - originalPos.y) > 0.01f)
        {
            if (currentDirection.y > 0 && clampedPos.y < originalPos.y) return true;
            if (currentDirection.y < 0 && clampedPos.y > originalPos.y) return true;
        }
        
        return false;
    }

    private void HandlePlayerInput()
    {
        // Обработка поворота танка
        HandleRotationInput();
        
        // Обработка движения
        HandleMovementInput();
    }

    private void HandleRotationInput()
    {
        Vector2 lastDirection = currentDirection;

        // Проверяем нажатия клавиш для поворота
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentDirection = Vector2.up;
            transform.rotation = Quaternion.Euler(0, 0, 0); // Вверх
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentDirection = Vector2.down;
            transform.rotation = Quaternion.Euler(0, 0, 180); // Вниз
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentDirection = Vector2.left;
            transform.rotation = Quaternion.Euler(0, 0, 90); // Влево
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentDirection = Vector2.right;
            transform.rotation = Quaternion.Euler(0, 0, -90); // Вправо
        }

        // Если направление изменилось, обновляем движение
        if (lastDirection != currentDirection && isMoving)
        {
            UpdateMovement();
        }
    }

    private void HandleMovementInput()
    {
        bool wasMoving = isMoving;
        
        // Проверяем, нажата ли какая-либо клавиша движения
        isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ||
                  Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ||
                  Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
                  Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // Если состояние движения изменилось
        if (wasMoving != isMoving)
        {
            if (isMoving)
            {
                UpdateMovement(); // Начинаем движение
            }
            else
            {
                StopMovement(); // Останавливаемся
            }
        }
    }

    private void UpdateMovement()
    {
        if (isMoving)
        {
            // Определяем фактическое направление движения на основе нажатых клавиш
            Vector2 movementDirection = GetMovementDirectionFromInput();
            if (movementDirection != Vector2.zero)
            {
                currentDirection = movementDirection;
                // Обновляем поворот в соответствии с направлением движения
                UpdateRotationFromDirection(currentDirection);
            }
        }
    }

    private Vector2 GetMovementDirectionFromInput()
    {
        // Определяем направление на основе активных клавиш
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) return Vector2.up;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) return Vector2.down;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) return Vector2.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return Vector2.right;
        
        return currentDirection; // Если нет активных клавиш, возвращаем текущее направление
    }

    private void UpdateRotationFromDirection(Vector2 direction)
    {
        // Обновляем поворот в соответствии с направлением
        if (direction == Vector2.up)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (direction == Vector2.down)
            transform.rotation = Quaternion.Euler(0, 0, 180);
        else if (direction == Vector2.left)
            transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (direction == Vector2.right)
            transform.rotation = Quaternion.Euler(0, 0, -90);
    }

    private void MoveTank()
    {
        // Двигаем танк в текущем направлении
        rb.linearVelocity = currentDirection * moveSpeed;
        rb.transform.position += new Vector3(rb.linearVelocityX, rb.linearVelocityY, 0) * Time.deltaTime;
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        isMoving = false;
    }

    private void HandleShooting()
    {
        if (isPlayer && Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    public void Shoot()
    {
        if (firePoint != null)
        {
            BulletManager.Instance.SpawnBullet(firePoint.position, currentDirection, isPlayer);
        }
        else
        {
            Debug.LogError("FirePoint is null!");
        }
    }

    // Метод для AI чтобы установить направление и начать движение
    public void SetAIDirectionAndMove(Vector2 direction)
    {
        if (!isPlayer)
        {
            currentDirection = direction.normalized;
            UpdateRotationFromDirection(currentDirection);
            isMoving = true;
            MoveTank();
        }
    }

    // Метод для AI чтобы остановиться
    public void StopAI()
    {
        if (!isPlayer)
        {
            isMoving = false;
            StopMovement();
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        OnHealthChanged?.Invoke(health);

        if (health <= 0)
        {
            DestroyTank();
        }
    }

    private void DestroyTank()
    {
        canMove = false;
        isMoving = false;
        StopMovement();
        OnTankDestroyed?.Invoke();

        if (isPlayer)
        {
            GameManager.Instance.PlayerDied();
        }

        // Эффекты разрушения
        Destroy(gameObject);
    }

    public void SetMovement(bool allowMovement)
    {
        canMove = allowMovement;
        if (!allowMovement)
        {
            isMoving = false;
            StopMovement();
        }
    }

    // Устаревший метод - оставлен для совместимости
    public void SetMovementInput(Vector2 input)
    {
        // Для AI используем новый метод
        if (!isPlayer)
        {
            SetAIDirectionAndMove(input);
        }
    }
    
    public void ActivateShield(float duration)
    {
        Debug.Log($"Shield activated for {duration} seconds");
    }
    
    public void BoostSpeed(float duration, float multiplier)
    {
        moveSpeed *= multiplier;
        Debug.Log($"Speed boosted by {multiplier}x for {duration} seconds");
        Invoke(nameof(ResetSpeed), duration);
    }
    
    private void ResetSpeed()
    {
        moveSpeed = 5f;
    }
    
    public void BoostFireRate(float duration, float multiplier)
    {
        float originalFireRate = fireRate;
        fireRate *= multiplier;
        Debug.Log($"Fire rate boosted by {multiplier}x for {duration} seconds");
        Invoke(nameof(ResetFireRate), duration);
    }
    
    private void ResetFireRate()
    {
        fireRate = 0.5f;
    }
    
    public void ActivatePowerBullet(float duration)
    {
        Debug.Log($"Power bullet activated for {duration} seconds");
    }
}