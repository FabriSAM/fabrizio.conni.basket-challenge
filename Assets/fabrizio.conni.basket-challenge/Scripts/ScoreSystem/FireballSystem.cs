using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FireballSystem : MonoBehaviour
{
    [SerializeField]
    private float maxPoint = 5f;
    [SerializeField]
    private Timer fireballTimer;

    private float currentPoint;
    public bool IsFireballActive { get; private set; }

    public UnityAction onFireballActivate;

    private void Start()
    {
        fireballTimer.OnTimerEnd += TimerEnded;
    }

    private void TimerEnded()
    {
        IsFireballActive = false;
        currentPoint = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsFireballActive) return;

        if (currentPoint >= maxPoint)
        {
            IsFireballActive = true;
            onFireballActivate?.Invoke();
            fireballTimer.StartTimer();
        } 
    }

    public void AddFireballPoint(float pointToAdd)
    {
        if (pointToAdd == 0f || IsFireballActive) return;

        currentPoint = Mathf.Clamp(currentPoint + pointToAdd, 0f, maxPoint);
        fireballTimer.UpdateTimerBar(currentPoint / maxPoint);
    }

    public void ResetFireballPoints()
    {
        if (!IsFireballActive) return;

        fireballTimer.StopTimer();
        IsFireballActive = false;
        currentPoint = 0f;
    }
}
