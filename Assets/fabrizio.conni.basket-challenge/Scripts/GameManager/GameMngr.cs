using FabrizioConni.BasketChallenge.Ball;
using UnityEngine;
using UnityEngine.SceneManagement;
using FabrizioConni.BasketChallenge.Score;
using FabrizioConni.BasketChallenge.Camera;
using FabrizioConni.BasketChallenge.TimerNamespace;
using FabrizioConni.BasketChallenge.AI;
using FabrizioConni.BasketChallenge.Audio;

namespace FabrizioConni.BasketChallenge.GameManager
{
    /// <summary>
    /// Main Game Manager class responsible for handling game state, scene transitions, event subscriptions,
    /// and coordination between core systems (score, timer, ball, AI, camera, fireball, etc.).
    /// </summary>
    public class GameMngr : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField, Tooltip("Delta from start to end bounus time (in Seconds)")]
        private int deltaBounsTime = 10; // Time window for bonus activation (in seconds)
        [SerializeField]
        private int fadeOutDuration = 2; // Duration for background music fade out at the end of the timer
        #endregion

        #region Private Fields
        private Timer gameTimer; // Reference to the Timer component
        private BallController ballController; // Reference to the player's BallController
        private AiController ballAIController; // Reference to the AI BallController
        private ScoreSystem scoreSystem; // Reference to the ScoreSystem
        private FailSystem[] failSystems; // Array of FailSystem components (for fail detection)
        private FixedCamera mainCamera; // Reference to the main camera controller
        private UI_MainMenu Ui_MainMenu; // Reference to the main menu UI
        private UI_Flyer UI_Flyer; // Reference to the UI_Flyer component
        private FireballSystem fireballSystem; // Reference to the FireballSystem
        private int playerScore; // Player's current score
        private int failCount; // Number of player fails
        private int totalShots; // Total shots taken by the player
        private string diffcoulty; // Selected difficulty (typo: should be "difficulty")
        private float[] bonusTime = new float[2]; // [start, end] time window for bonus activation
        private bool bonusActive; // Is the bonus currently active?
        private bool fadeOutStarted; // Has the background music fade out started?
        #endregion

        #region Properties
        /// <summary>
        /// Singleton instance of the GameMngr.
        /// </summary>
        public static GameMngr Instance { get; private set; }
        #endregion

        #region Monobehaviour Callbacks
        /// <summary>
        /// Called on scene start. Subscribes to main menu difficulty change event if in main menu scene.
        /// </summary>
        private void Start()
        {
            Scene curerntscene = SceneManager.GetActiveScene();
            if (curerntscene.buildIndex != 0) return;
            Ui_MainMenu = FindObjectOfType<UI_MainMenu>();
            Ui_MainMenu.onDifficultyChange += OnDifficultyChange;
        }

        /// <summary>
        /// Ensures singleton pattern and persists GameMngr across scenes.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Called when a new scene is loaded. Handles initialization and event (de)subscription per scene.
        /// </summary>
        /// <param name="level">Scene build index</param>
        private void OnLevelWasLoaded(int level)
        {
            switch (level)
            {
                case 0:
                    // Main Menu: subscribe to difficulty change event
                    Ui_MainMenu = FindObjectOfType<UI_MainMenu>();
                    Ui_MainMenu.onDifficultyChange += OnDifficultyChange;
                    break;
                case 1:
                    // Game Scene: initialize all references and systems
                    Init();
                    break;
                case 2:
                    // End Menu: unsubscribe from events and set end stats
                    DelegatesDesubscribiption();
                    FindObjectOfType<UI_EndMenu>().SetStats(totalShots, failCount, playerScore);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes all game systems, finds references, subscribes to events, and starts the game.
        /// </summary>
        private void Init()
        {
            AudioMngr.Instance.PlayBackground();
            // Find references to all required components
            FindReferences();

            // Subscribe to all relevant events
            DelegatesSubscribiption();

            // Initialize camera to follow player and look at hoop
            mainCamera.SetPlayer(ballController.gameObject);
            mainCamera.SetHoopPosition(ballController.HoopCenterLocation);

            UI_Flyer.SetPlayer(ballController.gameObject);

            // Randomly determine the bonus time window
            bonusTime[1] = UnityEngine.Random.Range(5f, 60f);
            bonusTime[0] = bonusTime[1] - 10f;

            // Start the game timer and reset the score
            gameTimer.StartTimer();
            scoreSystem.ResetScore();

            // Initialize AI with selected difficulty
            ballAIController.Init(diffcoulty);
            ballController.ResetBall();
        }

        /// <summary>
        /// Finds and assigns references to all required components in the scene.
        /// </summary>
        private void FindReferences()
        {
            ballController = FindObjectOfType<BallController>();
            ballAIController = FindObjectOfType<AiController>();
            scoreSystem = FindObjectOfType<ScoreSystem>();
            failSystems = FindObjectsOfType<FailSystem>();
            mainCamera = FindObjectOfType<FixedCamera>();
            gameTimer = GameObject.Find("Timer").GetComponent<Timer>();
            fireballSystem = FindObjectOfType<FireballSystem>();
            UI_Flyer = FindObjectOfType<UI_Flyer>(true);
        }
        #endregion

        #region Delegate Subscription/Desubscription
        /// <summary>
        /// Subscribes to all relevant events from FailSystem, ScoreSystem, BallController, Timer, and FireballSystem.
        /// </summary>
        private void DelegatesSubscribiption()
        {
            // Subscribe to fail events
            foreach (FailSystem fs in failSystems)
            {
                fs.onFail += OnFail;
            }

            // Subscribe to score change event
            scoreSystem.onScoreChanged += OnScoreChanged;

            // Subscribe to ball reset complete event
            ballController.onResetComplete += OnBallResetComplete;
            ballController.onShoot += mainCamera.SetShooting;

            // Subscribe to timer events
            gameTimer.OnTimerEnd += OnTimerEnd;
            gameTimer.OnTimerUpdatePerc += OnTimerUpdatePerc;

            // Subscribe to fireball activation event
            fireballSystem.onFireballActivate += OnFireballActivate;
        }

        /// <summary>
        /// Unsubscribes from all previously subscribed events.
        /// </summary>
        private void DelegatesDesubscribiption()
        {
            // Unsubscribe from fail events
            foreach (FailSystem fs in failSystems)
            {
                fs.onFail -= OnFail;
            }

            // Unsubscribe from score change event
            scoreSystem.onScoreChanged -= OnScoreChanged;

            // Unsubscribe from ball reset complete event
            ballController.onResetComplete -= OnBallResetComplete;
            ballController.onShoot -= mainCamera.SetShooting;

            // Unsubscribe from timer events
            gameTimer.OnTimerEnd -= OnTimerEnd;
            gameTimer.OnTimerUpdatePerc -= OnTimerUpdatePerc;

            // Unsubscribe from fireball activation event
            fireballSystem.onFireballActivate -= OnFireballActivate;
        }
        #endregion

        #region FailSystem Callbacks
        /// <summary>
        /// Called when a fail event occurs. Handles player or AI fail logic.
        /// </summary>
        /// <param name="index">0 for player, 1 for AI</param>
        private void OnFail(int index)
        {
            switch (index)
            {
                case 0:
                    // Player fail: increment fail count, reset fireball, update score bonus, reset ball
                    failCount++;
                    fireballSystem.ResetFireballPoints();
                    scoreSystem.FireballBonus = fireballSystem.IsFireballActive;
                    PlayerBallReset();
                    break;
                case 1:
                    // AI fail: reset AI ball
                    ballAIController.ResetBall();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Fireball Callbacks
        /// <summary>
        /// Called when the fireball is activated. Enables fireball bonus in the score system.
        /// </summary>
        private void OnFireballActivate()
        {
            scoreSystem.FireballBonus = true;
        }
        #endregion

        #region Score Callbacks
        /// <summary>
        /// Called when the score changes. Handles player or AI scoring logic.
        /// </summary>
        /// <param name="index">0 for player, 1 for AI</param>
        /// <param name="score">New score value</param>
        /// <param name="delta">Score increment</param>
        private void OnScoreChanged(int index, int score, int delta)
        {
            switch (index)
            {
                case 0:
                    // Player scored: reset ball, update score, add fireball points
                    PlayerBallReset();
                    playerScore = score;
                    fireballSystem.AddFireballPoint(delta);
                    UI_Flyer.Text = delta.ToString();
                    break;
                case 1:
                    // AI scored: reset AI ball
                    ballAIController.ResetBall();
                    return;
                default:
                    return;
            }
        }
        #endregion

        #region BallController
        /// <summary>
        /// Resets the player's ball and increments the total shot count.
        /// </summary>
        private void PlayerBallReset()
        {
            totalShots++;
            ballController.ResetBall();
        }

        /// <summary>
        /// Called when the ball reset is complete. Updates the camera position.
        /// </summary>
        /// <param name="arg0">Transform of the reset ball</param>
        private void OnBallResetComplete(Transform arg0)
        {
            mainCamera.SetPosition();
        }
        #endregion

        #region Timer Callbacks
        /// <summary>
        /// Called on timer update. Handles bonus activation window and background music fade out.
        /// </summary>
        /// <param name="arg0">Current timer percentage (not used)</param>
        private void OnTimerUpdatePerc(float arg0)
        {
            int timeRaianing = (int)(gameTimer.TimeRemaning);

            // Activate bonus if within the bonus time window and not already active
            if (timeRaianing <= bonusTime[1] && timeRaianing >= bonusTime[0] && !bonusActive)
            {
                bonusActive = true;
                scoreSystem.EnableBonus();
            }

            // Deactivate bonus if outside the bonus time window
            if (timeRaianing < bonusTime[0] && bonusActive)
            {
                scoreSystem.DisableBonus();
                bonusActive = false;
            }

            // Start fading out background music as timer approaches zero
            if (timeRaianing <= fadeOutDuration && !fadeOutStarted)
            {
                fadeOutStarted = true;
                AudioMngr.Instance.StopWithFade("BackgroundMusic", fadeOutDuration);
            }
        }

        /// <summary>
        /// Called when the timer ends. Loads the end menu scene.
        /// </summary>
        private void OnTimerEnd()
        {
            SceneManager.LoadScene("EndMenu");
        }
        #endregion

        #region AI Callbacks
        /// <summary>
        /// Called when the difficulty is changed in the main menu. Stores the selected difficulty and loads the game scene.
        /// </summary>
        /// <param name="arg0">Selected difficulty string</param>
        private void OnDifficultyChange(string arg0)
        {
            Ui_MainMenu.onDifficultyChange -= OnDifficultyChange;
            diffcoulty = arg0;
            SceneManager.LoadScene(1);
        }
        #endregion
    }
}