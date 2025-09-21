using FabrizioConni.BasketChallenge.Ball;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMngr : MonoBehaviour
{
    [SerializeField, Tooltip("Delta from start to end bounus time (in Seconds)")]
    private int deltaBounsTime = 10;


    private Timer gameTimer;

    public static GameMngr Instance { get; private set; }


    private BallController ballController;
    private AiController ballAIController;
    private ScoreSystem scoreSystem;
    private FailSystem[] failSystems;
    private FixedCamera mainCamera;

    private UI_MainMenu Ui_MainMenu;

    private FireballSystem fireballSystem;
    private int playerScore;
    private int failCount;
    private int totalShots;

    private string diffcoulty;

    private float[] bonusTime = new float[2];
    private bool bonusActive;

    private void Start()
    {
        Ui_MainMenu = FindObjectOfType<UI_MainMenu>();
        Ui_MainMenu.onDifficultyChange += OnDifficultyChange;
    }

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

    private void Init()
    {
        // Find references
        FindReferences();

        // Subscribe to events
        DelegatesSubscribiption();

        // Initialize Main Camera
        mainCamera.SetPlayer(ballController.gameObject);
        mainCamera.SetHoopPosition(ballController.HoopCenterLocation);

        bonusTime[1] = UnityEngine.Random.Range(5f, 60f);
        bonusTime[0] = bonusTime[1] - 10f;

        // Start Game
        gameTimer.StartTimer();
        scoreSystem.ResetScore();

        ballAIController.Init(diffcoulty);
    }

    private void FindReferences()
    {
        ballController = FindObjectOfType<BallController>();
        ballAIController = FindObjectOfType<AiController>();
        
        scoreSystem = FindObjectOfType<ScoreSystem>();
        failSystems = FindObjectsOfType<FailSystem>();
        mainCamera = FindObjectOfType<FixedCamera>();
        gameTimer = GameObject.Find("MatchTimer").GetComponent<Timer>();

        fireballSystem = FindObjectOfType<FireballSystem>();
    }

    #region Delegate Subscription/Desubscription
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

        // Subscribe to timer events
        gameTimer.OnTimerEnd += OnTimerEnd;
        gameTimer.OnTimerUpdatePerc += OnTimerUpdatePerc;

        fireballSystem.onFireballActivate += OnFireballActivate;
    }

    private void DelegatesDesubscribiption()
    {
        // Subscribe to fail events
        foreach (FailSystem fs in failSystems)
        {
            fs.onFail -= OnFail;
        }

        // Subscribe to score change event
        scoreSystem.onScoreChanged -= OnScoreChanged;

        // Subscribe to ball reset complete event
        ballController.onResetComplete -= OnBallResetComplete;

        // Subscribe to timer events
        gameTimer.OnTimerEnd -= OnTimerEnd;
        gameTimer.OnTimerUpdatePerc -= OnTimerUpdatePerc;

        fireballSystem.onFireballActivate -= OnFireballActivate;
    }
    #endregion

    #region FailSystem Callbacks
    private void OnFail(int index)
    {
        switch (index)
        {
            case 0:
                failCount++;
                fireballSystem.ResetFireballPoints();
                scoreSystem.FireballBonus = fireballSystem.IsFireballActive;
                PlayerBallReset();
                break;
            case 1:
                ballAIController.ResetBall();
                break;
            default:
                break;
        }

    }
    #endregion

    #region Fireball Callbacks
    private void OnFireballActivate()
    {
        scoreSystem.FireballBonus = true;
    }
    #endregion

    #region Score Callbacks
    private void OnScoreChanged(int index, int score, int delta)
    { 
        switch (index)
        {
            case 0:
                PlayerBallReset();
                playerScore = score;
                fireballSystem.AddFireballPoint(delta);
                break;
            case 1:
                ballAIController.ResetBall();
                return;
            default:
                return;
        }
    }
    #endregion

    #region BallController
    private void PlayerBallReset()
    {
        totalShots++;
        
        ballController.ResetBall();
    }
    private void OnBallResetComplete(Transform arg0)
    {
        mainCamera.SetPosition();
    }
    #endregion

    #region Timer Callbacks
    private void OnTimerUpdatePerc(float arg0)
    {
        int timeRaianing = (int)(gameTimer.TimeRemaning);

        if(timeRaianing <= bonusTime[1] && timeRaianing >= bonusTime[0] && !bonusActive)
        {
            bonusActive = true;
            
            scoreSystem.EnableBonus();            
        }

        if(timeRaianing < bonusTime[0] && bonusActive)
        {
            scoreSystem.DisableBonus();
            bonusActive = false;
        }            
    }

    private void OnTimerEnd()
    {
        SceneManager.LoadScene("EndMenu");
    }
    #endregion

    private void OnLevelWasLoaded(int level)
    {
        switch (level)
        {
            case 0:
                //Main Menu
                //DelegatesDesubscribiption();
                Ui_MainMenu = FindObjectOfType<UI_MainMenu>();
                Ui_MainMenu.onDifficultyChange += OnDifficultyChange;
                break;
            case 1:
                //Game Scene
                //Reassign references
                Init();
                break;
            case 2:
                //End Menu
                DelegatesDesubscribiption();
                FindObjectOfType<UI_EndMenu>().SetStats(totalShots, failCount, playerScore);
                break;
            default:
                break;
        }
    }

    private void OnDifficultyChange(string arg0)
    {
        Ui_MainMenu.onDifficultyChange -= OnDifficultyChange;
        diffcoulty = arg0;

        SceneManager.LoadScene(1);
    }
}
