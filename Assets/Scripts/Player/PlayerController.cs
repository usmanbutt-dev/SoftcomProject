using UnityEngine;
using OneButtonRunner.Core;

namespace OneButtonRunner.Player
{
    public enum PlayerState
    {
        Running,
        Charging,
        Attacking,
        Stunned
    }

    /// <summary>
    /// Main player controller - handles movement, gravity, attacks, and health.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Gravity Flip")]
        [SerializeField] private float flipDuration = 0.3f;

        [Header("Combat")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float lightAttackRange = 1.5f;
        [SerializeField] private float chargedAttackRange = 2.5f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float knockbackForce = 3f;
        [SerializeField] private float invincibilityTime = 1f;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color chargingColor = Color.yellow;
        [SerializeField] private Color hurtColor = Color.red;

        // Component references
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;

        // State
        public PlayerState CurrentState { get; private set; } = PlayerState.Running;
        public int CurrentHealth { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsGravityFlipped { get; private set; }

        // Internal tracking
        private bool isInvincible;
        private float invincibilityTimer;
        private float chargeStartTime;

        // Events
        public System.Action<int, int> OnHealthChanged; // current, max
        public System.Action OnPlayerDied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            // Subscribe to input events
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLightAttack += PerformLightAttack;
                InputManager.Instance.OnChargeStart += StartCharging;
                InputManager.Instance.OnChargedAttackRelease += PerformChargedAttack;
                InputManager.Instance.OnGravityFlip += FlipGravity;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLightAttack -= PerformLightAttack;
                InputManager.Instance.OnChargeStart -= StartCharging;
                InputManager.Instance.OnChargedAttackRelease -= PerformChargedAttack;
                InputManager.Instance.OnGravityFlip -= FlipGravity;
            }
        }

        private void Update()
        {
            // Handle invincibility timer
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                    SetSpriteColor(normalColor);
                }
                else
                {
                    // Flash effect during invincibility
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                    spriteRenderer.color = new Color(normalColor.r, normalColor.g, normalColor.b, alpha);
                }
            }

            // Visual feedback for charging
            if (CurrentState == PlayerState.Charging)
            {
                float chargeProgress = (Time.time - chargeStartTime) / 1f; // 1 second for full charge visual
                spriteRenderer.color = Color.Lerp(normalColor, chargingColor, Mathf.Clamp01(chargeProgress));
            }
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;
            if (CurrentState == PlayerState.Stunned) return;

            // Auto-run forward
            MoveForward();

            // Check grounded
            CheckGrounded();
        }

        #region Movement

        private void MoveForward()
        {
            float speed = GameManager.Instance?.CurrentSpeed ?? moveSpeed;
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }

        private void CheckGrounded()
        {
            // Cast down (or up if flipped) to check for ground
            Vector2 direction = IsGravityFlipped ? Vector2.up : Vector2.down;
            float distance = boxCollider.bounds.extents.y + 0.1f;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, LayerMask.GetMask("Ground"));
            IsGrounded = hit.collider != null;
        }

        #endregion

        #region Gravity Flip

        private void FlipGravity()
        {
            if (CurrentState == PlayerState.Stunned) return;

            IsGravityFlipped = !IsGravityFlipped;

            // Flip gravity
            rb.gravityScale = IsGravityFlipped ? -1f : 1f;

            // Flip sprite
            Vector3 scale = transform.localScale;
            scale.y = IsGravityFlipped ? -Mathf.Abs(scale.y) : Mathf.Abs(scale.y);
            transform.localScale = scale;

            Debug.Log($"[Player] Gravity flipped! Now: {(IsGravityFlipped ? "Inverted" : "Normal")}");
        }

        #endregion

        #region Combat

        private void StartCharging()
        {
            if (CurrentState == PlayerState.Stunned) return;

            CurrentState = PlayerState.Charging;
            chargeStartTime = Time.time;
            Debug.Log("[Player] Started charging...");
        }

        private void PerformLightAttack()
        {
            if (CurrentState == PlayerState.Stunned) return;

            CurrentState = PlayerState.Attacking;
            Debug.Log("[Player] Light Attack!");

            // Detect enemies in range
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                attackPoint != null ? attackPoint.position : transform.position,
                lightAttackRange,
                enemyLayer
            );

            foreach (Collider2D enemy in hitEnemies)
            {
                var enemyBase = enemy.GetComponent<Enemies.EnemyBase>();
                if (enemyBase != null)
                {
                    enemyBase.TakeDamage(GameConstants.LIGHT_ATTACK_DAMAGE, false);
                }
            }

            // Return to running
            CurrentState = PlayerState.Running;
            SetSpriteColor(normalColor);
        }

        private void PerformChargedAttack()
        {
            if (CurrentState != PlayerState.Charging) return;

            CurrentState = PlayerState.Attacking;
            Debug.Log("[Player] CHARGED Attack!");

            // Detect enemies in larger range
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                attackPoint != null ? attackPoint.position : transform.position,
                chargedAttackRange,
                enemyLayer
            );

            foreach (Collider2D enemy in hitEnemies)
            {
                var enemyBase = enemy.GetComponent<Enemies.EnemyBase>();
                if (enemyBase != null)
                {
                    enemyBase.TakeDamage(GameConstants.CHARGED_ATTACK_DAMAGE, true);
                }
            }

            // Return to running
            CurrentState = PlayerState.Running;
            SetSpriteColor(normalColor);
        }

        #endregion

        #region Health & Damage

        public void TakeDamage(int damage, Vector2 knockbackDirection)
        {
            if (isInvincible) return;

            CurrentHealth -= damage;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            Debug.Log($"[Player] Took {damage} damage! Health: {CurrentHealth}/{maxHealth}");

            if (CurrentHealth <= 0)
            {
                Die();
                return;
            }

            // Knockback
            ApplyKnockback(knockbackDirection);

            // Invincibility frames
            isInvincible = true;
            invincibilityTimer = invincibilityTime;
            SetSpriteColor(hurtColor);
        }

        private void ApplyKnockback(Vector2 direction)
        {
            CurrentState = PlayerState.Stunned;

            // Apply knockback force
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);

            // Recover after short delay
            Invoke(nameof(RecoverFromStun), 0.3f);
        }

        private void RecoverFromStun()
        {
            if (CurrentHealth > 0)
            {
                CurrentState = PlayerState.Running;
            }
        }

        private void Die()
        {
            CurrentState = PlayerState.Stunned;
            Debug.Log("[Player] DIED!");
            OnPlayerDied?.Invoke();
            GameManager.Instance?.GameOver();
        }

        #endregion

        #region Helpers

        private void SetSpriteColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize attack ranges in editor
            Vector3 point = attackPoint != null ? attackPoint.position : transform.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(point, lightAttackRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point, chargedAttackRange);
        }

        #endregion
    }
}
