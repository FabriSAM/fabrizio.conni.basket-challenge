using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] 
    private float timeLimit = 60f;
    [SerializeField]
    private string timerName;
    [SerializeField]
    private bool updateText = false;
    [SerializeField]
    private TMP_Text timerText;
    [SerializeField]
    private UI_TimeLeft uiTimeLeft;

    private float timeRemaining;
    private bool timerIsRunning = false;
    private UI_Timer uTimer;


    public float TimeRemaning { get { return timeRemaining; } }

    public UnityAction OnTimerEnd;
    public UnityAction OnTimerStart;
    public UnityAction<float> OnTimerUpdatePerc;

    void Start()
    {
        if (timerName == "") return;

        uTimer = GameObject.Find(timerName).GetComponent<UI_Timer>();

        if (updateText)
        {
            timerText = GameObject.Find("timer_text").GetComponent<TMP_Text>();
        }
       
    }

    public void StartTimer()
    {
        timeRemaining = timeLimit;
        timerIsRunning = true;
    }

    public void StopTimer()
    {
        timeRemaining = 0;
        timerIsRunning = false;
        updateTimerInternal();
        OnTimerEnd?.Invoke();
    }

    public void UpdateTimerBar(float newTimePerc)
    {
        if (timerIsRunning) return;
        uTimer.SetProgress(newTimePerc);
    }

    private void updateTimerInternal()
    {
        float timerperc = timeRemaining / timeLimit;
        if (uTimer)
            uTimer.SetProgress(timerperc);

        if (updateText)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

        if(uiTimeLeft != null)
            uiTimeLeft.UpdateTimeLeft(timeRemaining);

        OnTimerUpdatePerc?.Invoke(timerperc);

    }
    // Update is called once per frame
    void Update()
    {
        if(timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                updateTimerInternal();
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                OnTimerEnd?.Invoke();
            }
        }

    }
}
