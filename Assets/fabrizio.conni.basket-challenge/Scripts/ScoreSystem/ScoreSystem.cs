using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private HoopSystem hoopSystem;
    [SerializeField]
    private Collider bonusCollider;
    [SerializeField]
    private GameObject bonusArea;
    [SerializeField]
    private TMP_Text bonusText;

    private TMP_Text playerScoreText;

    private int score;
    private int currentIncrementScore;
    private int currentBonus;
    private int[] bonusValues = new int[] { 4, 6, 8 };
    public bool FireballBonus { get; set; }

    public UnityAction<int, int> onScoreChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (currentIncrementScore == 0)
        {
            currentIncrementScore = 3;
        }

        if (FireballBonus)
        {
            currentIncrementScore *= 2;
        }
        score += currentIncrementScore;
    }

    private void Start()
    {
        playerScoreText = GameObject.Find("score_text").GetComponent<TMP_Text>();
        playerScoreText.text = "0";
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Score: {score}, Increment: {currentIncrementScore}, currentBonus: {currentBonus}, FireballBonus: {FireballBonus}");
        playerScoreText.text = score.ToString();        
        onScoreChanged?.Invoke(score, currentIncrementScore);
        ResetIncrementScore();
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision with: " + other.gameObject.name);
        currentIncrementScore = 2 + currentBonus;
    }

    private void Awake()
    {
        hoopSystem.hitHoop += OnHitHoop;
    }

    private void OnHitHoop()
    {
        if (currentIncrementScore != 0) return;
        
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
        int index = Random.Range(0, bonusValues.Length);
        int newBonus = bonusValues[index];
        currentBonus = newBonus;
        bonusCollider.enabled = true;
        bonusArea.SetActive(true);
        bonusText.text = "+" + newBonus.ToString();
    }

    public void DisableBonus()
    {
        Debug.Log("Bonus Disabled");
        currentBonus = 0;
        bonusCollider.enabled = false;
        bonusArea.SetActive(false);
    }
}

