using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UI_EndMenu : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument;

    private int totalShots;
    private int madeShots;
    private int missedShots;
    private int score;

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        root.Q<Label>("shots-total").text = $"Tiri Totali: {totalShots}";
        root.Q<Label>("shots-made").text = $"Tiri Corretti: {madeShots}";
        root.Q<Label>("shots-missed").text = $"Tiri Sbagliati: {missedShots}";
        root.Q<Label>("score").text = $"Punteggio: {score}";

        root.Q<Button>("retry-button").clicked += () => SceneManager.LoadScene("GameScene");
        root.Q<Button>("main-menu-button").clicked += () => SceneManager.LoadScene("MainMenu");
    }

    public void SetStats(int totalShots, int missedShots, int score)
    {
        this.totalShots = totalShots;
        madeShots = totalShots - missedShots;
        this.missedShots = missedShots;
        this.score = score;
    }
}
