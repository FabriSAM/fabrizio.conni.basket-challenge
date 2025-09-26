using UnityEngine;
using TMPro;

public class UI_Score : MonoBehaviour
{
    [SerializeField] 
    private TMP_Text united;
    [SerializeField]
    private TMP_Text ten;
    [SerializeField]
    private TMP_Text hundreds;

    public void UpdateScore(int score)
    {
        int hundredsValue = score / 100;
        int tensValue = (score % 100) / 10;
        int unitsValue = score % 10;
        hundreds.text = hundredsValue.ToString();
        ten.text = tensValue.ToString();
        united.text = unitsValue.ToString();
    }
}
