using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Timer : MonoBehaviour
{
    [SerializeField]
    private Image timerFill;

    public void SetProgress(float progress)
    {
        timerFill.fillAmount = progress;
    }
}
