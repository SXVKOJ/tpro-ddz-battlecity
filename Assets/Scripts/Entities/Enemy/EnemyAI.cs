using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum AIState { Patrol, Chase, Attack, Escape }
    
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float decisionInterval = 2f;
    [SerializeField] private float patrolChangeInterval = 3f;
    
    private TankController tankController;
    private Transform player;
    private AIState currentState;
    private float nextDecisionTime;
    private float nextPatrolChangeTime;
    private Vector2 currentDirection;
    
    private void Awake()
    {
        tankController = GetComponent<TankController>();
        FindPlayer();
    }
    
    private void Start()
    {
        // Начальное случайное направление
        currentDirection = GetRandomDirection();
        tankController.SetAIDirectionAndMove(currentDirection);
        nextPatrolChangeTime = Time.time + patrolChangeInterval;
    }
    
    private void Update()
    {
        if (player == null) 
        {
            FindPlayer();
        }
        
        if (Time.time >= nextDecisionTime)
        {
            MakeDecision();
            nextDecisionTime = Time.time + decisionInterval;
        }
        
        ExecuteState();
        
        // Периодически меняем направление патрулирования
        if (Time.time >= nextPatrolChangeTime && currentState == AIState.Patrol)
        {
            currentDirection = GetRandomDirection();
            tankController.SetAIDirectionAndMove(currentDirection);
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
        
        if (distanceToPlayer <= attackRange)
        {
            currentState = AIState.Attack;
            tankController.StopAI(); // Останавливаемся для атаки
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = AIState.Chase;
        }
        else
        {
            currentState = AIState.Patrol;
        }
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
        // Продолжаем движение в текущем направлении
        // Движение уже установлено в SetAIDirectionAndMove
        
        // Случайный выстрел
        if (Random.Range(0f, 1f) < 0.02f)
        {
            tankController.Shoot();
        }
    }
    
    private void ChaseBehavior()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 discreteDirection = GetDiscreteDirection(directionToPlayer);
        
        if (discreteDirection != currentDirection)
        {
            currentDirection = discreteDirection;
            tankController.SetAIDirectionAndMove(currentDirection);
        }
        
        // Стрельба во время преследования
        if (Random.Range(0f, 1f) < 0.05f)
        {
            tankController.Shoot();
        }
    }
    
    private void AttackBehavior()
    {
        // Останавливаемся и стреляем
        // Остановка уже выполнена в MakeDecision()
        
        // Частая стрельба в режиме атаки
        if (Random.Range(0f, 1f) < 0.1f)
        {
            tankController.Shoot();
        }
    }
    
    private void EscapeBehavior()
    {
        // Бегство от игрока
        Vector2 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector2 discreteDirection = GetDiscreteDirection(directionAwayFromPlayer);
        
        if (discreteDirection != currentDirection)
        {
            currentDirection = discreteDirection;
            tankController.SetAIDirectionAndMove(currentDirection);
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
        
        if (absX > absY)
        {
            return continuousDirection.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            return continuousDirection.y > 0 ? Vector2.up : Vector2.down;
        }
    }
}