using System.Collections;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }

    [SerializeField] private float _timer;
    [SerializeField] private bool _deactivateTimer;

    private float _globalTimer;
    private float _remainingTime;
    private bool _isRunning;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _remainingTime = _timer;
    }

    void Update()
    {
        if (_deactivateTimer || !_isRunning) return;

        _globalTimer += Time.deltaTime;
        _remainingTime -= Time.deltaTime;

        int currentSecond = Mathf.CeilToInt(_remainingTime);

        if (_remainingTime <= 0f)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 respawnPosition = GameManager.Instance.GetLastCheckpointPosition();
                float respawnDelay = GameManager.Instance.GetPlayerData().respawnDelay;

                Destroy(player);
                GameManager.Instance.StartCoroutine(GameManager.Instance.Respawn(respawnDelay, respawnPosition));
            }
        }
    }

    public void StartTimer()
    {
        _isRunning = true;
        _globalTimer = 0f;
        _remainingTime = _timer;
    }

    public void StopTimer()
    {
        _isRunning = false;
    }


    public void ResetTimer()
    {
        _remainingTime = _timer;
    }

    public void AddTime(float timeToAdd)
    {
        _remainingTime += timeToAdd;
        _remainingTime = Mathf.Min(_remainingTime, _timer);
    }

    public float GetGlobalTimer()
    {
        return _globalTimer;
    }

    public float GetRemainingTime()
    {
        return _remainingTime;
    }

    public float GetMaxTime()
    {
        return _timer;
    }
}
