using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UI_TimeLeft : MonoBehaviour
{
    [SerializeField]
    private TMP_Text timeLeftText;

    public void UpdateTimeLeft(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60F);
        int seconds = Mathf.FloorToInt(timeLeft - minutes * 60);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        timeLeftText.text = niceTime;
    }
}
