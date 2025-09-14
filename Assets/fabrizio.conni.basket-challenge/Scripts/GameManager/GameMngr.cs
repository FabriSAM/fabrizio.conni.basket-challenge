using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FabrizioConni.BasketChallenge.Ball;
using System;

public class GameMngr : MonoBehaviour
{
    public static GameMngr Instance { get; private set; }

    [SerializeField]
    private BallController ballController;
    [SerializeField]
    private ScoreSystem scoreSystem;
    [SerializeField]
    private FailSystem[] failSystems;
    [SerializeField]
    private FixedCamera mainCamera;

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

        foreach (FailSystem fs in failSystems)
        {
            fs.onFail += OnFail;
        }
        scoreSystem.onScoreChanged += OnScoreChanged;
        ballController.onResetComplete += OnBallResetComplete;

        print($"Ball Controller is {ballController.gameObject}");
        mainCamera.SetPlayer(ballController.gameObject);
        mainCamera.SetHoopPosition(ballController.HoopCenterLocation);
    }

    private void OnBallResetComplete(Transform arg0)
    {
        mainCamera.SetPosition();
    }

    private void OnFail()
    {
        ResetBall();
    }

    private void OnScoreChanged(int arg0)
    {
        Debug.Log("Score Updated: " + arg0);
        scoreSystem.ResetIncrementScore();
        ResetBall();
    }

    private void ResetBall()
    {
        ballController.ResetBall();
    }
}
