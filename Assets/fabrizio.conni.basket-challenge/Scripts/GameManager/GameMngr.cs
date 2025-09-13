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
    }

    private void OnFail()
    {
        ResetBall();
    }

    private void OnScoreChanged(int arg0)
    {
        Debug.Log("Score Updated: " + arg0);
        ResetBall();
    }
        private void ResetBall()
    {
        GameObject emptyGO = new GameObject();
        emptyGO.transform.position = new Vector3(-0.699000001f, 0.52700001f, 0);
        emptyGO.transform.rotation = new Quaternion(0, 0.707106829f, 0, -0.707106829f);


        ballController.ResetBall(emptyGO.transform);
    }
}
