using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class UI_MainMenu : MonoBehaviour
{
    public UIDocument uiDocument;

    private VisualElement mainMenu;
    private VisualElement difficultyMenu;

    public UnityAction<string> onDifficultyChange;

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        mainMenu = root.Q<VisualElement>("main-menu");
        difficultyMenu = root.Q<VisualElement>("difficulty-menu");

        // Bottoni principali
        root.Q<Button>("play-button").clicked += ShowDifficultyMenu;
        root.Q<Button>("exit-button").clicked += () => Application.Quit();

        // Bottoni difficoltà
        root.Q<Button>("easy-button").clicked += () => SelectDifficulty("Easy");
        root.Q<Button>("medium-button").clicked += () => SelectDifficulty("Medium");
        root.Q<Button>("hard-button").clicked += () => SelectDifficulty("Hard");
        root.Q<Button>("back-button").clicked += ShowMainMenu;
    }

    private void ShowDifficultyMenu()
    {
        mainMenu.style.display = DisplayStyle.None;
        difficultyMenu.style.display = DisplayStyle.Flex;
    }

    private void ShowMainMenu()
    {
        difficultyMenu.style.display = DisplayStyle.None;
        mainMenu.style.display = DisplayStyle.Flex;
    }

    private void SelectDifficulty(string difficulty)
    {
        Debug.Log("Hai scelto difficoltà: " + difficulty);
        Debug.Log("onDifficultyChange è null? " + (onDifficultyChange == null));
        onDifficultyChange?.Invoke(difficulty);
    }

    private void OnEnable()
    {
        Label titleLabel = uiDocument.rootVisualElement.Q<Label>("title");

        if (Screen.width <= 720)
        {
            titleLabel.style.fontSize = 20;
            List<Button> buttons = uiDocument.rootVisualElement.Query<Button>().ToList();
            foreach (var button in buttons) button.style.fontSize = 10;
        }
        else
        {
            titleLabel.style.fontSize = 48;
            List<Button> buttons = uiDocument.rootVisualElement.Query<Button>().ToList();
            foreach (var button in buttons) button.style.fontSize = 20;
        }
    }
}
