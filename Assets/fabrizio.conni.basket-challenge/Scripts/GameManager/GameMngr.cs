using UnityEngine;
using FabrizioConni.BasketChallenge.Ball;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameMngr : MonoBehaviour
{
    public static GameMngr Instance { get; private set; }


    private BallController ballController;
    private ScoreSystem scoreSystem;
    private FailSystem[] failSystems;
    private FixedCamera mainCamera;
    private Timer gameTimer;
    private UI_Timer uTimer;
    private UI_Timer uPower;
    private TMP_Text playerScoreText;
    private TMP_Text timeLeftText;
    private int playerScore;
    private int failCount;
    private int totalShots;

    private float startBonusTime;
    private float endBonusTime;
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

        //startBonusTime = Random.Range(5f, 60f);
        startBonusTime = 58f;
        endBonusTime = startBonusTime - 3f;

        // Start Game
        gameTimer.StartTimer();
        scoreSystem.ResetScore();
        playerScoreText.text = "0";

    }

    private void FindReferences()
    {
        ballController = FindObjectOfType<BallController>();
        scoreSystem = FindObjectOfType<ScoreSystem>();
        failSystems = FindObjectsOfType<FailSystem>();
        mainCamera = FindObjectOfType<FixedCamera>();
        gameTimer = FindObjectOfType<Timer>();
        UI_Timer[] timers = FindObjectsOfType<UI_Timer>();
        uTimer = timers[0];
        uPower = timers[1];
        playerScoreText = GameObject.Find("score_text").GetComponent<TMP_Text>();
        timeLeftText = GameObject.Find("timer_text").GetComponent<TMP_Text>();
    }

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
        ballController.onShotPowerChange += OnShotPowerChange;

        // Subscribe to timer events
        gameTimer.OnTimerEnd += OnTimerEnd;
        gameTimer.OnTimerUpdatePerc += OnTimerUpdatePerc;
    }

    private void OnShotPowerChange(float arg0)
    {
        uPower.SetProgress(arg0);
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
        ballController.onShotPowerChange -= OnShotPowerChange;


        // Subscribe to timer events
        gameTimer.OnTimerEnd -= OnTimerEnd;
        gameTimer.OnTimerUpdatePerc -= OnTimerUpdatePerc;
    }

    private void OnBallResetComplete(Transform arg0)
    {
        mainCamera.SetPosition();
    }

    private void OnFail()
    {
        failCount++;
        ResetBall();
    }

    private void OnScoreChanged(int arg0)
    {
        scoreSystem.ResetIncrementScore();
        ResetBall();
        playerScore = arg0;
        playerScoreText.text = arg0.ToString();
    }

    private void ResetBall()
    {
        totalShots++;
        ballController.ResetBall();
    }

    private void OnTimerUpdatePerc(float arg0)
    {
        uTimer.SetProgress(arg0);
        int timeRaianing = (int)(gameTimer.TimeRemaning);
        timeLeftText.text = (timeRaianing).ToString();

        if(timeRaianing <= startBonusTime && timeRaianing >= endBonusTime && !bonusActive)
        {
            bonusActive = true;
            scoreSystem.EnableBonus();
            
        }

        if(timeRaianing < endBonusTime)
        {
            scoreSystem.DisableBonus();
            bonusActive = false;
        }
            
    }
    private void OnTimerEnd()
    {
        SceneManager.LoadScene("EndMenu");
    }

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
