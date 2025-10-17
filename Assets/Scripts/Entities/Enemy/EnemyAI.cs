using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum AIState { Patrol, Chase, Attack, Escape }
    
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float decisionInterval = 3f;
    [SerializeField] private float patrolChangeInterval = 4f;
    [SerializeField] private float shootingCooldown = 2f;
    [SerializeField] private float chaseProbability = 0.7f; // Вероятность преследования вместо патруля
    
    private TankController tankController;
    private Transform player;
    private AIState currentState;
    private float nextDecisionTime;
    private float nextPatrolChangeTime;
    private float nextShootTime;
    private Vector2 currentDirection;
    private float stateChangeCooldown;
    
    private void Awake()
    {
        tankController = GetComponent<TankController>();
    }
    
    private void Start()
    {
        FindPlayer();
        
        // Начальное случайное направление
        currentDirection = GetRandomDirection();
        tankController.SetAIDirectionAndMove(currentDirection);
        nextPatrolChangeTime = Time.time + patrolChangeInterval;
        nextShootTime = Time.time + 1f; // Задержка перед первой стрельбой
        
        Debug.Log($"🤖 Enemy spawned. Starting in Patrol mode.");
    }
    
    private void Update()
    {
        if (player == null) 
        {
            FindPlayer();
        }
        
        // Принимаем решения с интервалом
        if (Time.time >= nextDecisionTime)
        {
            MakeDecision();
            nextDecisionTime = Time.time + decisionInterval;
        }
        
        ExecuteState();
        
        // Периодически меняем направление патрулирования
        if (Time.time >= nextPatrolChangeTime && currentState == AIState.Patrol)
        {
            ChangePatrolDirection();
            nextPatrolChangeTime = Time.time + patrolChangeInterval;
        }
        
        // Случайная смена направления даже в режиме преследования (чтобы не был слишком прямолинейным)
        if (currentState == AIState.Chase && Time.time >= nextPatrolChangeTime)
        {
            if (Random.Range(0f, 1f) < 0.3f) // 30% шанс сменить направление
            {
                ChangePatrolDirection();
            }
            nextPatrolChangeTime = Time.time + patrolChangeInterval;
        }
    }
    
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    private void MakeDecision()
    {
        if (player == null) 
        {
            currentState = AIState.Patrol;
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Более сложная логика принятия решений
        if (distanceToPlayer <= attackRange)
        {
            // В зоне атаки - атакуем с вероятностью
            if (Random.Range(0f, 1f) < 0.8f) // 80% шанс атаки
            {
                currentState = AIState.Attack;
                tankController.StopAI();
            }
            else
            {
                // Иногда отступаем даже в зоне атаки
                currentState = AIState.Escape;
            }
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // В зоне обнаружения - решаем что делать
            if (Random.Range(0f, 1f) < chaseProbability)
            {
                currentState = AIState.Chase;
            }
            else
            {
                // Иногда игнорируем игрока и продолжаем патруль
                currentState = AIState.Patrol;
            }
        }
        else
        {
            // Вне зоны обнаружения - патрулируем
            currentState = AIState.Patrol;
        }
        
        Debug.Log($"🤖 Enemy: {currentState}, Distance: {distanceToPlayer:F1}");
    }
    
    private void ExecuteState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                PatrolBehavior();
                break;
            case AIState.Chase:
                ChaseBehavior();
                break;
            case AIState.Attack:
                AttackBehavior();
                break;
            case AIState.Escape:
                EscapeBehavior();
                break;
        }
    }
    
    private void PatrolBehavior()
    {
        // Просто продолжаем движение в текущем направлении
        // Изредка стреляем наугад
        if (Time.time >= nextShootTime && Random.Range(0f, 1f) < 0.01f)
        {
            TryShoot();
        }
    }
    
    private void ChaseBehavior()
    {
        // Более "ленивое" преследование - не всегда сразу к игроку
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        
        // С вероятностью 70% двигаемся к игроку, 30% - случайное направление
        if (Random.Range(0f, 1f) < 0.7f)
        {
            Vector2 discreteDirection = GetDiscreteDirection(directionToPlayer);
            if (discreteDirection != currentDirection)
            {
                currentDirection = discreteDirection;
                tankController.SetAIDirectionAndMove(currentDirection);
            }
        }
        
        // Стрельба во время преследования
        if (Time.time >= nextShootTime && Random.Range(0f, 1f) < 0.03f)
        {
            TryShoot();
        }
    }
    
    private void AttackBehavior()
    {
        // В режиме атаки иногда меняем позицию
        if (Random.Range(0f, 1f) < 0.2f) // 20% шанс сдвинуться
        {
            Vector2 dodgeDirection = GetRandomDirection();
            tankController.SetAIDirectionAndMove(dodgeDirection);
            
            // Через 1 секунду возвращаемся к атаке
            Invoke(nameof(ReturnToAttack), 1f);
        }
        
        // Активная стрельба в режиме атаки
        if (Time.time >= nextShootTime && Random.Range(0f, 1f) < 0.08f)
        {
            TryShoot();
        }
    }
    
    private void ReturnToAttack()
    {
        if (currentState == AIState.Attack)
        {
            tankController.StopAI();
        }
    }
    
    private void EscapeBehavior()
    {
        // Бегство от игрока, но не всегда прямо от него
        Vector2 directionAwayFromPlayer = (transform.position - player.position).normalized;
        
        // 60% - бегство от игрока, 40% - случайное направление
        if (Random.Range(0f, 1f) < 0.6f)
        {
            Vector2 discreteDirection = GetDiscreteDirection(directionAwayFromPlayer);
            if (discreteDirection != currentDirection)
            {
                currentDirection = discreteDirection;
                tankController.SetAIDirectionAndMove(currentDirection);
            }
        }
        else
        {
            // Случайное направление бегства
            ChangePatrolDirection();
        }
        
        // Во время бегства тоже можно стрелять
        if (Time.time >= nextShootTime && Random.Range(0f, 1f) < 0.02f)
        {
            TryShoot();
        }
    }
    
    private void ChangePatrolDirection()
    {
        Vector2 newDirection = GetRandomDirection();
        
        // Не меняем направление на противоположное (чтобы не ходить туда-сюда)
        if (newDirection != -currentDirection || Random.Range(0f, 1f) < 0.3f)
        {
            currentDirection = newDirection;
            tankController.SetAIDirectionAndMove(currentDirection);
        }
    }
    
    private void TryShoot()
    {
        if (Time.time >= nextShootTime)
        {
            tankController.Shoot();
            nextShootTime = Time.time + shootingCooldown + Random.Range(-0.5f, 0.5f); // Небольшая случайность
        }
    }
    
    private Vector2 GetRandomDirection()
    {
        int direction = Random.Range(0, 4);
        switch (direction)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            default: return Vector2.up;
        }
    }
    
    private Vector2 GetDiscreteDirection(Vector2 continuousDirection)
    {
        float absX = Mathf.Abs(continuousDirection.x);
        float absY = Mathf.Abs(continuousDirection.y);
        
        // Предпочтение горизонтальному движению для более интересного поведения
        if (absX > absY * 0.8f) // Небольшое предпочтение горизонтали
        {
            return continuousDirection.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            return continuousDirection.y > 0 ? Vector2.up : Vector2.down;
        }
    }
    
    // Визуализация зон в редакторе
    private void OnDrawGizmosSelected()
    {
        // Зона обнаружения (желтая)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Зона атаки (красная)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Текущее направление
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, currentDirection * 1.5f);
        
        // Текущее состояние
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"State: {currentState}");
        #endif
    }
}