using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{
    [SerializeField] private Transform[] uiCanvasii;
    [SerializeField] private TMP_Text victoryTimeText;
    [SerializeField] private HealthSystem _healthSystem;

    private float duration = 0.5f;
    private float time = 0f;
    private bool isAnimating = false;
    private bool isPaused = false;
    private string currentDifficulty;

    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        HealthSystem healthSystem = player.GetComponent<HealthSystem>();
        HealthSystem.SetInstance(healthSystem);

        healthSystem.healthUI = FindFirstObjectByType<HealthUI>();
        healthSystem.InitHealth();

        SetPlayerHealthSystem(healthSystem);
        ActivateMainMenu();
    }

    void Update()
    {
        if (Input.GetButtonDown("Select"))
        {
            if (isPaused)
                DeactivatePauseMenu();
            else
                ActivatePauseMenu();
        }

        if (isAnimating)
        {
            time += Time.unscaledDeltaTime;
            float ratio = Mathf.Clamp01(time / duration);
            uiCanvasii[1].localPosition = Vector3.Lerp(startPos, targetPos, EaseInOutQuart(ratio));
            uiCanvasii[2].localPosition = Vector3.Lerp(startPos, targetPos, EaseInOutQuart(ratio));

            if (ratio >= 1f)
                isAnimating = false;
        }
    }

    float EaseInOutQuart(float x) => x < 0.5f ? 8f * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 4f) / 2f;

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);

        return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }

    void ActivateMainMenu()
    {
        Time.timeScale = 0;

        foreach (Transform ui in uiCanvasii)
            ui.gameObject.SetActive(false);

        uiCanvasii[0].gameObject.SetActive(true);
        uiCanvasii[1].gameObject.SetActive(true);
        uiCanvasii[2].gameObject.SetActive(true);
    }

    void ActivatePauseMenu()
    {
        if (!uiCanvasii[0].gameObject.activeSelf && !uiCanvasii[4].gameObject.activeSelf)
        {
            isPaused = true;
            Time.timeScale = 0;
            uiCanvasii[3].gameObject.SetActive(true);
        }
    }

    void DeactivatePauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1;
        uiCanvasii[3].gameObject.SetActive(false);
    }

    public void PlayEasyMode()
    {
        currentDifficulty = "Easy";

        GameManager.Instance.GetPlayerData().health = 5;
        HealthSystem.Instance.InitHealth();
        TimeSystem.Instance.StartTimer();
        Time.timeScale = 1;
        uiCanvasii[0].gameObject.SetActive(false);
        uiCanvasii[5].gameObject.SetActive(true);
    }

    public void PlayNormalMode()
    {
        currentDifficulty = "Normal";

        GameManager.Instance.GetPlayerData().health = 3;
        HealthSystem.Instance.InitHealth();
        TimeSystem.Instance.StartTimer();
        Time.timeScale = 1;
        uiCanvasii[0].gameObject.SetActive(false);
        uiCanvasii[5].gameObject.SetActive(true);
    }

    public void PlayHardMode()
    {
        currentDifficulty = "Hard";

        GameManager.Instance.GetPlayerData().health = 1;
        HealthSystem.Instance.InitHealth();
        TimeSystem.Instance.StartTimer();
        Time.timeScale = 1;
        uiCanvasii[0].gameObject.SetActive(false);
        uiCanvasii[5].gameObject.SetActive(true);
    }

    public void SetPlayerHealthSystem(HealthSystem healthSystem)
    {
        _healthSystem = healthSystem;
        if (_healthSystem.healthUI != null)
        {
            _healthSystem.healthUI.SetHealthPoint(_healthSystem._maxHealth);
            _healthSystem.healthUI.UpdateHealth(false, _healthSystem._currentHealth - 1, _healthSystem._maxHealth);
        }
    }

    public void SlideMainMenu()
    {
        RectTransform rect = uiCanvasii[0].GetComponent<RectTransform>();

        float offsetY = rect.rect.height * -1f;
        startPos = rect.localPosition;
        targetPos = startPos + new Vector3(0, offsetY, 0);

        time = 0f;
        isAnimating = true;
    }

    public void ActivateVictoryScreen()
    {
        Time.timeScale = 0;
        TimeSystem.Instance.StopTimer();

        foreach (Transform ui in uiCanvasii)
            ui.gameObject.SetActive(false);

        float finalTime = TimeSystem.Instance.GetGlobalTimer();
        string bestTimeKey = $"BestTime_{currentDifficulty}";
        float bestTime = PlayerPrefs.GetFloat(bestTimeKey, float.MaxValue);
        bool isNewRecord = finalTime < bestTime;

        if (isNewRecord)
            PlayerPrefs.SetFloat(bestTimeKey, finalTime);

        string formattedFinalTime = FormatTime(finalTime);
        string formattedBestTime = FormatTime(Mathf.Min(finalTime, bestTime));

        victoryTimeText.text =
            $"Your time : {formattedFinalTime}\n" +
            $"Best time : {formattedBestTime}";

        if (isNewRecord)
            victoryTimeText.text += "\nNew record !";

        uiCanvasii[4].gameObject.SetActive(true);
    }

    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
