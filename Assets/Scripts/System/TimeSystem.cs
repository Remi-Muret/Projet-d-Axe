using System.Collections;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }

    [SerializeField] private float _timer;
    [SerializeField] private bool _deactivateTimer;

    private float _remainingTime;
    private int _lastPrintedSecond = -1;

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
        if (_deactivateTimer) return;

        _remainingTime -= Time.deltaTime;

        int currentSecond = Mathf.CeilToInt(_remainingTime);
        if (currentSecond != _lastPrintedSecond)
        {
            Debug.Log("Temps restant : " + currentSecond + "s");
            _lastPrintedSecond = currentSecond;
        }

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

    public void ResetTimer()
    {
        _remainingTime = _timer;
        _lastPrintedSecond = -1;
    }

    public void AddTime(float timeToAdd)
    {
        if (_deactivateTimer) return;

        _remainingTime += timeToAdd;
        _remainingTime = Mathf.Min(_remainingTime, _timer);
    }
}
