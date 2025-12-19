using System.Collections.Generic;
using UnityEngine;
using OneButtonRunner.Core;
using OneButtonRunner.Player;

namespace OneButtonRunner.Level
{
    /// <summary>
    /// Generates endless platforms and manages the scrolling world.
    /// Uses object pooling for performance.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Platform Settings")]
        [SerializeField] private GameObject platformPrefab;
        [SerializeField] private int platformPoolSize = 10;
        [SerializeField] private float platformWidth = 10f;
        [SerializeField] private float groundY = -3f;
        [SerializeField] private float ceilingY = 3f;

        [Header("Generation Settings")]
        [SerializeField] private float spawnAheadDistance = 30f;
        [SerializeField] private float despawnBehindDistance = 10f;

        private Queue<GameObject> platformPool = new Queue<GameObject>();
        private List<GameObject> activePlatforms = new List<GameObject>();
        private float nextSpawnX;
        private Transform playerTransform;

        private void Start()
        {
            // Initialize pool
            InitializePlatformPool();

            // Find player
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }

            // Spawn initial platforms
            nextSpawnX = -platformWidth;
            for (int i = 0; i < 5; i++)
            {
                SpawnPlatformPair();
            }
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;
            if (playerTransform == null) return;

            // Spawn new platforms ahead
            while (nextSpawnX < playerTransform.position.x + spawnAheadDistance)
            {
                SpawnPlatformPair();
            }

            // Despawn platforms behind
            DespawnOldPlatforms();
        }

        private void InitializePlatformPool()
        {
            for (int i = 0; i < platformPoolSize; i++)
            {
                GameObject platform = Instantiate(platformPrefab, Vector3.zero, Quaternion.identity, transform);
                platform.SetActive(false);
                platformPool.Enqueue(platform);
            }
        }

        private GameObject GetPooledPlatform()
        {
            if (platformPool.Count > 0)
            {
                return platformPool.Dequeue();
            }

            // Pool exhausted - create new one
            GameObject platform = Instantiate(platformPrefab, Vector3.zero, Quaternion.identity, transform);
            return platform;
        }

        private void ReturnToPool(GameObject platform)
        {
            platform.SetActive(false);
            platformPool.Enqueue(platform);
        }

        private void SpawnPlatformPair()
        {
            // Spawn ground platform
            SpawnPlatform(new Vector3(nextSpawnX, groundY, 0), false);

            // Spawn ceiling platform
            SpawnPlatform(new Vector3(nextSpawnX, ceilingY, 0), true);

            nextSpawnX += platformWidth;
        }

        private void SpawnPlatform(Vector3 position, bool isCeiling)
        {
            GameObject platform = GetPooledPlatform();
            platform.transform.position = position;

            if (isCeiling)
            {
                // Flip ceiling platforms
                platform.transform.localScale = new Vector3(1, -1, 1);
            }
            else
            {
                platform.transform.localScale = Vector3.one;
            }

            platform.SetActive(true);
            activePlatforms.Add(platform);
        }

        private void DespawnOldPlatforms()
        {
            if (playerTransform == null) return;

            float despawnX = playerTransform.position.x - despawnBehindDistance;

            for (int i = activePlatforms.Count - 1; i >= 0; i--)
            {
                if (activePlatforms[i].transform.position.x < despawnX)
                {
                    ReturnToPool(activePlatforms[i]);
                    activePlatforms.RemoveAt(i);
                }
            }
        }

        public void ResetLevel()
        {
            // Return all active platforms to pool
            foreach (var platform in activePlatforms)
            {
                ReturnToPool(platform);
            }
            activePlatforms.Clear();
            nextSpawnX = -platformWidth;

            // Spawn initial platforms
            for (int i = 0; i < 5; i++)
            {
                SpawnPlatformPair();
            }
        }
    }
}
