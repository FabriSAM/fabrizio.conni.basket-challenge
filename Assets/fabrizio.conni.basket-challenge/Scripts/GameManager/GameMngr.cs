using UnityEngine;
using FabrizioConni.BasketChallenge.Ball;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameMngr : MonoBehaviour
{
    [SerializeField, Tooltip("Delta from start to end bounus time (in Seconds)")]
    private int deltaBounsTime = 10;


    private Timer gameTimer;

    public static GameMngr Instance { get; private set; }


    private BallController ballController;
    private ScoreSystem scoreSystem;
    private FailSystem[] failSystems;
    private FixedCamera mainCamera;
       
    private FireballSystem fireballSystem;
    private int playerScore;
    private int failCount;
    private int totalShots;

    private float[] bonusTime = new float[2];
    private bool bonusActive;

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

        Init();
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

        bonusTime[1] = Random.Range(5f, 60f);
        bonusTime[0] = bonusTime[1] - 10f;

        // Start Game
        gameTimer.StartTimer();
        scoreSystem.ResetScore();
        
    }

    private void FindReferences()
    {
        ballController = FindObjectOfType<BallController>();
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
    private void OnFail()
    {
        failCount++;
        fireballSystem.ResetFireballPoints();
        scoreSystem.FireballBonus = fireballSystem.IsFireballActive;
        ResetBall();
    }
    #endregion

    #region Fireball Callbacks
    private void OnFireballActivate()
    {
        scoreSystem.FireballBonus = true;
    }
    #endregion

    #region Score Callbacks
    private void OnScoreChanged(int score, int delta)
    {
        ResetBall();
        playerScore = score;
        fireballSystem.AddFireballPoint(delta);
    }
    #endregion

    #region BallController
    private void ResetBall()
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

        if(timeRaianing < bonusTime[0])
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
                DelegatesDesubscribiption();
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


}
