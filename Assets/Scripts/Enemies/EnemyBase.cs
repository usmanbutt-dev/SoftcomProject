using UnityEngine;
using OneButtonRunner.Core;
using OneButtonRunner.Player;

namespace OneButtonRunner.Enemies
{
    public enum EnemyType
    {
        Basic,
        Medium,
        Unexpected
    }

    /// <summary>
    /// Base class for all enemies. Handles movement, damage, and player collision.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected int maxHealth = 50;
        [SerializeField] protected int damage = 10;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected bool immuneToLightAttacks = false;

        [Header("Visual")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Color normalColor = Color.white;
        [SerializeField] protected Color hurtColor = Color.red;

        // Properties
        public EnemyType Type { get; protected set; }
        public int CurrentHealth { get; protected set; }
        public bool IsDead { get; protected set; }

        // Components
        protected Rigidbody2D rb;
        protected Collider2D col;

        // Events
        public System.Action<EnemyBase> OnEnemyDied;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Start()
        {
            CurrentHealth = maxHealth;
        }

        protected virtual void FixedUpdate()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;
            if (IsDead) return;

            Move();
        }

        /// <summary>
        /// Override this in subclasses for custom movement patterns
        /// </summary>
        protected virtual void Move()
        {
            // Default: move left (towards player)
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
        }

        /// <summary>
        /// Called when hit by an attack
        /// </summary>
        /// <param name="damageAmount">How much damage the attack deals</param>
        /// <param name="isChargedAttack">Was this a charged attack?</param>
        public virtual void TakeDamage(int damageAmount, bool isChargedAttack)
        {
            if (IsDead) return;

            // Check immunity
            if (immuneToLightAttacks && !isChargedAttack)
            {
                Debug.Log($"[{Type}] Immune to light attacks!");
                // Visual feedback - enemy doesn't take damage
                FlashColor(Color.gray);
                return;
            }

            CurrentHealth -= damageAmount;
            Debug.Log($"[{Type}] Took {damageAmount} damage! Health: {CurrentHealth}/{maxHealth}");

            FlashColor(hurtColor);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            IsDead = true;
            Debug.Log($"[{Type}] Died!");

            OnEnemyDied?.Invoke(this);

            // Simple death - disable and destroy
            col.enabled = false;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;

            // Could add death animation here
            Destroy(gameObject, 0.5f);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsDead) return;

            // Check if we hit the player
            if (collision.gameObject.CompareTag(GameConstants.TAG_PLAYER))
            {
                var player = collision.gameObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Deal damage to player
                    Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                    knockbackDir.x = -1f; // Always knock back (player is moving right)
                    player.TakeDamage(damage, knockbackDir);

                    Debug.Log($"[{Type}] Hit player for {damage} damage!");
                }
            }
        }

        protected void FlashColor(Color flashColor)
        {
            if (spriteRenderer == null) return;

            spriteRenderer.color = flashColor;
            Invoke(nameof(ResetColor), 0.1f);
        }

        private void ResetColor()
        {
            if (spriteRenderer != null)
                spriteRenderer.color = normalColor;
        }

        /// <summary>
        /// Called by spawner to initialize the enemy
        /// </summary>
        public virtual void Initialize(Vector3 spawnPosition, bool isFlipped = false)
        {
            transform.position = spawnPosition;
            IsDead = false;
            CurrentHealth = maxHealth;

            if (isFlipped)
            {
                // Enemy on ceiling
                rb.gravityScale = -1f;
                transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);
            }
            else
            {
                rb.gravityScale = 1f;
                transform.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
            }
        }
    }
}
