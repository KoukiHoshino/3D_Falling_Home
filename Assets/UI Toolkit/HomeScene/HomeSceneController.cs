using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using System.Collections;
using System.Text;

public class HomeSceneController : MonoBehaviour
{
    // --- UI Element References ---
    private Button _startButton, _optionsButton, _quitButton, _optionsBackButton;
    private Button _navHowToPlayButton, _navSettingsButton;
    private VisualElement _mainMenuPanel, _optionsPanel;
    private VisualElement _howToPlayContent, _settingsContent;
    private List<Label> _scoreLabels;
    
    // --- Typing Effect References ---
    private List<Label> _howToPlayDescriptionLabels;
    private List<string> _howToPlayOriginalTexts;
    private Coroutine _typingCoroutine;
    // ▼▼▼ 1行あたりのタイピング時間を設定 ▼▼▼
    [SerializeField] private float totalTypingDuration = 0.2f;

    // --- Constants ---
    private const string SCORE_KEY = "highScores";

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Panel Setup
        _mainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        _optionsPanel = root.Q<VisualElement>("OptionsPanel");

        // Main Menu Buttons
        _startButton = root.Q<Button>("StartButton");
        _startButton.RegisterCallback<ClickEvent>(ev => StartGame());
        _optionsButton = root.Q<Button>("OptionsButton");
        _optionsButton.RegisterCallback<ClickEvent>(ev => ShowPanel(_optionsPanel));
        _quitButton = root.Q<Button>("QuitButton");
        _quitButton.RegisterCallback<ClickEvent>(ev => QuitGame());

        // Options Panel
        _optionsBackButton = root.Q<Button>("OptionsBackButton");
        _optionsBackButton.RegisterCallback<ClickEvent>(ev => ShowPanel(_mainMenuPanel));
        _navHowToPlayButton = root.Q<Button>("NavHowToPlayButton");
        _navHowToPlayButton.RegisterCallback<ClickEvent>(ev => ShowOptionsContent(_howToPlayContent));
        _navSettingsButton = root.Q<Button>("NavSettingsButton");
        _navSettingsButton.RegisterCallback<ClickEvent>(ev => ShowOptionsContent(_settingsContent));
        _howToPlayContent = root.Q<VisualElement>("HowToPlayContent");
        _settingsContent = root.Q<VisualElement>("SettingsContent");

        // Ranking UI
        _scoreLabels = new List<Label>();
        for (int i = 0; i < 3; i++) _scoreLabels.Add(root.Q<Label>($"ScoreLabel-{i}"));
        
        // Typing Effect Setup
        _howToPlayDescriptionLabels = _howToPlayContent.Query<Label>(className: "description-text").ToList();
        _howToPlayOriginalTexts = new List<string>();
        foreach(var label in _howToPlayDescriptionLabels)
        {
            _howToPlayOriginalTexts.Add(label.text);
        }

        // Initial Display
        _mainMenuPanel.style.display = DisplayStyle.Flex;
        _optionsPanel.style.display = DisplayStyle.None;
        _optionsPanel.RemoveFromClassList("visible");
        
        LoadAndDisplayScores();
    }
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) AddDummyScoreForTest();
    }

    private void LoadAndDisplayScores()
    {
        string json = PlayerPrefs.GetString(SCORE_KEY, "{}");
        ScoreData scoreData = JsonUtility.FromJson<ScoreData>(json);
        List<int> scores = scoreData?.scores ?? new List<int>();
        var sortedScores = scores.OrderByDescending(s => s).ToList();
        for (int i = 0; i < _scoreLabels.Count; i++)
            _scoreLabels[i].text = i < sortedScores.Count ? sortedScores[i].ToString() + " PTS" : "-----";
    }
    
    private void AddDummyScoreForTest()
    {
        string json = PlayerPrefs.GetString(SCORE_KEY, "{}");
        ScoreData scoreData = JsonUtility.FromJson<ScoreData>(json);
        List<int> scores = scoreData?.scores ?? new List<int>();
        scores.Add(Random.Range(5, 101));
        scoreData.scores = scores;
        PlayerPrefs.SetString(SCORE_KEY, JsonUtility.ToJson(scoreData));
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

    private void ShowPanel(VisualElement panelToShow)
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

        _mainMenuPanel.style.display = DisplayStyle.None;
        _optionsPanel.style.display = DisplayStyle.None;
        _optionsPanel.RemoveFromClassList("visible");

        panelToShow.style.display = DisplayStyle.Flex;

        if (panelToShow == _optionsPanel)
        {
            panelToShow.schedule.Execute(() => panelToShow.AddToClassList("visible"));
            ShowOptionsContent(_howToPlayContent);
        }
    }

    private void ShowOptionsContent(VisualElement contentToShow)
    {
        _howToPlayContent.style.display = DisplayStyle.None;
        _settingsContent.style.display = DisplayStyle.None;
        contentToShow.style.display = DisplayStyle.Flex;

        _navHowToPlayButton.RemoveFromClassList("active");
        _navSettingsButton.RemoveFromClassList("active");

        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        
        if (contentToShow == _howToPlayContent)
        {
            _navHowToPlayButton.AddToClassList("active");
            _typingCoroutine = StartCoroutine(TypingMasterCoroutine());
        }
        else if (contentToShow == _settingsContent)
        {
            _navSettingsButton.AddToClassList("active");
        }
    }

    private IEnumerator TypingMasterCoroutine()
    {
        foreach(var label in _howToPlayDescriptionLabels) label.text = "";
        for (int i = 0; i < _howToPlayDescriptionLabels.Count; i++)
        {
            yield return StartCoroutine(TypewriterEffectCoroutine(_howToPlayDescriptionLabels[i], _howToPlayOriginalTexts[i]));
        }
        _typingCoroutine = null;
    }

    // ▼▼▼ タイピングロジックを修正 ▼▼▼
    private IEnumerator TypewriterEffectCoroutine(Label label, string text)
    {
        // 1文字あたりの待機時間を、総時間と文字数から計算
        float perCharacterDelay = text.Length > 0 ? totalTypingDuration / text.Length : 0f;

        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            stringBuilder.Append(text[i]);
            label.text = stringBuilder.ToString();
            yield return new WaitForSeconds(perCharacterDelay);
        }
    }

    [System.Serializable] private class ScoreData { public List<int> scores = new List<int>(); }
}
