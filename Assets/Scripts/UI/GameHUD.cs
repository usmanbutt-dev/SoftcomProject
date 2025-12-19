using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneButtonRunner.Core;
using OneButtonRunner.Player;

namespace OneButtonRunner.UI
{
    /// <summary>
    /// Main HUD showing health, score, and charge meter.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Health UI")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFill;
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private string scoreFormat = "Distance: {0:F0}m";

        [Header("Charge Meter")]
        [SerializeField] private Slider chargeMeter;
        [SerializeField] private Image chargeFill;
        [SerializeField] private float maxChargeTime = 1f;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Start Panel")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private Button startButton;

        private void Start()
        {
            // Setup button listeners
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            // Subscribe to events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                GameManager.Instance.OnScoreChanged += OnScoreChanged;
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnHealthChanged += OnHealthChanged;
            }

            // Initial state
            ShowStartPanel();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
                GameManager.Instance.OnScoreChanged -= OnScoreChanged;
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnHealthChanged -= OnHealthChanged;
            }
        }

        private void Update()
        {
            UpdateChargeMeter();
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    ShowStartPanel();
                    break;

                case GameState.Playing:
                    HideAllPanels();
                    break;

                case GameState.GameOver:
                    ShowGameOverPanel();
                    break;
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthFill != null)
            {
                float healthPercent = (float)current / max;
                healthFill.color = Color.Lerp(lowHealthColor, healthyColor, healthPercent);
            }
        }

        private void OnScoreChanged(float score)
        {
            if (scoreText != null)
            {
                scoreText.text = string.Format(scoreFormat, score);
            }
        }

        private void UpdateChargeMeter()
        {
            if (chargeMeter == null) return;

            if (InputManager.Instance != null && InputManager.Instance.IsCharging)
            {
                chargeMeter.gameObject.SetActive(true);
                float chargeTime = InputManager.Instance.GetChargeTime();
                chargeMeter.value = Mathf.Clamp01(chargeTime / maxChargeTime);

                // Color feedback
                if (chargeFill != null)
                {
                    chargeFill.color = Color.Lerp(Color.yellow, Color.red, chargeMeter.value);
                }
            }
            else
            {
                chargeMeter.gameObject.SetActive(false);
            }
        }

        private void ShowStartPanel()
        {
            if (startPanel != null) startPanel.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void ShowGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (finalScoreText != null && GameManager.Instance != null)
                {
                    finalScoreText.text = $"Final Distance: {GameManager.Instance.Score:F0}m";
                }
            }
            if (startPanel != null) startPanel.SetActive(false);
        }

        private void HideAllPanels()
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void OnStartClicked()
        {
            GameManager.Instance?.StartGame();
        }

        private void OnRestartClicked()
        {
            GameManager.Instance?.RestartGame();
        }

        private void OnMenuClicked()
        {
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
