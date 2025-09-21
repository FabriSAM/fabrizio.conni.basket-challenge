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
    private TMP_Text aiScoreText;

    private int score;
    private int currentIncrementScore;
    private int currentBonus;

    private int aiScore;
    private int aiCurrentIncrementScore;

    private int[] bonusValues = new int[] { 4, 6, 8 };
    public bool FireballBonus { get; set; }

    public UnityAction<int, int, int> onScoreChanged;

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Player":
                AddScore();
                break;
            case "Enemy":
                AddAiScore();
                break;
            default:
                break;
        }
    }

    private void AddScore()
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

    private void AddAiScore()
    {
        if (aiCurrentIncrementScore == 0)
        {
            aiCurrentIncrementScore = 3;
        }
        aiScore += aiCurrentIncrementScore;
    }
    private void Start()
    {
        playerScoreText = GameObject.Find("score_text").GetComponent<TMP_Text>();
        aiScoreText = GameObject.Find("ai_score_text").GetComponent<TMP_Text>();
        playerScoreText.text = "0";
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Score: {score}, Increment: {currentIncrementScore}, currentBonus: {currentBonus}, FireballBonus: {FireballBonus}");
        if ( other.gameObject.tag == "Player")
        {
            playerScoreText.text = score.ToString();
            onScoreChanged?.Invoke(0, score, currentIncrementScore);
            ResetPlayerIncrementScore();
        }
        else if ( other.gameObject.tag == "Enemy")
        {
            Debug.Log($"Score: {aiScore}");
            aiScoreText.text = aiScore.ToString();
            onScoreChanged?.Invoke(1, aiScore, aiCurrentIncrementScore);
            ResetAiIncrementScore();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "Player":
                currentIncrementScore = 2 + currentBonus;
                break;
            case "Enemy":
                aiCurrentIncrementScore = 2;
                break;
            default:
                break;
        }        
    }

    private void Awake()
    {
        hoopSystem.hitHoop += OnHitHoop;
    }

    private void OnHitHoop(int index)
    {
        switch (index)
        {
            case 0:
                if (currentIncrementScore != 0) return;

                currentIncrementScore = 2;
                break;
            case 1:
                if (aiCurrentIncrementScore != 0) return;

                aiCurrentIncrementScore = 2;
                break;
            default:
                break;
        }
    }

    public void ResetScore()
    {
        score = 0;
        aiScore = 0;
    }
    
    public void ResetPlayerIncrementScore()
    { 
        currentIncrementScore = 0;
    }
    public void ResetAiIncrementScore()
    {
        aiCurrentIncrementScore = 0;
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

