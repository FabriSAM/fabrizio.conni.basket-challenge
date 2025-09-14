using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    [SerializeField] 
    private float timeLimit = 60f;
    private float timeRemaining;
    private bool timerIsRunning = false;

    public float TimeRemaning { get { return timeRemaining; } }

    public UnityAction OnTimerEnd;
    public UnityAction OnTimerStart;
    public UnityAction<float> OnTimerUpdate;

    public void StartTimer()
    {
        timeRemaining = timeLimit;
        timerIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                OnTimerUpdate?.Invoke(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
                OnTimerEnd?.Invoke();
            }
        }

    }
}
