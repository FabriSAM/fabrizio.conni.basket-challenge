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
    [SerializeField]
    private Collider bonusCollider;
    [SerializeField]
    private GameObject bonusArea;

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
    }

    private void OnTriggerExit(Collider other)
    {
        onScoreChanged?.Invoke(score);
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision");
        bonusCollider.enabled = false;
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

    public void EnableBonus()
    {
        bonusCollider.enabled = true;
        bonusArea.SetActive(true);
    }

    public void DisableBonus()
    {
        bonusCollider.enabled = false;
        bonusArea.SetActive(false);
    }
}

