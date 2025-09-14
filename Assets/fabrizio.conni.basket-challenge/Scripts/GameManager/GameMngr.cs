using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FabrizioConni.BasketChallenge.Ball;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using Codice.Client.Common.GameUI;

public class GameMngr : MonoBehaviour
{
    public static GameMngr Instance { get; private set; }


    private BallController ballController;
    private ScoreSystem scoreSystem;
    private FailSystem[] failSystems;
    private FixedCamera mainCamera;
    private Timer gameTimer;
    private UI_Timer uTimer;
    private TMP_Text playerScoreText;
    private int playerScore;
    private int failCount;
    private int totalShots;

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
        uTimer = FindObjectOfType<UI_Timer>();
        playerScoreText = GameObject.Find("score_text").GetComponent<TMP_Text>();
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

        // Subscribe to timer events
        gameTimer.OnTimerEnd += OnTimerEnd;
        gameTimer.OnTimerUpdatePerc += OnTimerUpdatePerc;
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
        Debug.Log("Score Updated: " + arg0);
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
