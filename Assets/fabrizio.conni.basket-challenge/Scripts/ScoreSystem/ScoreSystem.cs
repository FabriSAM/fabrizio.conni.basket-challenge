using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoreSystem : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private HoopSystem hoopSystem;

    private int score;
    private int currentIncrementScore;

    public UnityAction<int> onScoreChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (currentIncrementScore == 0)
        {
            currentIncrementScore = 3;
        }
        score += currentIncrementScore;
        
        Debug.Log("Score: " + score);
    }

    private void OnTriggerExit(Collider other)
    {
        onScoreChanged?.Invoke(score);
    }

    private void Awake()
    {
        hoopSystem.hitHoop += OnHitHoop;
    }

    private void OnHitHoop()
    {
        currentIncrementScore = 2;
    }

    public void ResetScore()
    {
        score = 0;
    }
    
    public void ResetIncrementScore()
    { 
        currentIncrementScore = 0;
    }
}
