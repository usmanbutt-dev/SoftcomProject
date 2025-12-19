using UnityEngine;
using OneButtonRunner.Core;

namespace OneButtonRunner.Enemies
{
    /// <summary>
    /// Basic Enemy - Slow, low health, dies to any attack.
    /// </summary>
    public class BasicEnemy : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Basic;

            // Override stats from constants
            maxHealth = GameConstants.BASIC_ENEMY_HEALTH;
            damage = GameConstants.BASIC_ENEMY_DAMAGE;
            moveSpeed = GameConstants.BASIC_ENEMY_SPEED;
            immuneToLightAttacks = false;
        }

        protected override void Start()
        {
            base.Start();

            // Visual distinction - could be set in prefab instead
            if (spriteRenderer != null)
            {
                normalColor = new Color(0.4f, 0.8f, 0.4f); // Greenish
                spriteRenderer.color = normalColor;
            }
        }
    }

    /// <summary>
    /// Medium Enemy - Fast, medium health, dies to 1 charged or 2 light attacks.
    /// </summary>
    public class MediumEnemy : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Medium;

            // Override stats from constants
            maxHealth = GameConstants.MEDIUM_ENEMY_HEALTH;
            damage = GameConstants.MEDIUM_ENEMY_DAMAGE;
            moveSpeed = GameConstants.MEDIUM_ENEMY_SPEED;
            immuneToLightAttacks = false;
        }

        protected override void Start()
        {
            base.Start();

            // Visual distinction
            if (spriteRenderer != null)
            {
                normalColor = new Color(0.8f, 0.6f, 0.2f); // Orange
                spriteRenderer.color = normalColor;
            }
        }

        protected override void Move()
        {
            // Medium enemies are faster and might have slight variation
            float variation = Mathf.Sin(Time.time * 2f) * 0.5f;
            rb.linearVelocity = new Vector2(-(moveSpeed + variation), rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Unexpected Enemy - High damage, immune to light attacks, only dies to charged.
    /// </summary>
    public class UnexpectedEnemy : EnemyBase
    {
        [Header("Unexpected Behavior")]
        [SerializeField] private float minSpeed = 2f;
        [SerializeField] private float maxSpeed = 6f;
        [SerializeField] private float speedChangeInterval = 1f;

        private float currentSpeed;
        private float nextSpeedChangeTime;

        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Unexpected;

            // Override stats from constants
            maxHealth = GameConstants.UNEXPECTED_ENEMY_HEALTH;
            damage = GameConstants.UNEXPECTED_ENEMY_DAMAGE;
            moveSpeed = GameConstants.UNEXPECTED_ENEMY_SPEED;
            immuneToLightAttacks = true; // KEY: Immune to light attacks!
        }

        protected override void Start()
        {
            base.Start();
            currentSpeed = Random.Range(minSpeed, maxSpeed);
            nextSpeedChangeTime = Time.time + speedChangeInterval;

            // Visual distinction - menacing!
            if (spriteRenderer != null)
            {
                normalColor = new Color(0.8f, 0.2f, 0.2f); // Red
                spriteRenderer.color = normalColor;
            }
        }

        protected override void Move()
        {
            // Unpredictable speed changes
            if (Time.time >= nextSpeedChangeTime)
            {
                currentSpeed = Random.Range(minSpeed, maxSpeed);
                nextSpeedChangeTime = Time.time + speedChangeInterval;
            }

            rb.linearVelocity = new Vector2(-currentSpeed, rb.linearVelocity.y);
        }

        public override void TakeDamage(int damageAmount, bool isChargedAttack)
        {
            if (!isChargedAttack)
            {
                // Show player their attack is useless
                Debug.Log("[UnexpectedEnemy] Light attack has no effect!");
                FlashColor(Color.gray);

                // Maybe taunt the player? Speed up briefly?
                currentSpeed = maxSpeed;
                return;
            }

            // Charged attacks work normally
            base.TakeDamage(damageAmount, isChargedAttack);
        }

        protected override void Die()
        {
            // Add extra effects for killing the tough enemy
            Debug.Log("[UnexpectedEnemy] The mighty one falls!");
            // Could add screen shake, special particles here

            base.Die();
        }
    }
}
