using UnityEngine;
using UnityEngine.Events;
using TMPro;
using FabrizioConni.BasketChallenge.Audio;

namespace FabrizioConni.BasketChallenge.Score
{
    /// <summary>
    /// The ScoreSystem class manages the scoring logic for both the player and AI in the basketball challenge game.
    /// It handles score increments, bonus logic, UI updates, and sound effects related to scoring events.
    /// </summary>
    public class ScoreSystem : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private HoopSystem hoopSystem; // Reference to the HoopSystem, used to subscribe to hoop hit events.
        [SerializeField]
        private Collider bonusCollider; // Collider that enables/disables the bonus area.
        [SerializeField]
        private GameObject bonusArea; // GameObject representing the bonus area in the scene.
        [SerializeField]
        private TMP_Text bonusText; // TextMeshPro text component to display the bonus value.
        [SerializeField]
        private UI_Score playerScore; // UI component to display the player's score.
        [SerializeField]
        private UI_Score aiScore; // UI component to display the AI's score.
        #endregion

        #region Private Fields
        private int score; // Player's total score.
        private int currentIncrementScore; // Current score increment for the player (reset after each scoring event).
        private int currentBonus; // Current bonus value applied to the player's score.
        private int aiTotalScore; // AI's total score.
        private int aiCurrentIncrementScore; // Current score increment for the AI (reset after each scoring event).
        private int lastSoundIndex; // Index of the last sound played to avoid repetition.
        private int[] bonusValues = new int[] { 4, 6, 8 }; // Possible bonus values for the player.
        #endregion

        #region Properties
        /// <summary>
        /// Indicates if the Fireball bonus is active, which doubles the player's score increment.
        /// </summary>
        public bool FireballBonus { get; set; }
        #endregion

        #region Actions
        /// <summary>
        /// Event invoked when the score changes. Parameters: (int playerIndex, int totalScore, int incrementScore)
        /// </summary>
        public UnityAction<int, int, int> onScoreChanged;
        #endregion

        #region Monobehaviour Callbacks
        /// <summary>
        /// Subscribes to the hoop hit event on Awake.
        /// </summary>
        private void Awake()
        {
            hoopSystem.hitHoop += OnHitHoop;
        }

        /// <summary>
        /// Handles trigger enter events for scoring. Adds score for player or AI based on the tag.
        /// </summary>
        /// <param name="other">The collider that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            switch (other.gameObject.tag)
            {
                case "Player":
                    AddScore();
                    break;
                case "Enemy":
                    AddAiScore();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles trigger exit events. Updates the UI and resets increment scores for player or AI.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                playerScore.UpdateScore(score);
                onScoreChanged?.Invoke(0, score, currentIncrementScore);
                ResetPlayerIncrementScore();
            }
            else if (other.gameObject.tag == "Enemy")
            {
                aiScore.UpdateScore(aiTotalScore);
                onScoreChanged?.Invoke(1, aiTotalScore, aiCurrentIncrementScore);
                ResetAiIncrementScore();
            }
        }

        /// <summary>
        /// Handles collision events to set the current increment score for player or AI.
        /// </summary>
        /// <param name="other">The collision object.</param>
        private void OnCollisionEnter(Collision other)
        {
            switch (other.gameObject.tag)
            {
                case "Player":
                    currentIncrementScore = 2 + currentBonus;
                    break;
                case "Enemy":
                    aiCurrentIncrementScore = 2;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds the current increment score to the player's total score.
        /// If FireballBonus is active, the increment is doubled.
        /// Plays a sound if this is the first increment for the current scoring event.
        /// </summary>
        private void AddScore()
        {
            if (currentIncrementScore == 0)
            {
                currentIncrementScore = 3;
                AudioMngr.Instance.Play("Basketball_05");
            }

            if (FireballBonus)
            {
                currentIncrementScore *= 2;
            }
            score += currentIncrementScore;
        }

        /// <summary>
        /// Adds the current increment score to the AI's total score.
        /// Plays a sound if this is the first increment for the current scoring event.
        /// </summary>
        private void AddAiScore()
        {
            if (aiCurrentIncrementScore == 0)
            {
                aiCurrentIncrementScore = 3;
                AudioMngr.Instance.Play("Basketball_06");
            }
            aiTotalScore += aiCurrentIncrementScore;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Handles the event when the hoop is hit.
        /// Sets the increment score for player or AI based on the index.
        /// Plays a random sound, ensuring it is not the same as the last one played.
        /// </summary>
        /// <param name="index">0 for player, 1 for AI.</param>
        private void OnHitHoop(int index)
        {
            switch (index)
            {
                case 0:
                    if (currentIncrementScore != 0) return;

                    currentIncrementScore = 2;
                    break;
                case 1:
                    if (aiCurrentIncrementScore != 0) return;

                    aiCurrentIncrementScore = 2;
                    break;
                default:
                    break;
            }
            int randomSound = Random.Range(1, 5);
            while (randomSound == lastSoundIndex)
            {
                randomSound = Random.Range(1, 5);
            }
            AudioMngr.Instance.Play("Basketball_0" + randomSound);
        }

        /// <summary>
        /// Resets both player and AI total scores to zero.
        /// </summary>
        public void ResetScore()
        {
            score = 0;
            aiTotalScore = 0;
        }

        /// <summary>
        /// Resets the player's current increment score to zero.
        /// </summary>
        public void ResetPlayerIncrementScore()
        {
            currentIncrementScore = 0;
        }

        /// <summary>
        /// Resets the AI's current increment score to zero.
        /// </summary>
        public void ResetAiIncrementScore()
        {
            aiCurrentIncrementScore = 0;
        }

        /// <summary>
        /// Enables the bonus area, randomly selects a bonus value, and updates the UI.
        /// </summary>
        public void EnableBonus()
        {
            int index = Random.Range(0, bonusValues.Length);
            int newBonus = bonusValues[index];
            currentBonus = newBonus;
            bonusCollider.enabled = true;
            bonusArea.SetActive(true);
            bonusText.text = "+" + newBonus.ToString();
        }

        /// <summary>
        /// Disables the bonus area and resets the bonus value.
        /// </summary>
        public void DisableBonus()
        {
            currentBonus = 0;
            bonusCollider.enabled = false;
            bonusArea.SetActive(false);
        }
        #endregion
    }
}