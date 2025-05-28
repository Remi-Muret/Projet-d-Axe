using UnityEngine;

public class MainUIController : MonoBehaviour
{
    [SerializeField] private Transform[] uiCanvasii;
    [SerializeField] private HealthSystem _healthSystem;

    private float duration = 0.5f;
    private float time = 0f;
    private bool isAnimating = false;
    private bool isPaused = false;

    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        ActivateMainMenu();
    }

    void Update()
    {
        if (isAnimating)
        {
            time += Time.unscaledDeltaTime;
            float ratio = Mathf.Clamp01(time / duration);
            uiCanvasii[1].localPosition = Vector3.Lerp(startPos, targetPos, EaseInOutQuart(ratio));
            uiCanvasii[2].localPosition = Vector3.Lerp(startPos, targetPos, EaseInOutQuart(ratio));

            if (ratio >= 1f)
                isAnimating = false;
        }

        if (Input.GetButtonDown("Select"))
        {
            if (isPaused)
                DeactivatePauseMenu();
            else
                ActivatePauseMenu();
        }
    }

    float EaseInOutQuart(float x) => x < 0.5f ? 8f * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 4f) / 2f;

    void ActivateMainMenu()
    {
        Time.timeScale = 0;

        foreach (Transform menu in uiCanvasii)
            menu.gameObject.SetActive(false);

        uiCanvasii[0].gameObject.SetActive(true);
        uiCanvasii[1].gameObject.SetActive(true);
        uiCanvasii[2].gameObject.SetActive(true);
    }

    public void PlayEasyMode()
    {
        GameManager.Instance.GetPlayerData().health = 5;
        HealthSystem.Instance.InitHealth();
        Time.timeScale = 1;
        uiCanvasii[0].gameObject.SetActive(false);
        uiCanvasii[4].gameObject.SetActive(true);
    }

    public void PlayNormalMode()
    {
        GameManager.Instance.GetPlayerData().health = 3;
        HealthSystem.Instance.InitHealth();
        Time.timeScale = 1;
        uiCanvasii[0].gameObject.SetActive(false);
        uiCanvasii[4].gameObject.SetActive(true);
    }

    public void PlayHardMode()
    {
        GameManager.Instance.GetPlayerData().health = 1;
        HealthSystem.Instance.InitHealth();
        Time.timeScale = 1;
        uiCanvasii[0].gameObject.SetActive(false);
        uiCanvasii[4].gameObject.SetActive(true);
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

    void ActivatePauseMenu()
    {
        if (!uiCanvasii[0].gameObject.activeSelf)
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

    public void SetPlayerHealthSystem(HealthSystem healthSystem)
    {
        _healthSystem = healthSystem;
        if (_healthSystem.healthUI != null)
        {
            _healthSystem.healthUI.SetHealthPoint(_healthSystem._maxHealth);
            _healthSystem.healthUI.UpdateHealth(false, _healthSystem._currentHealth - 1, _healthSystem._maxHealth);
        }
    }
}
