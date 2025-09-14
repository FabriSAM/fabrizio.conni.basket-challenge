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

    // Update is called once per frame
    void Update()
    {
        
    }
}
