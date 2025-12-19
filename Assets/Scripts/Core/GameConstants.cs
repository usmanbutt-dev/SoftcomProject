namespace OneButtonRunner.Core
{
    /// <summary>
    /// Game-wide constants for easy tuning
    /// </summary>
    public static class GameConstants
    {
        // Player
        public const float PLAYER_BASE_SPEED = 5f;
        public const int PLAYER_MAX_HEALTH = 100;
        public const float PLAYER_KNOCKBACK_FORCE = 3f;
        public const float PLAYER_INVINCIBILITY_TIME = 1f;

        // Attacks
        public const int LIGHT_ATTACK_DAMAGE = 25;
        public const int CHARGED_ATTACK_DAMAGE = 100;
        public const float LIGHT_ATTACK_RANGE = 1.5f;
        public const float CHARGED_ATTACK_RANGE = 2.5f;

        // Enemies
        public const int BASIC_ENEMY_HEALTH = 1;
        public const int BASIC_ENEMY_DAMAGE = 10;
        public const float BASIC_ENEMY_SPEED = 2f;

        public const int MEDIUM_ENEMY_HEALTH = 50;
        public const int MEDIUM_ENEMY_DAMAGE = 15;
        public const float MEDIUM_ENEMY_SPEED = 4f;

        public const int UNEXPECTED_ENEMY_HEALTH = 100;
        public const int UNEXPECTED_ENEMY_DAMAGE = 40;
        public const float UNEXPECTED_ENEMY_SPEED = 3f;

        // Gravity
        public const float GRAVITY_FLIP_DURATION = 0.3f;

        // Tags
        public const string TAG_PLAYER = "Player";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_GROUND = "Ground";
    }
}
