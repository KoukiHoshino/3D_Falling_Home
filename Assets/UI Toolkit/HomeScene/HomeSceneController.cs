using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class HomeSceneController : MonoBehaviour
{
    // --- UI Element References ---
    private Button _startButton;
    private Button _optionsButton;
    private Button _quitButton;
    private Button _optionsBackButton;

    // --- Options Panel Navigation ---
    private Button _navHowToPlayButton;
    private Button _navSettingsButton;

    private VisualElement _mainMenuPanel;
    private VisualElement _optionsPanel;
    
    // --- Options Panel Content ---
    private VisualElement _howToPlayContent;
    private VisualElement _settingsContent;

    // --- Ranking UI References ---
    private List<Label> _scoreLabels;

    // --- Constants ---
    private const string SCORE_KEY = "highScores";

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // --- Panel Setup ---
        _mainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        _optionsPanel = root.Q<VisualElement>("OptionsPanel");

        // --- Main Menu Buttons ---
        _startButton = root.Q<Button>("StartButton");
        _startButton.RegisterCallback<ClickEvent>(ev => StartGame());

        _optionsButton = root.Q<Button>("OptionsButton");
        _optionsButton.RegisterCallback<ClickEvent>(ev => ShowPanel(_optionsPanel));
        
        _quitButton = root.Q<Button>("QuitButton");
        _quitButton.RegisterCallback<ClickEvent>(ev => QuitGame());

        // --- Options Panel Buttons and Content ---
        _optionsBackButton = root.Q<Button>("OptionsBackButton");
        _optionsBackButton.RegisterCallback<ClickEvent>(ev => ShowPanel(_mainMenuPanel));

        _navHowToPlayButton = root.Q<Button>("NavHowToPlayButton");
        _navHowToPlayButton.RegisterCallback<ClickEvent>(ev => ShowOptionsContent(_howToPlayContent));

        _navSettingsButton = root.Q<Button>("NavSettingsButton");
        _navSettingsButton.RegisterCallback<ClickEvent>(ev => ShowOptionsContent(_settingsContent));

        _howToPlayContent = root.Q<VisualElement>("HowToPlayContent");
        _settingsContent = root.Q<VisualElement>("SettingsContent");

        // --- Ranking UI Setup ---
        _scoreLabels = new List<Label>();
        for (int i = 0; i < 3; i++)
        {
            _scoreLabels.Add(root.Q<Label>($"ScoreLabel-{i}"));
        }
        
        // --- Initial Display ---
        ShowPanel(_mainMenuPanel);
        LoadAndDisplayScores();
    }
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            AddDummyScoreForTest();
        }
    }

    private void LoadAndDisplayScores()
    {
        string json = PlayerPrefs.GetString(SCORE_KEY, "{}");
        ScoreData scoreData = JsonUtility.FromJson<ScoreData>(json);
        List<int> scores = scoreData.scores ?? new List<int>();

        var sortedScores = scores.OrderByDescending(s => s).ToList();

        for (int i = 0; i < _scoreLabels.Count; i++)
        {
            if (i < sortedScores.Count)
            {
                _scoreLabels[i].text = sortedScores[i].ToString() + " PTS";
            }
            else
            {
                _scoreLabels[i].text = "-----";
            }
        }
    }
    
    private void AddDummyScoreForTest()
    {
        string json = PlayerPrefs.GetString(SCORE_KEY, "{}");
        ScoreData scoreData = JsonUtility.FromJson<ScoreData>(json);
        List<int> scores = scoreData.scores ?? new List<int>();

        scores.Add(Random.Range(5, 101));
        
        scoreData.scores = scores;
        string newJson = JsonUtility.ToJson(scoreData);
        PlayerPrefs.SetString(SCORE_KEY, newJson);
        PlayerPrefs.Save();

        LoadAndDisplayScores();
    }

    private void StartGame()
    {
        Debug.Log("Starting Game... Loading MainScene.");
        // SceneManager.LoadScene("MainScene"); 
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // メインパネルとオプションパネルの表示を切り替える
    private void ShowPanel(VisualElement panelToShow)
    {
        _mainMenuPanel.style.display = DisplayStyle.None;
        _optionsPanel.style.display = DisplayStyle.None;
        
        panelToShow.style.display = DisplayStyle.Flex;

        // オプションパネルを表示するときは、デフォルトで"How to Play"を表示
        if (panelToShow == _optionsPanel)
        {
            ShowOptionsContent(_howToPlayContent);
        }
    }

    // オプションパネル内のコンテンツを切り替える
    private void ShowOptionsContent(VisualElement contentToShow)
    {
        // 全てのコンテンツを一旦非表示
        _howToPlayContent.style.display = DisplayStyle.None;
        _settingsContent.style.display = DisplayStyle.None;
        
        // 指定されたコンテンツのみ表示
        contentToShow.style.display = DisplayStyle.Flex;

        // ナビゲーションボタンのアクティブ状態を更新
        _navHowToPlayButton.RemoveFromClassList("active");
        _navSettingsButton.RemoveFromClassList("active");

        if (contentToShow == _howToPlayContent)
        {
            _navHowToPlayButton.AddToClassList("active");
        }
        else if (contentToShow == _settingsContent)
        {
            _navSettingsButton.AddToClassList("active");
        }
    }

    [System.Serializable]
    private class ScoreData
    {
        public List<int> scores = new List<int>();
    }
} 
