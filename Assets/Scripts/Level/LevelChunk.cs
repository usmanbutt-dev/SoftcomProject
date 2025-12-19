using UnityEngine;

namespace OneButtonRunner.Level
{
    /// <summary>
    /// Attach this to each Level Chunk prefab.
    /// Defines the chunk's properties and boundaries.
    /// </summary>
    public class LevelChunk : MonoBehaviour
    {
        [Header("Chunk Properties")]
        [Tooltip("How difficult is this chunk? (1=Easy, 10=Hard)")]
        [Range(1, 10)]
        public int difficulty = 1;

        [Tooltip("Width of this chunk in units")]
        public float chunkLength = 20f;

        [Tooltip("Optional: Tag for special chunks (e.g., 'Boss', 'Rest')")]
        public string chunkType = "Normal";

        [Header("Connection Points")]
        [Tooltip("Right edge of this chunk (where next chunk connects)")]
        public Transform exitPoint;

        /// <summary>
        /// Get the world X position of the exit point
        /// </summary>
        public float GetExitX()
        {
            if (exitPoint != null)
                return exitPoint.position.x;
            
            // Fallback: use chunk position + length
            return transform.position.x + chunkLength;
        }

        /// <summary>
        /// Position this chunk so its left edge starts at the given X
        /// </summary>
        public void PositionAt(float startX)
        {
            transform.position = new Vector3(startX, 0, 0);
        }

        private void OnDrawGizmos()
        {
            // Visualize chunk bounds in editor
            Gizmos.color = Color.cyan;
            Vector3 center = transform.position + new Vector3(chunkLength / 2f, 0, 0);
            Gizmos.DrawWireCube(center, new Vector3(chunkLength, 10f, 0.1f));

            // Exit point
            if (exitPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(exitPoint.position, 0.5f);
            }
        }
    }
}
