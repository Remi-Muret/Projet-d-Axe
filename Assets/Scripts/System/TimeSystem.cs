using System.Collections;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    [SerializeField] private float _timer;
    [SerializeField] private bool _deactivateTimer;

    private float _remainingTime;
    private int _lastPrintedSecond = -1;

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
                StartCoroutine(ResetTimerAfterDelay(respawnDelay));
                GameManager.Instance.StartCoroutine(GameManager.Instance.RespawnPlayerAfterDelay(respawnDelay, respawnPosition));
                Destroy(player);
            }
        }
    }

    IEnumerator ResetTimerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetTimer();
    }

    public void ResetTimer()
    {
        _remainingTime = _timer;
        _lastPrintedSecond = -1;
    }
}
