using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HomeSceneController : MonoBehaviour
{
    private Button _startButton;
    private Button _howToPlayButton;
    private Button _settingsButton;
    private Button _quitButton;
    private Button _howToPlayBackButton;
    private Button _settingsBackButton;

    private VisualElement _mainMenuPanel;
    private VisualElement _howToPlayPanel;
    private VisualElement _settingsPanel;

    void OnEnable()
    {
        // UI DocumentからVisualElementを取得
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("HomeSceneController requires a UIDocument component.");
            return;
        }
        var root = uiDocument.rootVisualElement;

        // 各パネルを取得
        _mainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        _howToPlayPanel = root.Q<VisualElement>("HowToPlayPanel");
        _settingsPanel = root.Q<VisualElement>("SettingsPanel");

        // ボタンを取得し、クリックイベントを登録
        _startButton = root.Q<Button>("StartButton");
        _startButton.RegisterCallback<ClickEvent>(ev => StartGame());

        _howToPlayButton = root.Q<Button>("HowToPlayButton");
        _howToPlayButton.RegisterCallback<ClickEvent>(ev => ShowPanel(_howToPlayPanel));
        
        _settingsButton = root.Q<Button>("SettingsButton");
        _settingsButton.RegisterCallback<ClickEvent>(ev => ShowPanel(_settingsPanel));

        _quitButton = root.Q<Button>("QuitButton");
        _quitButton.RegisterCallback<ClickEvent>(ev => QuitGame());

        // 戻るボタンのイベント登録
        _howToPlayBackButton = root.Q<Button>("HowToPlayBackButton");
        _howToPlayBackButton.RegisterCallback<ClickEvent>(ev => ShowPanel(_mainMenuPanel));
        
        _settingsBackButton = root.Q<Button>("SettingsBackButton");
        _settingsBackButton.RegisterCallback<ClickEvent>(ev => ShowPanel(_mainMenuPanel));

        // 初期表示
        ShowPanel(_mainMenuPanel);
    }

    private void StartGame()
    {
        // 本番のプロジェクトに統合する際、"MainScene"がBuild Settingsにないとエラーになる
        Debug.Log("Starting Game... Loading MainScene.");
        // SceneManager.LoadScene("MainScene"); 
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Game...");
        // Unityエディタでの再生を停止する
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void ShowPanel(VisualElement panelToShow)
    {
        _mainMenuPanel.style.display = DisplayStyle.None;
        _howToPlayPanel.style.display = DisplayStyle.None;
        _settingsPanel.style.display = DisplayStyle.None;
        
        panelToShow.style.display = DisplayStyle.Flex;
    }
}
 