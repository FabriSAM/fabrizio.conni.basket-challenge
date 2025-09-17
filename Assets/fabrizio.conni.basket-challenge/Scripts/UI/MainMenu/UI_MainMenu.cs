using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_MainMenu : MonoBehaviour
{
    public UIDocument uiDocument;

    // Start is called before the first frame update
    void Start()
    {
        
        print($"UI Document is {uiDocument}");
        var root = uiDocument.rootVisualElement;

        root.Q<Button>("play-button").clicked += () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        };
        root.Q<Button>("exit-button").clicked += () =>
        {
            Application.Quit();
        };

    }

    private void OnEnable()
    {
        Label titleLabel = uiDocument.rootVisualElement.Q<Label>("title");

        if (Screen.width <= 720)
        {
            titleLabel.style.fontSize = 20;
            List<Button> buttons = uiDocument.rootVisualElement.Query<Button>().ToList();

            foreach (var button in buttons)
            {
                button.style.fontSize = 10;
            }
        }
        else
        {
            titleLabel.style.fontSize = 48;
            List<Button> buttons = uiDocument.rootVisualElement.Query<Button>().ToList();

            foreach (var button in buttons)
            {
                button.style.fontSize = 20;
            }
        }


    }
}
