using System.Collections.Generic;
using UnityEngine;
using OneButtonRunner.Core;
using OneButtonRunner.Player;

namespace OneButtonRunner.Level
{
    /// <summary>
    /// Manages chunk-based level generation.
    /// Spawns pre-designed chunks ahead of player and despawns behind.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Chunk Prefabs")]
        [Tooltip("Drag all your chunk prefabs here")]
        [SerializeField] private LevelChunk[] chunkPrefabs;

        [Header("Spawn Settings")]
        [SerializeField] private float chunkSpacing = 6.3f; // Distance between chunk origins
        [SerializeField] private float spawnAheadDistance = 50f;
        [SerializeField] private float despawnBehindDistance = 20f;

        [Header("Difficulty Progression")]
        [Tooltip("Time in seconds to reach max difficulty")]
        [SerializeField] private float timeToMaxDifficulty = 120f;
        [Tooltip("Max difficulty level that can spawn (1-10)")]
        [SerializeField] private int maxDifficultyLevel = 10;

        // Active chunks
        private List<LevelChunk> activeChunks = new List<LevelChunk>();
        private float nextSpawnX = 0f;
        private Transform playerTransform;
        private float gameStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            Debug.Log("[LevelManager] ✓ Starting chunk-based level generation");

            // Find player
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }
            else
            {
                Debug.LogError("[LevelManager] ✗ No PlayerController found!");
            }

            // Validate chunks
            if (chunkPrefabs == null || chunkPrefabs.Length == 0)
            {
                Debug.LogError("[LevelManager] ✗ No chunk prefabs assigned!");
                return;
            }

            Debug.Log($"[LevelManager] Loaded {chunkPrefabs.Length} chunk prefabs");

            gameStartTime = Time.time;

            // Spawn initial chunks
            SpawnInitialChunks();
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;
            if (playerTransform == null) return;

            // Spawn chunks ahead
            while (nextSpawnX < playerTransform.position.x + spawnAheadDistance)
            {
                SpawnNextChunk();
            }

            // Despawn chunks behind
            DespawnOldChunks();
        }

        private void SpawnInitialChunks()
        {
            // First chunk at origin (0, 0, 0)
            nextSpawnX = 0f;

            // Spawn initial chunks
            for (int i = 0; i < 3; i++)
            {
                SpawnChunk();
            }
        }

        private void SpawnNextChunk()
        {
            SpawnChunk();
        }

        private void SpawnChunk()
        {
            // Pick a random chunk prefab
            LevelChunk prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];

            // Instantiate at current spawn position
            LevelChunk newChunk = Instantiate(prefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity, transform);
            activeChunks.Add(newChunk);

            Debug.Log($"[LevelManager] Spawned chunk at X={nextSpawnX:F1}");

            // Next chunk at +6 units
            nextSpawnX += chunkSpacing;
        }

        private void DespawnOldChunks()
        {
            float despawnX = playerTransform.position.x - despawnBehindDistance;

            for (int i = activeChunks.Count - 1; i >= 0; i--)
            {
                // Use chunk position + spacing as the "end" of the chunk
                float chunkEndX = activeChunks[i].transform.position.x + chunkSpacing;
                if (chunkEndX < despawnX)
                {
                    Destroy(activeChunks[i].gameObject);
                    activeChunks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Calculate what difficulty level is allowed based on game time
        /// </summary>
        private int GetCurrentMaxDifficulty()
        {
            float elapsed = Time.time - gameStartTime;
            float progress = Mathf.Clamp01(elapsed / timeToMaxDifficulty);
            
            // Gradually increase from 1 to maxDifficultyLevel
            int difficulty = Mathf.RoundToInt(1 + (maxDifficultyLevel - 1) * progress);
            return Mathf.Clamp(difficulty, 1, maxDifficultyLevel);
        }

        /// <summary>
        /// Reset the level (for game restart)
        /// </summary>
        public void ResetLevel()
        {
            // Destroy all chunks
            foreach (var chunk in activeChunks)
            {
                if (chunk != null)
                    Destroy(chunk.gameObject);
            }
            activeChunks.Clear();

            nextSpawnX = 0f;
            gameStartTime = Time.time;

            SpawnInitialChunks();
        }
    }
}
